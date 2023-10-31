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
    }
}
