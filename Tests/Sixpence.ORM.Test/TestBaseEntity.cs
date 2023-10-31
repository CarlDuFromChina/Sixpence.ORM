using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Sixpence.ORM.Entity;
using Sixpence.ORM.EntityManager;
using Sixpence.ORM.Postgres;
using Sixpence.ORM.Test.Entity;
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
            
            services.AddTransient<IEntityManagerPlugin, UserInfoPlugin>();
            services.AddTransient<IEntity, UserInfo>();
            services.AddTransient<IEntity, Test>();

            services.AddSorm(options =>
            {
                options.UsePostgres(DBSourceConfig.ConnectionString, DBSourceConfig.CommandTimeOut);
            });

            provider = services.BuildServiceProvider();
            var app = new ApplicationBuilder(provider);
            app.UseSorm(options =>
            {
                options.EnableLogging = false;
                options.MigrateDb = true;
            });
        }

        [Test]
        public void Check_Resolve_Entity()
        {
            var entity = provider.GetServices<IEntity>()
                .FirstOrDefault(item => item.EntityMap.Table == "user_info");
            Assert.IsNotNull(entity);
        }

        [Test]
        public void Check_Resolve_EntityManagerPlugin()
        {
            var plugin = provider.GetServices<IEntityManagerPlugin>()
                .FirstOrDefault(item => EntityCommon.MatchEntityManagerPlugin(item.GetType().Name, "user_info"));
            Assert.IsNotNull(plugin);
        }

        [Test]
        public void CheckPascalToUnderline()
        {
            var name = "UserInfo";
            Assert.IsTrue(EntityCommon.PascalToUnderline(name) == "user_info");
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
