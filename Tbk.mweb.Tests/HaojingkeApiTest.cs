using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PddOpenSdk.Services;

namespace Tbk.mweb.Tests
{
    /// <summary>
    /// UnitTest3 的摘要说明
    /// </summary>
    [TestClass]
    public class HaojingkeApiTest
    {
        private HaojingkeApi api;

        public HaojingkeApiTest()
        {
            api = new HaojingkeApi("f7e704a8ae9fc8bc", "1534105049", "http://api-gw.haojingke.com/index.php/api/index/myapi");
        }

        private TestContext testContextInstance;

        /// <summary>
        ///获取或设置测试上下文，该上下文提供
        ///有关当前测试运行及其功能的信息。
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }


        [TestMethod]
        public void GetJDGoodsDetailTest()
        {
            var model = api.GetJDGoodsDetail("32635634502");
            Assert.IsNotNull(model);
        }

        [TestMethod]
        public void GetJDUnionUrlTest()
        {
            //var hjkGoodsDetail = api.GetJDGoodsDetail("32635634502");
            var model = api.GetUnionUrl("25725309068", "http://item.jd.com/25725309068.html");
            Assert.IsNotNull(model);
        }
    }
}
