using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Collections;
using System.Text;


using Project.Utilities.Tween;
using static Project.Utilities.Tween.TweenUtilities;
using static Project.Utilities.ValueTypes.Arrays;
using Project.ScreenFader;
using Project.Enums;

namespace Project.NodeSystem
{
    public class TweenDialogueUI : DialogueUI
    {

        #region Fields


        #region Tween Settings


        [Space(10)]
        [Header("Tween Settings :")]
        [Space(10)]



        [SerializeField, Tooltip("Les Tweens à jouer quand un perso apparaît à l'écran (et que l'emplacement était libre).")]
        TweenSettings[] TS_CharAppears;

        [SerializeField, Tooltip("Les Tweens à jouer quand un perso se retire de la conversation.")]
        TweenSettings[] TS_CharDisappears;




        [SerializeField, Tooltip("Le Tween à jouer quand un perso doit se retourner.")]
        TweenSettings[] TS_CharRotates;

        [SerializeField, Tooltip("Le Tween à jouer quand un perso doit se retourner.")]
        TweenSettings[] TS_CharRotatesNegative;

        [SerializeField, Tooltip("Le Tween à jouer quand un perso déjà présent commence à parler.")]
        TweenSettings[] TS_CharTalks;

        [SerializeField, Tooltip("Le Tween à jouer quand ce n'est plus au tour du perso de parler.")]
        TweenSettings[] TS_CharMuted;




        [SerializeField, Tooltip("Les Tweens à jouer sur les persos quand le dialogue est terminé.")]
        TweenSettings[] TS_CharOnDialogueEnded;

        [SerializeField, Tooltip("Les Tweens à jouer sur le container quand le dialogue est terminé.")]
        TweenSettings[] TS_ContainerOnDialogueEnded;


        #endregion


        #region Tweeners

        [Space(10)]
        [Header("Tweeners :")]
        [Space(10)]

        ObjectTweener m_leftCharGoTweener;
        ObjectTweener m_leftCharImgTweener;
        ObjectTweener m_rightCharGoTweener;
        ObjectTweener m_rightCharImgTweener;
        ObjectTweener m_containerTweener;

        DialogueSide? _whoIsTalking;  //On grise le perso qui n'est pas en train de parler
        Vector2 _leftStartPos, _rightStartPos;

        #endregion



        #region Background

        [Space(10)]
        [Header("Background :")]
        [Space(10)]

        [SerializeField, Tooltip("Le SO nous permettant de faire un fondu pour afficher le nouveau décor.")]
        ScreenFaderSO m_screenFader;

        [SerializeField, Tooltip("La texture de départ pour le décor (un empty transparent).")]
        Texture2D m_startTex;


        Image _backgroundImg;                       //L'image du décor, pour exécuter un blend sur son material.
        Material _backgroundMat;                    //Pour accéder au Material plus rapidement
        BackgroundData_Transition _tmpTransition;   //Gardée en mémoire pour assigner la texture menuellement dans SetBackgroundManually

        #endregion




        #region Dialogue Characters

        [Space(10)]
        [Header("Dialogue Characters :")]
        [Space(10)]

        protected GameObject m_leftCharGo;
        protected GameObject m_rightCharGo;
        protected Image m_leftCharImg;
        protected Image m_rightCharImg;
        protected TextMeshProUGUI m_leftNameText;
        protected TextMeshProUGUI m_rightNameText;

        #endregion



        #region Dialogue Text

        [Space(10)]
        [Header("Dialogue Text :")]
        [Space(10)]

        GameObject m_dialogueContent;
        GameObject m_continueBtnText;
        Button m_continueBtn;
        Button m_skipRepliqueBtn;
        TextMeshProUGUI m_dialogueText;

        #endregion



        #region Dialogue Choices

        [Space(10)]
        [Header("Dialogue Choices :")]
        [Space(10)]

        GameObject m_choicesContent;

        Button[] m_choiceBtns;   //Rempli une seule fois par code pour y accéder plus rapidement
        TextMeshProUGUI[] m_choiceTmps;   //Rempli une seule fois par code pour y accéder plus rapidement

        #endregion


        #region Audio

        [Space(10)]
        [Header("Audio :")]
        [Space(10)]

        [Tooltip("Pour chacun de ces clips, on crée un AudioSource qui va appeler un de ces sons à chaque caractère inscrit (Mettre un nombre suffisant pour éviter les blancs).")]
        private int _nbCharPrintClips = 10;

        [Tooltip("Permet de passer d'un clip à un autre lors de l'écriture des caractères.")]
        private int _curCharClipIndex = 0;

        private AudioSource _voiceClipSource;
        private AudioSource[] _charClipSources;



        #endregion


        #endregion




        #region Mono

        protected override void Awake()
        {
            base.Awake();


            m_choicesContent.SetActive(false);
            m_dialogueContent.SetActive(false);

            //On garde les positions des sprites en mémoire pour les réinitialiser au prochain dialogue
            _leftStartPos = m_leftCharGo.transform.position;
            _rightStartPos = m_rightCharGo.transform.position;

            //Cache les persos pour les vérifs auto du script
            m_leftCharGo.SetActive(false);
            m_rightCharGo.SetActive(false);

            //On empêche la remise à zéro auto des tweeners pour replacer les sprites nous-mêmes quand ils disparaîssent
            m_leftCharImgTweener.resetTransformOnDisable = m_rightCharImgTweener.resetTransformOnDisable = false;


            //On assigne une texture transparent au _backgroundMat avant le début du dialogue
            _backgroundMat.SetTexture("_MainTex", m_startTex);
            _backgroundMat.SetFloat("_Blend", 0f);

        }



        #endregion




        #region Init

        protected override void GetComponents()
        {
            base.GetComponents();


            //Voice audio
            _voiceClipSource = gameObject.AddComponent<AudioSource>();
            _voiceClipSource.volume = .2f;
            _voiceClipSource.playOnAwake = false;


            //Char print audio
            _charClipSources = new AudioSource[_nbCharPrintClips];
            for (int i = 0; i < _nbCharPrintClips; i++)
            {
                _charClipSources[i] = gameObject.AddComponent<AudioSource>();
                _charClipSources[i].volume = .2f;
                _charClipSources[i].playOnAwake = false;
            }


            //Character sprites & names
            m_leftCharGo = T.Find("character left").gameObject;
            m_leftCharImg = T.Find("character left sprite").GetComponent<Image>();
            m_leftNameText = T.Find("left name text").GetComponent<TextMeshProUGUI>();

            m_rightCharGo = T.Find("character right").gameObject;
            m_rightCharImg = T.Find("character right sprite").GetComponent<Image>();
            m_rightNameText = T.Find("right name text").GetComponent<TextMeshProUGUI>();


            //Tweeners
            m_leftCharGoTweener = m_leftCharGo.GetComponent<ObjectTweener>();
            m_leftCharImgTweener = m_leftCharImg.GetComponent<ObjectTweener>();
            m_rightCharGoTweener = m_rightCharGo.GetComponent<ObjectTweener>();
            m_rightCharImgTweener = m_rightCharImg.GetComponent<ObjectTweener>();
            m_containerTweener = T.Find("container").GetComponent<ObjectTweener>();


            //Background
            _backgroundImg = T.Find("background img").GetComponent<Image>();
            _backgroundMat = _backgroundImg.material;

            //Dialogue content
            m_dialogueContent = T.Find("dialogue content").gameObject;
            m_continueBtn = T.Find("continue btn").GetComponent<Button>();
            m_continueBtnText = m_continueBtn.transform.GetChild(0).gameObject;
            m_skipRepliqueBtn = T.Find("skip replique btn").GetComponent<Button>();
            m_dialogueText = T.Find("dialogue text").GetComponent<TextMeshProUGUI>();


            //Choices content
            m_choicesContent = T.Find("choices content").gameObject;
            int count = m_choicesContent.transform.childCount;
            m_choiceBtns = new Button[count];
            m_choiceTmps = new TextMeshProUGUI[count];
            for (int i = 0; i < count; i++)
            {
                m_choiceBtns[i] = m_choicesContent.transform.GetChild(i).GetComponent<Button>();
                m_choiceTmps[i] = m_choiceBtns[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            }
        }

        protected override void SubscribeToManager()
        {
            base.SubscribeToManager();

            //Character
            _dialogueManager.OnRunStartNode += HideCharacterOnStart;
            _dialogueManager.OnRunCharacterNode += SetCharacter;

            //Replique
            _dialogueManager.OnRunRepliqueNode += SetText;
            _dialogueManager.OnRunRepliqueNode += SetVoiceAudio;

            //Background
            _dialogueManager.OnRunBackgroundNode += SetBackground;
            m_screenFader.OnCompleteTransitionMiddle += SetBackgroundManually;
            m_screenFader.OnTransitionEnded += _dialogueManager.OnTransitionEnded;

            //Buttons
            _dialogueManager.OnChoiceInfosSet += SetChoices;
            _dialogueManager.OnChoicesSet += ShowChoicesPanel;
            _dialogueManager.OnContinueBtnReached += SetContinueBtn;
        }

        protected override void UnsubscribeFromManager()
        {
            base.UnsubscribeFromManager();

            //Character
            _dialogueManager.OnRunStartNode -= HideCharacterOnStart;
            _dialogueManager.OnRunCharacterNode -= SetCharacter;

            //Replique
            _dialogueManager.OnRunRepliqueNode -= SetText;
            _dialogueManager.OnRunRepliqueNode -= SetVoiceAudio;

            //Background
            _dialogueManager.OnRunBackgroundNode -= SetBackground;
            m_screenFader.OnCompleteTransitionMiddle -= SetBackgroundManually;
            m_screenFader.OnTransitionEnded -= _dialogueManager.OnTransitionEnded;

            //Buttons
            _dialogueManager.OnChoiceInfosSet -= SetChoices;
            _dialogueManager.OnChoicesSet -= ShowChoicesPanel;
            _dialogueManager.OnContinueBtnReached -= SetContinueBtn;
        }


        //Ramène les valeurs internes à leur défaut pour la prochaine lecture d'un dialogue
        protected override void ResetUI()
        {
            _whoIsTalking = null;
            m_leftCharImgTweener.Cg.alpha = m_leftCharImgTweener.Cg.alpha = 1f;
            m_leftCharGo.transform.position = _leftStartPos;
            m_rightCharGo.transform.position = _rightStartPos;
            m_leftCharImg.transform.rotation = m_rightCharImg.transform.rotation = Quaternion.identity;
        }

        protected override void ShowUI()
        {
            base.ShowUI();

            //Clean text for next replique
            m_dialogueText.text = "";


            //On active le tweener du container pour le faire apparaître
            m_containerTweener.gameObject.SetActive(true);

            //On ramène les positions et rotations des persos à zéro                    
            m_leftCharImg.transform.rotation = m_rightCharImg.transform.rotation = Quaternion.identity;

        }


        protected override void HideUI(bool endDialogue = false)
        {
            //Pour chaque perso, on joue le tween de fin de dialogue pour les faire disparaître
            //(S'il le perso n'est pas visible alors il est désactivé, donc ça craint rien)
            for (int i = 0; i < TS_CharOnDialogueEnded.Length; i++)
            {
                m_leftCharGoTweener.BeginTweens(TS_CharOnDialogueEnded);
                m_rightCharGoTweener.BeginTweens(TS_CharOnDialogueEnded);
            }

            //Pour le container, on ferme la fenêtre de dialogue lorsque le tween de fin du container a terminé
            for (int i = 0; i < TS_ContainerOnDialogueEnded.Length; i++)
            {
                m_containerTweener.BeginTweenByCode(TS_ContainerOnDialogueEnded[i]).setOnComplete(() =>
                {
                    m_leftCharImg.transform.rotation = m_rightCharImg.transform.rotation = Quaternion.identity;
                    base.HideUI(endDialogue);
                });
            }
        }

        #endregion




        #region Buttons

        //Quand on clique sur le bouton continuer, on afficher le panel des choix 
        //s'il y a des ports, sinon, on joue la node suivante
        private void SetContinueBtn(UnityAction onContinueClicked)
        {
            m_continueBtn.onClick.RemoveAllListeners();
            m_continueBtn.onClick.AddListener(StopAllCoroutines); //Supprime la coroutine d'auto délai
            m_continueBtn.onClick.AddListener(onContinueClicked);
        }


        //Quand l'UI doit afficher des choix, le bouton continuer appelle cette fonction
        private void ShowChoicesPanel()
        {
            StopAllCoroutines();

            m_dialogueContent.SetActive(false);
            m_choicesContent.SetActive(true);
        }




        private void SetChoices(List<DialogueButtonContainer> dialogueButtonContainers)
        {
            m_choicesContent.SetActive(true);

            for (int i = 0; i < m_choiceBtns.Length; i++)
            {
                Button b = m_choiceBtns[i];

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

                        //b.onClick.AddListener(OnChoiceSelected);
                        //On désactive tous les boutons; le choiceContainer sera désactivé quand la prochaine réplique sera jouée
                        b.onClick.AddListener(() =>
                        {
                            for (int j = 0; j < m_choiceBtns.Length; j++)
                            {
                                m_choiceBtns[j].gameObject.SetActive(false);
                            }
                        });

                        //On affiche le texte du choix sur le bouton
                        m_choiceTmps[i].text = dialogueButtonContainers[i].Text;
                    }
                    else
                    {
                        //On n'active le bouton que si on doit le griser, puis on met son interactable à false
                        //pour montrer au joueur le choix indisponible en gris
                        b.gameObject.SetActive(dialogueButtonContainers[i].ChoiceState == ChoiceStateType.GreyOut);
                        b.interactable = false;

                        //On affiche le texte du choix sur le bouton
                        m_choiceTmps[i].text = dialogueButtonContainers[i].Text;
                    }


                }
            }
        }


        #endregion




        #region Background

        private void SetBackground(BackgroundData_Transition transition, TransitionSettingsSO startSettings, TransitionSettingsSO endSettings)
        {
            _tmpTransition = transition;

            switch (startSettings.TransitionType)
            {
                case FaderTransitionType.TextureBlend:
                    m_screenFader.StartBlend(_backgroundImg, Time.unscaledDeltaTime, transition.BackgroundTex.Value, startSettings);
                    break;
                default:
                    if (endSettings)
                    {
                        m_screenFader.StartCompleteFade(this, false, false, Time.unscaledDeltaTime, startSettings, endSettings);
                    }
                    else
                    {
                        m_screenFader.StartFade(this, false, false, Time.unscaledDeltaTime, startSettings);
                    }
                    break;
            }

        }

        //OnCompleteTransitionPaused (appelée entre le 1er et le 2è fade)
        private void SetBackgroundManually()
        {
            _backgroundMat.SetTexture("_MainTex", _tmpTransition.BackgroundTex.Value);
            _backgroundMat.SetFloat("_Blend", 0f);
            _backgroundImg.SetMaterialDirty();
        }


        #endregion




        #region Repliques

        private void SetVoiceAudio(RepliqueData_Replique data, LanguageType selectedLanguage)
        {
            AudioClip clip = data.AudioClips.Find(text => text.Language == selectedLanguage).Data;
            if (clip)
            {
                _voiceClipSource.Stop();
                _voiceClipSource.clip = clip;
                _voiceClipSource.Play();
            }
        }

        private void SetText(RepliqueData_Replique data, LanguageType selectedLanguage)
        {

            //Quand un choix est sélectionné, ramener la fenêtre de la réplique
            m_dialogueContent.SetActive(true);
            m_choicesContent.SetActive(false);

            //Arrête les auto délais
            m_skipRepliqueBtn.onClick.RemoveAllListeners();

            //Quand appendToText est à true, cliquer sur skip ajoute du texte en trop
            //Cette variable permet de remettre le texte à son état initial
            string previousText = m_dialogueText.text;


            if (m_writeCharByChar)
            {
                m_skipRepliqueBtn.onClick.AddListener(() => WriteAllText(previousText, data, selectedLanguage));    //On assigne au bouton skip sa fonction
                StartCoroutine(WriteRepliqueCharByCharCo(data, selectedLanguage));
            }
            else
            {
                WriteAllText(previousText, data, selectedLanguage);
            }
        }




        private void WriteAllText(string previousText, RepliqueData_Replique data, LanguageType selectedLanguage)
        {
            //On arrête la coroutine pour éviter d'écrire à la suite de la réplique entière
            StopAllCoroutines();

            //On n'est plus en train d'écrire, donc on remet les valeurs par défaut

            m_skipRepliqueBtn.gameObject.SetActive(false);
            m_continueBtnText.SetActive(true);
            m_continueBtn.gameObject.SetActive(true);
            _isWriting = false;


            //Arrêter le son d'écriture
            for (int i = 0; i < _charClipSources.Length; i++)
            {
                _charClipSources[i].Stop();
            }

            string replique = data.Texts.Find(text => text.Language == selectedLanguage).Data;


            if (data.AppendToText.Value)
            {
                m_dialogueText.text = $"{previousText}{replique}";
            }
            else
            {
                m_dialogueText.text = replique;
            }


            //Si on veut un délai avant la prochaine réplique
            if (data.UseAutoDelay.Value)
            {
                StartCoroutine(DelayBeforeContinueCo(data));
            }
        }


        //Pour écrire la réplique caractère par caractère à l'aide du StringBuilder
        private IEnumerator WriteRepliqueCharByCharCo(RepliqueData_Replique data, LanguageType selectedLanguage)
        {
            //Si on a le droit d'appuyer sur continuer pour passer la réplique, on active le skipRepliqueBtn
            if (data.CanClickOnContinue.Value) m_skipRepliqueBtn.gameObject.SetActive(true);


            m_continueBtnText.SetActive(false);    //Désactive le texte du continueBtn
            m_continueBtn.gameObject.SetActive(false);        //Pour éviter de cliquer dessus
            _isWriting = true;                               //On indique au reste du script que l'écriture a commencé


            string replique = data.Texts.Find(text => text.Language == selectedLanguage).Data;

            int length = replique.Length;


            //Si on doit attacher le nouveau texte à l'ancien, on s'assure que le StringBuilder est assez grand
            if (data.AppendToText.Value)
            {
                _sb.EnsureCapacity(_sb.Capacity + length);
                _sb.Clear();
                _sb.Append(m_dialogueText.text);   //Ajoute le texte dans le cas où la prochaine réplique doit être attachée elle aussi
            }
            //Sinon, on doit vider le texte de dialogue, donc on réinitialise le StringBuilder
            else
            {
                _sb = new StringBuilder(length);
            }

            //Si on doit remplacer la vitesse d'écriture de base par celle de la réplique
            WaitForSeconds wait = new WaitForSeconds(data.OverrideWriteSpeed.Value ? data.WriteSpeed.Value : m_charWriteSpeed);


            for (int i = 0; i < length; i++)
            {
                //Si le son est déjà occupé, jouer le suivant
                if (_charClipSources[_curCharClipIndex].isPlaying)
                    _curCharClipIndex.Clamp360(1, _charClipSources);

                //Joue le son d'écriture s'il y en a un
                _charClipSources[_curCharClipIndex].Play();
                _curCharClipIndex.Clamp360(1, _charClipSources);


                _sb.Append(replique[i]);
                m_dialogueText.text = _sb.ToString();

                //Wait a certain amount of time, then continue with the for loop
                yield return wait;
            }



            //Arrêter le son d'écriture
            for (int i = 0; i < _charClipSources.Length; i++)
            {
                _charClipSources[i].Stop();
            }



            _isWriting = false;

            //Si on a le droit d'appuyer sur continuer pour passer la réplique, on active le continueBtn
            if (data.CanClickOnContinue.Value) m_continueBtn.gameObject.SetActive(true);

            m_continueBtnText.SetActive(true);
            m_skipRepliqueBtn.gameObject.SetActive(false);


            //Si on veut un délai avant la prochaine réplique
            if (data.UseAutoDelay.Value)
            {
                StartCoroutine(DelayBeforeContinueCo(data));
            }
        }



        //Si on veut un délai avant la prochaine réplique
        private IEnumerator DelayBeforeContinueCo(RepliqueData_Replique data)
        {
            WaitForSeconds wait = new WaitForSeconds(data.AutoDelayDuration.Value);

            yield return wait;

            m_continueBtn.gameObject.SetActive(true);
            m_continueBtn.onClick?.Invoke();

        }

        #endregion




        #region Characters


        /// <summary>
        /// Abonné au DialogueManager, appelée quand le script analyse la StartData.
        /// </summary>
        private void HideCharacterOnStart()
        {
            //Comme c'est la StartNode, on se contente de cacher les sprites des persos pour ne pas avoir à le faire manuellement.
            SetCharacter("", DialogueSide.Right, DialogueSide.Left);    //Perso de gauche
            SetCharacter("", DialogueSide.Left, DialogueSide.Right);    //Perso de droite

        }

        /// <summary>
        /// Appelé par HideCharacterOnStart() pour cacher le perso demandé. Renvoie un perso vide
        /// </summary>
        /// <param name="characterName"></param>
        /// <param name="faceDir"></param>
        /// <param name="sidePlacement"></param>
        private void SetCharacter(string characterName, DialogueSide faceDir, DialogueSide sidePlacement)
        {
            //Crée un perso vide pour indiquer à DisplayCharacterAndName() qu'elle doit masquer le sprite correspondant
            CharacterData_CharacterSO nullData = new CharacterData_CharacterSO();
            nullData.CharacterName.Value = characterName;
            nullData.FaceDirection.Value = faceDir;
            nullData.SidePlacement.Value = sidePlacement;

            SetCharacter(nullData);
        }


        private void SetCharacter(CharacterData_CharacterSO data)
        {
            bool hasACharacter = data.Character.Value != null;

            string characterName = hasACharacter ? data.CharacterName.Value : "";
            Color characterNameColor = hasACharacter ? data.Character.Value.CharacterNameColor : new Color(252, 3, 252, 1);
            Sprite characterSprite = hasACharacter ? data.Sprite.Value : null;
            DialogueSide faceDir = data.FaceDirection.Value;
            DialogueSide sidePlacement = data.SidePlacement.Value;
            AudioClip charPrintClip = hasACharacter ? data.Character.Value.CharPrintClip : null;

            //On assigne le son d'écriture du perso en cours
            SetCharClip(charPrintClip);

            //On change le sprite du perso dans la bonne direction
            if (characterSprite) OnCharacterSet(characterName, characterSprite, faceDir, sidePlacement);

            //On affiche le perso et son nom à l'écran
            DisplayCharacterAndName(characterName, characterNameColor, characterSprite, faceDir, sidePlacement);


        }




        /// <summary>
        /// Assigne un son d'écriture propre à chaque personnage (si null, l'écriture sera silencieuse)
        /// </summary>
        /// <param name="charPrintClip"></param>
        private void SetCharClip(AudioClip charPrintClip)
        {
            for (int i = 0; i < _nbCharPrintClips; i++)
            {
                _charClipSources[i].clip = charPrintClip;
            }
        }


        private void DisplayCharacterAndName(string characterName, Color characterNameColor, Sprite characterSprite, DialogueSide faceDir, DialogueSide sidePlacement)
        {
            if (sidePlacement == DialogueSide.Left)
            {
                //S'il n'y a pas de sprite en paramètre, on veut cacher le personnage de ce côté
                if (!characterSprite && m_leftCharGo.activeSelf)
                {
                    m_leftCharGoTweener.BeginTweens(TS_CharDisappears).SetOnTweensComplete(() => m_leftCharGo.gameObject.SetActive(false));
                    m_leftNameText.transform.parent.gameObject.SetActive(false);
                    return;
                }
                //Sinon, on l'active
                else if (characterSprite && !m_leftCharGo.activeSelf)
                {
                    m_leftCharGo.gameObject.SetActive(true);
                    m_leftCharGoTweener.BeginTweens(TS_CharAppears);

                    //On change le nom du perso en question
                    m_leftNameText.transform.parent.gameObject.SetActive(true);
                    m_rightNameText.transform.parent.gameObject.SetActive(false);


                }


            }
            else
            {
                //S'il n'y a pas de sprite en paramètre, on veut cacher le personnage de ce côté
                if (!characterSprite && m_rightCharGo.activeSelf)
                {
                    m_rightCharGoTweener.BeginTweens(TS_CharDisappears).SetOnTweensComplete(() => m_rightCharGo.gameObject.SetActive(false));
                    m_rightNameText.transform.parent.gameObject.SetActive(false);
                    return;
                }
                //Sinon, on l'active
                else if (characterSprite && !m_rightCharGo.activeSelf)
                {
                    m_rightCharGo.gameObject.SetActive(true);
                    m_rightCharGoTweener.BeginTweens(TS_CharAppears);

                    //On change le nom du perso en question
                    m_leftNameText.transform.parent.gameObject.SetActive(false);
                    m_rightNameText.transform.parent.gameObject.SetActive(true);
                }


            }


            m_leftNameText.text = m_rightNameText.text = characterName;
            m_leftNameText.color = m_rightNameText.color = characterNameColor;
        }

        private void OnCharacterSet(string characterName, Sprite characterSprite, DialogueSide faceDir, DialogueSide sidePlacement)
        {
            _whoIsTalking = sidePlacement;
            Sprite lastSprite;
            bool shouldRotate;

            if (sidePlacement == DialogueSide.Left)
            {
                lastSprite = m_leftCharImg.sprite;
                m_leftCharImg.sprite = characterSprite;

                //Tourner le perso
                float rotY = m_leftCharImg.transform.eulerAngles.y;
                bool rotYIsZero = Mathf.Approximately(rotY, 0f);
                shouldRotate = faceDir == DialogueSide.Left ^ !rotYIsZero;
                if (shouldRotate)
                {
                    //Si on doit changer de direction et que le perso est visible, on peut l'animer
                    if (m_leftCharGo.activeSelf)
                    {
                        m_leftCharImgTweener.BeginTweens(rotYIsZero ? TS_CharRotates : TS_CharRotatesNegative);
                    }
                    //Sinon, ça veut dire que notre perso a été désactivé ; on doit le tourner dans la bonne direction avant de l'afficher 
                    else
                    {
                        Vector3 euler = m_leftCharImg.transform.eulerAngles;
                        euler.y = faceDir == DialogueSide.Left ? 180f : 0f;
                        m_leftCharImg.transform.eulerAngles = euler;
                    }
                }


            }
            else
            {
                lastSprite = m_rightCharImg.sprite;
                m_rightCharImg.sprite = characterSprite;



                //Tourner le perso
                float rotY = m_rightCharImg.transform.eulerAngles.y;
                bool rotYIsZero = Mathf.Approximately(rotY, 0f);
                shouldRotate = faceDir == DialogueSide.Right ^ !rotYIsZero;
                if (shouldRotate)
                {
                    //Si on doit changer de direction et que le perso est visible, on peut l'animer
                    if (m_rightCharGo.activeSelf)
                    {
                        m_rightCharImgTweener.BeginTweens(rotYIsZero ? TS_CharRotates : TS_CharRotatesNegative);
                    }
                    //Sinon, ça veut dire que notre perso a été désactivé ; on doit le tourner dans la bonne direction avant de l'afficher 
                    else
                    {
                        Vector3 euler = m_rightCharImg.transform.eulerAngles;
                        euler.y = faceDir == DialogueSide.Right ? 180f : 0f;
                        m_rightCharImg.transform.eulerAngles = euler;
                    }
                }


            }






            if (_whoIsTalking == DialogueSide.Left)
            {
                //Animer le perso s'il affiche un sprite différent du précédent
                if (lastSprite != m_leftCharImg.sprite && !shouldRotate)
                    m_leftCharImgTweener.BeginTweens(TS_CharTalks);

                m_rightCharImgTweener.BeginTweens(TS_CharMuted);
            }
            else
            {
                //Animer le perso s'il affiche un sprite différent du précédent
                if (lastSprite != m_rightCharImg.sprite && !shouldRotate)
                    m_rightCharImgTweener.BeginTweens(TS_CharTalks);

                m_leftCharImgTweener.BeginTweens(TS_CharMuted);
            }
        }


        #endregion

    }
}