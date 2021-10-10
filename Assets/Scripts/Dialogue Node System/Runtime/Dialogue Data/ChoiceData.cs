using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor.UIElements;
#endif
using UnityEngine.Serialization;

namespace Project.NodeSystem
{
    [System.Serializable]
    public class ChoiceData : BaseData
    {
        public List<ChoiceData_Container> Choices = new List<ChoiceData_Container>();
    }

    [System.Serializable]
    public class ChoiceData_Container : NodeData_BaseContainer
    {

#if UNITY_EDITOR

        public Box BoxContainer { get; set; }
        public TextField TextField { get; set; }
        public TextField DescTextField { get; set; }
        public ObjectField ObjectField { get; set; }
        public Box ChoiceStateEnumBox { get; set; } 


#endif



        public ContainerValue<string> Guid = new ContainerValue<string>();
        public ContainerEnum<ChoiceStateType> ChoiceStateType = new ContainerEnum<ChoiceStateType>();
        public List<LanguageGeneric<string>> Texts = new List<LanguageGeneric<string>>();
        public List<LanguageGeneric<AudioClip>> AudioClips = new List<LanguageGeneric<AudioClip>>();
        public List<ChoiceData_Condition> Conditions = new List<ChoiceData_Condition>();
        public NodeData_Port LinkedPort = new NodeData_Port();
    }


    [System.Serializable]
    public class ChoiceData_Condition : NodeData_BaseContainer
    {

#if UNITY_EDITOR

        public TextField DescTextField { get; set; }

#endif

        public ContainerValue<string> Guid = new ContainerValue<string>();

        /// <summary>
        /// Short description to explain to the player why the conditions are not met (leave empty if choice always available)
        /// </summary>
        public List<LanguageGeneric<string>> DescriptionsIfNotMet = new List<LanguageGeneric<string>>();
        public EventData_StringEventCondition StringCondition = new EventData_StringEventCondition();
    }
}