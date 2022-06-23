using Sixpence.Common;
using Sixpence.Common.Current;
using Sixpence.Common.Utils;
using Sixpence.ORM.Entity;
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

            var user = CallContext<CurrentUserModel>.GetData(CallContextType.User);

            switch (context.Action)
            {
                case EntityAction.PreCreate:
                    {
                        if ((!entity.GetAttributes().ContainsKey("created_by") || entity.GetAttributeValue("created_by") == null) && entity.GetType().GetProperty("created_by") != null)
                        {
                            entity.SetAttributeValue("created_by", user.Id);
                            entity.SetAttributeValue("created_by_name", user.Name);
                        }
                        if ((!entity.GetAttributes().ContainsKey("created_at") || entity.GetAttributeValue("created_at") == null) && entity.GetType().GetProperty("created_at") != null)
                        {
                            entity.SetAttributeValue("created_at", DateTime.Now);
                        }
                        entity.SetAttributeValue("updated_by", user.Id);
                        entity.SetAttributeValue("updated_by_name", user.Name);
                        entity.SetAttributeValue("updated_at", DateTime.Now);

                        SetBooleanName(entity);
                        CheckDuplicate(entity, manager);
                    }
                    break;
                case EntityAction.PreUpdate:
                    {
                        entity.SetAttributeValue("updated_by", user.Id);
                        entity.SetAttributeValue("updated_by_name", user.Name);
                        entity.SetAttributeValue("updated_at", DateTime.Now);

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

            attrs.Select(item => item as KeyAttributesAttribute)
               .Each(item =>
               {
                   if (item.AttributeList == null || item.AttributeList.Count == 0) return;

                   var paramList = new Dictionary<string, object>() { { "@id", entity.GetPrimaryColumn().Value } };
                   var sqlParam = new List<string>() { $" AND {entity.GetPrimaryColumn().Name} <> @id" }; // 排除自身
                   item.AttributeList.Distinct().Each(attr =>
                   {
                       var keyValue = manager.Driver.HandleNameValue($"@{attr}", entity[attr]);
                       sqlParam.Add($" AND {attr} = {keyValue.name}");
                       paramList.Add(keyValue.name, keyValue.value);
                   });

                   var sql = string.Format(@"SELECT {0} FROM {1} WHERE 1 = 1 ",  entity.GetPrimaryColumn().Name, entity.GetEntityName()) + string.Join("", sqlParam);
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
                    if (entity.GetType().GetProperties().Where(p => p.Name == $"{item.Key}_name").FirstOrDefault() != null)
                    {
                        dic.Add($"{item.Key}_name", (bool)item.Value ? "是" : "否");
                    }
                });

            dic.Each(item => entity.SetAttributeValue(item.Key, item.Value));
        }
    }
}
