using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Sixpence.Common;
using Sixpence.Common.Utils;
using Sixpence.ORM.Broker;
using Sixpence.ORM.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sixpence.ORM.Test
{
    internal class SixpenceORMSetupTest
    {
        [SetUp]
        public void SetUp()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddServiceContainer(options =>
            {
                options.Assembly.Add("Sixpence.ORM.Test");
            });
        }

        [Test]
        public void Check_Entity_AutoGenerate()
        {
            SixpenceORMSetup.UseEntityGenerate(null);
            var broker = PersistBrokerFactory.GetPersistBroker();
            var result = broker.ExecuteScalar(broker.DbClient.Driver.TableExsit("test"));
            Assert.IsTrue(ConvertUtil.ConToBoolean(result));
        }
    }
}
