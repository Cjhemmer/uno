﻿using System;
using System.Runtime.InteropServices;
using Uno.Extensions;
using Uno.UI;
using Uno.UI.DataBinding;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace Windows.UI.Xaml.Controls;

/// <summary>
/// Declares a Content presenter
/// </summary>
/// <remarks>
/// The content presenter is used for compatibility with WPF concepts,
/// but the ContentSource property is not available, because there are ControlTemplates for now.
/// </remarks>
public partial class ContentPresenter : FrameworkElement
{
	private Rect? _lastArrangeRect;
	private Rect _lastGlobalRect;
	private bool _nativeHostRegistered;

	partial void InitializePlatform()
	{
		Loaded += (s, e) => RegisterNativeHostSupport();
		Unloaded += (s, e) => UnregisterNativeHostSupport();
	}

	partial void TryRegisterNativeElement(object newValue)
	{
		if (CoreWindow.Main.IsNativeElement(newValue))
		{
			IsNativeHost = true;

			if (ContentTemplate is not null)
			{
				throw new InvalidOperationException("ContentTemplate cannot be set when the Content is a native element");
			}
			if (ContentTemplateSelector is not null)
			{
				throw new InvalidOperationException("ContentTemplateSelector cannot be set when the Content is a native element");
			}

			RegisterNativeHostSupport();
		}
		else if (IsNativeHost)
		{
			IsNativeHost = false;
			UnregisterNativeHostSupport();
		}
	}

	private void RegisterNativeHostSupport()
	{
		if (IsNativeHost && XamlRoot is not null)
		{
			XamlRoot.InvalidateRender += UpdateNativeElementPosition;
			_nativeHostRegistered = true;
		}
	}

	private void UnregisterNativeHostSupport()
	{
		if (_nativeHostRegistered)
		{
			_nativeHostRegistered = false;
			XamlRoot.InvalidateRender -= UpdateNativeElementPosition;
		}
	}

	partial void ArrangeNativeElement(Rect arrangeRect)
	{
		if (IsNativeHost)
		{
			_lastArrangeRect = arrangeRect;

			UpdateNativeElementPosition();
		}
	}

	partial void TryAttachNativeElement()
	{
		if (IsNativeHost)
		{
			CoreWindow.Main.AttachNativeElement(XamlRoot, Content);
		}
	}

	partial void TryDetachNativeElement()
	{
		if (IsNativeHost)
		{
			CoreWindow.Main.DetachNativeElement(XamlRoot, Content);
		}
	}

	private Size MeasureNativeElement(Size size)
	{
		if (IsNativeHost)
		{
			return CoreWindow.Main.MeasureNativeElement(XamlRoot, Content, size);
		}
		else
		{
			return size;
		}
	}

	private void UpdateNativeElementPosition()
	{
		if (_lastArrangeRect is { } lastArrangeRect)
		{
			var globalPosition = TransformToVisual(null).TransformPoint(lastArrangeRect.Location);
			var globalRect = new Rect(globalPosition, lastArrangeRect.Size);

			if (_lastGlobalRect != globalRect)
			{
				_lastGlobalRect = globalRect;

				CoreWindow.Main.ArrangeNativeElement(XamlRoot, Content, globalRect);
			}
		}
	}
}
