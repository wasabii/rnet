using System;
using System.Windows;
using System.Windows.Controls;
using Rnet.Drivers;

namespace Rnet.Manager
{

    public class ProfilePresenter : Control
    {

        /// <summary>
        /// Initializes the static instance.
        /// </summary>
        static ProfilePresenter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ProfilePresenter), new FrameworkPropertyMetadata(typeof(ProfilePresenter)));
        }

        public static readonly DependencyProperty ProfileProperty = DependencyProperty.Register(
            "Profile", typeof(Profile), typeof(ProfilePresenter), new PropertyMetadata(Profile_Changed));

        static void Profile_Changed(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            var p = d as ProfilePresenter;
            if (p == null)
                return;
        }

        public Profile Profile
        {
            get { return (Profile)GetValue(ProfileProperty); }
            set { SetValue(ProfileProperty, value); }
        }

        public static readonly DependencyProperty XamlSourceProperty = DependencyProperty.Register(
            "XamlSource", typeof(Uri), typeof(ProfilePresenter));

        public Uri XamlSource
        {
            get { return (Uri)GetValue(XamlSourceProperty); }
            set { SetValue(XamlSourceProperty, value); }
        }

    }

}
