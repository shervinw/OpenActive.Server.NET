using System;
using System.Collections.Generic;
using System.Text;

namespace OpenActive.Server.NET
{
    public static class PropertyFilter
    {
        /// <summary>
        /// This extension method returns a new instance of the type with its properties filtered based on the supplied list
        /// Recommend using e.g. nameof(Organization.Name) to populate the filter list
        /// Only shallow filtering is available (i.e. only properties within the specified type, no deeper).
        /// </summary>
        /// <param name="obj">Instance of type to filter</param>
        /// <param name="props">Property names to include in new instance</param>
        /// <returns>New instance of the type containing only specified property names, or null if obj is null</returns>
        public static T FilterProperties<T>(this T obj, List<string> props) where T : class, new() // TODO: Add unit test for this
        {
            if (obj == null) return null;
            if (props == null) return obj;

            var filteredObj = new T();
            var objType = obj.GetType();

            foreach(var prop in props)
            { 
                if (objType.GetProperty(prop) != null) throw new ArgumentException("Invalid property filter defined type properties", prop);

                objType.GetProperty(prop).SetValue(filteredObj, objType.GetProperty(prop).GetValue(filteredObj));
            }

            return filteredObj;
        }
    }
}
