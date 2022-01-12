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
            SixpenceORMSetup.UseEntityGenerate(null);
            testRepository = new Repository<Test>();
        }

        [Test]
        [Order(1)]
        public void Check_Repository_Insert()
        {
            testRepository.Create(new Test() { code = "A001", name = "Test", id = "124" });
            var data = testRepository.Query("124");
            Assert.IsNotNull(data);
        }

        [Test]
        [Order(2)]
        public void Check_Repository_Query()
        {
            var data = testRepository.Query();
            Assert.IsTrue(data != null && data.ToList().Count > 0);
        }

        [Test]
        [Order(3)]
        public void Check_Repository_Update()
        {
            var data = testRepository.Query("124");
            data.name = "test";
            testRepository.Update(data);
            data = testRepository.Query("124");
            Assert.IsTrue(data.name.Equals("test"));
        }

        [Test]
        [Order(4)]
        public void Check_Repository_Delete()
        {
            testRepository.Delete("124");
            var data = testRepository.Query("124");
            Assert.IsNull(data);
        }
    }
}
