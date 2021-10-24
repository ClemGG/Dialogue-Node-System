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

        [SerializeField, Tooltip("Le SO nous permettant de faire un fondu pour afficher le nouveau décor.")]
        private ScreenFaderSO _screenFader;
        [SerializeField, Tooltip("La texture de départ pour le décor (un empty transparent).")]
        private Texture2D _startTex;                        


        private Image _backgroundImg;                       //The background image, stored to execute a blend on its material
        private Material _backgroundMat;                    //To access the material more quickly
        private BackgroundData_Transition _tmpTransition;   //Stored to assign the texture manually in SetBackgroundManually()



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



        #region Tween

        [Space(10)]
        [Header("Tween Settings :")]
        [Space(10)]

        [SerializeField, Tooltip("Les Tweens à jouer quand un perso apparaît à l'écran (et que l'emplacement était libre).")]
        private TweenSettings[] TS_CharAppears;
        [SerializeField, Tooltip("Les Tweens à jouer quand un perso se retire de la conversation.")]
        private TweenSettings[] TS_CharDisappears;
        [SerializeField, Tooltip("Le Tween à jouer quand un perso doit se retourner.")]
        private TweenSettings[] TS_CharRotates, TS_CharRotatesNegative;
        [SerializeField, Tooltip("Le Tween à jouer quand un perso déjà présent commence à parler.")]
        private TweenSettings[] TS_CharTalks;
        [SerializeField, Tooltip("Le Tween à jouer quand ce n'est plus au tour du perso de parler.")]
        private TweenSettings[] TS_CharMuted;
        [SerializeField, Tooltip("Les Tweens à jouer sur les persos quand le dialogue est terminé.")]
        private TweenSettings[] TS_CharOnDialogueEnded;
        [SerializeField, Tooltip("Les Tweens à jouer sur le container quand le dialogue est terminé.")]
        private TweenSettings[] TS_ContainerOnDialogueEnded;
        [SerializeField, Tooltip("Les Tweens à jouer sur le container quand le dialogue démarre.")]
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

        private bool ControllerIsConnected
        {
            get
            {
                return Input.GetJoystickNames().Length > 0;
            }
        }

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
            //Le container sera activé automatiquement au moment d'afficher les persos ou une réplique
            _containerTweener.gameObject.SetActive(false);


            //Cache les persos pour les vérifs auto du script
            _leftCharGo.SetActive(false);
            _rightCharGo.SetActive(false);


            //On empêche la remise à zéro auto des tweeners pour replacer les sprites nous-mêmes quand ils disparaîssent
            _leftCharImgTweener.resetTransformOnDisable = _rightCharImgTweener.resetTransformOnDisable = false;
            _leftCharImgTweener.resetTransformOnTweenEnded = _rightCharImgTweener.resetTransformOnTweenEnded = false;


            //On récupère toutes les caméras de la scène pour pouvoir récupérer celle en cours d'utilisation pour les transitions
            _screenFader.GetAllCamerasInScene();

            //On assigne une texture transparent au _backgroundMat avant le début du dialogue
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


            //To start the characters' tweens from ShowUI()
            _leftCharImg.sprite = _rightCharImg.sprite = null;

        }


        protected override void ShowUI(UIData data = null)
        {

            _uiVisibleFlag = true;

            //Clean text for next replique
            _dialogueText.text = "";


            //Clears sprites if we don't want the characters' transition on Show
            if (data != null && data.ClearCharSprites.Value)
            {
                _leftCharImg.sprite = _rightCharImg.sprite = null;
            }


            //We display the characters only if they have a sprite
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

            //Container appears
            _containerTweener.gameObject.SetActive(true);
            _containerTweener.BeginTweens(TS_ContainerOnDialogueStarted);
        }

        //Is not called from the DialogueManager if EndNodeType = End
        //Bc the DialogueTrigger assigns the UnloadScene() method which closes the dialogue immediately
        protected override void HideUI(UIData data = null)
        {
            _uiVisibleFlag = false;

            //Clean text for next replique
            _dialogueText.text = "";

            //Character Dissapears
            //(If the character isn't visible then he's disabled, so we risk nothing)
            if (_leftCharGo.activeSelf)
            {
                _leftCharGoTweener.BeginTweens(TS_CharOnDialogueEnded, () => 
                {
                    _leftCharImgTweener.ResetAlpha();  //So that the character doesn't stay "muted"
                    _leftCharGo.gameObject.SetActive(false);
                });
            }
            if (_rightCharGo.activeSelf)
            {
                _rightCharGoTweener.BeginTweens(TS_CharOnDialogueEnded, () => 
                {
                    _rightCharImgTweener.ResetAlpha();  //So that the character doesn't stay "muted"
                    _rightCharGo.gameObject.SetActive(false);
                });
            }

            //Once the container ends its tweening, we disable it.
            _containerTweener.BeginTweens(TS_ContainerOnDialogueEnded, () => 
            { 
                _containerTweener.gameObject.SetActive(false);


                //Clears sprites if we don't want the characters' transition on the next Show
                if (data != null && data.ClearCharSprites.Value)
                {
                    _leftCharImg.sprite = _rightCharImg.sprite = null;
                }
            });
        }



        private void OnRunUINode(UIData data, Action onRunEnded)
        {
            if (data.Show.Value)
            {
                if (!_uiVisibleFlag)
                {
                    ShowUI(data);
                }
            }
            else
            {
                if (_uiVisibleFlag)
                {
                    HideUI(data);
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

        //When the continue btn is clicked
        private void OnContinueBtnReached(UnityAction onContinueClicked)
        {
            _continueBtn.onClick.RemoveAllListeners();
            _continueBtn.onClick.AddListener(StopAllCoroutines); //Removes the auto delay Coroutine
            _continueBtn.onClick.AddListener(onContinueClicked);

            //To prevent reading the next nodes during a delay, which creates a scrambled text
            _continueBtn.onClick.AddListener(() => _continueBtn.interactable = false);
        }


        //When the UI must display choices
        private void OnChoicesSet()
        {

            //If no choice is displayed, show he canvas
            if (!_uiVisibleFlag)
            {
                //makes sure the canvas is visible first before reloading this method to enable the _choicesContent
                UIData uiData = new UIData();
                uiData.Show.Value = true;
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

                //If there are too many buttons, we disable them
                if (i >= dialogueButtonContainers.Count)
                {
                    b.gameObject.SetActive(false);
                }
                else
                {
                    //If all conditions are met, we enable the button normally
                    if (dialogueButtonContainers[i].ConditionCheck)
                    {
                        b.gameObject.SetActive(true);
                        b.interactable = true;

                        b.onClick.RemoveAllListeners();

                        //disables all buttons once the choice is selected;
                        //also restarts the animations of there are 2 consecutive ChoiceNodes
                        b.onClick.AddListener(() =>
                        {
                            _choicesContent.gameObject.SetActive(false);
                        });

                        //Go to next node
                        b.onClick.AddListener(dialogueButtonContainers[i].OnChoiceClicked);
                    }
                    else
                    {
                        //If we should grey out the button, we enable it but we set it as interactable = false
                        b.gameObject.SetActive(dialogueButtonContainers[i].ChoiceState == ChoiceStateType.GreyOut);
                        b.interactable = false;

                    }


                    //We print the text of the choice on the button
                    _choiceTmps[i].text = dialogueButtonContainers[i].Text;
                }
            }
        }


        #endregion




        #region Audio


        /// <summary>
        /// Plays the Replique field's audioclip if any
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
        /// Plays the character's printing sound if any
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
        /// Subscribed to the DialogueManager, called when the script analyzes StartData.
        /// </summary>
        private void OnRunStartNode()
        {
            //As it's the StartNode, we hide the characters here to not have to do it manually at each restart.

            //Creats an empty character to notify DisplayCharacterAndName() that it must hide the corresponding sprite
            CharacterData_CharacterSO nullData = new CharacterData_CharacterSO();
            nullData.CharacterName.Value = "";

            //Left
            if (_leftCharImg.sprite)
            {
                nullData.FaceDirection.Value = DialogueSide.Right;
                nullData.SidePlacement.Value = DialogueSide.Left;
                OnRunCharacterNode(nullData);
            }

            //Right
            if (_rightCharImg.sprite)
            {
                nullData.FaceDirection.Value = DialogueSide.Left;
                nullData.SidePlacement.Value = DialogueSide.Right;
                OnRunCharacterNode(nullData);
            }

            //Resets flags and temporary settings
            ResetUI();
        }


        private void OnRunCharacterNode(CharacterData_CharacterSO data, Action onRunEnded = null)
        {

            bool hasACharacter = data.Character.Value != null;
            string characterName = hasACharacter ? data.CharacterName.Value : "";
            Color characterNameColor = hasACharacter ? data.Character.Value.CharacterNameColor : new Color(252, 3, 252, 1); //rose fluo débug
            Sprite characterSprite = hasACharacter ? data.Sprite.Value : null;
            DialogueSide faceDir = data.FaceDirection.Value;
            DialogueSide sidePlacement = data.SidePlacement.Value;
            AudioClip charPrintClip = hasACharacter ? data.Character.Value.CharPrintClip : null;


            //makes sure the canvas is visible first before reloading this method to enable the characters
            if (!_uiVisibleFlag && hasACharacter)
            {
                UIData uiData = new UIData();
                uiData.Show.Value = true;
                OnRunUINode(uiData, () => OnRunCharacterNode(data, onRunEnded));
                return;
            }



            //Assigns char printing sound
            SetCharAudio(charPrintClip);

            //Loads the character sprite in the right direction
            SetCharacterSprite(characterSprite, faceDir, sidePlacement, onRunEnded);

            //Displays the character and his name on screen
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

                //Rotating the character
                float rotY = _leftCharImg.transform.eulerAngles.y;
                bool rotYIsZero = Mathf.Approximately(rotY, 0f);
                shouldRotate = faceDir == DialogueSide.Left ^ !rotYIsZero;
                if (shouldRotate)
                {
                    //If the direction must be change, we can animate the rotation
                    if (_leftCharGo.activeSelf)
                    {
                        _leftCharImgTweener.BeginTweens(rotYIsZero ? TS_CharRotates : TS_CharRotatesNegative, onRunEnded);
                    }
                    //Otherwise, the character is disabled, so we rotate it manually
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


                //Rotating the character
                float rotY = _rightCharImg.transform.eulerAngles.y;
                bool rotYIsZero = Mathf.Approximately(rotY, 0f);
                shouldRotate = faceDir == DialogueSide.Right ^ !rotYIsZero;
                if (shouldRotate)
                {
                    //If the direction must be change, we can animate the rotation
                    if (_rightCharGo.activeSelf)
                    {
                        _rightCharImgTweener.BeginTweens(rotYIsZero ? TS_CharRotates : TS_CharRotatesNegative, onRunEnded);
                    }
                    //Otherwise, the character is disabled, so we rotate it manually
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
                //Animates the character if it displays a new sprite
                if (lastSprite != _leftCharImg.sprite && !shouldRotate && _leftCharGo.activeSelf)
                {
                    _leftCharImgTweener.BeginTweens(TS_CharTalks, onRunEnded);
                }

                //Mutes the other character if visible
                if (_rightCharImg.gameObject.activeInHierarchy)
                {
                    _rightCharImgTweener.BeginTweens(TS_CharMuted);
                }
            }
            else
            {
                //Animates the character if it displays a new sprite
                if (lastSprite != _rightCharImg.sprite && !shouldRotate && _rightCharGo.activeSelf)
                {
                    _rightCharImgTweener.BeginTweens(TS_CharTalks, onRunEnded);
                }

                //Mutes the other character if visible
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
                //Hides this character if it has no sprite
                if (!hasSprite && _leftCharGo.activeSelf)
                {
                    _leftCharGoTweener.BeginTweens(TS_CharDisappears, () =>
                    {
                        onRunEnded?.Invoke();
                        _leftCharImg.sprite = null;
                        _leftCharGo.gameObject.SetActive(false);
                    });

                    _leftNameText.transform.parent.gameObject.SetActive(false);
                    return;
                }
                //enables it otherwise
                else if (hasSprite && !_leftCharGo.activeSelf)
                {
                    _leftCharGo.gameObject.SetActive(true);
                    _leftCharGoTweener.BeginTweens(TS_CharAppears, onRunEnded);

                    //Displays the character's name on the left container
                    _leftNameText.transform.parent.gameObject.SetActive(true);
                    _rightNameText.transform.parent.gameObject.SetActive(false);
                }


            }
            else
            {
                //Hides this character if it has no sprite

                if (!hasSprite && _rightCharGo.activeSelf)
                {
                    _rightCharGoTweener.BeginTweens(TS_CharDisappears).SetOnTweensComplete(() =>
                    {
                        onRunEnded?.Invoke();
                        _rightCharImg.sprite = null;
                        _rightCharGo.gameObject.SetActive(false);
                    });

                    _rightNameText.transform.parent.gameObject.SetActive(false);
                    return;
                }
                //enables it otherwise
                else if (hasSprite && !_rightCharGo.activeSelf)
                {
                    _rightCharGo.gameObject.SetActive(true);
                    _rightCharGoTweener.BeginTweens(TS_CharAppears, onRunEnded);

                    //Displays the character's name on the right container
                    _leftNameText.transform.parent.gameObject.SetActive(false);
                    _rightNameText.transform.parent.gameObject.SetActive(true);
                }


            }

            //Changes the name and the color of the containers
            _leftNameText.text = _rightNameText.text = characterName;
            _leftNameText.color = _rightNameText.color = characterNameColor;

        }


        #endregion




        #region Repliques

        private void OnRunRepliqueNode(RepliqueData_Replique data, LanguageType selectedLanguage)
        {
            //Load character print sound
            SetVoiceAudio(data, selectedLanguage);

            //makes sure the canvas is visible first before reloading this method to enable the _dialogueContent
            if (!_uiVisibleFlag)
            {
                
                UIData uiData = new UIData();
                uiData.Show.Value = true;
                OnRunUINode(uiData, () => OnRunRepliqueNode(data, selectedLanguage));
                return;
            }

            _dialogueContent.SetActive(true);
            _choicesContent.SetActive(false);


            //When appendToText = true, clicking on skip adds too much text
            //This variable restores the text to its initial value
            string previousText = _dialogueText.text;


            if (_writeCharByChar)
            {
                //Stops all auto delays
                _skipRepliqueBtn.onClick.RemoveAllListeners();
                _continueBtn.interactable = true;

                //Assign the skip method to the skip button
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
            //Stop writing Coroutine
            StopAllCoroutines();

            _skipRepliqueBtn.gameObject.SetActive(false);
            _continueBtn.gameObject.SetActive(true);
            _continueBtn.interactable = true;
            if(ControllerIsConnected) _continueBtn.Select();    //if controller is connected, select the button

            _isWriting = false;


            //Stop printing sound
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

            //If we want a delay before the next replique
            if (data.UseAutoDelay.Value)
            {
                StartCoroutine(DelayBeforeContinueCo(data));
            }
        }


        //Write the dialogue line caracter by caracter using the StringBuilder
        private IEnumerator WriteRepliqueCharByCharCo(RepliqueData_Replique data, LanguageType selectedLanguage)
        {
            //Activates the skip button
            if (data.CanClickOnContinue.Value) 
            {
                _skipRepliqueBtn.gameObject.SetActive(true);
                if (ControllerIsConnected) _skipRepliqueBtn.Select();   //if controller is connected, select the button
            }


            _continueBtn.gameObject.SetActive(false);        
            _isWriting = true;                               


            string replique = data.Texts.Find(text => text.Language == selectedLanguage).Data;
            int length = replique.Length;


            //Makes sure the StringBuilder is large enough
            if (data.AppendToText.Value)
            {
                _sb.EnsureCapacity(_sb.Capacity + length);
                _sb.Clear();
                _sb.Append(_dialogueText.text);   //Adds the previous text in cas the new text must be appended to it
            }
            //Otherwise, we must empty the text field, so we reset the StringBuilder
            else
            {
                _sb = new StringBuilder(length);
            }

            //Overwrites writing speed if needed
            WaitForSeconds wait = new WaitForSeconds(data.OverrideWriteSpeed.Value ? data.WriteSpeed.Value : _charWriteSpeed);


            for (int i = 0; i < length; i++)
            {
                if (_charClipSources[_curCharClipIndex].isPlaying)
                    _curCharClipIndex.Clamp360(1, _charClipSources);

                //Plays the printing sound if any
                _charClipSources[_curCharClipIndex].Play();
                _curCharClipIndex.Clamp360(1, _charClipSources);


                _sb.Append(replique[i]);
                _dialogueText.text = _sb.ToString();

                //Wait a certain amount of time, then continue with the for loop
                yield return wait;
            }



            //We are done writing, so we stop the printing sound
            for (int i = 0; i < _charClipSources.Length; i++)
            {
                _charClipSources[i].Stop();
            }



            _isWriting = false;

            //enables continueBtn
            if (data.CanClickOnContinue.Value) 
            {
                _continueBtn.gameObject.SetActive(true);
                _continueBtn.interactable = true;
                if (ControllerIsConnected) _continueBtn.Select();   //if controller is connected, select the button
            }

            _skipRepliqueBtn.gameObject.SetActive(false);


            //If we want a delay before the next replique
            if (data.UseAutoDelay.Value)
            {
                StartCoroutine(DelayBeforeContinueCo(data));
            }
        }



        //If we want a delay before the next replique
        private IEnumerator DelayBeforeContinueCo(RepliqueData_Replique data)
        {
            WaitForSeconds wait = new WaitForSeconds(data.AutoDelayDuration.Value);

            yield return wait;

            _continueBtn.gameObject.SetActive(true);
            _continueBtn.interactable = true;
            _continueBtn.onClick?.Invoke();

        }

        #endregion



        #region Background

        private void OnRunBackgroundNode(BackgroundData_Transition transition, TransitionSettingsSO startSettings, TransitionSettingsSO endSettings)
        {
            //To avoid going to the next node during a transition
            _continueBtn.gameObject.SetActive(false);

            _tmpTransition = transition;

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

        //Changes the background texture (called manually or in OnCompleteTransitionMiddle)
        private void SetBackgroundManually()
        {
            _backgroundMat.SetTexture("_MainTex", _tmpTransition.BackgroundTex.Value);
            _backgroundMat.SetFloat("_Blend", 0f);
            _backgroundImg.SetMaterialDirty();
        }


        #endregion



    }
}