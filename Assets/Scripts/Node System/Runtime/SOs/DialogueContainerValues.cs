#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
#endif

namespace Project.NodeSystem
{

    /// <summary>
    /// Permet de trier les conteneurs des nodes par un ID
    /// </summary>
    [System.Serializable]
    public class NodeData_BaseContainer
    {
        public ContainerValue<int> ID = new ContainerValue<int>();
    }


    [System.Serializable]
    public class NodeData_Port : NodeData_BaseContainer
    {
#if UNITY_EDITOR
        public Port port;
#endif
        public string portGuid;
        public string inputGuid;
        public string outputGuid;
    }



    /// <summary>
    /// Une classe pour contenir une référence à la valeur au lieu de la valeur elle-même.
    /// Ca nous permet de la modifier directement par les champs qu'on crée via BaseNode et de garder ces changements en mémoire.
    /// On en fait un générique pour éviter d'avoir à recréer chaque classe pour chaque valeur (string, int float, sprite, etc.)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [System.Serializable]
    public class ContainerValue<T>
    {
        public T value;
    }



    /// <summary>
    /// Conteneur pour les enums.
    /// </summary>
    /// <typeparam name="T">Le type des données à contenir</typeparam>
    [System.Serializable]
    public class ContainerEnum<T> where T : System.Enum
    {
#if UNITY_EDITOR
        public EnumField enumField;
#endif
        public T value;
    }


    /// <summary>
    /// Conteneurs pour les events.
    /// </summary>
    [System.Serializable]
    public class EventData_StringEvent
    {
        public ContainerValue<string> stringEvent = new ContainerValue<string>();   //Name of the variable to modify
        public ContainerValue<float> number = new ContainerValue<float>(); //The number to compare it to / to assign to it

    }

    /// <summary>
    /// Conteneur pour les events qui modifient la variable entrée.
    /// </summary>
    [System.Serializable]
    public class EventData_StringModifier : EventData_StringEvent
    {
        public ContainerEnum<StringEventModifierType> modifierType = new ContainerEnum<StringEventModifierType>();
    }

    /// <summary>
    /// Conteneur pour les events qui comparent la variable entrée avec une autre valeur.
    /// </summary>
    [System.Serializable]
    public class EventData_StringCondition : EventData_StringEvent
    {
        public ContainerEnum<StringEventConditionType> conditionType = new ContainerEnum<StringEventConditionType>();
    }



    /// <summary>
    /// Permet de sauvegarder les données du container en fonction de la langue et du type T.
    /// </summary>
    /// <typeparam name="T">T peut être une string, un audioClip, etc.</typeparam>
    [System.Serializable]
    public class LanguageGeneric<T>
    {
        public T data;   //Les données propres à chaque langue
        public LanguageType language;
    }
}