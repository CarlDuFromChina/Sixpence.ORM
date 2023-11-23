using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sixpence.ORM
{
    /// <summary>
    /// 创建所有实体前
    /// </summary>
    public interface IEntityMigrationInterceptor
    {
        void Execute(EntityMigrationInterceptorContext context);
    }

    public enum EntityMigrationAction
    {
        PreUpdateEntities,
        PostUpdateEntities,
        PreUpdateEntity,
        PostUpdateEntity
    }

    public class EntityMigrationInterceptorContext
    {
        public EntityMigrationAction Action { get; set; }
        public IEntityManager Manager { get; set; }
        public IEnumerable<IEntity> EntityList { get; set; }
        public IEntity CurrentEntity { get; set; }
        public ILogger Logger { get; set; }
    }
}
