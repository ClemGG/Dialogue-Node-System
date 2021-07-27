using System;

namespace Project.Utilities.ValueTypes
{
    public static class Enums
    {
        public static T[] ValuesOf<T>() where T : System.Enum
        {
            return (T[])Enum.GetValues(typeof(T));
        }

        public static int LengthOf<T>() where T : System.Enum
        {
            return ValuesOf<T>().Length;
        }

        public static void ForEach<T>(Action<T> action) where T : System.Enum
        {
            foreach (T value in ValuesOf<T>())
            {
                action.Invoke(value);
            }
        }
    }
}