using System;
using UnityEngine;

namespace Project.NodeSystem
{

    public static class UseStringEventCondition
    {
        public static bool ConditionIntCheck(int currentValue, float checkValue, StringEventConditionType stringEventConditionType)
        {
            switch (stringEventConditionType)
            {
                case StringEventConditionType.Equals:
                    return ValueEquals(currentValue, checkValue);

                case StringEventConditionType.EqualsOrBigger:
                    return ValueEqualsAndBigger(currentValue, checkValue);

                case StringEventConditionType.EqualsOrSmaller:
                    return ValueEqualsAndSmaller(currentValue, checkValue);

                case StringEventConditionType.Bigger:
                    return ValueBigger(currentValue, checkValue);

                case StringEventConditionType.Smaller:
                    return ValueSmaller(currentValue, checkValue);

                default:
                    Debug.LogWarning("GameEvents dint find a event");
                    return false;
            }
        }

        public static bool ConditionFloatCheck(float currentValue, float checkValue, StringEventConditionType stringEventConditionType)
        {
            switch (stringEventConditionType)
            {
                case StringEventConditionType.Equals:
                    return ValueEquals(currentValue, checkValue);

                case StringEventConditionType.EqualsOrBigger:
                    return ValueEqualsAndBigger(currentValue, checkValue);

                case StringEventConditionType.EqualsOrSmaller:
                    return ValueEqualsAndSmaller(currentValue, checkValue);

                case StringEventConditionType.Bigger:
                    return ValueBigger(currentValue, checkValue);

                case StringEventConditionType.Smaller:
                    return ValueSmaller(currentValue, checkValue);

                default:
                    Debug.LogWarning("GameEvents dint find a event");
                    return false;
            }
        }

        public static bool ConditionBoolCheck(bool currentValue, StringEventConditionType stringEventConditionType)
        {
            switch (stringEventConditionType)
            {
                case StringEventConditionType.True:
                    return ValueBool(currentValue, true);

                case StringEventConditionType.False:
                    return ValueBool(currentValue, false);

                default:
                    Debug.LogWarning("GameEvents dint find a event");
                    return false;
            }
        }

        private static bool ValueBool(bool currentValue, bool checkValue)
        {
            return currentValue == checkValue;
        }

        private static bool ValueEquals(float currentValue, float checkValue)
        {
            return currentValue == checkValue;
        }

        private static bool ValueEqualsAndBigger(float currentValue, float checkValue)
        {
            return currentValue >= checkValue;
        }

        private static bool ValueEqualsAndSmaller(float currentValue, float checkValue)
        {
            return currentValue <= checkValue;
        }

        private static bool ValueBigger(float currentValue, float checkValue)
        {
            return currentValue > checkValue;
        }

        private static bool ValueSmaller(float currentValue, float checkValue)
        {
            return currentValue < checkValue;
        }

    }
}