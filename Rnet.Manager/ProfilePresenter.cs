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
            "Profile", typeof(ProfileHandle), typeof(ProfilePresenter), new PropertyMetadata(Profile_Changed));

        static void Profile_Changed(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            var p = d as ProfilePresenter;
            if (p == null)
                return;
        }

        public ProfileHandle Profile
        {
            get { return (ProfileHandle)GetValue(ProfileProperty); }
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
