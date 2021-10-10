using System;
using System.Reflection;
using UnityEngine;

namespace Project.NodeSystem
{

    public static class UseStringEventCondition
    {
        //Used by the BranchNode and the ChoiceNode
        public static bool DialogueConditionEvents(DE_EventCaller triggerScript, string stringEvent, StringEventConditionType stringEventConditionType, float value = 0)
        {
            //Uses reflection to retrieve the variable or property
            //automatically without having to rewrite it in a different scrip each time.


            //FieldInfo prop = triggerScript.GetType().GetField(stringEvent);
            PropertyInfo prop = triggerScript.GetType().GetProperty(stringEvent);
            bool? propIsBool = (prop.GetValue(triggerScript) as bool?);
            float? propIsFloat = (prop.GetValue(triggerScript) as float?);
            int? propIsInt = (prop.GetValue(triggerScript) as int?);



            switch (stringEventConditionType)
            {
                case StringEventConditionType.True:
                    if (propIsBool != null)
                        return ConditionBoolCheck(propIsBool.Value, stringEventConditionType);
                    else return false;

                case StringEventConditionType.False:
                    if (propIsBool != null)
                        return ConditionBoolCheck(propIsBool.Value, stringEventConditionType);
                    else return false;

                default:
                    if (propIsFloat != null)
                        return ConditionFloatCheck(propIsFloat.Value, value, stringEventConditionType);
                    else if (propIsInt != null)
                        return ConditionIntCheck(propIsInt.Value, (int)value, stringEventConditionType);
                    else return false;
            }
        }



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
                    Debug.LogWarning("GameEvents didn't find a event");
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
                    Debug.LogWarning("GameEvents didn't find a event");
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
                    Debug.LogWarning("GameEvents didn't find a event");
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