using System;
using System.ComponentModel;

namespace Rnet.Manager
{

    public static class Util
    {

        /// <summary>
        /// Subscribes to the PropertyChanged event if possible.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="h"></param>
        public static void AddPropertyChanged(this object self, PropertyChangedEventHandler h)
        {
            var i = self as INotifyPropertyChanged;
            if (i != null)
                i.PropertyChanged += h;
        }

        /// <summary>
        /// Subscribes to the PropertyChanged event if possible.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="h"></param>
        public static void RemovePropertyChanged(this object self, PropertyChangedEventHandler h)
        {
            var i = self as INotifyPropertyChanged;
            if (i != null)
                i.PropertyChanged -= h;
        }

    }

}
