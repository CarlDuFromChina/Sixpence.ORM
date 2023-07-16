using Microsoft.Extensions.DependencyInjection;
using Sixpence.ORM.Interface;
using Sixpence.ORM.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sixpence.ORM
{
    internal static class DbMapperBuilderExtension
    {
        public static IServiceCollection AddMapper(this IServiceCollection services, ServiceCollectionOptions options)
        {
            // 注册所有的实体
            GetEntityTypes().Each(item =>
            {
                services.AddTransient(typeof(IEntity), item);
            });

            var provider = services.BuildServiceProvider();
            var entityList = provider.GetServices<IEntity>();

            if (options.EntityMaps == null)
            {
                options.EntityMaps = new Dictionary<string, IDbEntityMap>();
            }

            if (options?.DbSetting?.Driver == null)
            {
                throw new ArgumentException("Entity Mapper 前必须定义 Driver");
            }

            entityList.Each(item =>
            {
                var entityMap = new DbEntityMap(item, options.DbSetting.Driver);
                options.EntityMaps.Add(item.GetType().FullName, entityMap);
            });

            return services;
        }

        private static List<Assembly> GetAssemblies()
        {
            var assemblies = new List<Assembly>();
            var assembly = Assembly.GetEntryAssembly();
            if (assembly != null)
            {
                assemblies.Add(assembly);
            }
            assemblies.AddRange(Assembly.GetExecutingAssembly().GetReferencedAssemblies().Select(Assembly.Load));
            return assemblies;
        }

        private static List<Type> GetEntityTypes()
        {
            var assemblies = GetAssemblies();
            var entityTypes = new List<Type>();
            assemblies.Each(assembly =>
            {
                var types = assembly.GetTypes().Where(type => type.GetInterfaces().Contains(typeof(IEntity)));
                entityTypes.AddRange(types);
            });
            return entityTypes;
        }
    }
}
