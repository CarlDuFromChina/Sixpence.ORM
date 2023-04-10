using Microsoft.Extensions.DependencyInjection;
using Sixpence.ORM.Common;
using Sixpence.ORM.Common.IoC;
using Sixpence.ORM.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sixpence.ORM.Test
{
    public static class SixpenceCommonSetup
    {
        public static void AddServiceContainer(this IServiceCollection services, Action<ServiceOptions> action = null)
        {
            ServiceContainer.Services = services;
            var options = new ServiceOptions() { Assembly = new List<string>() { "Sixpence.*.dll" } };
            if (action != null)
            {
                action.Invoke(options);
            }

            var types = AssemblyUtil.GetAssemblies(options.Assembly?.ToArray()).GetTypes();
            var interfaces = types.Where(item => item.IsInterface && item.IsDefined(typeof(ServiceRegisterAttribute), false)).ToList();
            interfaces.ForEach(item =>
            {
                var _types = types.Where(type => !type.IsInterface && !type.IsAbstract && type.GetInterfaces().Contains(item) && !type.IsDefined(typeof(IgnoreServiceRegisterAttribute), false)).ToList();
                _types.ForEach(type => ServiceContainer.Register(item, type));
            });
        }

        /// <summary>
        /// 注册程序集名
        /// </summary>
        public class ServiceOptions
        {
            public List<string> Assembly { get; set; }
        }
    }
}
