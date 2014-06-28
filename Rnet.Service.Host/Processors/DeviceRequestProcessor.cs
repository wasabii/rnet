using System;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;

using Rnet.Drivers;
using Rnet.Service.Host.Models;

namespace Rnet.Service.Host.Processors
{

    [RequestProcessor(typeof(RnetDevice), -75)]
    public class DeviceRequestProcessor : 
        ObjectRequestProcessor<RnetDevice>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="target"></param>
        [ImportingConstructor]
        protected DeviceRequestProcessor(
            RootProcessor module,
            ProfileManager profileManager)
            : base(module, profileManager)
        {
            Contract.Requires<ArgumentNullException>(module != null);
            Contract.Requires<ArgumentNullException>(profileManager != null);
        }

        public override async Task<object> Resolve(IContext context, RnetDevice target, string[] path)
        {
            // referring to a data path
            if (path[0] == Util.DATA_URI_SEGMENT)
                return await ResolveData(context, target, path);

            return await base.Resolve(context, target, path);
        }

        /// <summary>
        /// Resolves a data handle for from the device.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="path"></param>
        /// <param name="dataPath"></param>
        /// <returns></returns>
        Task<object> ResolveData(IContext context, RnetDevice device, string[] path)
        {
            Contract.Requires<ArgumentNullException>(device != null);
            Contract.Requires<ArgumentNullException>(path != null);
            Contract.Requires<ArgumentOutOfRangeException>(path.Length >= 1);

            // list of all available data items
            if (path.Length == 1)
                return Task.FromResult<object>(ToDataCollection(context, device));
            else
                return Task.FromResult<object>(ResolveDataItem(device, path[1]));
        }

        DataHandleCollection ToDataCollection(IContext context, RnetDevice device)
        {
            Contract.Requires<ArgumentNullException>(device != null);
            Contract.Requires<ArgumentNullException>(device.Data != null);

            return new DataHandleCollection(device.Data.Select(i => new DataHandleData()
            {
                Uri = device.GetUri(context).UriCombine(Util.DATA_URI_SEGMENT).UriCombine(i.Path.ToString()),
                Path = i.Path.ToString(),
            }));
        }

        DataHandleData ResolveDataItem(RnetDevice device, string path)
        {
            Contract.Requires<ArgumentNullException>(device != null);
            Contract.Requires<ArgumentNullException>(path != null);

            return null;
        }

    }

}
