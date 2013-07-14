using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rnet
{

    public class RnetDeviceCollection : IEnumerable<RnetDevice>
    {

        AsyncCollection<RnetDevice> items = new AsyncCollection<RnetDevice>();

        internal void Add(RnetDevice device)
        {
            items.Add(device);
        }

        internal void Remove(RnetDevice device)
        {
            items.Remove(device);
        }

        public Task<RnetDevice> GetAsync(RnetDeviceId id)
        {
            return items.GetAsync(i => i.Id == id);
        }

        public IEnumerator<RnetDevice> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }

}
