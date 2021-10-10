using Project.Enums;
using Project.ScreenFader;
using Project.Utilities.Tween;
using Project.Utilities.ValueTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Project.NodeSystem
{
    public class TweenedDialogueUI : DialogueUI
    {

        #region Fields

        #region UI Components

        [Space(10)]
        [Header("Dialogue Characters :")]
        [Space(10)]

        private Transform _container;
        private GameObject _leftCharGo;
        private GameObject _rightCharGo;
        private Image _leftCharImg;
        private Image _rightCharImg;
        private TextMeshProUGUI _leftNameText;
        private TextMeshProUGUI _rightNameText;


        [Space(10)]
        [Header("Dialogue Text :")]
        [Space(10)]

        private GameObject _dialogueContent;
        private Button _continueBtn;
        private Button _skipRepliqueBtn;
        private TextMeshProUGUI _dialogueText;


        [Space(10)]
        [Header("Dialogue Choices :")]
        [Space(10)]

        private GameObject _choicesContent;
        private Button[] _choiceBtns;
        private TextMeshProUGUI[] _choiceTmps;


        [Space(10)]
        [Header("Background :")]
        [Space(10)]

        [SerializeField, Tooltip("Le SO nous permettant de faire un fondu pour afficher le nouveau d�cor.")]
        private ScreenFaderSO _screenFader;
        [SerializeField, Tooltip("La texture de d�part pour le d�cor (un empty transparent).")]
        private Texture2D _startTex;                        


        private Image _backgroundImg;                       //L'image du d�cor, pour ex�cuter un blend sur son material.
        private Material _backgroundMat;                    //Pour acc�der au Material plus rapidement
        private BackgroundData_Transition _tmpTransition;   //Gard�e en m�moire pour assigner la texture menuellement dans SetBackgroundManually



        #endregion


        #region Audio

        [Space(10)]
        [Header("Audio :")]
        [Space(10)]

        [Tooltip("Pour chacun de ces clips, on cr�e un AudioSource qui va appeler un de ces sons � chaque caract�re inscrit (Mettre un nombre suffisant pour �viter les blancs).")]
        private int _nbCharPrintClips = 10;

        [Tooltip("Permet de passer d'un clip � un autre lors de l'�criture des caract�res.")]
        private int _curCharClipIndex = 0;

        private AudioSource _voiceClipSource;
        private AudioSource[] _charClipSources;



        #endregion



        #region Tween

        [Space(10)]
        [Header("Tween Settings :")]
        [Space(10)]


        [SerializeField, Tooltip("Les Tweens � jouer quand un perso appara�t � l'�cran (et que l'emplacement �tait libre).")]
        private TweenSettings[] TS_CharAppears;
        [SerializeField, Tooltip("Les Tweens � jouer quand un perso se retire de la conversation.")]
        private TweenSettings[] TS_CharDisappears;
        [SerializeField, Tooltip("Le Tween � jouer quand un perso doit se retourner.")]
        private TweenSettings[] TS_CharRotates, TS_CharRotatesNegative;
        [SerializeField, Tooltip("Le Tween � jouer quand un perso d�j� pr�sent commence � parler.")]
        private TweenSettings[] TS_CharTalks;
        [SerializeField, Tooltip("Le Tween � jouer quand ce n'est plus au tour du perso de parler.")]
        private TweenSettings[] TS_CharMuted;
        [SerializeField, Tooltip("Les Tweens � jouer sur les persos quand le dialogue est termin�.")]
        private TweenSettings[] TS_CharOnDialogueEnded;
        [SerializeField, Tooltip("Les Tweens � jouer sur le container quand le dialogue est termin�.")]
        private TweenSettings[] TS_ContainerOnDialogueEnded;
        [SerializeField, Tooltip("Les Tweens � jouer sur le container quand le dialogue d�marre.")]
        private TweenSettings[] TS_ContainerOnDialogueStarted;


        [Space(10)]
        [Header("Tweeners :")]
        [Space(10)]

        private ObjectTweener _leftCharGoTweener;
        private ObjectTweener _leftCharImgTweener;
        private ObjectTweener _rightCharGoTweener;
        private ObjectTweener _rightCharImgTweener;
        private ObjectTweener _containerTweener;


        #endregion



        #region Flags

        private bool _uiVisibleFlag;                            //POur savoir si l'UI est visible ou non


        #endregion

        #endregion














        #region Init

        protected override void GetComponents()
        {
            base.GetComponents();


            //Container
            _container = _dialoguePanel.transform.GetChild(3);

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



            //Background
            _backgroundImg = _dialoguePanel.transform.GetChild(0).GetComponent<Image>();
            _backgroundMat = _backgroundImg.material;


            //Character sprites & names
            _leftCharGo = _dialoguePanel.transform.GetChild(1).gameObject;
            _leftCharImg = _leftCharGo.transform.GetChild(0).GetComponent<Image>();

            _rightCharGo = _dialoguePanel.transform.GetChild(2).gameObject;
            _rightCharImg = _rightCharGo.transform.GetChild(0).GetComponent<Image>();

            //Tweeners
            _leftCharGoTweener = _leftCharGo.GetComponent<ObjectTweener>();
            _leftCharImgTweener = _leftCharImg.GetComponent<ObjectTweener>();
            _rightCharGoTweener = _rightCharGo.GetComponent<ObjectTweener>();
            _rightCharImgTweener = _rightCharImg.GetComponent<ObjectTweener>();
            _containerTweener = _container.GetComponent<ObjectTweener>();



            //Choices content
            _choicesContent = _container.GetChild(0).gameObject;
            int count = _choicesContent.transform.childCount;
            _choiceBtns = new Button[count];
            _choiceTmps = new TextMeshProUGUI[count];
            for (int i = 0; i < count; i++)
            {
                _choiceBtns[i] = _choicesContent.transform.GetChild(i).GetComponent<Button>();
                _choiceTmps[i] = _choiceBtns[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            }


            //Dialogue content
            _dialogueContent = _container.GetChild(1).gameObject;
            _dialogueText = _dialogueContent.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            _leftNameText = _dialogueContent.transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>();
            _rightNameText = _dialogueContent.transform.GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>();
            _continueBtn = _dialogueContent.transform.GetChild(3).GetComponent<Button>();
            _skipRepliqueBtn = _dialogueContent.transform.GetChild(4).GetComponent<Button>();
        }

        protected override void SetUpComponents()
        {
            //Le container sera activ� automatiquement au moment d'afficher les persos ou une r�plique
            _containerTweener.gameObject.SetActive(false);


            //Cache les persos pour les v�rifs auto du script
            _leftCharGo.SetActive(false);
            _rightCharGo.SetActive(false);


            //On emp�che la remise � z�ro auto des tweeners pour replacer les sprites nous-m�mes quand ils dispara�ssent
            _leftCharImgTweener.resetTransformOnDisable = _rightCharImgTweener.resetTransformOnDisable = false;
            _leftCharImgTweener.resetTransformOnTweenEnded = _rightCharImgTweener.resetTransformOnTweenEnded = false;


            //On assigne une texture transparent au _backgroundMat avant le d�but du dialogue
            _backgroundMat.SetTexture("_MainTex", _startTex);
            _backgroundMat.SetFloat("_Blend", 0f);
            _backgroundImg.SetMaterialDirty();

        }

        protected override void SubscribeToManager()
        {
            base.SubscribeToManager();

            //UI
            _dialogueManager.OnRunUINode += OnRunUINode;

            //Character
            _dialogueManager.OnRunStartNode += OnRunStartNode;
            _dialogueManager.OnRunCharacterNode += OnRunCharacterNode;

            //Replique
            _dialogueManager.OnRunRepliqueNode += OnRunRepliqueNode;

            //Background
            _dialogueManager.OnRunBackgroundNode += OnRunBackgroundNode;
            _screenFader.OnCompleteTransitionMiddle += SetBackgroundManually;
            _screenFader.OnTransitionEnded += () => _dialogueManager.OnTransitionEnded?.Invoke();

            //Buttons
            _dialogueManager.OnChoiceInfosSet += OnChoiceInfosSet;
            _dialogueManager.OnChoicesSet += OnChoicesSet;
            _dialogueManager.OnContinueBtnReached += OnContinueBtnReached;
        }

        protected override void UnsubscribeFromManager()
        {
            base.UnsubscribeFromManager();

            //UI
            _dialogueManager.OnRunUINode -= OnRunUINode;

            //Character
            _dialogueManager.OnRunStartNode -= OnRunStartNode;
            _dialogueManager.OnRunCharacterNode -= OnRunCharacterNode;

            //Replique
            _dialogueManager.OnRunRepliqueNode -= OnRunRepliqueNode;

            //Background
            _dialogueManager.OnRunBackgroundNode -= OnRunBackgroundNode;
            _screenFader.OnCompleteTransitionMiddle -= SetBackgroundManually;
            _screenFader.OnTransitionEnded -= () => _dialogueManager.OnTransitionEnded?.Invoke();

            //Buttons
            _dialogueManager.OnChoiceInfosSet -= OnChoiceInfosSet;
            _dialogueManager.OnChoicesSet -= OnChoicesSet;
            _dialogueManager.OnContinueBtnReached -= OnContinueBtnReached;
        }



        #endregion



        #region Display

        protected override void ResetUI()
        {
            base.ResetUI();

            _choicesContent.SetActive(false);
            _dialogueContent.SetActive(false);


            //Pour lancer les tweens des persos depuis ShowUI()
            _leftCharImg.sprite = _rightCharImg.sprite = null;

        }


        protected override void ShowUI()
        {

            _uiVisibleFlag = true;

            //Clean text for next replique
            _dialogueText.text = "";


            //On n'affiche les persos que s'ils ont d�j� un sprite
            if (_leftCharImg.sprite)
            {
                _leftCharGo.SetActive(true);
                _leftCharGoTweener.BeginTweens(TS_CharAppears);
            }
            else
            {
                _leftNameText.transform.parent.gameObject.SetActive(false);
            }
            if (_rightCharImg.sprite)
            {
                _rightCharGo.SetActive(true);
                _rightCharGoTweener.BeginTweens(TS_CharAppears);
            }
            else
            {
                _rightNameText.transform.parent.gameObject.SetActive(false);
            }

            //On active le tweener du container pour le faire appara�tre
            _containerTweener.gameObject.SetActive(true);
            _containerTweener.BeginTweens(TS_ContainerOnDialogueStarted);
        }

        //N'est pas appel�e depuis le DialogueManager si le EndNoeType = End
        //car le DialogueTrigger assigne la fonction UnloadScene qui quitte imm�diatement le dialogue
        protected override void HideUI()
        {
            _uiVisibleFlag = false;

            //Clean text for next replique
            _dialogueText.text = "";

            //Pour chaque perso, on joue le tween de fin de dialogue pour les faire dispara�tre
            //(S'il le perso n'est pas visible alors il est d�sactiv�, donc �a craint rien)
            if (_leftCharGo.activeSelf)
            {
                _leftCharGoTweener.BeginTweens(TS_CharOnDialogueEnded, () => 
                {
                    _leftCharImgTweener.ResetAlpha();  //Pour que le perso ne garde pas l'apparence "muet"
                    _leftCharGo.gameObject.SetActive(false);
                });
            }
            if (_rightCharGo.activeSelf)
            {
                _rightCharGoTweener.BeginTweens(TS_CharOnDialogueEnded, () => 
                {
                    _rightCharImgTweener.ResetAlpha();  //Pour que le perso ne garde pas l'apparence "muet"
                    _rightCharGo.gameObject.SetActive(false);
                });
            }

            //Pour le container, on ferme la fen�tre de dialogue lorsque le tween de fin du container a termin�
            _containerTweener.BeginTweens(TS_ContainerOnDialogueEnded, () => _containerTweener.gameObject.SetActive(false));
        }



        private void OnRunUINode(UIData data, Action onRunEnded)
        {
            if (data.show.Value)
            {
                if (!_uiVisibleFlag)
                {
                    ShowUI();
                }
            }
            else
            {
                if (_uiVisibleFlag)
                {
                    HideUI();
                }
            }

            StartCoroutine(DelayCo(.5f, onRunEnded));
        }

        private IEnumerator DelayCo(float delay, Action onDelayEnded)
        {
            WaitForSeconds wait = new WaitForSeconds(delay);
            yield return wait;

            onDelayEnded?.Invoke();
        }


        #endregion



        #region Buttons

        //Quand on clique sur le bouton continuer, on afficher le panel des choix 
        //s'il y a des ports, sinon, on joue la node suivante
        private void OnContinueBtnReached(UnityAction onContinueClicked)
        {
            _continueBtn.onClick.RemoveAllListeners();
            _continueBtn.onClick.AddListener(StopAllCoroutines); //Supprime la coroutine d'auto d�lai
            _continueBtn.onClick.AddListener(onContinueClicked);
        }


        //Quand l'UI doit afficher des choix, le bouton continuer appelle cette fonction
        private void OnChoicesSet()
        {

            //Si aucun choix n'est affich�, on affiche le canvas
            if (!_uiVisibleFlag)
            {
                //Pour s'assurer que les choix apparaissent une fois le canvas affich�, on 
                //passe deux fois dans OnRunUINode pour passer _uiVisibleFlag � true
                UIData uiData = new UIData();
                uiData.show.Value = true;
                OnRunUINode(uiData, () => OnChoicesSet());
                return;
            }


            StopAllCoroutines();

            _dialogueContent.SetActive(false);
            _choicesContent.SetActive(true);
        }




        private void OnChoiceInfosSet(List<DialogueButtonContainer> dialogueButtonContainers)
        {
            for (int i = 0; i < _choiceBtns.Length; i++)
            {
                Button b = _choiceBtns[i];

                //Si on a des boutons en trop, on les d�sactive
                if (i >= dialogueButtonContainers.Count)
                {
                    b.gameObject.SetActive(false);
                }
                else
                {
                    //Si les conditions sont r�unies, on active le bouton normalement
                    if (dialogueButtonContainers[i].ConditionCheck)
                    {
                        b.gameObject.SetActive(true);
                        b.interactable = true;

                        b.onClick.RemoveAllListeners();

                        //On d�sactive tous les boutons; relance les anims dans le cas o� l'on a 2 ChoiceNodes successives
                        b.onClick.AddListener(() =>
                        {
                            _choicesContent.gameObject.SetActive(false);
                        });

                        //Passe � la node suivante
                        b.onClick.AddListener(dialogueButtonContainers[i].OnChoiceClicked);
                    }
                    else
                    {
                        //On n'active le bouton que si on doit le griser, puis on met son interactable � false
                        //pour montrer au joueur le choix indisponible en gris
                        b.gameObject.SetActive(dialogueButtonContainers[i].ChoiceState == ChoiceStateType.GreyOut);
                        b.interactable = false;

                    }


                    //On affiche le texte du choix sur le bouton
                    _choiceTmps[i].text = dialogueButtonContainers[i].Text;
                }
            }
        }


        #endregion




        #region Audio


        /// <summary>
        /// Joue l'interjection li�e � la r�plique (si null, ne jouer aucune voix).
        /// </summary>
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

        /// <summary>
        /// Assigne le son d'�criture propre � chaque personnage (si null, l'�criture sera silencieuse).
        /// </summary>
        private void SetCharAudio(AudioClip charPrintClip)
        {
            for (int i = 0; i < _nbCharPrintClips; i++)
            {
                _charClipSources[i].clip = charPrintClip;
            }
        }


        #endregion





        #region Characters


        /// <summary>
        /// Abonn� au DialogueManager, appel�e quand le script analyse la StartData.
        /// </summary>
        private void OnRunStartNode()
        {
            //Comme c'est la StartNode, on se contente de cacher les sprites des persos pour ne pas avoir � le faire manuellement.

            //Cr�e un perso vide pour indiquer � DisplayCharacterAndName() qu'elle doit masquer le sprite correspondant
            CharacterData_CharacterSO nullData = new CharacterData_CharacterSO();
            nullData.CharacterName.Value = "";

            //Perso de gauche
            if (_leftCharImg.sprite)
            {
                nullData.FaceDirection.Value = DialogueSide.Right;
                nullData.SidePlacement.Value = DialogueSide.Left;
                OnRunCharacterNode(nullData);
            }

            //Perso de droite
            if (_rightCharImg.sprite)
            {
                nullData.FaceDirection.Value = DialogueSide.Left;
                nullData.SidePlacement.Value = DialogueSide.Right;
                OnRunCharacterNode(nullData);
            }

            //Remet � z�ro les flags et param�tres temporaires
            ResetUI();
        }


        private void OnRunCharacterNode(CharacterData_CharacterSO data, Action onRunEnded = null)
        {

            bool hasACharacter = data.Character.Value != null;
            string characterName = hasACharacter ? data.CharacterName.Value : "";
            Color characterNameColor = hasACharacter ? data.Character.Value.CharacterNameColor : new Color(252, 3, 252, 1); //rose fluo d�bug
            Sprite characterSprite = hasACharacter ? data.Sprite.Value : null;
            DialogueSide faceDir = data.FaceDirection.Value;
            DialogueSide sidePlacement = data.SidePlacement.Value;
            AudioClip charPrintClip = hasACharacter ? data.Character.Value.CharPrintClip : null;


            //Si aucun perso n'est affich�, on affiche le canvas
            if (!_uiVisibleFlag && hasACharacter)
            {
                //Pour s'assurer que le perso apparaisse une fois le canvas affich�, on 
                //passe deux fois dans OnRunUINode pour passer _uiVisibleFlag � true
                UIData uiData = new UIData();
                uiData.show.Value = true;
                OnRunUINode(uiData, () => OnRunCharacterNode(data, onRunEnded));
                return;
            }



            //On assigne le son d'�criture du perso en cours
            SetCharAudio(charPrintClip);

            //On change le sprite du perso dans la bonne direction
            SetCharacterSprite(characterSprite, faceDir, sidePlacement, onRunEnded);

            //On affiche le perso et son nom � l'�cran
            DisplayCharacterAndName(characterName, characterNameColor, hasACharacter, sidePlacement, onRunEnded);


        }





        private void SetCharacterSprite(Sprite newSprite, DialogueSide faceDir, DialogueSide sidePlacement, Action onRunEnded = null)
        {
            if (!newSprite) return;


            Sprite lastSprite;
            bool shouldRotate;

            if (sidePlacement == DialogueSide.Left)
            {
                lastSprite = _leftCharImg.sprite;
                _leftCharImg.sprite = newSprite;

                //Tourner le perso
                float rotY = _leftCharImg.transform.eulerAngles.y;
                bool rotYIsZero = Mathf.Approximately(rotY, 0f);
                shouldRotate = faceDir == DialogueSide.Left ^ !rotYIsZero;
                if (shouldRotate)
                {
                    //Si on doit changer de direction et que le perso est visible, on peut l'animer
                    if (_leftCharGo.activeSelf)
                    {
                        _leftCharImgTweener.BeginTweens(rotYIsZero ? TS_CharRotates : TS_CharRotatesNegative, onRunEnded);
                    }
                    //Sinon, �a veut dire que notre perso a �t� d�sactiv� ; on doit le tourner dans la bonne direction avant de l'afficher 
                    else
                    {
                        Vector3 euler = _leftCharImg.transform.eulerAngles;
                        euler.y = faceDir == DialogueSide.Left ? 180f : 0f;
                        _leftCharImg.transform.eulerAngles = euler;
                    }
                }


            }
            else
            {
                lastSprite = _rightCharImg.sprite;
                _rightCharImg.sprite = newSprite;



                //Tourner le perso
                float rotY = _rightCharImg.transform.eulerAngles.y;
                bool rotYIsZero = Mathf.Approximately(rotY, 0f);
                shouldRotate = faceDir == DialogueSide.Right ^ !rotYIsZero;
                if (shouldRotate)
                {
                    //Si on doit changer de direction et que le perso est visible, on peut l'animer
                    if (_rightCharGo.activeSelf)
                    {
                        _rightCharImgTweener.BeginTweens(rotYIsZero ? TS_CharRotates : TS_CharRotatesNegative, onRunEnded);
                    }
                    //Sinon, �a veut dire que notre perso a �t� d�sactiv� ; on doit le tourner dans la bonne direction avant de l'afficher 
                    else
                    {
                        Vector3 euler = _rightCharImg.transform.eulerAngles;
                        euler.y = faceDir == DialogueSide.Right ? 180f : 0f;
                        _rightCharImg.transform.eulerAngles = euler;
                    }
                }


            }






            if (sidePlacement == DialogueSide.Left)
            {
                //Animer le perso s'il affiche un sprite diff�rent du pr�c�dent
                if (lastSprite != _leftCharImg.sprite && !shouldRotate && _leftCharGo.activeSelf)
                {
                    _leftCharImgTweener.BeginTweens(TS_CharTalks, onRunEnded);
                }

                //Si l'autre perso est visible, on le passe en muet
                if (_rightCharImg.gameObject.activeInHierarchy)
                {
                    _rightCharImgTweener.BeginTweens(TS_CharMuted);
                }
            }
            else
            {
                //Animer le perso s'il affiche un sprite diff�rent du pr�c�dent
                if (lastSprite != _rightCharImg.sprite && !shouldRotate && _rightCharGo.activeSelf)
                {
                    _rightCharImgTweener.BeginTweens(TS_CharTalks, onRunEnded);
                }

                //Si l'autre perso est visible, on le passe en muet
                if (_leftCharImg.gameObject.activeInHierarchy)
                {
                    _leftCharImgTweener.BeginTweens(TS_CharMuted);
                }
            }
        }



        private void DisplayCharacterAndName(string characterName, Color characterNameColor, bool hasSprite, DialogueSide sidePlacement, Action onRunEnded = null)
        {

            if (sidePlacement == DialogueSide.Left)
            {
                //S'il n'y a pas de sprite en param�tre, on veut cacher le personnage de ce c�t�
                if (!hasSprite && _leftCharGo.activeSelf)
                {
                    _leftCharGoTweener.BeginTweens(TS_CharDisappears, () =>
                    {
                        onRunEnded?.Invoke();
                        _leftCharGo.gameObject.SetActive(false);
                    });

                    _leftNameText.transform.parent.gameObject.SetActive(false);
                    return;
                }
                //Sinon, on l'active
                else if (hasSprite && !_leftCharGo.activeSelf)
                {
                    _leftCharGo.gameObject.SetActive(true);
                    _leftCharGoTweener.BeginTweens(TS_CharAppears, onRunEnded);

                    //On affiche le nom de gauche
                    _leftNameText.transform.parent.gameObject.SetActive(true);
                    _rightNameText.transform.parent.gameObject.SetActive(false);
                }


            }
            else
            {
                //S'il n'y a pas de sprite en param�tre, on veut cacher le personnage de ce c�t�
                if (!hasSprite && _rightCharGo.activeSelf)
                {
                    _rightCharGoTweener.BeginTweens(TS_CharDisappears).SetOnTweensComplete(() =>
                    {
                        onRunEnded?.Invoke();
                        _rightCharGo.gameObject.SetActive(false);
                    });

                    _rightNameText.transform.parent.gameObject.SetActive(false);
                    return;
                }
                //Sinon, on l'active
                else if (hasSprite && !_rightCharGo.activeSelf)
                {
                    _rightCharGo.gameObject.SetActive(true);
                    _rightCharGoTweener.BeginTweens(TS_CharAppears, onRunEnded);

                    //On affiche le nom de droite
                    _leftNameText.transform.parent.gameObject.SetActive(false);
                    _rightNameText.transform.parent.gameObject.SetActive(true);
                }


            }

            //On change le nom du perso en question
            _leftNameText.text = _rightNameText.text = characterName;
            _leftNameText.color = _rightNameText.color = characterNameColor;

        }


        #endregion




        #region Repliques

        private void OnRunRepliqueNode(RepliqueData_Replique data, LanguageType selectedLanguage)
        {
            //Charger l'audio du perso
            SetVoiceAudio(data, selectedLanguage);

            //Affiche le canvas
            if (!_uiVisibleFlag)
            {
                //Pour s'assurer que le dialogue apparaisse une fois le canvas affich�, on 
                //passe deux fois dans OnRunUINode pour passer _uiVisibleFlag � true
                UIData uiData = new UIData();
                uiData.show.Value = true;
                OnRunUINode(uiData, () => OnRunRepliqueNode(data, selectedLanguage));
                return;
            }

            //Quand un choix est s�lectionn�, ramener la fen�tre de la r�plique
            _dialogueContent.SetActive(true);
            _choicesContent.SetActive(false);



            //Quand appendToText est � true, cliquer sur skip ajoute du texte en trop
            //Cette variable permet de remettre le texte � son �tat initial
            string previousText = _dialogueText.text;


            if (_writeCharByChar)
            {
                //Arr�te les auto d�lais
                _skipRepliqueBtn.onClick.RemoveAllListeners();   

                //On assigne au bouton skip sa fonction
                _skipRepliqueBtn.onClick.AddListener(() => WriteAllText(previousText, data, selectedLanguage)); 

                StartCoroutine(WriteRepliqueCharByCharCo(data, selectedLanguage));
            }
            else
            {
                WriteAllText(previousText, data, selectedLanguage);
            }
        }




        private void WriteAllText(string previousText, RepliqueData_Replique data, LanguageType selectedLanguage)
        {
            //On arr�te la coroutine pour �viter d'�crire � la suite de la r�plique enti�re
            StopAllCoroutines();

            //On n'est plus en train d'�crire, donc on remet les valeurs par d�faut

            _skipRepliqueBtn.gameObject.SetActive(false);
            _continueBtn.gameObject.SetActive(true);
            _isWriting = false;


            //Arr�ter le son d'�criture
            for (int i = 0; i < _charClipSources.Length; i++)
            {
                _charClipSources[i].Stop();
            }

            string replique = data.Texts.Find(text => text.Language == selectedLanguage).Data;


            if (data.AppendToText.Value)
            {
                _dialogueText.text = $"{previousText}{replique}";
            }
            else
            {
                _dialogueText.text = replique;
            }


            //Si on veut un d�lai avant la prochaine r�plique
            if (data.UseAutoDelay.Value)
            {
                StartCoroutine(DelayBeforeContinueCo(data));
            }
        }


        //Pour �crire la r�plique caract�re par caract�re � l'aide du StringBuilder
        private IEnumerator WriteRepliqueCharByCharCo(RepliqueData_Replique data, LanguageType selectedLanguage)
        {
            //Si on a le droit d'appuyer sur continuer pour passer la r�plique, on active le skipRepliqueBtn
            if (data.CanClickOnContinue.Value) _skipRepliqueBtn.gameObject.SetActive(true);


            _continueBtn.gameObject.SetActive(false);        //Pour �viter de cliquer dessus
            _isWriting = true;                               //On indique au reste du script que l'�criture a commenc�


            string replique = data.Texts.Find(text => text.Language == selectedLanguage).Data;

            int length = replique.Length;


            //Si on doit attacher le nouveau texte � l'ancien, on s'assure que le StringBuilder est assez grand
            if (data.AppendToText.Value)
            {
                _sb.EnsureCapacity(_sb.Capacity + length);
                _sb.Clear();
                _sb.Append(_dialogueText.text);   //Ajoute le texte dans le cas o� la prochaine r�plique doit �tre attach�e elle aussi
            }
            //Sinon, on doit vider le texte de dialogue, donc on r�initialise le StringBuilder
            else
            {
                _sb = new StringBuilder(length);
            }

            //Si on doit remplacer la vitesse d'�criture de base par celle de la r�plique
            WaitForSeconds wait = new WaitForSeconds(data.OverrideWriteSpeed.Value ? data.WriteSpeed.Value : _charWriteSpeed);


            for (int i = 0; i < length; i++)
            {
                //Si le son est d�j� occup�, jouer le suivant
                if (_charClipSources[_curCharClipIndex].isPlaying)
                    _curCharClipIndex.Clamp360(1, _charClipSources);

                //Joue le son d'�criture s'il y en a un
                _charClipSources[_curCharClipIndex].Play();
                _curCharClipIndex.Clamp360(1, _charClipSources);


                _sb.Append(replique[i]);
                _dialogueText.text = _sb.ToString();

                //Wait a certain amount of time, then continue with the for loop
                yield return wait;
            }



            //Arr�ter le son d'�criture
            for (int i = 0; i < _charClipSources.Length; i++)
            {
                _charClipSources[i].Stop();
            }



            _isWriting = false;

            //Si on a le droit d'appuyer sur continuer pour passer la r�plique, on active le continueBtn
            if (data.CanClickOnContinue.Value) _continueBtn.gameObject.SetActive(true);

            _skipRepliqueBtn.gameObject.SetActive(false);


            //Si on veut un d�lai avant la prochaine r�plique
            if (data.UseAutoDelay.Value)
            {
                StartCoroutine(DelayBeforeContinueCo(data));
            }
        }



        //Si on veut un d�lai avant la prochaine r�plique
        private IEnumerator DelayBeforeContinueCo(RepliqueData_Replique data)
        {
            WaitForSeconds wait = new WaitForSeconds(data.AutoDelayDuration.Value);

            yield return wait;

            _continueBtn.gameObject.SetActive(true);
            _continueBtn.onClick?.Invoke();

        }

        #endregion



        #region Background

        private void OnRunBackgroundNode(BackgroundData_Transition transition, TransitionSettingsSO startSettings, TransitionSettingsSO endSettings)
        {
            _tmpTransition = transition;

            //Si on a des param�tres pour une transition de d�part, on lance la transition sur l'UI
            //en sp�cifiant de passer � la node suivante une fois la transition termin�e
            if (startSettings)
            {

                switch (startSettings.TransitionType)
                {
                    case FaderTransitionType.TextureBlend:
                        _screenFader.StartBlend(_backgroundImg, Time.unscaledDeltaTime, transition.BackgroundTex.Value, startSettings);
                        break;
                    default:
                        if (endSettings)
                        {
                            _screenFader.StartCompleteFade(this, false, false, Time.unscaledDeltaTime, startSettings, endSettings);
                        }
                        else
                        {
                            _screenFader.StartFade(this, false, false, Time.unscaledDeltaTime, startSettings);
                        }
                        break;
                }
            }
            else
            {
                SetBackgroundManually();
                _screenFader.OnTransitionEnded?.Invoke();
            }
        }

        //OnCompleteTransitionMiddle (appel�e entre le 1er et le 2� fade)
        private void SetBackgroundManually()
        {
            _backgroundMat.SetTexture("_MainTex", _tmpTransition.BackgroundTex.Value);
            _backgroundMat.SetFloat("_Blend", 0f);
            _backgroundImg.SetMaterialDirty();
        }


        #endregion



    }
}