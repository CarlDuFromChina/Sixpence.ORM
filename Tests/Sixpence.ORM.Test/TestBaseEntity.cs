using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Sixpence.ORM.Entity;
using Sixpence.ORM.EntityManager;
using Sixpence.ORM.Postgres;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sixpence.ORM.Test
{
    [TestFixture]
    internal class TestBaseEntity
    {
        private Test test = new Test() { Code = "A001", Name = "Test", Id = Guid.NewGuid().ToString(), IsSuper = true };
        private IServiceProvider provider;

        [SetUp]
        public void SetUp()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddSorm(options =>
            {
                options.UsePostgres(DBSourceConfig.ConnectionString, DBSourceConfig.CommandTimeOut);
            });
            provider = services.BuildServiceProvider();
            var app = new ApplicationBuilder(provider);
            SormAppBuilderExtensions.UseSorm(app, options =>
            {
                options.EnableLogging = true;
                options.MigrateDb = true;
            });
        }

        [Test]
        public void ContainsKey()
        {
            Assert.IsTrue(test.ContainKey("code"));
        }

        [Test]
        public void Check_Resolve_Entity()
        {
            var entity = provider.GetServices<IEntity>()
                .FirstOrDefault(item => EntityCommon.CompareEntityName(nameof(item), "user_info"));
            Assert.IsNotNull(entity);
        }

        [Test]
        public void Check_Resolve_EntityManagerPlugin()
        {
            var plugin = provider.GetServices<IEntityManagerPlugin>()
                .FirstOrDefault(item => EntityCommon.MatchEntityManagerPlugin(nameof(item), "user_info"));
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
