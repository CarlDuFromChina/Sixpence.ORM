using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Sixpence.Common;
using Sixpence.Common.IoC;
using Sixpence.ORM.Entity;
using Sixpence.ORM.EntityManager;
using Sixpence.ORM.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sixpence.ORM.Test
{
    [TestFixture]
    internal class BaseEntityTest
    {
        private Test test;

        [SetUp]
        public void SetUp()
        {
            test = new Test() { code = "A001", name = "Test", id = Guid.NewGuid().ToString() };
            IServiceCollection services = new ServiceCollection();
            services.AddServiceContainer(options =>
            {
                options.Assembly.Add("Sixpence.ORM.Test");
            });
            SixpenceORMBuilderExtension.UseORM(null, options =>
            {
                options.AutoGenerate = true;
                options.EntityClassNameCase = ClassNameCase.UnderScore;
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

    [Entity("user_info", "用户信息")]
    public class user_info : BaseEntity
    {
        [PrimaryColumn]
        public string id { get; set; }

        [Column("code", "编码", DataType.Varchar, 100, false)]
        public string code { get; set; }
    }

    public class UserInfoPlugin : IEntityManagerPlugin
    {
        public void Execute(EntityManagerPluginContext context)
        {
            throw new NotImplementedException();
        }
    }
}
