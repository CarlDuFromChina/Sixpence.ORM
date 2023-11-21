using System;

namespace Sixpence.ORM.Entity
{
    /// <summary>
    /// 实体通用函数
    /// </summary>
    public static partial class EntityCommon
    {
        /// <summary>
        /// 生成实体随机 ID
        /// </summary>
        /// <param name="primaryType"></param>
        /// <returns></returns>
        public static object GenerateID(PrimaryType? primaryType = PrimaryType.GUID)
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

        /// <summary>
        /// 生成实体随机 ID
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public static object GenerateID<TEntity>() where TEntity : IEntity, new()
        {
            var entity = new TEntity();
            return GenerateID(entity.PrimaryColumn.PrimaryType);
        }

        /// <summary>
        /// 生成实体随机 ID
        /// </summary>
        /// <returns></returns>
        public static string GenerateGuid()
        {
            return Guid.NewGuid().ToString();
        }

        /// <summary>
        /// 生成实体随机 ID（数字版）
        /// </summary>
        /// <returns></returns>
        public static long GenerateGuidNumber()
        {
            byte[] buffer = Guid.NewGuid().ToByteArray();
            return BitConverter.ToInt64(buffer, 0);
        }

    }
}
