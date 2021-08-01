#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif


namespace Project.NodeSystem
{

    /// <summary>
    /// Permet de sauvegarder les données du container en fonction de la langue et du type T.
    /// </summary>
    /// <typeparam name="T">T peut être une string, un audioClip, etc.</typeparam>
    [System.Serializable]
    public class LanguageGeneric<T>
    {
        public T Data;   //Les données propres à chaque langue
        public LanguageType Language;
    }


    [System.Serializable]
    public class NodeData_Port : NodeData_BaseContainer
    {
#if UNITY_EDITOR
        public Port Port;
#endif
        public string PortGuid;
        public string InputGuid;
        public string OutputGuid;
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
        public T Value;
    }



    /// <summary>
    /// Conteneur pour les enums.
    /// </summary>
    /// <typeparam name="T">Le type des données à contenir</typeparam>
    [System.Serializable]
    public class ContainerEnum<T> where T : System.Enum
    {
#if UNITY_EDITOR
        public EnumField EnumField;
#endif
        public T Value;
    }








    /// <summary>
    /// Permet de trier les conteneurs des nodes par un ID
    /// </summary>
    [System.Serializable]
    public class NodeData_BaseContainer
    {
#if UNITY_EDITOR
        public Box BtnsBox;
        public Box EventBox;
#endif

        public ContainerValue<int> ID = new ContainerValue<int>();
    }



    /// <summary>
    /// Conteneurs pour les events.
    /// </summary>
    [System.Serializable]
    public class EventData_StringEvent : NodeData_BaseContainer
    {
        public ContainerValue<string> StringEvent = new ContainerValue<string>();   //Name of the variable to modify
        public ContainerValue<float> Number = new ContainerValue<float>(); //The number to compare it to / to assign to it

    }

    /// <summary>
    /// Conteneur pour les events qui comparent la variable entrée avec une autre valeur.
    /// </summary>
    [System.Serializable]
    public class EventData_StringEventCondition : EventData_StringEvent
    {
        public ContainerEnum<StringEventConditionType> ConditionType = new ContainerEnum<StringEventConditionType>();
    }

    /// <summary>
    /// Conteneur pour les events qui modifient la variable entrée.
    /// </summary>
    [System.Serializable]
    public class EventData_StringEventModifier : EventData_StringEvent
    {

        public ContainerEnum<StringEventModifierType> ModifierType = new ContainerEnum<StringEventModifierType>();

        /// <summary>
        /// Pour chaque stringEvent, on récupère la variable correspondante dans DE_Trigger et on la modifie selon le modifierType
        /// </summary>
        /// <param name="triggerScript"></param>
        public void Invoke(DE_Trigger triggerScript)
        {
            UseStringEventModifier.DialogueModifierEvents(triggerScript, StringEvent.Value, ModifierType.Value, Number.Value);
        }
    }

    /// <summary>
    /// Conteneur pour les events sous forme de Scriptable Objects
    /// </summary>
    [System.Serializable]
    public class EventData_ScriptableEvent : NodeData_BaseContainer
    {
        public ContainerValue<DialogueEventSO> ScriptableObject = new ContainerValue<DialogueEventSO>();
    }


}