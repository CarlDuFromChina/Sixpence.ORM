using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sixpence.ORM.Utils
{
    public class AttributeUtil
    {
        public static T GetAttribute<T>(Type type) where T : Attribute
        {
            // 使用反射获取类型上的所有自定义属性
            object[] attributes = type.GetCustomAttributes(typeof(T), false);

            // 如果找到了指定类型的属性，返回第一个找到的
            if (attributes.Length > 0)
            {
                return (T)attributes[0];
            }
            else
            {
                // 如果找不到，返回null或者抛出异常，根据你的需求来决定
                return null;
            }
        }

        public static T GetAttribute<T>(PropertyInfo property) where T: Attribute
        {
            // 使用反射获取类型上的所有自定义属性
            object[] attributes = property.GetCustomAttributes(typeof(T), false);

            // 如果找到了指定类型的属性，返回第一个找到的
            if (attributes.Length > 0)
            {
                return (T)attributes[0];
            }
            else
            {
                // 如果找不到，返回null或者抛出异常，根据你的需求来决定
                return null;
            }
        }
    }
}
