namespace Project.Utilities.ValueTypes
{
    public static class Conversions {



        /// <summary>
        /// Si BoolTo1AndMinus1 est à true, la fonction renverra -1 au lieu de 0 si la string passée en paramètre est une booléenne
        /// </summary>
        /// <param name="str"></param>
        /// <param name="minus1"></param>
        /// <returns></returns>
        public static int ToInt(this string str, bool minus1)
        {
            if (minus1)
            {
                if (str.ToLower().Equals("true"))
                {
                    return 1;
                }
                else if (str.ToLower().Equals("false"))
                {
                    return -1;
                }
            }
            else
            {
                if (str.ToLower().Equals("true"))
                {
                    return 1;
                }
                else if (str.ToLower().Equals("false"))
                {
                    return 0;
                }
            }

            return int.Parse(str);
        }
        
        
        /// <summary>
        /// Si BoolTo1AndMinus1 est à true, la fonction renverra -1 au lieu de 0 si la string passée en paramètre est une booléenne
        /// </summary>
        /// <param name="b"></param>
        /// <param name="minus1"></param>
        /// <returns></returns>
        public static int ToInt(this bool b, bool minus1)
        {
            if (minus1)
            {
                if (b)
                {
                    return 1;
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                if (b)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }
        
        
        
        
        
        /// <summary>
        /// Si BoolTo1AndMinus1 est à true, la fonction renverra -1 au lieu de 0 si la string passée en paramètre est une booléenne
        /// </summary>
        /// <param name="b"></param>
        /// <param name="minus1"></param>
        /// <returns></returns>
        public static float ToFloat(this bool b, bool minus1)
        {
            if (minus1)
            {
                if (b)
                {
                    return 1f;
                }
                else
                {
                    return -1f;
                }
            }
            else
            {
                if (b)
                {
                    return 1f;
                }
                else
                {
                    return 0f;
                }
            }
        }




        public static bool ToBool(this int i)
        {
            return i > 0;
        }
        public static bool ToBool(this float f)
        {
            return f > 0f;
        }
    }
}

