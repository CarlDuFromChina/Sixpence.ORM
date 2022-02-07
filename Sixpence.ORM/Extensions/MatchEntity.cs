using System;
using System.Collections.Generic;
using System.Text;

namespace Sixpence.ORM.Extensions
{
    public static class MatchEntity
    {

        /// <summary>
        /// 比较类名和实体名
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
        public static bool CompareEntityName(string className, string entityName)
        {
            switch (SixpenceORMBuilderExtension.Options.EntityClassNameCase)
            {
                case ClassNameCase.Pascal:
                    return entityName.Replace("_", "").Equals(className, StringComparison.OrdinalIgnoreCase);
                case ClassNameCase.UnderScore:
                default:
                    return entityName.Equals(className, StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// 匹配 EntityManager Plugin
        /// </summary>
        /// <param name="className"></param>
        /// <param name="entityName"></param>
        /// <returns></returns>
        public static bool MatchEntityManagerPlugin(string className, string entityName)
        {
            return className.StartsWith(entityName.Replace("_", ""), StringComparison.OrdinalIgnoreCase);
        }
    }
}
