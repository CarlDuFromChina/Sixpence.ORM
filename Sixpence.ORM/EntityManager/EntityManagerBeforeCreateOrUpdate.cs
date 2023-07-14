using Sixpence.ORM.Entity;
using Sixpence.ORM.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sixpence.ORM.EntityManager
{
    /// <summary>
    /// 创建数据钩子
    /// </summary>
    public class EntityManagerBeforeCreateOrUpdate : IEntityManagerBeforeCreateOrUpdate
    {
        public void Execute(EntityManagerPluginContext context)
        {
            var entity = context.Entity;
            var manager = context.EntityManager;

            switch (context.Action)
            {
                case EntityAction.PreCreate:
                    {
                        SetBooleanName(entity);
                        CheckDuplicate(entity, manager);
                    }
                    break;
                case EntityAction.PreUpdate:
                    {
                        SetBooleanName(entity);
                        CheckDuplicate(entity, manager);
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 重复字段检查
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="manager"></param>
        private void CheckDuplicate(BaseEntity entity, IEntityManager manager)
        {
            var attrs = entity.GetType().GetCustomAttributes(typeof(KeyAttributesAttribute), false);
            if (attrs.Length == 0) return;

            var dialect = manager.Driver.Dialect;
            var parameterPrefix = dialect.ParameterPrefix;

            attrs.Select(item => item as KeyAttributesAttribute)
               .Each(item =>
               {
                   if (item.AttributeList == null || item.AttributeList.Count == 0) return;

                   var paramList = new Dictionary<string, object>() { { $"{parameterPrefix}id", entity.PrimaryColumn.Value } };
                   var sqlParam = new List<string>() { $" AND {entity.PrimaryColumn.DbPropertyMap.Name} <> {parameterPrefix}id" }; // 排除自身
                   item.AttributeList.Distinct().Each(attr =>
                   {
                       var keyValue = dialect.HandleParameter($"{parameterPrefix}{attr}", entity[attr]);
                       sqlParam.Add($" AND {attr} = {keyValue.name}");
                       paramList.Add(keyValue.name, keyValue.value);
                   });

                   var sql = string.Format(@"SELECT {0} FROM {1} WHERE 1 = 1 ",  entity.PrimaryColumn.DbPropertyMap.Name, entity.EntityMap.Table) + string.Join("", sqlParam);
                   AssertUtil.IsTrue(manager.Query<string>(sql, paramList)?.Count() > 0, item.RepeatMessage);
               });
        }

        /// <summary>
        /// 设置布尔值
        /// </summary>
        /// <param name="entity"></param>
        private void SetBooleanName(BaseEntity entity)
        {
            var dic = new Dictionary<string, object>();

            entity.GetAttributes()
                .Where(item => item.Value is bool)
                .Each(item =>
                {
                    var keyName = $"{item.Key}_name";
                    if (entity.ContainKey(keyName))
                    {
                        dic.Add(keyName, (bool)item.Value ? "是" : "否");
                    }
                });

            dic.Each(item => entity.SetAttributeValue(item.Key, item.Value));
        }
    }
}
