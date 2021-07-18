using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Project.NodeSystem
{

    public static class GameEvents
    {
        private static DE_Trigger de_trigger;
        private static event Action onDialogueEventCalled;

        public static Action OnDialogueEventCalled { get => onDialogueEventCalled; set => onDialogueEventCalled = value; }
        private static DE_Trigger DE_Trigger => de_trigger ??= GameObject.FindObjectOfType<DE_Trigger>();




        public static void CalledDialogueRuntimeEvent()
        {
            OnDialogueEventCalled?.Invoke();
        }


        //Utilisé par l'EventNode
        public static void DialogueModifierEvents(string stringEvent, StringEventModifierType stringEventModifierType, float value = 0)
        {

            //On utilise  la réflection pour récupérer la variable ou propriété 
            //automatiquement sans avoir à la réécrire dans un script à chaque fois.


            FieldInfo prop = DE_Trigger.GetType().GetField(stringEvent);
            bool? propIsBool = (prop.GetValue(DE_Trigger) as bool?);
            float? propIsFloat = (prop.GetValue(DE_Trigger) as float?);
            int? propIsInt = (prop.GetValue(DE_Trigger) as int?);




            switch (stringEventModifierType)
            {
                case StringEventModifierType.SetTrue:
                    if (propIsBool != null)
                        prop.SetValue(DE_Trigger, UseStringEventModifier.ModifierBoolCheck(stringEventModifierType));
                    break;
                case StringEventModifierType.SetFalse:
                    if (propIsBool != null)
                        prop.SetValue(DE_Trigger, UseStringEventModifier.ModifierBoolCheck(stringEventModifierType));
                    break;
                default:
                    if (propIsFloat != null)
                        prop.SetValue(DE_Trigger, UseStringEventModifier.ModifierFloatCheck(value, stringEventModifierType));
                    else if (propIsInt != null)
                        prop.SetValue(DE_Trigger, UseStringEventModifier.ModifierIntCheck((int)value, stringEventModifierType));
                    break;
            }
        }


        //Utilisé par la BranchNode et la ChoiceNode
        public static bool DialogueConditionEvents(string stringEvent, StringEventConditionType stringEventConditionType, float value = 0)
        {
            //On utilise  la réflection pour récupérer la variable ou propriété 
            //automatiquement sans avoir à la réécrire dans un script à chaque fois.


            FieldInfo prop = DE_Trigger.GetType().GetField(stringEvent);
            bool? propIsBool = (prop.GetValue(DE_Trigger) as bool?);
            float? propIsFloat = (prop.GetValue(DE_Trigger) as float?);
            int? propIsInt = (prop.GetValue(DE_Trigger) as int?);



            switch (stringEventConditionType)
            {
                case StringEventConditionType.True:
                    if (propIsBool != null)
                        return UseStringEventCondition.ConditionBoolCheck(propIsBool.Value, stringEventConditionType);
                    else return false;

                case StringEventConditionType.False:
                    if (propIsBool != null)
                        return UseStringEventCondition.ConditionBoolCheck(propIsBool.Value, stringEventConditionType);
                    else return false;

                default:
                    if (propIsFloat != null)
                        return UseStringEventCondition.ConditionFloatCheck(propIsFloat.Value, value, stringEventConditionType);
                    else if (propIsInt != null)
                        return UseStringEventCondition.ConditionIntCheck(propIsInt.Value, (int)value, stringEventConditionType);
                    else return false;
            }
        }
    }
}