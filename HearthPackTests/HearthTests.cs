using System;
using System.Threading.Tasks;
using Alexa.NET.Response;
using HearthPackTracker20;
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

        [TestMethod]
        public void TestGetPackOpenedResponse()
        {
            var function = new Function();
            Task<SkillResponse> t = function.GetPackOpenedResponse(Properties.Resources.TestAcct, 10, Properties.Resources.TestPackType);
            t.Wait();
            Assert.IsNull(t.Exception);
        }

        [TestMethod]
        public void TestLegendaryOpenedResponse()
        {
            var function = new Function();
            Task<SkillResponse> t = function.GetPackOpenedResponse(Properties.Resources.TestAcct, 10, Properties.Resources.TestPackType, 1);
            t.Wait();
            Assert.IsNull(t.Exception);
        }

        [TestMethod]
        public void TestGetCurrentCountResponse()
        {
            var function = new Function();
            Task<SkillResponse> t = function.GetCurrentCountResponse(Properties.Resources.TestAcct);
            t.Wait();
            Assert.IsNull(t.Exception);
        }

        [TestMethod]
        public void TestGetHelpResponse()
        {
            var function = new Function();
            SkillResponse t = function.GetHelpResponse();
            Assert.IsNotNull(t);
        }

        [TestMethod]
        public void TestGetPackTypesReponse()
        {
            var function = new Function();
            SkillResponse t = function.GetPackTypesReponse();
            Assert.IsNotNull(t);
        }
    }
}
