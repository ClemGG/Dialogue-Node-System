using System;

namespace Project.Utilities.ValueTypes
{
    public static class Enums
    {
        public static void ForEach<T>(Action<T> action) where T : System.Enum
        {
            foreach (T value in (T[])Enum.GetValues(typeof(T)))
            {
                action.Invoke(value);
            }
        }
    }
}