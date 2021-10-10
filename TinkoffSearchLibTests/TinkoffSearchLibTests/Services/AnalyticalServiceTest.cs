using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using TinkoffSearchLib.Models;
using TinkoffSearchLib.Services;
using Tinkoff.Trading.OpenApi.Models;

namespace TinkoffSearchLibTests
{
    [TestClass]
    public class AnalyticalServiceTest
    {
        [TestMethod]
        public void GetGrowthTest()
        {
            var securityList = new List<Security>()
            {
                new Security()
                {
                    Candles = new List<CandlePayload>
                    {
                        new CandlePayload(10,11,22,8,200, new System.DateTime(2021,1,1,0,1,0),CandleInterval.Day, "testFigi"),
                        new CandlePayload(11,20,33,10,245, new System.DateTime(2021,1,2,0,1,0),CandleInterval.Day, "testFigi")
                    },
                    Name = "testFigi"
                }
            };

            AnalyticalService.GetGrowth(securityList);

            Assert.AreEqual(100, securityList[0].Growth);
        }

        [TestMethod]
        public void GetLinearityTest_PerfectlyLinear()
        {
            var securityList = new List<Security>()
            {
                new Security()
                {
                    Candles = new List<CandlePayload>
                    {
                        new CandlePayload(10,11,22,8,200, new System.DateTime(2021,1,1,0,1,0),CandleInterval.Day, "testFigi"),
                        new CandlePayload(11,20,33,10,245, new System.DateTime(2021,1,2,0,1,0),CandleInterval.Day, "testFigi")
                    },
                    Name = "testFigi"
                }
            };

            AnalyticalService.GetLinearity(securityList);

            Assert.AreEqual(0, securityList[0].Linearity);
        }

        [TestMethod]
        public void GetLinearityTest_10PercentDigression()
        {
            var securityList = new List<Security>()
            {
                new Security()
                {
                    Candles = new List<CandlePayload>
                    {
                        new CandlePayload(10,11,22,8,200, new System.DateTime(2021,1,1,0,1,0),CandleInterval.Day, "testFigi"),
                        new CandlePayload(11,14,22,8,200, new System.DateTime(2021,1,2,0,1,0),CandleInterval.Day, "testFigi"),
                        new CandlePayload(11,20,33,10,245, new System.DateTime(2021,1,3,0,1,0),CandleInterval.Day, "testFigi")
                    },
                    Name = "testFigi"
                }
            };

            AnalyticalService.GetLinearity(securityList);

            Assert.AreEqual(25, securityList[0].Linearity);
        }
    }
}
