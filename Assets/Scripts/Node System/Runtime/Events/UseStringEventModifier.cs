
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

        public static float ModifierFloatCheck(float currentValue, float checkValue, StringEventModifierType stringEventModifierType)
        {
            switch (stringEventModifierType)
            {
                case StringEventModifierType.Add:
                    return currentValue + checkValue;

                case StringEventModifierType.Substract:
                    return currentValue - checkValue;

                default:
                    return 0f;
            }
        }

        public static int ModifierIntCheck(int currentValue, int checkValue, StringEventModifierType stringEventModifierType)
        {
            switch (stringEventModifierType)
            {
                case StringEventModifierType.Add:
                    return currentValue + checkValue;

                case StringEventModifierType.Substract:
                    return currentValue - checkValue;

                default:
                    return 0;
            }
        }
    }
}
