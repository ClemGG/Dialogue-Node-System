#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif


namespace Project.NodeSystem
{

    /// <summary>
    /// Saves the data by language.
    /// </summary>
    /// <typeparam name="T">T can be a string, an audioClip, etc.</typeparam>
    [System.Serializable]
    public class LanguageGeneric<T>
    {
        public T Data;
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
    /// A generic class to store and serialize the values of each field in our nodes
    /// </summary>
    [System.Serializable]
    public class ContainerValue<T>
    {
        public T Value;
    }



    /// <summary>
    /// Same as ContainerValue but for enums
    /// </summary>
    [System.Serializable]
    public class ContainerEnum<T> where T : System.Enum
    {
#if UNITY_EDITOR
        public EnumField EnumField;
#endif
        public T Value;
    }








    /// <summary>
    /// Alows to sort containers by ID
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
    /// Event container
    /// </summary>
    [System.Serializable]
    public class EventData_StringEvent : NodeData_BaseContainer
    {
        public ContainerValue<string> StringEvent = new ContainerValue<string>();   //Name of the variable to modify
        public ContainerValue<float> Number = new ContainerValue<float>(); //The number to compare it to / to assign to it

    }

    /// <summary>
    /// Event comparer container.
    /// </summary>
    [System.Serializable]
    public class EventData_StringEventCondition : EventData_StringEvent
    {
        public ContainerEnum<StringEventConditionType> ConditionType = new ContainerEnum<StringEventConditionType>();
    }

    /// <summary>
    /// Event modifier container.
    /// </summary>
    [System.Serializable]
    public class EventData_StringEventModifier : EventData_StringEvent
    {

        public ContainerEnum<StringEventModifierType> ModifierType = new ContainerEnum<StringEventModifierType>();

        /// <summary>
        /// Modifies each stringEvent in the triggerScript depending on the mofidierType
        /// </summary>
        /// <param name="triggerScript"></param>
        public void Invoke(DE_EventCaller triggerScript)
        {
            UseStringEventModifier.DialogueModifierEvents(triggerScript, StringEvent.Value, ModifierType.Value, Number.Value);
        }
    }

    /// <summary>
    /// SO event container.
    /// </summary>
    [System.Serializable]
    public class EventData_ScriptableEvent : NodeData_BaseContainer
    {
        public ContainerValue<DialogueEventSO> ScriptableObject = new ContainerValue<DialogueEventSO>();
    }


}