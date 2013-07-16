using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Windows;

namespace Rnet.Monitor.Wpf
{

    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
        }

        void LayoutRoot_Loaded(object sender, RoutedEventArgs args)
        {
            LayoutRoot.DataContext = new BusViewModel();
        }

        void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ((BusViewModel)LayoutRoot.DataContext).SelectedDataItem = e.NewValue != null ? ((RnetDeviceDirectory)e.NewValue).Data : null;
        }

    }

}
