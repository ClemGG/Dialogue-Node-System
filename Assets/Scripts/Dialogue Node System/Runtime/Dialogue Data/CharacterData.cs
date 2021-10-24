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
        public List<CharacterData_CharacterSO> Characters = new List<CharacterData_CharacterSO>();

    }

    [System.Serializable]
    public class CharacterData_CharacterSO : NodeData_BaseContainer
    {
        public ContainerValue<DialogueCharacterSO> Character = new ContainerValue<DialogueCharacterSO>();
        public List<string> CharacterNames = new List<string>();
        public ContainerValue<string> CharacterName = new ContainerValue<string>();
        public ContainerValue<Sprite> Sprite = new ContainerValue<Sprite>();
        public ContainerEnum<CharacterMood> Mood = new ContainerEnum<CharacterMood>();
        public ContainerEnum<DialogueSide> FaceDirection = new ContainerEnum<DialogueSide>();
        public ContainerEnum<DialogueSide> SidePlacement = new ContainerEnum<DialogueSide>();


        /// <summary>
        /// If true, the script will go automatically to the next character after some time has passed
        /// </summary>
        public ContainerValue<bool> UseAutoDelay = new ContainerValue<bool>();

        /// <summary>
        /// Preferably at the same speed as the tween used for its animation.
        /// If no tween, we can leave it at 0
        /// </summary>
        public ContainerValue<float> AutoDelayDuration = new ContainerValue<float>();

#if UNITY_EDITOR

        public Box BoxContainer { get; set; }      //Contains the character to remove it more easily in DialogueSaveLoad
        public Image SpriteField { get; set; }
        public TextField NameField { get; set; }

#endif
    }

}