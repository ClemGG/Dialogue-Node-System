using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.NodeSystem
{
    public class DialogueTrigger : MonoBehaviour
    {
        #region Components

        [SerializeField, Tooltip("Le nom de la sc�ne � charger contenant l'UI et le DialogueManager.")]
        private string _dialogueSceneName = "Dialogue Scene";

        [SerializeField, Tooltip("Le script qui s'abonnera aux fonctions des events.")]
        private DE_Trigger _triggerScript;

        [SerializeField, Tooltip("Le dialogue � jouer.")]
        private DialogueContainerSO _dialogue;

        private DialogueManager _dm;

        #endregion

        [ContextMenu("Load Dialogue Scene (Play Mode Only)")]
        void Start()
        {
            if (!Application.isPlaying) return;

            SceneManager.sceneLoaded += StartDialogue;
            SceneManager.LoadSceneAsync(_dialogueSceneName, LoadSceneMode.Additive);
        }

        private void StartDialogue(Scene sceneLoaded, LoadSceneMode loadSceneMode)
        {
            SceneManager.sceneLoaded -= StartDialogue;

            //Le DialogueManager que l'on doit r�cup�rer doit �tre celui de la sc�ne "Dialogue Scene"
            GameObject[] roots = sceneLoaded.GetRootGameObjects();
            do
            {
                for (int i = 0; i < roots.Length; i++)
                {
                    DialogueManager dm = roots[i].GetComponentInChildren<DialogueManager>();
                    if (dm)
                    {
                        _dm = dm;
                        break;
                    }
                }
            }
            while (!_dm);
            

            
            _dm.OnEndDialogue += UnloadDialogueScene;
            _dm.InitAndStartNewDialogue(_dialogue, LanguageType.French, _triggerScript);
        }

        private void UnloadDialogueScene()
        {
            _dm.OnEndDialogue -= UnloadDialogueScene;
            SceneManager.UnloadSceneAsync(_dialogueSceneName, UnloadSceneOptions.None);
        }
    }
}