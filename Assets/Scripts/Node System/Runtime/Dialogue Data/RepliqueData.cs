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
        public List<RepliqueData_Replique> repliques = new List<RepliqueData_Replique>();
    }


    [System.Serializable]
    public class RepliqueData_Replique : NodeData_BaseContainer
    {

#if UNITY_EDITOR

        public Box boxContainer;        //Contient le conteneur de la réplique pour le supprimer plus facilement dans DialogueSaveLoad

        public TextField TextField { get; set; }
        public ObjectField AudioField { get; set; }
#endif

        public ContainerValue<string> guid = new ContainerValue<string>();
        public List<LanguageGeneric<string>> texts = new List<LanguageGeneric<string>>();
        public List<LanguageGeneric<AudioClip>> audioClips = new List<LanguageGeneric<AudioClip>>();

    }


}