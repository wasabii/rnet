using System.ComponentModel.Composition.Hosting;
using Owin;

namespace Rnet.Service.Host
{

    public static class OwinExtensions
    {

        /// <summary>
        /// Inserts the Rnet service host processor into the OWIN pipeline. Ensure that the <see cref="RnetBus"/> 
        /// instance is available within the <see cref="CompositionContainer"/> along with all of the supporting
        /// discoverable components.
        /// </summary>
        /// <param name="appBuilder"></param>
        /// <param name="container"></param>
        public static void UseRnet(this IAppBuilder appBuilder, CompositionContainer container)
        {
            appBuilder.Use(async (context, func) =>
            {
                await container.GetExportedValue<RootProcessor>().Invoke(new OwinContext(context));
                await func();
            });
        }

    }

}
