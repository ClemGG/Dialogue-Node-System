using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Utilities.ValueTypes
{
    public static class Arrays
    {
        public static void Clamp360(ref this int index, int increase, Array array)
        {
            index += increase;
            if(index >= array.Length)
            {
                index = 0;
            }
            else if(index < 0)
            {
                index = array.Length - 1;
            }
        }


        public static void DestroyAllIn(GameObject[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                UnityEngine.Object.DestroyImmediate(array[i]);
            }
        }
        public static void DestroyAllIn(Component[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                GameObject.DestroyImmediate(array[i].gameObject);
            }
        }
        public static void DestroyAllIn(List<GameObject> array)
        {
            for (int i = 0; i < array.Count; i++)
            {
                UnityEngine.Object.DestroyImmediate(array[i]);
            }
        }
        public static void DestroyAllIn(List<Component> array)
        {
            for (int i = 0; i < array.Count; i++)
            {
                GameObject.DestroyImmediate(array[i].gameObject);
            }
        }
    }
}