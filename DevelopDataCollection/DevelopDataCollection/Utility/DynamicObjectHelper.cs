using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevelopDataCollection.Utility

{

    public static class DynamicObjectHelper
    {
        /// <summary>
        /// 轉成可以擴充的物件
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static dynamic ToDynamic(this object obj)
        {
            IDictionary<string, object> result = new ExpandoObject();

            foreach (PropertyDescriptor pro in TypeDescriptor.GetProperties(obj.GetType()))
            {
                result.Add(pro.Name, pro.GetValue(obj));
            }

            return result as ExpandoObject;
        } 
    }
}
