using NUnit.Framework;
using Sixpence.ORM.Broker;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sixpence.ORM.Test
{
    [TestFixture]
    internal class PersistBrokerTest
    {
        IPersistBroker broker;

        [SetUp]
        public void SetUp()
        {
            broker = PersistBrokerFactory.GetPersistBroker();
        }

        [Test]
        public void Check_Connection_IsOpen()
        {
            Assert.IsTrue(broker.DbClient.ConnectionState == System.Data.ConnectionState.Open);
        }
    }
}
