using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace ISSLocator
{
    public partial class ArrorMarkerControl : UserControl
    {
        public ArrorMarkerControl()
        {
            InitializeComponent();

          //  this.Loaded += ArrorMarkerControl_Loaded;
        }

        void ArrorMarkerControl_Loaded(object sender, RoutedEventArgs e)
        {

          //  Storyboard1.Begin();
            this.AddHandler(FrameworkElement.TapEvent, new EventHandler<System.Windows.Input.GestureEventArgs>(Grid_Tap_1), true);
        }


        private void Grid_Tap_1(object sender, System.Windows.Input.GestureEventArgs e)
        {

        }
    }
}
