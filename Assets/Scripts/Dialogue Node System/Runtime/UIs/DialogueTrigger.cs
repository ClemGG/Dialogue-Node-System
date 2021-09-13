using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.NodeSystem
{
    public class DialogueTrigger : MonoBehaviour
    {
        #region Components

        [SerializeField, Tooltip("Le script qui s'abonnera aux fonctions des events.")]
        private DE_Trigger _triggerScript;

        [SerializeField, Tooltip("Le dialogue à jouer.")]
        private DialogueContainerSO _dialogue;

        private DialogueManager _dm;

        #endregion

        // Start is called before the first frame update
        void Start()
        {
            SceneManager.sceneLoaded += StartDialogue;
            SceneManager.LoadSceneAsync("Dialogue Scene", LoadSceneMode.Additive);
        }

        private void StartDialogue(Scene sceneLoaded, LoadSceneMode loadSceneMode)
        {
            SceneManager.sceneLoaded -= StartDialogue;

            _dm = FindObjectOfType<DialogueManager>();
            _dm.OnEndDialogue += UnloadDialogueScene;
            _dm.InitAndStartNewDialogue(_dialogue, LanguageType.French, _triggerScript);
        }

        private void UnloadDialogueScene()
        {
            _dm.OnEndDialogue -= UnloadDialogueScene;
            SceneManager.UnloadSceneAsync("Dialogue Scene", UnloadSceneOptions.None);
        }
    }
}