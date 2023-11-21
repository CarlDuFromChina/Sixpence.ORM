using Microsoft.Extensions.DependencyInjection;
using Sixpence.ORM.Mappers;
using Sixpence.ORM.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sixpence.ORM
{
    public static class SormServiceCollectionExtensions
    {
        public static ServiceCollectionOptions Options;

        public static IServiceCollection AddSorm(this IServiceCollection services, Action<ServiceCollectionOptions> action)
        {
            services.AddSingleton<IEntityManager, EntityManager>();
            services.AddTransient<IEntityManagerBeforeCreateOrUpdate, EntityManagerBeforeCreateOrUpdate>();

            Options = new ServiceCollectionOptions();
            action?.Invoke(Options);

            if (Options.DbSetting == null)
            {
                throw new ArgumentNullException("数据库设置不能为空");
            }

            services.AddMapper(Options);

            return services;
        }
    }
}
