using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Nancy.Bootstrapper.Mef.Tests
{

    [TestClass]
    public class NancyReflectionContextTests
    {

        static readonly NancyReflectionContext context = new NancyReflectionContext();
        static readonly Assembly assembly = context.MapAssembly(typeof(NancyEngine).Assembly);

        [TestMethod]
        public void AddGeneratedPartAttribute()
        {
            var t = assembly.GetType("Nancy.NancyEngine");
            var a = t.GetCustomAttributes<NancyGeneratedPartAttribute>().ToArray();
            Assert.IsTrue(a.Count() == 1);
        }

        [TestMethod]
        public void AddExportAttributes()
        {
            var t = assembly.GetType("Nancy.NancyEngine");
            var a = t.GetCustomAttributes<NancyExportAttribute>().ToArray();
            Assert.IsTrue(a.Any(i => i.ContractType == typeof(NancyEngine)));
            Assert.IsTrue(a.Any(i => i.ContractType == typeof(INancyEngine)));
            Assert.IsTrue(a.Count() == 2);
        }

        [TestMethod]
        public void AddConstructorAttribute()
        {
            var t = assembly.GetType("Nancy.NancyEngine");
            var c = t.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            var a = c[0].GetCustomAttribute<ImportingConstructorAttribute>();
            Assert.IsTrue(a != null);
        }

    }

}
