using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.NodeSystem
{
    public class DialogueTrigger : MonoBehaviour
    {
        #region Fields

        [SerializeField, Tooltip("The scene containing the UI and the DialogueManager.")]
        private string _dialogueSceneName = "Dialogue Scene";

        [SerializeField, Tooltip("The language to start the dialogue in.")]
        private LanguageType selectedLanguage = LanguageType.English;

        [SerializeField, Tooltip("The event script to subscribe to the manager.")]
        private DE_EventCaller _triggerScript;

        [SerializeField, Tooltip("The dialogue to play.")]
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

            //The DialogueManager to retrive must be in the scene "Dialogue Scene"
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
            _dm.InitAndStartNewDialogue(_dialogue, selectedLanguage, _triggerScript);
        }

        private void UnloadDialogueScene()
        {
            _dm.OnEndDialogue -= UnloadDialogueScene;
            SceneManager.UnloadSceneAsync(_dialogueSceneName, UnloadSceneOptions.None);
        }
    }
}