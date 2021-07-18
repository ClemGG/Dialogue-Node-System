using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor.UIElements;
#endif

namespace Project.NodeSystem
{

    [System.Serializable]
    public class DialogueData : BaseData
    {
        public List<DialogueData_BaseContainer> baseContainers = new List<DialogueData_BaseContainer>();
        public List<DialogueData_Translation> translations = new List<DialogueData_Translation>();
        public List<DialogueData_CharacterSO> characters = new List<DialogueData_CharacterSO>();
        public List<DialogueData_Port> ports = new List<DialogueData_Port>();
    }


    [System.Serializable]
    public class DialogueData_BaseContainer
    {
        public ContainerValue<int> ID = new ContainerValue<int>();
    }

    [System.Serializable]
    public class DialogueData_CharacterSO : DialogueData_BaseContainer
    {
        public ContainerValue<DialogueCharacterSO> character = new ContainerValue<DialogueCharacterSO>();
        public ContainerValue<string> characterName = new ContainerValue<string>();
        public ContainerValue<Sprite> sprite = new ContainerValue<Sprite>();
        public ContainerEnum<CharacterMood> mood = new ContainerEnum<CharacterMood>();
        public ContainerEnum<DialogueSide> faceDirection = new ContainerEnum<DialogueSide>();
        public ContainerEnum<DialogueSide> sidePlacement = new ContainerEnum<DialogueSide>();

        public Image spriteField { get; set; }  //Le champ pour l'image du perso
        public TextField nameField { get; set; }  //Le champ pour le nom du perso
    }

    [System.Serializable]
    public class DialogueData_Translation : DialogueData_BaseContainer
    {

#if UNITY_EDITOR
        public TextField TextField { get; set; }
        public ObjectField AudioField { get; set; }
#endif

        public ContainerValue<string> guid = new ContainerValue<string>();
        public List<LanguageGeneric<string>> texts = new List<LanguageGeneric<string>>();
        public List<LanguageGeneric<AudioClip>> audioClips = new List<LanguageGeneric<AudioClip>>();

    }


    [System.Serializable]
    public class DialogueData_Port : DialogueData_BaseContainer
    {
        public string portGuid;
        public string inputGuid;
        public string outputGuid;
    }

}