using Sixpence.ORM.Entity;
using Sixpence.ORM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sixpence.ORM
{
    public interface IEntity
    {
        /// <summary>
        /// 获取实体名（表名）
        /// </summary>
        /// <returns></returns>
        string GetEntityName();

        /// <summary>
        /// 获取逻辑名
        /// </summary>
        /// <returns></returns>
        string GetRemark();

        /// <summary>
        /// 获取主键列
        /// </summary>
        /// <returns></returns>
        (string Name, string Value, PrimaryType Type) GetPrimaryColumn();

        /// <summary>
        /// 获取属性值
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        object GetAttributeValue(string name);

        /// <summary>
        /// 设置属性值
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        void SetAttributeValue(string name, object value);

        /// <summary>
        /// 获取所有字段
        /// </summary>
        /// <returns></returns>
        IDictionary<string, object> GetAttributes();

        /// <summary>
        /// 获取所有值
        /// </summary>
        /// <returns></returns>
        IEnumerable<object> GetValues();

        /// <summary>
        /// 获取所有键
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetKeys();

        /// <summary>
        /// 是否存在该键
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        bool ContainKey(string name);
    }
}
