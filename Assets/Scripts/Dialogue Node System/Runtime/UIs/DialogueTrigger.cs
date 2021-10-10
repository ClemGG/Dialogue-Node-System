using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.NodeSystem
{
    public class DialogueTrigger : MonoBehaviour
    {
        #region Fields

        [SerializeField, Tooltip("Le nom de la scène à charger contenant l'UI et le DialogueManager.")]
        private string _dialogueSceneName = "Dialogue Scene";

        [SerializeField, Tooltip("Le nom de la scène à charger contenant l'UI et le DialogueManager.")]
        private LanguageType selectedLanguage = LanguageType.English;

        [SerializeField, Tooltip("Le script qui s'abonnera aux fonctions des events.")]
        private DE_EventCaller _triggerScript;

        [SerializeField, Tooltip("Le dialogue à jouer.")]
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

            //Le DialogueManager que l'on doit récupérer doit être celui de la scène "Dialogue Scene"
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