
using System;

namespace Project.NodeSystem
{
    public static class UseStringEventModifier
    {
        public static bool ModifierBoolCheck(StringEventModifierType stringEventModifierType)
        {
            switch (stringEventModifierType)
            {
                case StringEventModifierType.SetTrue:
                    return true;

                case StringEventModifierType.SetFalse:
                    return false;

                default:
                    return false;
            }
        }

        public static float ModifierFloatCheck(float inputValue, StringEventModifierType stringEventModifierType)
        {
            switch (stringEventModifierType)
            {
                case StringEventModifierType.Add:
                    return inputValue;

                case StringEventModifierType.Substract:
                    return -inputValue;

                default:
                    return 0f;
            }
        }

        public static int ModifierIntCheck(int inputValue, StringEventModifierType stringEventModifierType)
        {
            switch (stringEventModifierType)
            {
                case StringEventModifierType.Add:
                    return inputValue;

                case StringEventModifierType.Substract:
                    return -inputValue;

                default:
                    return 0;
            }
        }
    }
}
