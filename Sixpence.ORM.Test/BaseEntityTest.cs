using NUnit.Framework;
using Sixpence.ORM.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sixpence.ORM.Test
{
    [TestFixture]
    internal class BaseEntityTest
    {
        private account _account;

        [SetUp]
        public void SetUp()
        {
            _account = new account() { code = "A001", name = "Test", Id = Guid.NewGuid().ToString() };
        }

        [Test]
        public void ContainsKey()
        {
            Assert.IsTrue(_account.ContainKey("code"));
        }
    }

    [Entity("account", "客户", false)]
    class account : BaseEntity
    {
        public string code { get; set; }
    }
}
