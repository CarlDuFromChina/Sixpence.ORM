using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Sixpence.Common;
using Sixpence.Common.Current;
using Sixpence.Common.Utils;
using Sixpence.ORM.Broker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sixpence.ORM.Test
{
    [TestFixture]
    internal class PersistBrokerTest
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
        }

        [Test]
        [Order(1)]
        public void Check_Entity_AutoGenerate()
        {
            SixpenceORMSetup.UseEntityGenerate(null);
            var broker = PersistBrokerFactory.GetPersistBroker();
            var result = broker.ExecuteScalar(broker.DbClient.Driver.TableExsit("test"));
            Assert.IsTrue(ConvertUtil.ConToBoolean(result));
        }

        [Test]
        [Order(2)]
        public void Check_Entity_Create()
        {
            var broker = PersistBrokerFactory.GetPersistBroker();
            var entity = new Test() { code = "A001", name = "Test", id = "123" };
            var result = broker.Create(entity);
            Assert.IsNotEmpty(result);
        }

        [Test]
        [Order(3)]
        public void Check_Entity_Query()
        {
            var broker = PersistBrokerFactory.GetPersistBroker();
            var result = broker.Retrieve<Test>("123");
            Assert.IsNotNull(result);
        }

        [Test]
        [Order(4)]
        public void Check_Entity_Update()
        {
            var broker = PersistBrokerFactory.GetPersistBroker();
            var data = broker.Retrieve<Test>("123");
            data.name = "test";
            broker.Update(data);
            data = broker.Retrieve<Test>("123");
            Assert.IsTrue(data.name.Equals("test"));
        }

        [Test]
        [Order(5)]
        public void Check_Entity_Delete()
        {
            var broker = PersistBrokerFactory.GetPersistBroker();
            broker.Delete("test", "123");
            var data = broker.Retrieve<Test>("123");
            Assert.IsNull(data);

            Check_Entity_Create();
            data = broker.Retrieve<Test>("123");
            broker.Delete(data);
            data = broker.Retrieve<Test>("123");
            Assert.IsNull(data);


            Check_Entity_Create();
            var dataList = broker.RetrieveMultiple<Test>("select * from test where id = '123'").ToArray();
            broker.Delete(dataList);
            dataList = broker.RetrieveMultiple<Test>("select * from test where id = '123'").ToArray();
            Assert.IsTrue(dataList.Length == 0);
        }

        [Test]
        [Order(6)]
        public void Check_BulkCreate()
        {
            var dataList = new List<Test>()
            {
                new Test() { id = Guid.NewGuid().ToString(), code = "B001", name = "测试1", created_at = DateTime.Now, created_by = "user", created_by_name = "user", updated_at = DateTime.Now, updated_by = "user", updated_by_name = "user" },
                new Test() { id = Guid.NewGuid().ToString(), code = "B002", name = "测试2" , created_at = DateTime.Now, created_by = "user", created_by_name = "user", updated_at = DateTime.Now, updated_by = "user", updated_by_name = "user" },
                new Test() { id = Guid.NewGuid().ToString(), code = "B003", name = "测试3", created_at = DateTime.Now, created_by = "user", created_by_name = "user", updated_at = DateTime.Now, updated_by = "user", updated_by_name = "user" },
            };
            var broker = PersistBrokerFactory.GetPersistBroker();
            broker.ExecuteTransaction(() =>
            {
                broker.BulkCreate(dataList);
            });
            var count = broker.QueryCount("select COUNT(1) from test where code in ('B001', 'B002', 'B003')");
            Assert.AreEqual(3, count);
        }

        [Test]
        [Order(7)]
        public void Check_BulkUpdate()
        {
            var broker = PersistBrokerFactory.GetPersistBroker();
            var dataList = broker.RetrieveMultiple<Test>("select * from test where code in ('B001', 'B002', 'B003')").ToList();
            dataList[0].name = "test1";
            dataList[1].name = "test2";
            dataList[2].name = "test3";
            broker.ExecuteTransaction(() =>
            {
                broker.BulkUpdate(dataList);
            });
            dataList = broker.RetrieveMultiple<Test>("select * from test where code in ('B001', 'B002', 'B003')").ToList();
            Assert.AreEqual(dataList[0].name, "test1");
            Assert.AreEqual(dataList[1].name, "test2");
            Assert.AreEqual(dataList[2].name, "test3");
            broker.Execute("TRUNCATE test");
        }
    }
}
