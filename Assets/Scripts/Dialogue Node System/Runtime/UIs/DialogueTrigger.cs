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


        #endregion

        // Start is called before the first frame update
        void Start()
        {
            SceneManager.LoadSceneAsync("Dialogue Scene", LoadSceneMode.Additive);
            SceneManager.sceneLoaded += StartDialogue;
        }

        private void StartDialogue(Scene sceneLoaded, LoadSceneMode loadSceneMode)
        {
            SceneManager.sceneLoaded -= StartDialogue;
            FindObjectOfType<DialogueManager>().InitAndStartNewDialogue(_dialogue, LanguageType.French, _triggerScript);
        }
    }
}