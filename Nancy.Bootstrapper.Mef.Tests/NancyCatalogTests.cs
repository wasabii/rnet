using System.ComponentModel.Composition.Hosting;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Nancy.Bootstrapper.Mef.Tests
{

    [TestClass]
    public abstract class NancyCatalogTests
    {

        protected abstract NancyCatalog CreateCatalog();

        [TestMethod]
        public void PartQueryTest()
        {
            var c = CreateCatalog();
            var p = c.Parts.Where(i => i.ExportDefinitions.Any(j => j.ContractName == "Nancy.INancyEngine"));
            Assert.IsTrue(p.Any());
        }

        [TestMethod]
        public void ExportTest()
        {
            var c = new CompositionContainer(CreateCatalog());
            var e = c.GetExports<INancyEngine>();
            Assert.IsTrue(e.Any());
        }

    }

}
