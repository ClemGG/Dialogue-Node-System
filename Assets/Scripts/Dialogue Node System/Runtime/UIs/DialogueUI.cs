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

        protected StringBuilder _sb = new StringBuilder();  //To write each caracter one at a time

        #endregion




        #endregion




        #region Mono

        private void Awake()
        {
            GetComponents();

            SetUpComponents();

            SubscribeToManager();


        }

        private void OnDisable()
        {
            UnsubscribeFromManager();
        }


        #endregion


        #region Dialogue


        #region Init


        //Override this in the child classes to get all UI components by script
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
            //When we (re)start the dialogue, we reset all the UIs to their default values.
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
        /// Displays the canvas on screen.
        /// </summary>
        /// <param name="onRunEnded">What to do once the canvas is displayed.</param>
        protected abstract void ShowUI();

        /// <summary>
        /// Hides the canvas on screen.
        /// </summary>
        /// <param name="onRunEnded">What to do once the canvas is hidden.</param>
        protected abstract void HideUI();


        #endregion




        #endregion
    }
}