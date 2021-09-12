using System.Text;
using UnityEngine;


namespace Project.NodeSystem 
{

    public class DialogueUI : MonoBehaviour
    {

        #region Fields

        #region Components


        [Space(10)]
        [Header("Components :")]
        [Space(10)]


        protected GameObject _dialoguePanel;
        protected DialogueManager _dialogueManager;

        #endregion


        #region Write Settings


        [Space(10)]
        [Header("Write Settings :")]
        [Space(10)]

        [SerializeField, Tooltip("Ecrire caract�re par caract�re ou afficher la r�plique enti�re d'un coup ?")]
        protected bool _writeCharByChar = true;

        [SerializeField, Tooltip("Vitesse d'�criture caract�re par caract�re (Ne pas la mettre trop basse pour laisser le temps � l'audio de se jouer).")]
        protected float _charWriteSpeed = .032f;

        [ReadOnly, SerializeField, Tooltip("Drapeau qui indique si le UI est en train d'�crire la r�plique en cours.")]
        protected bool _isWriting = false;


        #endregion



        #region Dialogue Text

        protected StringBuilder _sb = new StringBuilder();  //Pour afficher les caract�res du texte un � la fois

        #endregion




        #endregion




        #region Mono

        protected virtual void Awake()
        {
            GetComponents();

            // On abonne les m�thodes au DialogueManager
            SubscribeToManager();


        }

        private void OnDisable()
        {
            UnsubscribeFromManager();
        }


        #endregion


        #region Dialogue


        #region Init


        //A surcharger dans les classes filles pour obtenir les sripts pr�sents sur les UIs
        protected virtual void GetComponents()
        {
            _dialogueManager = GetComponent<DialogueManager>();
            _dialoguePanel = transform.GetChild(0).gameObject;
        }
        protected virtual void SubscribeToManager()
        {
            _dialogueManager.OnStartDialogue += StartDialogue;
            _dialogueManager.OnEndDialogue += EndDialogue;
        }


        protected virtual void UnsubscribeFromManager()
        {
            _dialogueManager.OnStartDialogue -= StartDialogue;
            _dialogueManager.OnEndDialogue -= EndDialogue;
        }



        #endregion



        #region Display


        private void StartDialogue()
        {
            //Quand on lance le dialogue, on active l'interface
            ResetUI();
            ShowUI();
        }

        private void EndDialogue()
        {
            HideUI(true);
        }

        protected virtual void ResetUI() 
        {
        }

        protected virtual void ShowUI()
        {
            _dialoguePanel.SetActive(true);
        }

        protected virtual void HideUI(bool endDialogue = false)
        {
            if (endDialogue)
            {
                _dialoguePanel.SetActive(false);
            }
        }


        #endregion




        #endregion
    }
}