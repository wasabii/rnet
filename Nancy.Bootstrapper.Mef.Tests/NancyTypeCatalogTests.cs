using System.ComponentModel.Composition.Hosting;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nancy.ViewEngines;

namespace Nancy.Bootstrapper.Mef.Tests
{

    [TestClass]
    public class NancyTypeCatalogTests : NancyCatalogTests
    {

        protected override NancyCatalog CreateCatalog()
        {
            return new NancyTypeCatalog(typeof(Nancy.NancyEngine).Assembly.GetTypes());
        }

        //[TestMethod]
        //public void ViewLocationProviderTest()
        //{
        //    var a = new NancyTypeCatalog(typeof(IViewLocationProvider), typeof(ResourceViewLocationProvider));
        //    var c = new CompositionContainer(a);
        //    var i = c.GetExports<IViewLocationProvider>();
        //    Assert.IsTrue(i.Count() == 1);
        //}

    }

}
