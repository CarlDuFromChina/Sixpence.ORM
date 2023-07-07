using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Sixpence.Common;
using Sixpence.Common.IoC;
using Sixpence.ORM.Entity;
using Sixpence.ORM.EntityManager;
using Sixpence.ORM.Interface;
using Sixpence.ORM.Postgres;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sixpence.ORM.Test
{
    [TestFixture]
    internal class TestBaseEntity
    {
        private Test test;

        [SetUp]
        public void SetUp()
        {
            test = new Test() { code = "A001", name = "Test", id = Guid.NewGuid().ToString(), is_super = true };
            IServiceCollection services = new ServiceCollection();
            services.AddServiceContainer(options =>
            {
                options.Assembly.Add("Sixpence.ORM.Test");
            });
            SixpenceORMBuilderExtension
                .UseORM(null, options =>
                {
                    options.EntityClassNameCase = NameCase.Pascal;
                })
                .UsePostgres(DBSourceConfig.Config.ConnectionString, DBSourceConfig.Config.CommandTimeOut)
                .UseMigrateDB();
        }

        [Test]
        public void ContainsKey()
        {
            Assert.IsTrue(test.ContainKey("code"));
        }

        [Test]
        public void Check_Resolve_Entity()
        {
            var entity = ServiceContainer.Resolve<IEntity>(className => EntityCommon.CompareEntityName(className, "user_info"));
            Assert.IsNotNull(entity);
        }

        [Test]
        public void Check_Resolve_EntityManagerPlugin()
        {
            var plugin = ServiceContainer.ResolveAll<IEntityManagerPlugin>(item => EntityCommon.MatchEntityManagerPlugin(item, "user_info"));
            Assert.IsNotNull(plugin);
        }
    }

    public class UserInfoPlugin : IEntityManagerPlugin
    {
        public void Execute(EntityManagerPluginContext context)
        {
            throw new NotImplementedException();
        }
    }
}
