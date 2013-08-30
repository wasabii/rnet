using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Markup;

namespace Rnet.Manager
{

    [ContentProperty("Parameters")]
    public class PathConstructor : MarkupExtension
    {

        public string Path { get; set; }

        public IList Parameters { get; set; }

        public PathConstructor()
        {
            Parameters = new List<object>();
        }

        public PathConstructor(string path, object p0)
        {
            Path = path;
            Parameters = new[] { p0 };
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return new PropertyPath(Path, Parameters.Cast<object>().ToArray());
        }
    }


}
