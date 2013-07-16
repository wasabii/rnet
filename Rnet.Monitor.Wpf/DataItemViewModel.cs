using System;
using System.Text;
using ReactiveUI;

namespace Rnet.Monitor.Wpf
{

    public class DataItemViewModel : ReactiveObject
    {

        public DataItemViewModel(RnetDataItem dataItem)
        {
            DataItem = dataItem;
        }

        public RnetDataItem DataItem { get; private set; }

        public string Text
        {
            get { return GetText(); }
        }

        string GetText()
        {
            return DataItem != null && DataItem.Buffer != null ? Encoding.ASCII.GetString(DataItem.Buffer) : null;
        }

    }

}
