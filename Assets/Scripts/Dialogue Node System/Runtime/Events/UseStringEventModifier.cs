
using System;
using System.Reflection;

namespace Project.NodeSystem
{
    public static class UseStringEventModifier
    {


        //Utilisé par l'EventNode
        public static void DialogueModifierEvents(DE_Trigger triggerScript, string stringEvent, StringEventModifierType stringEventModifierType, float value = 0)
        {

            //On utilise  la réflection pour récupérer la variable ou propriété 
            //automatiquement sans avoir à la réécrire dans un script à chaque fois.


            //FieldInfo prop = triggerScript.GetType().GetField(stringEvent);
            PropertyInfo prop = triggerScript.GetType().GetProperty(stringEvent);
            bool? propIsBool = (prop.GetValue(triggerScript) as bool?);
            float? propIsFloat = (prop.GetValue(triggerScript) as float?);
            int? propIsInt = (prop.GetValue(triggerScript) as int?);




            switch (stringEventModifierType)
            {
                case StringEventModifierType.SetTrue:
                    if (propIsBool != null)
                        prop.SetValue(triggerScript, ModifierBoolCheck(stringEventModifierType));
                    break;
                case StringEventModifierType.SetFalse:
                    if (propIsBool != null)
                        prop.SetValue(triggerScript, ModifierBoolCheck(stringEventModifierType));
                    break;
                default:
                    if (propIsFloat != null)
                        prop.SetValue(triggerScript, ModifierFloatCheck(propIsFloat.Value, value, stringEventModifierType));
                    else if (propIsInt != null)
                        prop.SetValue(triggerScript, ModifierIntCheck(propIsInt.Value, (int)value, stringEventModifierType));
                    break;
            }
        }



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
