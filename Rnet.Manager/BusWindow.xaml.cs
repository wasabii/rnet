using System.Windows;

namespace Rnet.Manager
{

    public partial class BusWindow : Window
    {

        public BusWindow()
        {
            InitializeComponent();
        }

        void LayoutRoot_Loaded(object sender, RoutedEventArgs args)
        {
            LayoutRoot.DataContext = new BusViewModel();
        }

    }

}
