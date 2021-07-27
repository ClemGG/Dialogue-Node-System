using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEngine.UIElements;
#endif

namespace Project.NodeSystem
{

    [System.Serializable]
    public class CharacterData : BaseData
    {
        public List<CharacterData_CharacterSO> characters = new List<CharacterData_CharacterSO>();
    }

    [System.Serializable]
    public class CharacterData_CharacterSO : NodeData_BaseContainer
    {
        public ContainerValue<DialogueCharacterSO> character = new ContainerValue<DialogueCharacterSO>();
        public List<string> characterNames = new List<string>();
        public ContainerValue<string> characterName = new ContainerValue<string>();
        public ContainerValue<Sprite> sprite = new ContainerValue<Sprite>();
        public ContainerEnum<CharacterMood> mood = new ContainerEnum<CharacterMood>();
        public ContainerEnum<DialogueSide> faceDirection = new ContainerEnum<DialogueSide>();
        public ContainerEnum<DialogueSide> sidePlacement = new ContainerEnum<DialogueSide>();

#if UNITY_EDITOR

        public Box boxContainer;        //Contient le conteneur du perso pour le supprimer plus facilement dans DialogueSaveLoad
        public Image spriteField { get; set; }  //Le champ pour l'image du perso
        public TextField nameField { get; set; }  //Le champ pour le nom du perso

#endif
    }

}