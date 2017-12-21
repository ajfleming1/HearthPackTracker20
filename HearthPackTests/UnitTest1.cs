using System;
using HearthPackTracker20.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HearthPackTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestGetPacks()
        {
            var packDBHelper = new PackDBHelper();
            var packs = packDBHelper.GetPacks("amzn1.ask.account.AGPW4MGUNNZZVPEAFHSYABP22PNRCXS7K4OODMILQD5F4FOF3I5ZTSOUEFTSYBGLYU5YKEW3QWUS4DXCT6DZLT5FQCY73AKWUWHSS5UXGGFP5SZPHRGWITXJQJBIVWBRSUXE74HOI4JEGRNMTTLAL2XQ3U2ECY5QI2VE6KHFWKI3TQQGZ7DSIZLLRYFKHAQDB3XGG3ZP3DPYWPI");
            Assert.IsNotNull(packs);
        }
    }
}
