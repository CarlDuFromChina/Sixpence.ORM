using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Sixpence.ORM.Common.Current;
using Sixpence.ORM.Common.Logging;
using Sixpence.ORM.Extensions;
using Sixpence.ORM.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sixpence.ORM.Test
{
    [TestFixture]
    public class TestLogger
    {
        [SetUp]
        public void SetUp()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddServiceContainer(options =>
            {
                options.Assembly.Add("Sixpence.ORM.Test");
            });
            CallContext<CurrentUserModel>.SetData(CallContextType.User, new CurrentUserModel() { Id = "1", Code = "1", Name = "test" });
            SixpenceORMBuilderExtension.UseORM(null);
        }


        [Test]
        [Order(1)]
        public void TestLogUtil()
        {
            LogUtil.Debug("测试日志");
        }

        [Test]
        [Order(2)]
        public void TestLogFactory()
        {
            var logger = LoggerFactory.GetLogger("test");
            logger.Debug("测试日志工厂类");
        }
    }
}
