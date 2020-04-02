using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Function
{
    public abstract class IncRelection
    {
        public static List<string> GetFieldsName(Type objType)
        {
            List<string> res = new List<string>();
            System.Reflection.PropertyInfo[] ps = objType.GetProperties();
            foreach (PropertyInfo i in ps)
            {

                //object obj = i.GetValue(contract, null);
                //string name = i.Name;
            }
            return res;
        }

        public static void setValueToField(object obj, string name, object value)
        {
            //todo implate it. 需要反射得到对象属性的类型.并试图把vvalue 转化为此类型的值.
        }
    }
}