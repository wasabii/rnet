﻿using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Nancy.Bootstrapper.Mef
{

    public abstract class NancyCatalog : FilteredCatalog
    {

        /// <summary>
        /// Filters out non-generated parts.
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        static bool Filter(ComposablePartDefinition d)
        {
            var t = System.ComponentModel.Composition.ReflectionModel.ReflectionModelServices.GetPartType(d).Value.Name;

            return d.ExportDefinitions.Any(j =>
            {
                var mv = AttributedModelServices.GetMetadataView<INancyGeneratedPartMetadataView>(j.Metadata);
                if (mv != null &&
                    mv.AutoGenerated != null &&
                    mv.AutoGenerated.Any(i => i))
                    return true;

                return false;
            });
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        protected NancyCatalog(ComposablePartCatalog source)
            : base(source, Filter)
        {
            Contract.Requires<NullReferenceException>(source != null);
        }

    }

}
