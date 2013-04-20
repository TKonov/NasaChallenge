using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Text;

namespace ISSLocator
{
    public class DevicesService
    {
        public GeoCoordinateWatcher Watcher { get; private set; }

        public DevicesService()
        {
            PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;

            Watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.Default)
            {
                MovementThreshold = 20
            };
        }
    }
}
