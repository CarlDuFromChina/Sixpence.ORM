using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Sixpence.Common;
using Sixpence.Common.Current;
using Sixpence.ORM.Entity;
using Sixpence.ORM.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sixpence.ORM.Test
{
    [TestFixture]
    public class TestRepository
    {
        private Repository<Test> testRepository;

        [SetUp]
        public void SetUp()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddServiceContainer(options =>
            {
                options.Assembly.Add("Sixpence.ORM.Test");
            });
            CallContext<CurrentUserModel>.SetData(CallContextType.User, new CurrentUserModel() { Id = "1", Code = "1", Name = "test" });
            SixpenceORMExtension.UseEntityGenerate(null);
            testRepository = new Repository<Test>();
        }

        [Test]
        [Order(1)]
        public void Check_Repository_Insert()
        {
            testRepository.Create(new Test() { code = "A001", name = "Test", id = "124" });
            var data = testRepository.FindOne("124");
            Assert.IsNotNull(data);
        }

        [Test]
        [Order(2)]
        public void Check_Repository_Query()
        {
            var dataList = testRepository.Find();
            Assert.IsTrue(dataList != null && dataList.ToList().Count > 0);
            var data = testRepository.FindOne(new Dictionary<string, object>() { { "id", "124" } });
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
            data.name = "test";
            testRepository.Update(data);
            data = testRepository.FindOne("124");
            Assert.IsTrue(data.name.Equals("test"));
        }

        [Test]
        [Order(4)]
        public void Check_Repository_Delete()
        {
            testRepository.Delete("124");
            var data = testRepository.FindOne("124");
            Assert.IsNull(data);

           var id = testRepository.Save(new Test() { code = "A001", name = "Test" });
            Assert.IsNotEmpty(id);
            data = testRepository.FindOne(id);
            Assert.IsNotNull(data);

            testRepository.Delete(new string[] { id });
            data = testRepository.FindOne(id);
            Assert.IsNull(data);
        }
    }
}
