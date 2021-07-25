using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
#endif

namespace Project.NodeSystem
{
    [System.Serializable]
    public class ChoiceData : BaseData
    {
        public List<ChoiceData_Container> choices = new List<ChoiceData_Container>();
    }

    [System.Serializable]
    public class ChoiceData_Container : NodeData_BaseContainer
    {

#if UNITY_EDITOR

        public Box choiceContainer;        //Contient tous les éléments suivants + les Boxs contenant les conditions

        public TextField TextField { get; set; }
        public TextField DescTextField { get; set; }
        public ObjectField ObjectField { get; set; }

        public Box choiceStateEnumBox;  //Conteneur du choiceStateType et ses labels


#endif



        public ContainerValue<string> guid = new ContainerValue<string>();
        public ContainerEnum<ChoiceStateType> choiceStateType = new ContainerEnum<ChoiceStateType>();
        public List<LanguageGeneric<string>> texts = new List<LanguageGeneric<string>>();
        public List<LanguageGeneric<AudioClip>> audioClips = new List<LanguageGeneric<AudioClip>>();
        public List<ChoiceData_Condition> conditions = new List<ChoiceData_Condition>();
        public NodeData_Port linkedPort = new NodeData_Port();
    }


    [System.Serializable]
    public class ChoiceData_Condition : NodeData_BaseContainer
    {

#if UNITY_EDITOR

        public TextField DescTextField { get; set; }

#endif

        public ContainerValue<string> guid = new ContainerValue<string>();

        /// <summary>
        /// Courte description pour indiquer au joueur pourquoi les conditions ne sont pas remplies (laisser vide si choix tjs dispo)
        /// </summary>
        public List<LanguageGeneric<string>> descriptionsIfNotMet = new List<LanguageGeneric<string>>();
        public EventData_StringEventCondition stringCondition = new EventData_StringEventCondition();
    }
}