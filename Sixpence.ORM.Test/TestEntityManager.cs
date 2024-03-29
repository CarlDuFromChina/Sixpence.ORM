﻿using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Sixpence.Common;
using Sixpence.ORM.Common.Current;
using Sixpence.Common.Utils;
using Sixpence.ORM.EntityManager;
using Sixpence.ORM.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sixpence.ORM.Test
{
    [TestFixture]
    internal class TestEntityManager
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
            SixpenceORMBuilderExtension.UseORM(null);
            using(IEntityManager manager = EntityManagerFactory.GetManager())
            {
                var result = manager.ExecuteScalar(manager.DbClient.Driver.TableExsit("test"));
                Assert.IsTrue(ConvertUtil.ConToBoolean(result));
            }
        }

        [Test]
        [Order(2)]
        public void Check_Entity_Create()
        {
            using(var manager = EntityManagerFactory.GetManager())
            {
                var entity = new Test() { code = "A001", name = "Test", id = "123", is_super = true };
                var result = manager.Create(entity);
                Assert.IsNotEmpty(result);
            }
        }

        [Test]
        [Order(3)]
        public void Check_Entity_Query()
        {
            using(var manager = EntityManagerFactory.GetManager())
            {
                var result = manager.QueryFirst<Test>("123");
                Assert.IsNotNull(result);
                result = manager.QueryFirst<Test>("select * from test where id = @id", new { id = "123" });
                Assert.IsNotNull(result);
            }
        }

        [Test]
        [Order(4)]
        public void Check_Entity_Update()
        {
            using(var manager = EntityManagerFactory.GetManager())
            {
                var data = manager.QueryFirst<Test>("123");
                data.name = "test";
                manager.Update(data);
                data = manager.QueryFirst<Test>("123");
                Assert.IsTrue(data.name.Equals("test"));
            }
        }

        [Test]
        [Order(5)]
        public void Check_Entity_Delete()
        {
            using(var manager = EntityManagerFactory.GetManager())
            {
                manager.Delete("test", "123");
                var data = manager.QueryFirst<Test>("123");
                Assert.IsNull(data);

                var entity = new Test() { code = "A001", name = "Test", id = "123" };
                manager.Create(entity);

                data = manager.QueryFirst<Test>("123");
                manager.Delete(data);
                data = manager.QueryFirst<Test>("123");
                Assert.IsNull(data);

                manager.Create(entity);
                manager.Delete("test", entity.id);
                data = manager.QueryFirst<Test>("123");
                Assert.IsNull(data);

                Check_Entity_Create();
                var dataList = manager.Query<Test>("select * from test where id = @id", new { id = "123" }).ToArray();
                manager.Delete(dataList);
                dataList = manager.Query<Test>("select * from test where id = @id", new { id = "123" }).ToArray();
                Assert.IsTrue(dataList.Length == 0);
            }
        }

        [Test]
        [Order(6)]
        public void Check_BulkCreate()
        {
            var dataList = new List<Test>()
            {
                new Test() { id = Guid.NewGuid().ToString(), code = "B001", name = "测试1", created_at = DateTime.Now, created_by = "user", created_by_name = "user", updated_at = DateTime.Now, updated_by = "user", updated_by_name = "user", is_super = true },
                new Test() { id = Guid.NewGuid().ToString(), code = "B002", name = "测试2" , created_at = DateTime.Now, created_by = "user", created_by_name = "user", updated_at = DateTime.Now, updated_by = "user", updated_by_name = "user", is_super = false },
                new Test() { id = Guid.NewGuid().ToString(), code = "B003", name = "测试3", created_at = DateTime.Now, created_by = "user", created_by_name = "user", updated_at = DateTime.Now, updated_by = "user", updated_by_name = "user", is_super = false },
            };
            using(var manager = EntityManagerFactory.GetManager())
            {
                manager.BulkCreate(dataList);
                var count = manager.QueryCount("select COUNT(1) from test where code in ('B001', 'B002', 'B003')");
                Assert.AreEqual(3, count);
            }
        }

        [Test]
        [Order(7)]
        public void Check_BulkUpdate()
        {
            using(var manager = EntityManagerFactory.GetManager())
            {
                var dataList = manager.Query<Test>("select * from test where code in ('B001', 'B002', 'B003')").ToList();
                dataList[0].name = "test1";
                dataList[1].name = "test2";
                dataList[2].name = "test3";
                manager.BulkUpdate(dataList);
                dataList = manager.Query<Test>("select * from test where code in ('B001', 'B002', 'B003')").ToList();
                Assert.AreEqual(dataList[0].name, "test1");
                Assert.AreEqual(dataList[1].name, "test2");
                Assert.AreEqual(dataList[2].name, "test3");
                manager.Execute("TRUNCATE test");
            }
        }

        [Test]
        [Order(8)]
        public void Check_BulkDelete()
        {
            var dataList = new List<Test>()
            {
                new Test() { id = Guid.NewGuid().ToString(), code = "B001", name = "测试1", created_at = DateTime.Now, created_by = "user", created_by_name = "user", updated_at = DateTime.Now, updated_by = "user", updated_by_name = "user" },
                new Test() { id = Guid.NewGuid().ToString(), code = "B002", name = "测试2" , created_at = DateTime.Now, created_by = "user", created_by_name = "user", updated_at = DateTime.Now, updated_by = "user", updated_by_name = "user" },
                new Test() { id = Guid.NewGuid().ToString(), code = "B003", name = "测试3", created_at = DateTime.Now, created_by = "user", created_by_name = "user", updated_at = DateTime.Now, updated_by = "user", updated_by_name = "user" },
            };
            using(var manager = EntityManagerFactory.GetManager())
            {
                manager.ExecuteTransaction(() =>
                {
                    manager.BulkCreate(dataList);
                    manager.BulkDelete(dataList);
                    dataList = manager.Query<Test>("select * from test where code in ('B001', 'B002', 'B003')").ToList();
                    Assert.IsTrue(dataList.IsEmpty());
                });
            }
        }
    }
}
