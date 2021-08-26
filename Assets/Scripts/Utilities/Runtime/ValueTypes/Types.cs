using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Project.Utilities.ValueTypes
{
    public static class Types
    {

        public static Type[] SubclassesOf<T>()
        {
            //Récupère tous les types de nodes dérivant de BaseNode
            List<Type> subclassTypes = Assembly
               .GetAssembly(typeof(T))
               .GetTypes()
               .Where(t => t.IsSubclassOf(typeof(T))).ToList();

            subclassTypes.Sort(delegate (Type x, Type y)
            {
                return x.Name.CompareTo(y.Name);
            });

            return subclassTypes.ToArray();
        }

        public static int LengthOf<T>()
        {
            return SubclassesOf<T>().Length;
        }

        public static void ForEach<T>(Action<Type> action)
        {
            Type[] classes = SubclassesOf<T>();
            for (int i = 0; i < LengthOf<T>(); i++)
            {
                action.Invoke(classes[i]);
            }
        }
        public static void ForEach(this Type[] array, Action<Type> action)
        {
            for (int i = 0; i < array.Length; i++)
            {
                action.Invoke(array[i]);
            }
        }
    }
}