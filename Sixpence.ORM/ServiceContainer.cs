using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Sixpence.ORM
{
    internal static class ServiceContainer
    {
        internal static IServiceProvider Provider;
        internal static IApplicationBuilder Builder;

        public static void Configure(IApplicationBuilder builder)
        {
            Provider = builder.ApplicationServices;
            Builder = builder;
        }
    }
}
