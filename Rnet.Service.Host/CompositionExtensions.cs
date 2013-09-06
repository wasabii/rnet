using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics.Contracts;

namespace Rnet.Service
{

    public static class CompositionExtensions
    {

        public static void ComposeExportedValue(this CompositionContainer container, string contractName, object exportedValue)
        {
            Contract.Requires<ArgumentNullException>(container != null);
            Contract.Requires<ArgumentNullException>(exportedValue != null);

            var batch = new CompositionBatch();
            var metadata = new Dictionary<string, object> 
            {
                { "ExportTypeIdentity", AttributedModelServices.GetTypeIdentity(exportedValue.GetType()) }
            };
            batch.AddExport(new Export(contractName, metadata, () => exportedValue));
            container.Compose(batch);
        }

        public static void ComposeExportedValue(this CompositionContainer container, Type contractType, object exportedValue)
        {
            Contract.Requires<ArgumentNullException>(container != null);
            Contract.Requires<ArgumentNullException>(exportedValue != null);
            ComposeExportedValue(container, AttributedModelServices.GetContractName(contractType), exportedValue);
        }

        public static T GetExportedValue<T>(this CompositionContainer container, Type contractType)
        {
            Contract.Requires<ArgumentNullException>(container != null);
            return container.GetExportedValue<T>(AttributedModelServices.GetContractName(contractType));
        }

        public static T GetExportedValueOrDefault<T>(this CompositionContainer container, Type contractType)
        {
            Contract.Requires<ArgumentNullException>(container != null);
            return container.GetExportedValueOrDefault<T>(AttributedModelServices.GetContractName(contractType));
        }

    }

}
