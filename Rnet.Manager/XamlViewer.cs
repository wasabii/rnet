using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Resources;

namespace Rnet.Manager
{

    /// <summary>
    ///
    /// </summary>
    public class XamlViewer : ContentControl, IUriContext
    {

        /// <summary>
        /// Initializes the static instance.
        /// </summary>
        static XamlViewer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(XamlViewer), new FrameworkPropertyMetadata(typeof(XamlViewer)));
        }

        /// <summary>
        /// Dependency property for the Source property.
        /// </summary>
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source",
            typeof(Uri), typeof(XamlViewer), new FrameworkPropertyMetadata(Source_Changed));

        /// <summary>
        /// Invoked when the value of Source is changed.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="args"></param>
        static void Source_Changed(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            ((XamlViewer)d).OnSourceChanged();
        }

        /// <summary>
        /// URI from which to retrieve XAML.
        /// </summary>
        public Uri Source
        {
            get { return (Uri)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        void OnSourceChanged()
        {
            if (Source == null)
                Content = null;

            if (BaseUri == null)
                return;

            var uri = !Source.IsAbsoluteUri ? new Uri(new Uri(BaseUri, "."), Source) : Source;
            if (uri == null)
                return;

            if (SetXaml(Application.GetContentStream(uri)))
                return;
            if (SetXaml(Application.GetResourceStream(uri)))
                return;
        }

        bool SetXaml(StreamResourceInfo r)
        {
            if (r == null)
                return false;

            if (r.Stream == null)
                return false;

            var o = XamlReader.Load(r.Stream);
            if (o == null)
                return false;

            Content = o;
            return true;
        }

        void SetXaml(Stream stream)
        {

        }

        public Uri BaseUri { get; set; }

    }

}
