using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rnet.Profiles.Media.Audio;
using Rnet.Profiles.Metadata;

namespace Rnet.Tests.Profiles
{

    [TestClass]
    public class ProfileDescriptorTests
    {

        [TestMethod]
        public void LoadTypeTest()
        {
            var p = new ProfileDescriptor();
            p.Load(typeof(IEqualization));
        }

    }

}
