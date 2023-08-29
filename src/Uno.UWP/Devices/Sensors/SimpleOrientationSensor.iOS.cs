#nullable enable
using CoreMotion;
using Foundation;
using Uno.Extensions;
using Uno.Foundation.Logging;

namespace Windows.Devices.Sensors
{
	public partial class SimpleOrientationSensor
	{
		private SimpleOrientation _previousOrientation;
		private static CMMotionManager? _motionManager;
		private const double _updateInterval = 0.5;
		private const double _threshold = 0.5;

		partial void Initialize()
		{
			_motionManager = new CMMotionManager();
			if (_motionManager.DeviceMotionAvailable) // DeviceMotion is not available on all devices. iOS4+
			{
				var operationQueue = (NSOperationQueue.CurrentQueue == null || NSOperationQueue.CurrentQueue == NSOperationQueue.MainQueue) ? new NSOperationQueue() : NSOperationQueue.CurrentQueue;
				this.Log().Info("DeviceMotion is available");
				_motionManager.DeviceMotionUpdateInterval = _updateInterval;

				_motionManager.StartDeviceMotionUpdates(operationQueue, (motion, error) =>
				{
					// Motion and Error can be null: https://developer.apple.com/documentation/coremotion/cmdevicemotionhandler
					if (error is not null)
					{
						this.Log().Error($"SimpleOrientationSensor returned error when reading Device Motion updates. {error.Description}");
						return;
					}

					if (motion is null)
					{
						this.Log().Error($"SimpleOrientationSensor failed read Device Motion updates.");
						return;
					}

					OnMotionChanged(motion);
				});
			}
			else // For iOS devices that don't support CoreMotion
			{
				this.Log().Error("SimpleOrientationSensor failed to initialize because CoreMotion is not available");
			}
		}

		private void OnMotionChanged(CMDeviceMotion motion)
		{
			var orientation = ToSimpleOrientation(motion.Gravity.X, motion.Gravity.Y, motion.Gravity.Z, _threshold, _previousOrientation);
			_previousOrientation = orientation;
			SetCurrentOrientation(orientation);
		}
	}
}
