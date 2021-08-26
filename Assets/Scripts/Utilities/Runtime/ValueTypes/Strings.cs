namespace Project.Utilities.ValueTypes
{
    public static class Strings
    {
        public static bool AllNull(params string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (!string.IsNullOrEmpty(args[i]) || !string.IsNullOrWhiteSpace(args[i]))
                {
                    return false;
                }
            }

            return true;
        }
        public static bool AnyNull(params string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (string.IsNullOrEmpty(args[i]) || string.IsNullOrWhiteSpace(args[i]))
                {
                    return true;
                }
            }

            return false;
        }
        public static bool NoneNull(params string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (string.IsNullOrEmpty(args[i]) || string.IsNullOrWhiteSpace(args[i]))
                {
                    return false;
                }
            }

            return true;
        }


        public static bool IsNull(this string arg)
        {
            if (string.IsNullOrEmpty(arg) || string.IsNullOrWhiteSpace(arg))
            {
                return true;
            }

            return false;
        }


        public static bool ContainsAllCharactersFrom(this string str, string list)
        {
            bool contains = true;

            for (int i = 0; i < list.Length; i++)
            {
                if (!str.Contains(list[i].ToString()))
                {
                    contains = false;
                    break;
                }
            }

            return contains;
        }


        public static bool ContainsAnyCharacterFrom(this string str, string list)
        {
            bool contains = false;

            for (int i = 0; i < list.Length; i++)
            {
                if (str.Contains(list[i].ToString()))
                {
                    contains = true;
                    break;
                }
            }

            return contains;
        }



        /// <summary>
        /// Enlève les espaces de la string pour ne comparer que les caractères
        /// </summary>
        /// <param name="str"></param>
        /// <param name="respectUppercases"></param>
        /// <returns></returns>
        public static string SetupForComparison(this string str, bool respectUppercases)
        {
            if (!respectUppercases)
            {
                str = str.ToLower();
            }

            return str.Trim().Replace(" ", null);
        }


        public static bool IsSameStringButWithDifferentOrder(this string str, string compared, bool caseSensitive)
        {

            if (str.Length != compared.Length)
                return false;
            else
            {
                if (caseSensitive)
                    return str.ContainsAllCharactersFrom(compared);
                else
                    return str.ToLower().ContainsAllCharactersFrom(compared.ToLower());

            }

        }
    }
}