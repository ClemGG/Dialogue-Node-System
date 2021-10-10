using System;
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

        [SerializeField, Tooltip("Ecrire caractère par caractère ou afficher la réplique entière d'un coup ?")]
        protected bool _writeCharByChar = true;

        [SerializeField, Tooltip("Vitesse d'écriture caractère par caractère (Ne pas la mettre trop basse pour laisser le temps à l'audio de se jouer).")]
        protected float _charWriteSpeed = .032f;

        [ReadOnly, SerializeField, Tooltip("Drapeau qui indique si le UI est en train d'écrire la réplique en cours.")]
        protected bool _isWriting = false;


        #endregion



        #region Dialogue Text

        protected StringBuilder _sb = new StringBuilder();  //Pour afficher les caractères du texte un à la fois

        #endregion




        #endregion




        #region Mono

        private void Awake()
        {
            GetComponents();

            SetUpComponents();

            // On abonne les méthodes au DialogueManager
            SubscribeToManager();


        }

        private void OnDisable()
        {
            UnsubscribeFromManager();
        }


        #endregion


        #region Dialogue


        #region Init


        //A surcharger dans les classes filles pour obtenir les scripts présents sur les UIs
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
            //Quand on lance le dialogue, on remet les éléments d'UI à leur valeur par défaut
            ResetUI();
        }


        private void EndDialogue()
        {
            HideUI();
        }



        #endregion



        #region Display





        protected virtual void ResetUI() 
        {
            _dialoguePanel.SetActive(true);
        }

        /// <summary>
        /// Affiche le canvas à l'écran
        /// </summary>
        /// <param name="onRunEnded">L'action à effectuer une fois le canvas affiché</param>
        protected abstract void ShowUI();

        /// <summary>
        /// Cache le canvas à l'écran
        /// </summary>
        /// <param name="onRunEnded">L'action à effectuer une fois le canvas caché</param>
        protected abstract void HideUI();


        #endregion




        #endregion
    }
}