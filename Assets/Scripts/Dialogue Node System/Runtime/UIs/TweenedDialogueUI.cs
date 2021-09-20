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
        private ObjectTweener _leftNameContainerTweener;
        private ObjectTweener _rightCharGoTweener;
        private ObjectTweener _rightCharImgTweener;
        private ObjectTweener _rightNameContainerTweener;
        private ObjectTweener _containerTweener;


        #endregion



        #region Flags

        private DialogueSide? _whoIsTalking;                    //On grise le perso qui n'est pas en train de parler
        private bool _leftCharIsVisible, _rightCharIsVisible;   //Pour savoir si les persos sont affich�s � l'�cran ou non
        private bool _uiVisibleFlag;                            //POur savoir si l'UI est visible ou non


        #endregion

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
            _leftCharGo = GameObject.Find("character left").gameObject;
            _leftCharImg = GameObject.Find("character left img").GetComponent<Image>();
            _leftNameText = GameObject.Find("left name text").GetComponent<TextMeshProUGUI>();

            _rightCharGo = GameObject.Find("character right").gameObject;
            _rightCharImg = GameObject.Find("character right img").GetComponent<Image>();
            _rightNameText = GameObject.Find("right name text").GetComponent<TextMeshProUGUI>();


            //Tweeners
            _leftCharGoTweener = _leftCharGo.GetComponent<ObjectTweener>();
            _leftCharImgTweener = _leftCharImg.GetComponent<ObjectTweener>();
            _rightCharGoTweener = _rightCharGo.GetComponent<ObjectTweener>();
            _rightCharImgTweener = _rightCharImg.GetComponent<ObjectTweener>();
            _containerTweener = GameObject.Find("container").GetComponent<ObjectTweener>();


            //Background
            _backgroundImg = GameObject.Find("background img").GetComponent<Image>();
            _backgroundMat = _backgroundImg.material;

            //Dialogue content
            _dialogueContent = GameObject.Find("dialogue content").gameObject;
            _continueBtn = GameObject.Find("continue btn").GetComponent<Button>();
            _skipRepliqueBtn = GameObject.Find("skip replique btn").GetComponent<Button>();
            _dialogueText = GameObject.Find("dialogue text").GetComponent<TextMeshProUGUI>();


            //Choices content
            _choicesContent = GameObject.Find("choices content").gameObject;
            int count = _choicesContent.transform.childCount;
            _choiceBtns = new Button[count];
            _choiceTmps = new TextMeshProUGUI[count];
            for (int i = 0; i < count; i++)
            {
                _choiceBtns[i] = _choicesContent.transform.GetChild(i).GetComponent<Button>();
                _choiceTmps[i] = _choiceBtns[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            }
        }

        protected override void SetUpComponents()
        {

            _choicesContent.SetActive(false);
            _dialogueContent.SetActive(false);

            //Le container sera activ� automatiquement au moment d'afficher les persos ou une r�plique
            _containerTweener.gameObject.SetActive(false);


            //Cache les persos pour les v�rifs auto du script
            _leftCharGo.SetActive(false);
            _rightCharGo.SetActive(false);

            //On emp�che la remise � z�ro auto des tweeners pour replacer les sprites nous-m�mes quand ils dispara�ssent
            _leftCharImgTweener.resetTransformOnDisable = _rightCharImgTweener.resetTransformOnDisable = false;
            _leftCharImgTweener.resetTransformOnTweenEnded = _rightCharImgTweener.resetTransformOnTweenEnded = false;

            //On d�sactive les containers des noms pour �viter d'avoir � les attacher au container
            _leftNameText.transform.parent.gameObject.SetActive(false);
            _rightNameText.transform.parent.gameObject.SetActive(false);


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
            //_dialogueManager.OnRunStartNode += OnRunStartNode;
            //_dialogueManager.OnRunCharacterNode += OnRunCharacterNode;

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
            //_dialogueManager.OnRunStartNode -= OnRunStartNode;
            //_dialogueManager.OnRunCharacterNode -= OnRunCharacterNode;

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

        protected override void ShowUI(Action onRunEnded = null)
        {

            _uiVisibleFlag = true;

            //Clean text for next replique
            _dialogueText.text = "";



            //On affiche les persos s'ils ont des sprites assign�s
            //et on d�sactive les containers des noms pour �viter d'avoir � les attacher au container
            if (_leftCharImg.sprite)
            {
                _leftCharGoTweener.BeginTweens(TS_CharAppears);
            }
            else
            {
                _leftNameText.transform.parent.gameObject.SetActive(false);
            }
            if (_rightCharImg.sprite)
            {
                _rightCharGoTweener.BeginTweens(TS_CharAppears);
            }
            else
            {
                _rightNameText.transform.parent.gameObject.SetActive(false);
            }


            //On active le tweener du container pour le faire appara�tre
            _containerTweener.gameObject.SetActive(true);
            _containerTweener.BeginTweens(TS_ContainerOnDialogueStarted, () =>
            {
                onRunEnded?.Invoke();
            });
        }


        protected override void HideUI(Action onRunEnded = null)
        {
            _uiVisibleFlag = false;

            //Clean text for next replique
            _dialogueText.text = "";

            //Pour chaque perso, on joue le tween de fin de dialogue pour les faire dispara�tre
            //(S'il le perso n'est pas visible alors il est d�sactiv�, donc �a craint rien)
            _leftCharGoTweener.BeginTweens(TS_CharOnDialogueEnded);
            _rightCharGoTweener.BeginTweens(TS_CharOnDialogueEnded);

            //Pour le container, on ferme la fen�tre de dialogue lorsque le tween de fin du container a termin�
            _containerTweener.BeginTweens(TS_ContainerOnDialogueEnded, () =>
            {
                onRunEnded?.Invoke();
                _containerTweener.gameObject.SetActive(false);
            });
        }



        private void OnRunUINode(UIData data, Action onRunEnded)
        {
            if (data.show.Value)
            {
                ShowUI(onRunEnded);
            }
            else
            {
                HideUI(onRunEnded);
            }
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
            StopAllCoroutines();

            _dialogueContent.SetActive(false);
            _choicesContent.SetActive(true);
        }




        private void OnChoiceInfosSet(List<DialogueButtonContainer> dialogueButtonContainers)
        {
            _choicesContent.SetActive(true);

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
                        b.onClick.AddListener(dialogueButtonContainers[i].OnChoiceClicked);

                        //b.onClick.AddListener(OnChoiceSelected);
                        //On d�sactive tous les boutons; le choiceContainer sera d�sactiv� quand la prochaine r�plique sera jou�e
                        b.onClick.AddListener(() =>
                        {
                            for (int j = 0; j < _choiceBtns.Length; j++)
                            {
                                _choiceBtns[j].gameObject.SetActive(false);
                            }
                        });

                        //On affiche le texte du choix sur le bouton
                        _choiceTmps[i].text = dialogueButtonContainers[i].Text;
                    }
                    else
                    {
                        //On n'active le bouton que si on doit le griser, puis on met son interactable � false
                        //pour montrer au joueur le choix indisponible en gris
                        b.gameObject.SetActive(dialogueButtonContainers[i].ChoiceState == ChoiceStateType.GreyOut);
                        b.interactable = false;

                        //On affiche le texte du choix sur le bouton
                        _choiceTmps[i].text = dialogueButtonContainers[i].Text;
                    }


                }
            }
        }


        #endregion




        #region Audio

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

        // Assigne un son d'�criture propre � chaque personnage (si null, l'�criture sera silencieuse)
        private void SetCharClip(AudioClip charPrintClip)
        {
            for (int i = 0; i < _nbCharPrintClips; i++)
            {
                _charClipSources[i].clip = charPrintClip;
            }
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
                OnRunUINode(uiData, () => 
                {
                    OnRunRepliqueNode(data, selectedLanguage);
                });
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