using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Utilities.Arrays
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
    }
}