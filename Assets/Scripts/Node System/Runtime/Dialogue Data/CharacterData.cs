using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEngine.UIElements;
#endif
using UnityEngine.Serialization;

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
        /// Si à true, le script passera automatiquement au perso suivant où à la node suivante au bout du temps donné
        /// </summary>
        public ContainerValue<bool> useAutoDelay = new ContainerValue<bool>();

        /// <summary>
        /// De préférence la même vitesse que le tween utilisé pour son animation.
        /// Si pas de tween, on peut laisser à 0
        /// </summary>
        public ContainerValue<float> autoDelayDuration = new ContainerValue<float>();

#if UNITY_EDITOR

        public Box BoxContainer { get; set; }      //Contient le conteneur du perso pour le supprimer plus facilement dans DialogueSaveLoad
        public Image SpriteField { get; set; }  //Le champ pour l'image du perso
        public TextField NameField { get; set; }  //Le champ pour le nom du perso

#endif
    }

}