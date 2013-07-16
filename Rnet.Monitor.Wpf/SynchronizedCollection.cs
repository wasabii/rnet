using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;

namespace Rnet.Monitor.Wpf
{

    public class SynchronizedCollection<T> : IEnumerable<T>, INotifyPropertyChanged, INotifyCollectionChanged
    {

        SynchronizationContext sync;
        IEnumerable<T> source;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public SynchronizedCollection()
        {
            sync = SynchronizationContext.Current;
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public SynchronizedCollection(IEnumerable<T> source)
            : this()
        {
            Source = source;
        }

        public IEnumerable<T> Source
        {
            get { return source; }
            set { SetSource(value); }
        }

        void SetSource(IEnumerable<T> newSource)
        {
            var oldNotify = source as INotifyCollectionChanged;
            if (oldNotify != null)
                oldNotify.CollectionChanged -= source_CollectionChanged;

            this.source = newSource;

            var newNotify = source as INotifyCollectionChanged;
            if (newNotify != null)
                newNotify.CollectionChanged += source_CollectionChanged;
        }

        void source_CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            sync.Post(i => RaiseCollectionChanged(args), null);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        void RaiseCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            if (CollectionChanged != null)
                CollectionChanged(this, args);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Source != null ? Source.GetEnumerator() : Enumerable.Empty<T>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }

}
