using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Project.NodeSystem
{
    public class TweenDialogueUI : DialogueUI
    {

        #region Fields


        #region Dialogue


        [Space(10)]
        [Header("Tween Settings :")]
        [Space(10)]



        [SerializeField, Tooltip("Les Tweens à jouer sur les persos quand le dialogue est terminé.")]
        TweenSettings[] TS_CharOnDialogueEnded;

        [SerializeField, Tooltip("Les Tweens à jouer sur le container quand le dialogue est terminé.")]
        TweenSettings[] TS_ContainerOnDialogueEnded;

        [SerializeField, Tooltip("Le Tween à jouer quand un perso doit se retourner.")]
        TweenSettings[] TS_CharOnRot;


        [SerializeField, Tooltip("Le Tween à jouer quand un perso commence à parle.")]
        TweenSettings[] TS_CharOnStartTalk;
        [SerializeField, Tooltip("Le Tween à jouer quand un perso finit de parler.")]
        TweenSettings[] TS_CharOnEndTalk;

        #endregion


        #region UIs


        #region Dialogue Characters

        [Space(10)]
        [Header("Dialogue Characters :")]
        [Space(10)]

        [SerializeField] ObjectTweener charLeftGoTweener;
        [SerializeField] ObjectTweener charLeftSpriteTweener;
        [SerializeField] ObjectTweener charRightGoTweener;
        [SerializeField] ObjectTweener charRightSpriteTweener;
        [SerializeField] ObjectTweener containerTweener;

        DialogueSide lastLeftFaceDir, lastLeftSidePlace;
        DialogueSide lastRightFaceDir, lastRightSidePlace;
        DialogueSide whoIsTalking;  //On grise le perso qui n'est pas en train de parler
        bool rotateLeftAllowed = false, rotateRightAllowed = false; //Pour éviter de tourner les persos si c'est le tout premier sprite à apparaître

        #endregion


        #endregion



        #endregion




        #region Mono

        void Awake()
        {
            source = gameObject.AddComponent<AudioSource>();
            choicesContent.SetActive(false);
            dialogueContent.SetActive(false);

        }


        #endregion


        #region Dialogue

        public void StartDialogue()
        {
            //Quand on lance le dialogue, on active l'interface
            dialogueUI.SetActive(true);

            charLeftGoTweener.gameObject.SetActive(true);
            charRightGoTweener.gameObject.SetActive(true);
            containerTweener.gameObject.SetActive(true);
        }

        public void EndDialogue()
        {

            for (int i = 0; i < TS_CharOnDialogueEnded.Length; i++)
            {
                charLeftGoTweener.BeginTweens(TS_CharOnDialogueEnded);
                charRightGoTweener.BeginTweens(TS_CharOnDialogueEnded);
            }

            for (int i = 0; i < TS_ContainerOnDialogueEnded.Length; i++)
            {
                containerTweener.BeginTweenByCode(TS_ContainerOnDialogueEnded[i]).setOnComplete(() => dialogueUI.SetActive(false));
            }
        }




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



        public void SetText(string replique)
        {
            dialogueContent.SetActive(true);

            if (whoIsTalking == DialogueSide.Left)
            {
                charLeftSpriteTweener.BeginTweens(TS_CharOnStartTalk);
                charRightSpriteTweener.BeginTweens(TS_CharOnEndTalk);
            }
            else
            {
                charRightSpriteTweener.BeginTweens(TS_CharOnStartTalk);
                charLeftSpriteTweener.BeginTweens(TS_CharOnEndTalk);
            }

            dialogueText.text = replique;
        }


        public void SetCharacter(string characterName, Color characterNameColor, Sprite characterSprite, DialogueSide faceDir, DialogueSide sidePlacement)
        {
            if (sidePlacement == DialogueSide.Left)
            {
                //S'il n'y a pas de sprite en paramètre, on veut cacher le personnage de ce côté
                if (!characterSprite)
                {
                    charLeftGoTweener.gameObject.SetActive(false);
                    return;
                }
                else
                {
                    charLeftGoTweener.gameObject.SetActive(true); 
                    whoIsTalking = sidePlacement;   //On garde en mémoire qui est en train de parler pour griser le bon perso

                }

                characterLeftImg.sprite = characterSprite;

                //Tourner le perso
                if(lastLeftFaceDir != faceDir && rotateLeftAllowed)
                {
                    lastLeftFaceDir = faceDir; 
                    charRightSpriteTweener.BeginTweens(TS_CharOnRot);

                }

                rotateLeftAllowed = true;
            }
            else
            {
                //S'il n'y a pas de sprite en paramètre, on veut cacher le personnage de ce côté
                if (!characterSprite)
                {
                    charRightGoTweener.gameObject.SetActive(false);
                    return;
                }
                else
                {
                    charRightGoTweener.gameObject.SetActive(true);
                    whoIsTalking = sidePlacement;   //On garde en mémoire qui est en train de parler pour griser le bon perso
                }

                characterRightImg.sprite = characterSprite;

                //Tourner le perso
                if (lastRightFaceDir != faceDir && rotateRightAllowed)
                {
                    lastRightFaceDir = faceDir;
                    charRightSpriteTweener.BeginTweens(TS_CharOnRot);
                }

                rotateRightAllowed = true;
            }


            characterNameText.text = characterName;
            characterNameText.color = characterNameColor;
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


        public void PlaySound(AudioClip clip)
        {
            source.Stop();
            source.clip = clip;
            source.Play();
        }

        #endregion
    }
}