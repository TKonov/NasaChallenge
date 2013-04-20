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
    public partial class PassesList : PhoneApplicationPage
    {
        public StarViewModel Model { get; set; }

        public PassesList()
        {
            InitializeComponent();
            this.Loaded += PassesList_Loaded;
        }

        void PassesList_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = ((App)App.Current).Model as StarViewModel;
           
        }
    }
}