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

        public Box BoxContainer { get; set; }       //Contient le conteneur de la réplique pour le supprimer plus facilement dans DialogueSaveLoad
        public TextField TextField { get; set; }
        public ObjectField AudioField { get; set; }
#endif

        /// <summary>
        /// Si à true, le texte s'écrira à la suite du précédent au lieu de l'effacer
        /// </summary>
        public ContainerValue<bool> AppendToText = new ContainerValue<bool>();

        /// <summary>
        /// Si à true, le joueur ne peut pas cliquer sur Continuer tant que le texte s'écrit
        /// </summary>
        public ContainerValue<bool> CanClickOnContinue = new ContainerValue<bool>();

        /// <summary>
        /// Si à true, on remplace la vitesse d'écriture par défaut par celle-ci
        /// </summary>
        public ContainerValue<bool> OverrideWriteSpeed = new ContainerValue<bool>();

        /// <summary>
        /// Si à true, le script passera automatiquement à la node suivante au bout du temps donné
        /// </summary>
        public ContainerValue<bool> UseAutoDelay = new ContainerValue<bool>();


        public ContainerValue<float> WriteSpeed = new ContainerValue<float>();
        public ContainerValue<float> AutoDelayDuration = new ContainerValue<float>();

        public ContainerValue<string> Guid = new ContainerValue<string>();
        public List<LanguageGeneric<string>> Texts = new List<LanguageGeneric<string>>();
        public List<LanguageGeneric<AudioClip>> AudioClips = new List<LanguageGeneric<AudioClip>>();

    }


}