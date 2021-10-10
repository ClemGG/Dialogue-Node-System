using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace Project.NodeSystem
{

    [System.Serializable]
    public class RepliqueData : BaseData
    {
        public List<RepliqueData_Replique> Repliques = new List<RepliqueData_Replique>();

    }


    [System.Serializable]
    public class RepliqueData_Replique : NodeData_BaseContainer
    {

#if UNITY_EDITOR

        public Box BoxContainer { get; set; }
        public TextField TextField { get; set; }
        public ObjectField AudioField { get; set; }
#endif

        /// <summary>
        /// If true, append the new text to the previous one
        /// </summary>
        public ContainerValue<bool> AppendToText = new ContainerValue<bool>();

        /// <summary>
        /// If true, the player cannot click on Continue or Skip while the text is being written
        /// </summary>
        public ContainerValue<bool> CanClickOnContinue = new ContainerValue<bool>();

        /// <summary>
        /// If true, overrides the writing speed
        /// </summary>
        public ContainerValue<bool> OverrideWriteSpeed = new ContainerValue<bool>();

        /// <summary>
        /// If true, go to the next node after some time has passed
        /// </summary>
        public ContainerValue<bool> UseAutoDelay = new ContainerValue<bool>();


        public ContainerValue<float> WriteSpeed = new ContainerValue<float>();
        public ContainerValue<float> AutoDelayDuration = new ContainerValue<float>();

        public ContainerValue<string> Guid = new ContainerValue<string>();
        public List<LanguageGeneric<string>> Texts = new List<LanguageGeneric<string>>();
        public List<LanguageGeneric<AudioClip>> AudioClips = new List<LanguageGeneric<AudioClip>>();

    }


}