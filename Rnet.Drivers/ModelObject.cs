using System.ComponentModel;

namespace Rnet.Drivers
{

    /// <summary>
    /// Provides a simple base class for model objects.
    /// </summary>
    public abstract class ModelObject : INotifyPropertyChanged
    {

        /// <summary>
        /// Raised when the value of a property is changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="propertyName"></param>
        protected void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

    }

}
