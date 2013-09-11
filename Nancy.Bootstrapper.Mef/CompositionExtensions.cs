using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics.Contracts;

namespace Nancy.Bootstrapper.Mef
{

    static class CompositionExtensions
    {

        /// <summary>
        ///  Creates a part from the specified object under the specified contract name and composes it in the specified
        ///  composition container.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="contractName"></param>
        /// <param name="exportedValue"></param>
        public static void ComposeExportedValue(this CompositionContainer container, string contractName, object exportedValue)
        {
            Contract.Requires<ArgumentNullException>(container != null);
            Contract.Requires<ArgumentNullException>(contractName != null);
            Contract.Requires<ArgumentNullException>(exportedValue != null);

            var b = new CompositionBatch();
            var m = new Dictionary<string, object>();
            b.AddExport(new Export(contractName, m, () => exportedValue));
            container.Compose(b);
        }

        public static void ComposeExportedValue(this CompositionContainer container, Type contractType, object exportedValue)
        {
            Contract.Requires<ArgumentNullException>(container != null);
            Contract.Requires<ArgumentNullException>(contractType != null);
            Contract.Requires<ArgumentNullException>(exportedValue != null);

            var contractName = AttributedModelServices.GetTypeIdentity(contractType);
            var b = new CompositionBatch();
            var m = new Dictionary<string, object>();
            m[CompositionConstants.ExportTypeIdentityMetadataName] = contractName;
            b.AddExport(new Export(contractName, m, () => exportedValue));
            container.Compose(b);
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
