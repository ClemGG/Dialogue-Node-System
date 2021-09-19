using System.Text;
using UnityEngine;


namespace Project.NodeSystem 
{

    public abstract class DialogueUI : MonoBehaviour
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

        private void Awake()
        {
            GetComponents();

            SetUpComponents();

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


        //A surcharger dans les classes filles pour obtenir les scripts pr�sents sur les UIs
        protected virtual void GetComponents()
        {
            _dialoguePanel = transform.GetChild(0).gameObject;
            _dialogueManager = GetComponent<DialogueManager>();
        }


        protected abstract void SetUpComponents();


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


        private void StartDialogue()
        {
            //Quand on lance le dialogue, on remet les �l�ments d'UI � leur valeur par d�faut
            ResetUI();
        }

        private void EndDialogue()
        {
            HideUI(true);
        }



        #endregion



        #region Display





        protected virtual void ResetUI() 
        {
            _dialoguePanel.SetActive(true);
        }

        protected abstract void ShowUI();

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