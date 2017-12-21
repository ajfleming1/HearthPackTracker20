using System;
using System.Threading.Tasks;
using HearthPackTracker20.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models;

namespace HearthPackTests
{
    [TestClass]
    public class HearthTests
    {
        [TestMethod]
        public void TestGetPacks()
        {
            var packDBHelper = new PackDBHelper();
            Task packs = packDBHelper.GetPacks(Properties.Resources.TestAcct);
            packs.Wait();
            Assert.IsNull(packs.Exception);
        }

        [TestMethod]
        public void TestSavePack()
        {
            var packDBHelper = new PackDBHelper();
            Task<Packs> packs = packDBHelper.GetPacks(Properties.Resources.TestAcct);
            packs.Wait();
            var userPacks = packs.Result;
            userPacks.Pack = new PackMap()
            {
                ClassicCount = 1,
                FrozenThroneCount = 1,
                GadgetzanCount = 1,
                GVGCount = 1,
                KoboldsCount = 1,
                OldGodsCount = 1,
                TGTCount = 1,
                UnGoroCount = 1
            };

            Task saved = packDBHelper.SavePack(userPacks);
            saved.Wait();
            Assert.IsNull(saved.Exception);
        }
    }
}
