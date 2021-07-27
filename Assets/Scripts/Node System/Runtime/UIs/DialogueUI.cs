using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Project.NodeSystem {
    public class DialogueUI : MonoBehaviour
    {

        #region Fields


        #region Dialogue


        [Space(10)]
        [Header("General :")]
        [Space(10)]

        [SerializeField, Tooltip("Ecrire caractère par caractère ou afficher la réplique entière d'un coup ?")]
        protected bool writeCharByChar = true;

        [ReadOnly, SerializeField, Tooltip("Drapeau qui indique si le UI est en train d'écrire la réplique en cours.")]
        protected bool isWriting;

        [SerializeField, Tooltip("Vitesse d'écriture caractère par caractère.")]
        protected float charWriteSpeed = .016f;

        [SerializeField] protected GameObject dialogueUI;

        #endregion


        #region UIs



        #region Dialogue Characters

        [Space(10)]
        [Header("Dialogue Characters :")]
        [Space(10)]

        [SerializeField] protected GameObject characterLeftGo;
        [SerializeField] protected GameObject characterRightGo;
        [SerializeField] protected Image characterLeftImg;
        [SerializeField] protected Image characterRightImg;
        [SerializeField] protected TextMeshProUGUI characterNameText;

        #endregion


        #region Dialogue Text

        [Space(10)]
        [Header("Dialogue Text :")]
        [Space(10)]

        [SerializeField] protected GameObject dialogueContent;
        [SerializeField] protected Button continueBtn;
        [SerializeField] protected Button skipRepliqueBtn;
        [SerializeField] protected TextMeshProUGUI dialogueText;
        [SerializeField] protected TextMeshProUGUI continueBtnText;
        protected StringBuilder sb;  //Pour afficher les caractères du texte un à la fois
        #endregion


        #region Dialogue Choices

        [Space(10)]
        [Header("Dialogue Choices :")]
        [Space(10)]

        [SerializeField] protected GameObject choicesContent;

        [SerializeField] protected Button[] choiceBtns;   //Rempli une seule fois par code pour y accéder plus rapidement
        [SerializeField] protected TextMeshProUGUI[] choiceTmps;   //Rempli une seule fois par code pour y accéder plus rapidement

        #endregion


        #region Audio

        [Space(10)]
        [Header("Audio :")]
        [Space(10)]

        [SerializeField] private AudioClip charPrintClip;
        protected AudioSource voiceClipSource;
        protected AudioSource charClipSource;

        #endregion


        #endregion



        #endregion




        #region Mono

        void Awake()
        {
            voiceClipSource = gameObject.AddComponent<AudioSource>();
            charClipSource = gameObject.AddComponent<AudioSource>();
            voiceClipSource.volume = charClipSource.volume = .2f;

            voiceClipSource.playOnAwake = charClipSource.playOnAwake = false;
            charClipSource.loop = true;
            charClipSource.clip = charPrintClip;
            choicesContent.SetActive(false);
            dialogueContent.SetActive(false);
        }


        #endregion


        #region Dialogue

        public virtual void StartDialogue()
        {
            //Quand on lance le dialogue, on active l'interface
            dialogueUI.SetActive(true);
        }

        public virtual void EndDialogue()
        {
            dialogueUI.SetActive(false);
        }



        #region Buttons

        //Quand on clique sur le bouton continuer, on afficher le panel des choix 
        //s'il y a des ports, sinon, on joue la node suivante
        public void SetContinueBtn(UnityAction onContinueClicked)
        {
            continueBtn.onClick.RemoveAllListeners(); 
            continueBtn.onClick.AddListener(onContinueClicked);
        }


        //Quand l'UI doit afficher des choix, le bouton continuer appelle cette fonction
        public void ShowChoicesPanel()
        {
            dialogueContent.SetActive(false);
            choicesContent.SetActive(true);
        }



        //Quand un choix est sélectionné, ramené la fenêtre de la réplique
        public void OnChoiceSelected()
        {
            dialogueContent.SetActive(true);
            choicesContent.SetActive(false);
        }


        public void SetChoices(List<DialogueButtonContainer> dialogueButtonContainers)
        {
            choicesContent.SetActive(true);

            for (int i = 0; i < choiceBtns.Length; i++)
            {
                Button b = choiceBtns[i];

                //Si on a des boutons en trop, on les désactive
                if (i >= dialogueButtonContainers.Count)
                {
                    b.gameObject.SetActive(false);
                }
                else
                {
                    //Si les conditions sont réunies, on active le bouton normalement
                    if (dialogueButtonContainers[i].ConditionCheck)
                    {
                        b.gameObject.SetActive(true);
                        b.interactable = true;

                        b.onClick.RemoveAllListeners();
                        b.onClick.AddListener(dialogueButtonContainers[i].OnChoiceClicked);
                        b.onClick.AddListener(OnChoiceSelected);

                        choiceTmps[i].text = dialogueButtonContainers[i].Text;
                    }
                    else
                    {
                        //On n'active le bouton que si on doit le griser, puis on met son interactable à false
                        //pour montrer au joueur le choix indisponible en gris
                        b.gameObject.SetActive(dialogueButtonContainers[i].ChoiceState == ChoiceStateType.GreyOut);
                        b.interactable = false;

                        choiceTmps[i].text = dialogueButtonContainers[i].Text;
                    }


                }
            }
        }


        #endregion


        #region Repliques

        public virtual void SetText(string replique)
        {
            dialogueContent.SetActive(true);
            skipRepliqueBtn.onClick.RemoveAllListeners();   

            if (writeCharByChar)
            {
                skipRepliqueBtn.onClick.AddListener(() => WriteAllText(replique));    //On assigne au bouton skip sa fonction
                StartCoroutine(WriteRepliqueCharByCharCo(replique));
            }
            else
            {
                dialogueText.text = replique;

                skipRepliqueBtn.gameObject.SetActive(false);
                continueBtnText.gameObject.SetActive(true);
                continueBtn.interactable = true;
            }
        }

        private void WriteAllText(string replique)
        {
            //On n'est plus en train d'écrire, donc on remet les valeurs par défaut
            skipRepliqueBtn.gameObject.SetActive(false);
            continueBtnText.gameObject.SetActive(true);
            continueBtn.interactable = true;
            isWriting = false;


            //Arrêter le son d'écriture
            charClipSource.Stop();

            //On arrête la coroutine pour éviter d'écrire à la suite de la réplique entière
            StopAllCoroutines();
            dialogueText.text = replique;
        }


        //Pour écrire la réplique caractère par caractère à l'aide du StringBuilder
        private IEnumerator WriteRepliqueCharByCharCo(string replique)
        {
            skipRepliqueBtn.gameObject.SetActive(true);
            continueBtnText.gameObject.SetActive(false);
            continueBtn.interactable = false;   //Pour éviter de cliquer dessus, on sait jamais
            isWriting = true;

            int length = replique.Length;
            sb = new StringBuilder(length);
            WaitForSeconds wait = new WaitForSeconds(charWriteSpeed);

            //Joue le son d'écriture en boucle s'il y en a un
            charClipSource.Play();


            for (int i = 0; i < length; i++)
            {
                sb.Append(replique[i]);
                dialogueText.text = sb.ToString();


                //Wait a certain amount of time, then continue with the for loop
                yield return wait;
            }


            //Arrêter le son d'écriture
            charClipSource.Stop();

            isWriting = false;
            continueBtn.interactable = true;   //Pour éviter de cliquer dessus, on sait jamais
            continueBtnText.gameObject.SetActive(true);
            skipRepliqueBtn.gameObject.SetActive(false);
        }


        #endregion


        #region Characters

        public void SetCharacter(string characterName, Color characterNameColor, Sprite characterSprite, DialogueSide faceDir, DialogueSide sidePlacement)
        {

            if (sidePlacement == DialogueSide.Left)
            {
                //S'il n'y a pas de sprite en paramètre, on veut cacher le personnage de ce côté
                if (!characterSprite)
                {
                    characterLeftGo.gameObject.SetActive(false);
                    return;
                }
                else
                {
                    characterLeftGo.gameObject.SetActive(true); 

                }


            }
            else
            {
                //S'il n'y a pas de sprite en paramètre, on veut cacher le personnage de ce côté
                if (!characterSprite)
                {
                    characterRightGo.gameObject.SetActive(false);
                    return;
                }
                else
                {
                    characterRightGo.gameObject.SetActive(true);
                }


            }

            if(characterSprite) OnCharacterSet(characterSprite, faceDir, sidePlacement);

            characterNameText.text = characterName;
            characterNameText.color = characterNameColor;
        }



        /// <summary>
        /// On déplace l'assignation des persos ici pour pouvoir ajouter les tweens sans avoir à tout réécrire.
        /// (Appelé uniquement si on a un sprite à afficher)
        /// </summary>
        /// <param name="characterSprite"></param>
        /// <param name="faceDir"></param>
        /// <param name="sidePlacement"></param>
        protected virtual void OnCharacterSet(Sprite characterSprite, DialogueSide faceDir, DialogueSide sidePlacement)
        {
            if (sidePlacement == DialogueSide.Left)
            {
                characterLeftImg.sprite = characterSprite;
            }
            else
            {
                characterRightImg.sprite = characterSprite;
            }
        }


        public void PlaySound(AudioClip clip)
        {
            if (voiceClipSource.clip) voiceClipSource.Stop();
            voiceClipSource.clip = clip;
            voiceClipSource.Play();
        }

        #endregion



        #endregion
    }
}