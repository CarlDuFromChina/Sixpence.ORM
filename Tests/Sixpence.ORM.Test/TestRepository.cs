using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Sixpence.ORM.Entity;
using Sixpence.ORM.Postgres;
using Sixpence.ORM.Repository;
using System.Linq;
using System.Text.Json;

namespace Sixpence.ORM.Test
{
    [TestFixture]
    public class TestRepository
    {
        private IRepository<Test> testRepository;
        private IRepository<TestGuidNumber> testGuidNumerRepository;

        [SetUp]
        public void SetUp()
        {
            IServiceCollection services = new ServiceCollection();

            // Plugin
            services.AddTransient<IEntityManagerPlugin, UserInfoPlugin>();
            // Entity
            services.AddTransient<IEntity, Test>();
            services.AddTransient<IEntity, TestGuidNumber>();
            // Repository
            services.AddTransient<IRepository<Test>, Repository<Test>>();
            services.AddTransient<IRepository<TestGuidNumber>, Repository<TestGuidNumber>>();
            
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
            testRepository = provider.GetRequiredService<IRepository<Test>>();
            testGuidNumerRepository = provider.GetRequiredService<IRepository<TestGuidNumber>>();
        }

        [Test]
        [Order(1)]
        public void Check_Repository_Insert()
        {
            testRepository.Create(new Test() { Code = "A001", Name = "Test", Id = "124", IsSuper = true });
            var data = testRepository.FindOne("124");
            Assert.IsNotNull(data);
        }

        [Test]
        [Order(2)]
        public void Check_Repository_Query()
        {
            var dataList = testRepository.Find();
            Assert.IsTrue(dataList != null && dataList.ToList().Count > 0);
            var data = testRepository.FindOne(new { id = "124" });
            Assert.IsNotNull(data);
            data = testRepository.FindOne("124");
            Assert.IsNotNull(data);
            dataList = testRepository.FindByIds("124");
            Assert.IsTrue(dataList != null && dataList.ToList().Count > 0);
        }

        [Test]
        [Order(3)]
        public void Check_Repository_Update()
        {
            var data = testRepository.FindOne("124");
            data.Name = "test";
            data.Tags = JsonConvert.SerializeObject(new string[] { "t1", "t2" });
            testRepository.Update(data);
            data = testRepository.FindOne("124");
            Assert.IsTrue(data.Name.Equals("test"));
        }

        [Test]
        [Order(4)]
        public void Check_Repository_Delete()
        {
            testRepository.Delete("124");
            var data = testRepository.FindOne("124");
            Assert.IsNull(data);

           var id = testRepository.Save(new Test() { Id = "124", Code = "A001", Name = "Test" });
            Assert.IsNotEmpty(id);
            data = testRepository.FindOne(id);
            Assert.IsNotNull(data);

            testRepository.Delete(new string[] { id });
            data = testRepository.FindOne(id);
            Assert.IsNull(data);
        }

        [Test]
        [Order(5)]
        public void Check_Guid_Number_Generate()
        {
            var id = testGuidNumerRepository.Create(new TestGuidNumber() { id = EntityCommon.GenerateID()?.ToString() });
            Assert.IsTrue(!string.IsNullOrEmpty(id));

            testGuidNumerRepository.Delete(id);

            var data = testGuidNumerRepository.FindOne(id);
            Assert.IsNull(data);
        }
    }
}
