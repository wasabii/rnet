using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rnet.Tests
{

    [TestClass]
    public class UnitTest1
    {

        [TestMethod]
        public void TestMethod1()
        {
            using (var rnet = new RnetTcpConnection("tokyo.larvalstage.net", 9999))
            {

            }
        }

    }

}
