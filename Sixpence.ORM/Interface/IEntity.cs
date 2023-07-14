using Sixpence.ORM.Entity;
using Sixpence.ORM.Interface;
using Sixpence.ORM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sixpence.ORM
{
    public interface IEntity
    {
        IDbEntityMap? EntityMap { get; }
        ISormPrimaryColumn PrimaryColumn { get; }
        IList<ISormColumn> Columns { get; }

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
        /// 获取字段值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        T GetAttributeValue<T>(string name) where T : class;

        /// <summary>
        /// 生成新 ID
        /// </summary>
        /// <returns></returns>
        string NewId();

        /// <summary>
        /// 获取所有字段名
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetKeys();

        /// <summary>
        /// 是否存在字段
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        bool ContainKey(string name);
    }
}
