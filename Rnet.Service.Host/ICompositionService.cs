using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;

namespace Rnet.Service.Host
{

    /// <summary>
    /// Provides methods to interact with the composition service.
    /// </summary>
    public interface ICompositionService : System.ComponentModel.Composition.ICompositionService
    {

        /// <summary>
        /// Gets the <see cref="CompositionContainer"/>.
        /// </summary>
        CompositionContainer Container { get; }

    }

}
