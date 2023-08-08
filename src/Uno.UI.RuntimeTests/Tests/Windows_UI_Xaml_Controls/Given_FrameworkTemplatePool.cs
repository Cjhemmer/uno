﻿using System;
using System.Linq;
using System.Threading.Tasks;
using FrameworkPoolEditorRecycling;
using Uno.UI.RuntimeTests.Helpers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using static Private.Infrastructure.TestServices;

namespace Uno.UI.RuntimeTests.Tests.Windows_UI_Xaml_Controls;

[TestClass]
[RunsOnUIThread]
internal class Given_FrameworkTemplatePool
{
	[TestMethod]
	public async Task TestCheckBox()
	{
		using (FeatureConfigurationHelper.UseTemplatePooling())
		{
			var scrollViewer = new ScrollViewer();
			var elevatedViewChild = new Uno.UI.Toolkit.ElevatedView();
			scrollViewer.Content = elevatedViewChild;

			var c = new CheckBox();
			c.IsChecked = true;
			bool uncheckedFired = false;
			c.Unchecked += (_, _) => uncheckedFired = true;
			elevatedViewChild.ElevatedContent = c;

			WindowHelper.WindowContent = scrollViewer;
			await WindowHelper.WaitForLoaded(scrollViewer);
			var template = scrollViewer.Template;

			scrollViewer.Template = null;
			scrollViewer.Template = template;

			scrollViewer.ApplyTemplate();

			Assert.IsFalse(uncheckedFired);
		}
	}

	[TestMethod]
	public async Task TestTextBox()
	{
		using (FeatureConfigurationHelper.UseTemplatePooling())
		{
			var scrollViewer = new ScrollViewer();

			var textBox = new TextBox();
			bool textChangedFired = false;
			textBox.TextChanged += (_, _) => textChangedFired = true;
			textBox.Text = "Text";

			// TextBox dispatches raising the event. We wait here until the above TextChanged is raised.
			await Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { });

			Assert.IsTrue(textChangedFired);
			textChangedFired = false;

			scrollViewer.Content = textBox;

			WindowHelper.WindowContent = scrollViewer;
			await WindowHelper.WaitForLoaded(scrollViewer);
			var template = scrollViewer.Template;

			scrollViewer.Template = null;
			scrollViewer.Template = template;

			scrollViewer.ApplyTemplate();

			await Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { });

			Assert.IsFalse(textChangedFired);
		}
	}

	[TestMethod]
	public async Task TestToggleSwitch()
	{
		using (FeatureConfigurationHelper.UseTemplatePooling())
		{
			var scrollViewer = new ScrollViewer();

			var toggleSwitch = new ToggleSwitch();
			toggleSwitch.IsOn = true;
			bool toggledFired = false;
			toggleSwitch.Toggled += (_, _) => toggledFired = true;
			scrollViewer.Content = toggleSwitch;

			WindowHelper.WindowContent = scrollViewer;
			await WindowHelper.WaitForLoaded(scrollViewer);
			var template = scrollViewer.Template;

			scrollViewer.Template = null;
			scrollViewer.Template = template;

			scrollViewer.ApplyTemplate();

			Assert.IsFalse(toggledFired);
		}
	}

	[TestMethod]
	public async Task When_Editor_Recycling()
	{
		using var _ = FeatureConfigurationHelper.UseTemplatePooling();

		var page = new EditorTestPage();

		WindowHelper.WindowContent = page;
		await WindowHelper.WaitForLoaded(page);
		await WindowHelper.WaitForIdle();

		var vm = page.ViewModel;

		void AssertEditorContents()
		{
			var textBox = page.FindFirstChild<TextBox>();
			var checkBox = page.FindFirstChild<CheckBox>();
			var toggleSwitch = page.FindFirstChild<ToggleSwitch>();
			Assert.IsTrue(vm.Editors.All(e => !string.IsNullOrEmpty(e.Text)));
			Assert.IsTrue(vm.Editors.All(e => e.IsChecked));
			Assert.IsTrue(vm.Editors.All(e => e.IsOn));
			Assert.IsTrue(!string.IsNullOrEmpty(textBox.Text));
			Assert.IsTrue(checkBox.IsChecked);
			Assert.IsTrue(toggleSwitch.IsOn);
			Assert.AreEqual(vm.CurrentEditor.IsChecked, checkBox.IsChecked);
			Assert.AreEqual(vm.CurrentEditor.IsOn, toggleSwitch.IsOn);
		}

		// Verify initial state
		AssertEditorContents();

		TextBox textBox = null;
		CheckBox checkBox = null;
		ToggleSwitch toggleSwitch = null;
		// Cycle twice - once without focus, once with focus			
		for (int i = 0; i < vm.Editors.Length * 2; i++)
		{
			vm.SetNextEditor();

			await WindowHelper.WaitForIdle();
			await WindowHelper.WaitFor(() => (textBox = page.FindFirstChild<TextBox>()) is not null);
			await WindowHelper.WaitFor(() => (checkBox = page.FindFirstChild<CheckBox>()) is not null);
			await WindowHelper.WaitFor(() => (toggleSwitch = page.FindFirstChild<ToggleSwitch>()) is not null);
			if (i > vm.Editors.Length - 1)
			{
				textBox.Focus(FocusState.Programmatic);
				await WindowHelper.WaitFor(() => FocusManager.GetFocusedElement(WindowHelper.XamlRoot) == textBox);
			}
			await Task.Delay(500);

			AssertEditorContents();
		}
	}
}
