using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace Rnet.Monitor.Wpf
{

    public class TreeViewBehavior : Behavior<TreeView>
    {

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(object), typeof(TreeViewBehavior), new UIPropertyMetadata(null, OnSelectedItemChanged));

        static void OnSelectedItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var item = args.NewValue as TreeViewItem;
            if (item != null)
                item.SetValue(TreeViewItem.IsSelectedProperty, true);
        }

        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.SelectedItemChanged += OnTreeViewSelectedItemChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (AssociatedObject != null)
                AssociatedObject.SelectedItemChanged -= OnTreeViewSelectedItemChanged;
        }

        void OnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> args)
        {
            SelectedItem = args.NewValue;
        }

    }

}