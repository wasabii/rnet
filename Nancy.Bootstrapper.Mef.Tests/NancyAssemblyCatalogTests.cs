using System.ComponentModel.Composition.Hosting;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Nancy.ViewEngines;

namespace Nancy.Bootstrapper.Mef.Tests
{

    [TestClass]
    public class NancyAssemblyCatalogTests : NancyCatalogTests
    {

        protected override NancyCatalog CreateCatalog()
        {
            return new NancyAssemblyCatalog(typeof(Nancy.NancyEngine).Assembly);
        }

        [TestMethod]
        public void ViewLocationProviderExportTest()
        {
            var c = new CompositionContainer(CreateCatalog());
            var e = c.GetExports<IViewLocationProvider>().ToArray();
            Assert.IsTrue(e != null);
            Assert.IsTrue(e.Count() == 1);
        }

        [TestMethod]
        public void ViewLocatorExportTest()
        {
            var c = new CompositionContainer(CreateCatalog());
            var e = c.GetExports<IViewLocator>();
            Assert.IsTrue(e != null);
            Assert.IsTrue(e.Count() == 1);
        }

        [TestMethod]
        public void EngineExportTest()
        {
            var c = new CompositionContainer(CreateCatalog());
            var e = c.GetExports<INancyEngine>();
            Assert.IsTrue(e != null);
            Assert.IsTrue(e.Count() == 1);
        }

    }

}
