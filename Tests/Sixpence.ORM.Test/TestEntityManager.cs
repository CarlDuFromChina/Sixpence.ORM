using Microsoft.Extensions.DependencyInjection;
using Sixpence.ORM;
using Sixpence.ORM.Postgres;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sixpence.ORM.Utils;
using Microsoft.AspNetCore.Builder;

namespace Sixpence.ORM.Test
{
    [TestFixture]
    internal class TestEntityManager
    {
        private IEntityManager manager;

        [SetUp]
        public void SetUp()
        {
            IServiceCollection services = new ServiceCollection();
            // Plugin
            services.AddTransient<IEntityManagerPlugin, UserInfoPlugin>();

            // Entity
            services.AddTransient<IEntity, Test>();

            services.AddSorm(options =>
            {
                options.UsePostgres(DBSourceConfig.ConnectionString, DBSourceConfig.CommandTimeOut);
            });
            var provider = services.BuildServiceProvider();
            var app = new ApplicationBuilder(provider);
            app.UseSorm(options =>
            {
                options.EnableLogging = false;
                options.MigrateDb = true;
            });

            manager = provider.GetRequiredService<IEntityManager>();
        }

        [Test]
        [Order(1)]
        public void Check_Entity_AutoGenerate()
        {
            var result = manager.ExecuteScalar(manager.DbClient.Driver.Dialect.GetTableExsitSql("test"));
            Assert.IsTrue((bool)result);
        }

        [Test]
        [Order(2)]
        public void Check_Entity_Create()
        {
            var entity = new Test() { Code = "A001", Name = "Test", Id = "123", IsSuper = true };
            var result = manager.Create(entity);
            Assert.IsNotEmpty(result);
        }

        [Test]
        [Order(3)]
        public void Check_Entity_Query()
        {
            var result = manager.QueryFirst<Test>("123");
            Assert.IsNotNull(result);
            result = manager.QueryFirst<Test>("select * from test where id = @id", new { id = "123" });
            Assert.IsNotNull(result);
        }

        [Test]
        [Order(4)]
        public void Check_Entity_Update()
        {
            var data = manager.QueryFirst<Test>("123");
            data.Name = "test";
            manager.Update(data);
            data = manager.QueryFirst<Test>("123");
            Assert.IsTrue(data.Name.Equals("test"));
        }

        [Test]
        [Order(5)]
        public void Check_Entity_Delete()
        {
            manager.Delete("test", "123");
            var data = manager.QueryFirst<Test>("123");
            Assert.IsNull(data);

            var entity = new Test() { Code = "A001", Name = "Test", Id = "123" };
            manager.Create(entity);

            data = manager.QueryFirst<Test>("123");
            manager.Delete(data);
            data = manager.QueryFirst<Test>("123");
            Assert.IsNull(data);

            manager.Create(entity);
            manager.Delete("test", entity.Id);
            data = manager.QueryFirst<Test>("123");
            Assert.IsNull(data);

            Check_Entity_Create();
            var dataList = manager.Query<Test>("select * from test where id = @id", new { id = "123" }).ToArray();
            manager.Delete(dataList);
            dataList = manager.Query<Test>("select * from test where id = @id", new { id = "123" }).ToArray();
            Assert.IsTrue(dataList.Length == 0);
        }

        [Test]
        [Order(6)]
        public void Check_BulkCreate()
        {
            var dataList = new List<Test>()
            {
                new Test() { Id= Guid.NewGuid().ToString(), Code = "B001", Name = "测试1", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now, IsSuper = true },
                new Test() { Id = Guid.NewGuid().ToString(), Code = "B002", Name = "测试2" , CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now, IsSuper = false },
                new Test() { Id = Guid.NewGuid().ToString(), Code = "B003", Name = "测试3", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now, IsSuper = false },
            };
            manager.BulkCreate(dataList);
            var count = manager.QueryCount("select COUNT(1) from test where code in ('B001', 'B002', 'B003')");
            Assert.AreEqual(3, count);
        }

        [Test]
        [Order(7)]
        public void Check_BulkUpdate()
        {
            var dataList = manager.Query<Test>("select * from test where code in ('B001', 'B002', 'B003')").ToList();
            dataList[0].Name = "test1";
            dataList[1].Name = "test2";
            dataList[2].Name = "test3";
            manager.BulkUpdate(dataList);
            dataList = manager.Query<Test>("select * from test where code in ('B001', 'B002', 'B003')").ToList();
            Assert.AreEqual(dataList[0].Name, "test1");
            Assert.AreEqual(dataList[1].Name, "test2");
            Assert.AreEqual(dataList[2].Name, "test3");
            manager.Execute("TRUNCATE test");
        }

        [Test]
        [Order(8)]
        public void Check_BulkDelete()
        {
            var dataList = new List<Test>()
            {
                new Test() { Id = Guid.NewGuid().ToString(), Code = "B001", Name = "测试1", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new Test() { Id = Guid.NewGuid().ToString(), Code = "B002", Name = "测试2" , CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new Test() { Id = Guid.NewGuid().ToString(), Code = "B003", Name = "测试3", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
            };
            manager.ExecuteTransaction(() =>
            {
                manager.BulkCreate(dataList);
                manager.BulkDelete(dataList);
                dataList = manager.Query<Test>("select * from test where code = ANY(@ids)", new { ids = new string[] { "B001", "B002", "B003" } }).ToList();
                Assert.IsTrue(dataList == null || dataList.Count == 0);
            });
        }
    }
}
