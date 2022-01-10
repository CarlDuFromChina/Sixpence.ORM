﻿using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Sixpence.Common;
using Sixpence.ORM.Broker;
using Sixpence.ORM.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Dapper;

namespace Sixpence.ORM.Test
{
    [TestFixture]
    internal class PersistBrokerTest
    {
        IPersistBroker broker;

        [SetUp]
        public void SetUp()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddServiceContainer(options =>
            {
                options.Assembly.Add("Sixpence.ORM.Test");
            });
            broker = PersistBrokerFactory.GetPersistBroker();
        }
    }
}
