#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif

namespace Project.NodeSystem
{

    /// <summary>
    /// Permet de sauvegarder les donn�es du container en fonction de la langue et du type T.
    /// </summary>
    /// <typeparam name="T">T peut �tre une string, un audioClip, etc.</typeparam>
    [System.Serializable]
    public class LanguageGeneric<T>
    {
        public T data;   //Les donn�es propres � chaque langue
        public LanguageType language;
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
    /// Une classe pour contenir une r�f�rence � la valeur au lieu de la valeur elle-m�me.
    /// Ca nous permet de la modifier directement par les champs qu'on cr�e via BaseNode et de garder ces changements en m�moire.
    /// On en fait un g�n�rique pour �viter d'avoir � recr�er chaque classe pour chaque valeur (string, int float, sprite, etc.)
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
    /// <typeparam name="T">Le type des donn�es � contenir</typeparam>
    [System.Serializable]
    public class ContainerEnum<T> where T : System.Enum
    {
#if UNITY_EDITOR
        public EnumField enumField;
#endif
        public T value;
    }








    /// <summary>
    /// Permet de trier les conteneurs des nodes par un ID
    /// </summary>
    [System.Serializable]
    public class NodeData_BaseContainer
    {
#if UNITY_EDITOR
        public Box btnsBox, eventBox;
#endif

        public ContainerValue<int> ID = new ContainerValue<int>();
    }



    /// <summary>
    /// Conteneurs pour les events.
    /// </summary>
    [System.Serializable]
    public class EventData_StringEvent : NodeData_BaseContainer
    {
        public ContainerValue<string> stringEvent = new ContainerValue<string>();   //Name of the variable to modify
        public ContainerValue<float> number = new ContainerValue<float>(); //The number to compare it to / to assign to it

    }

    /// <summary>
    /// Conteneur pour les events qui comparent la variable entr�e avec une autre valeur.
    /// </summary>
    [System.Serializable]
    public class EventData_StringEventCondition : EventData_StringEvent
    {
        public ContainerEnum<StringEventConditionType> conditionType = new ContainerEnum<StringEventConditionType>();
    }

    /// <summary>
    /// Conteneur pour les events qui modifient la variable entr�e.
    /// </summary>
    [System.Serializable]
    public class EventData_StringEventModifier : EventData_StringEvent
    {

        public ContainerEnum<StringEventModifierType> modifierType = new ContainerEnum<StringEventModifierType>();

        /// <summary>
        /// Pour chaque stringEvent, on r�cup�re la variable correspondante dans DE_Trigger et on la modifie selon le modifierType
        /// </summary>
        /// <param name="triggerScript"></param>
        public void Invoke(DE_Trigger triggerScript)
        {
            UseStringEventModifier.DialogueModifierEvents(triggerScript, stringEvent.value, modifierType.value, number.value);
        }
    }

    /// <summary>
    /// Conteneur pour les events sous forme de Scriptable Objects
    /// </summary>
    [System.Serializable]
    public class EventData_ScriptableEvent : NodeData_BaseContainer
    {
        public ContainerValue<DialogueEventSO> scriptableObject = new ContainerValue<DialogueEventSO>();
    }


}