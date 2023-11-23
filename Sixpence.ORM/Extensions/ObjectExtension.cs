using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace Sixpence.ORM
{
    internal static class ObjectExtension
    {
        internal static Dictionary<string, object> ToDictionary(this object? param)
        {
            if (param == null)
                return new Dictionary<string, object>();

            if (param is Dictionary<string, object>)
                return param as Dictionary<string, object> ?? new Dictionary<string, object>();

            if (param is NameValueCollection)
            {
                var nvc = param as NameValueCollection;
                Dictionary<string, object> dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                foreach (string key in nvc.Keys)
                    dict.Add(key, nvc[key]);
                return dict;
            }

            if (param is Hashtable)
            {
                var hs = param as Hashtable;
                Dictionary<string, object> dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                foreach (string key in hs.Keys.OfType<string>())
                    dict.Add(key, hs[key]);
                return dict;
            }

            var dic = new Dictionary<string, object>();
            var properties = param.GetType().GetProperties();
            foreach (var item in properties)
            {
                dic.Add(item.Name, item.GetValue(param));
            }
            return dic;
        }
    }
}
