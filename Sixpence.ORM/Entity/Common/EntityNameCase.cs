using System.Text;
using System.Text.RegularExpressions;

namespace Sixpence.ORM.Entity
{
    /// <summary>
    /// 实体通用函数-名称转换
    /// </summary>
    public static partial class EntityCommon
    {

        /// <summary>
        /// 帕斯卡命名转下划线命名
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string PascalToUnderline(string name)
        {
            var formatName = new StringBuilder();
            for (int i = 0; i < name.Length; i++)
            {
                var temp = name[i].ToString();
                if (Regex.IsMatch(temp, "[A-Z]"))
                {
                    if (i == 0)
                        temp = temp.ToLower();
                    else
                        temp = $"_{temp.ToLower()}";
                }
                formatName.Append(temp);
            }
            return formatName.ToString();
        }

        /// <summary>
        /// 下划线命名转帕斯卡命名
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string UnderlineToPascal(string name)
        {
            var formatName = new StringBuilder();
            var nameArray = name.Split('_');
            foreach (var item in nameArray)
            {
                formatName.Append(item.Substring(0, 1).ToUpper() + item.Substring(1));
            }
            return formatName.ToString();
        }

    }
}
