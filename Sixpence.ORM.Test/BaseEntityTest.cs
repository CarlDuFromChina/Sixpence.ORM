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
        private Test test;

        [SetUp]
        public void SetUp()
        {
            test = new Test() { code = "A001", name = "Test", Id = Guid.NewGuid().ToString() };
        }

        [Test]
        public void ContainsKey()
        {
            Assert.IsTrue(test.ContainKey("code"));
        }
    }
}
