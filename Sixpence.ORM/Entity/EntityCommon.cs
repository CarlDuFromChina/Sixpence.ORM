using Sixpence.ORM.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sixpence.ORM.Entity
{
    /// <summary>
    /// 实体通用函数
    /// </summary>
    public static class EntityCommon
    {
        /// <summary>
        /// 生成唯一 ID
        /// </summary>
        /// <param name="primaryType"></param>
        /// <returns></returns>
        public static object GenerateID(PrimaryType primaryType = PrimaryType.GUID)
        {
            switch (primaryType)
            {
                case PrimaryType.GUIDNumber:
                    return GenerateGuidNumber();
                case PrimaryType.GUID:
                default:
                    return GenerateGuid();
            }
        }

        public static string GenerateGuid()
        {
            return Guid.NewGuid().ToString();
        }

        public static long GenerateGuidNumber()
        {
            byte[] buffer = Guid.NewGuid().ToByteArray();
            return BitConverter.ToInt64(buffer, 0);
        }

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
