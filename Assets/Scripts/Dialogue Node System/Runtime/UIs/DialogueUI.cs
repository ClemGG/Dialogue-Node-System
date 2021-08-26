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


        [SerializeField] protected GameObject m_dialoguePanel;

        protected DialogueManager _dialogueManager;

        #endregion


        #region Write Settings


        [Space(10)]
        [Header("Write Settings :")]
        [Space(10)]

        [SerializeField, Tooltip("Ecrire caractère par caractère ou afficher la réplique entière d'un coup ?")]
        protected bool m_writeCharByChar = true;

        [SerializeField, Tooltip("Vitesse d'écriture caractère par caractère (Ne pas la mettre trop basse pour laisser le temps à l'audio de se jouer).")]
        protected float m_charWriteSpeed = .032f;

        [ReadOnly, SerializeField, Tooltip("Drapeau qui indique si le UI est en train d'écrire la réplique en cours.")]
        protected bool _isWriting = false;


        #endregion



        #region Dialogue Text

        protected StringBuilder _sb = new StringBuilder();  //Pour afficher les caractères du texte un à la fois

        #endregion




        #endregion




        #region Mono

        protected virtual void Awake()
        {
            // On abonne les méthodes au DialogueManager
            _dialogueManager = GetComponent<DialogueManager>();
            SubscribeToManager();

            
        }

        private void OnDisable()
        {
            UnsubscribeFromManager();
        }


        #endregion


        #region Dialogue


        #region Init

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

        protected virtual void ResetUI() { }

        protected virtual void ShowUI()
        {
            m_dialoguePanel.SetActive(true);
        }

        protected virtual void HideUI(bool endDialogue = false)
        {
            if (endDialogue)
            {
                m_dialoguePanel.SetActive(false);
            }
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








        #endregion
    }
}