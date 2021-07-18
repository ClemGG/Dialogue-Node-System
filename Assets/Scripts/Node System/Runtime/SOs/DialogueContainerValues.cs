#if UNITY_EDITOR
using UnityEditor.UIElements;
#endif

namespace Project.NodeSystem
{
    [System.Serializable]
    public class DialogueContainerValues
    {
    }


    //Une classe pour contenir une référence à la valeur au lieu de la valeur elle-même.
    //Ca nous permet de la modifier directement par les champs qu'on crée via BaseNode et de garder ces changements en mémoire.
    //On en fait un générique pour éviter d'avoir à recréer chaque classe pour chaque valeur (string, int float, sprite, etc.)
    [System.Serializable]
    public class ContainerValue<T>
    {
        public T value;
    }


    //Conteneur pour les enums
    [System.Serializable]
    public class ContainerEnum<T> where T : System.Enum
    {
#if UNITY_EDITOR
        public EnumField enumField;
#endif
        public T value;
    }


    //Conteneurs pour les events
    [System.Serializable]
    public class EventData_StringModifier
    {
        public ContainerValue<string> stringEvent = new ContainerValue<string>();   //Name of the event
        public ContainerValue<float> number = new ContainerValue<float>();
        public ContainerEnum<StringEventModifierType> modifierType = new ContainerEnum<StringEventModifierType>();
    }

    [System.Serializable]
    public class EventData_StringCondition
    {
        public ContainerValue<string> stringEvent = new ContainerValue<string>();  //Name of the event
        public ContainerValue<float> number = new ContainerValue<float>();
        public ContainerEnum<StringEventConditionType> conditionType = new ContainerEnum<StringEventConditionType>();
    }



    //Permet de sauvegarder les données du container en fonction de la langue et du type T
    //(T peut être une string, un audioClip, etc...)
    [System.Serializable]
    public class LanguageGeneric<T>
    {
        public T data;   //Les données propres à chaque langue
        public LanguageType language;
    }
}