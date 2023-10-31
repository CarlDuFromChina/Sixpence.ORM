using Microsoft.Extensions.DependencyInjection;
using Sixpence.ORM.EntityManager;
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
        public static ServiceCollectionOptions Options = new ServiceCollectionOptions();

        public static IServiceCollection AddSorm(this IServiceCollection services, Action<ServiceCollectionOptions> action)
        {
            services.AddTransient<IEntityManagerBeforeCreateOrUpdate, EntityManagerBeforeCreateOrUpdate>();
            services.AddTransient(typeof(IRepository<>), typeof(Repository<>));

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
