using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor.UIElements;
#endif

namespace Project.NodeSystem
{
    [System.Serializable]
    public class ChoiceData : BaseData
    {

#if UNITY_EDITOR

        public TextField TextField { get; set; }
        public ObjectField ObjectField { get; set; }

#endif

        public ContainerEnum<ChoiceStateType> choiceStateType = new ContainerEnum<ChoiceStateType>();
        public List<LanguageGeneric<string>> texts = new List<LanguageGeneric<string>>();
        public List<LanguageGeneric<AudioClip>> audioClips = new List<LanguageGeneric<AudioClip>>();
        public List<EventData_StringCondition> stringConditions = new List<EventData_StringCondition>();

    }
}