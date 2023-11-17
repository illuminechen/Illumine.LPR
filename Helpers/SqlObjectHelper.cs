using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Illumine.LPR
{
    public static class SqlObjectHelper
    {
        public static object CreateSqlObj(
          string sqlKey,
          SqlObjectHelper.ObjectKeyword objName,
          params object[] objects)
        {
            string typeName = string.Format(sqlKey, (object)objName.ToString());
            Type type = Type.GetType(typeName);
            if ((object)type == null)
                type = ((IEnumerable<Assembly>)AppDomain.CurrentDomain.GetAssemblies()).Select<Assembly, Type>((Func<Assembly, Type>)(a => a.GetType(typeName))).FirstOrDefault<Type>((Func<Type, bool>)(t => t != (Type)null));
            return Activator.CreateInstance(type, objects);
        }

        public enum ObjectKeyword
        {
            Connection,
            DataAdapter,
            Command,
            Transaction,
            Parameter,
        }
    }
}
