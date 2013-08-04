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

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

    }

}
