using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dumper
{
    public static class ObjectDumper
    {
        private static readonly HashSet<object> DumpedObjects = new HashSet<object>();
        public static string Dump(this object obj)
        {
            DumpedObjects.Clear();
            if (obj == null) return "null object";
            var type = obj.GetType();
            return DumpObject(obj, 0, type.Name);
        }

        private static string ParseArray(IList list,int depth)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < list.Count; i++)
            {
                var type = list[i].GetType();
                sb.AppendLine(list[i].DumpObject(depth, type.Name + ",Index " + (i).ToString()));
            }
            return sb.ToString();
        }

        private static string DumpObject(this object obj, int depth, string name = null)
        {
            if (obj == null)
                return GetLine(name, "null", depth);
            var type = obj.GetType();
            if (type.IsPrimitive || type == typeof (string) || type.IsEnum)
                return GetLine(name, obj.ToString(), depth);
            if (!type.IsValueType)
            {
                if (DumpedObjects.Contains(obj))
                    return GetLine(name, "Already dumped", depth);
                DumpedObjects.Add(obj);
            }
            StringBuilder builder = new StringBuilder();
            if (type.IsArray || typeof (IList).IsAssignableFrom(type))
            {
                builder.AppendLine(GetLine(name, null, depth));
                builder.AppendLine(ParseArray(obj as IList, depth + 1));
            }
            else if (type.IsClass)
            {
                builder.AppendLine(GetLine(name, null, depth));
                var propertys = type.GetProperties();
                if (!propertys.Any())
                    builder.AppendLine(GetLine(name, obj.ToString(), depth));
                foreach (var p in propertys)
                {
                    try
                    {
                        var value = p.GetValue(obj, null);
                        var line = DumpObject(value, depth + 1, p.Name);
                        builder.AppendLine(line);
                    }
                    catch (Exception ex)
                    {
                        string msg = GetLine(name, ex.Message, depth + 1);
                        builder.AppendLine(msg);
                    }
                }
            }
            else
            {
                builder.AppendLine(GetLine(name, obj.ToString(), depth));
            }
            return builder.ToString();
        }

        static string GetLine(string name, string value, int depth)
        {
            string indent = new string(' ', depth*2);
            var line = string.Format("{0}{1}: {2}", indent, name, value);
            return line;
        }
    }
}
