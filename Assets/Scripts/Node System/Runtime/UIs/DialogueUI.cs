using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

using static Project.Utilities.Arrays.Arrays;

namespace Project.NodeSystem {
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


        #region Dialogue Characters

        [Space(10)]
        [Header("Dialogue Characters :")]
        [Space(10)]

        [SerializeField] protected GameObject m_leftCharGo;
        [SerializeField] protected GameObject m_rightCharGo;
        [SerializeField] protected Image m_leftCharImg;
        [SerializeField] protected Image m_rightCharImg;
        [SerializeField] protected TextMeshProUGUI m_leftNameText;
        [SerializeField] protected TextMeshProUGUI m_rightNameText;

        #endregion


        #region Dialogue Text

        [Space(10)]
        [Header("Dialogue Text :")]
        [Space(10)]

        [SerializeField] protected GameObject m_dialogueContent;
        [SerializeField] protected Button m_continueBtn;
        [SerializeField] protected Button m_skipRepliqueBtn;
        [SerializeField] protected TextMeshProUGUI m_dialogueText;
        [SerializeField] protected TextMeshProUGUI m_continueBtnText;
        protected StringBuilder _sb = new StringBuilder();  //Pour afficher les caractères du texte un à la fois
        #endregion


        #region Dialogue Choices

        [Space(10)]
        [Header("Dialogue Choices :")]
        [Space(10)]

        [SerializeField] protected GameObject m_choicesContent;

        [SerializeField] protected Button[] m_choiceBtns;   //Rempli une seule fois par code pour y accéder plus rapidement
        [SerializeField] protected TextMeshProUGUI[] m_choiceTmps;   //Rempli une seule fois par code pour y accéder plus rapidement

        #endregion


        #region Audio

        [Space(10)]
        [Header("Audio :")]
        [Space(10)]

        [SerializeField, Tooltip("Pour chacun de ces clips, on crée un AudioSource qui va appeler un de ces sons à chaque caractère inscrit (Mettre un nombre suffisant pour éviter les blancs).")]
        private int m_nbCharPrintClips = 10;

        [Tooltip("Permet de passer d'un clip à un autre lors de l'écriture des caractères.")]
        private int _curCharClipIndex = 0;

        private AudioSource _voiceClipSource;
        private AudioSource[] _charClipSources;



        #endregion



        #endregion




        #region Mono

        protected virtual void Awake()
        {
            // On abonne les méthodes au DialogueManager
            _dialogueManager = GetComponent<DialogueManager>();
            SubscribeToManager();

            //Voice audio
            _voiceClipSource = gameObject.AddComponent<AudioSource>();
            _voiceClipSource.volume = .2f;
            _voiceClipSource.playOnAwake = false;

            //Char print audio
            _charClipSources = new AudioSource[m_nbCharPrintClips];
            for (int i = 0; i < m_nbCharPrintClips; i++)
            {
                _charClipSources[i] = gameObject.AddComponent<AudioSource>();
                _charClipSources[i].volume = .2f;
                _charClipSources[i].playOnAwake = false;
            }


            m_choicesContent.SetActive(false);
            m_dialogueContent.SetActive(false);
        }

        private void OnDisable()
        {
            UnsubscribeFromManager();
        }

        protected virtual void SubscribeToManager()
        {
            _dialogueManager.OnStartDialogue += StartDialogue;
            _dialogueManager.OnEndDialogue += EndDialogue;
            _dialogueManager.OnRunStartNode += HideCharacterOnStart;
            _dialogueManager.OnChoiceInfosSet += SetChoices;
            _dialogueManager.OnChoicesSet += ShowChoicesPanel;
            _dialogueManager.OnRunCharacterNode += SetCharacter;
            _dialogueManager.OnRunRepliqueNode += SetText;
            _dialogueManager.OnRunRepliqueNode += SetVoiceAudio;
            _dialogueManager.OnContinueBtnReached += SetContinueBtn;
        }


        protected virtual void UnsubscribeFromManager()
        {
            _dialogueManager.OnStartDialogue -= StartDialogue;
            _dialogueManager.OnEndDialogue -= EndDialogue;
            _dialogueManager.OnRunStartNode -= HideCharacterOnStart;
            _dialogueManager.OnChoiceInfosSet -= SetChoices;
            _dialogueManager.OnChoicesSet -= ShowChoicesPanel;
            _dialogueManager.OnRunCharacterNode -= SetCharacter;
            _dialogueManager.OnRunRepliqueNode -= SetText;
            _dialogueManager.OnRunRepliqueNode -= SetVoiceAudio;
            _dialogueManager.OnContinueBtnReached -= SetContinueBtn;
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
            m_dialogueText.text = "";
        }

        protected virtual void HideUI(bool endDialogue = false)
        {
            if (endDialogue)
            {
                m_dialoguePanel.SetActive(false);
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
            m_continueBtnText.gameObject.SetActive(true);
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


            m_continueBtnText.gameObject.SetActive(false);    //Désactive le texte du continueBtn
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

            m_continueBtnText.gameObject.SetActive(true);
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
        protected void SetCharClip(AudioClip charPrintClip)
        {
            for (int i = 0; i < m_nbCharPrintClips; i++)
            {
                _charClipSources[i].clip = charPrintClip;
            }
        }




        /// <summary>
        /// On déplace l'assignation des persos ici pour pouvoir ajouter les tweens sans avoir à tout réécrire.
        /// (Appelé uniquement si on a un sprite à afficher)
        /// </summary>
        /// <param name="characterSprite"></param>
        /// <param name="faceDir"></param>
        /// <param name="sidePlacement"></param>
        protected virtual void OnCharacterSet(string characterName, Sprite characterSprite, DialogueSide faceDir, DialogueSide sidePlacement)
        {
            if (sidePlacement == DialogueSide.Left)
            {
                m_leftCharImg.sprite = characterSprite;
            }
            else
            {
                m_rightCharImg.sprite = characterSprite;
            }
        }


        protected virtual void DisplayCharacterAndName(string characterName, Color characterNameColor, Sprite characterSprite, DialogueSide faceDir, DialogueSide sidePlacement)
        {

            if (sidePlacement == DialogueSide.Left)
            {
                //S'il n'y a pas de sprite en paramètre, on veut cacher le personnage de ce côté
                if (!characterSprite)
                {
                    m_leftCharGo.gameObject.SetActive(false);
                    m_leftNameText.transform.parent.gameObject.SetActive(false);
                    return;
                }
                else
                {
                    m_leftCharGo.gameObject.SetActive(true);

                    //On change le nom du perso en question
                    m_leftNameText.transform.parent.gameObject.SetActive(true);
                    m_rightNameText.transform.parent.gameObject.SetActive(false);
                    m_leftNameText.text = characterName;
                    m_leftNameText.color = characterNameColor;

                }


            }
            else
            {
                //S'il n'y a pas de sprite en paramètre, on veut cacher le personnage de ce côté
                if (!characterSprite)
                {
                    m_rightCharGo.gameObject.SetActive(false);
                    m_rightNameText.transform.parent.gameObject.SetActive(false);
                    return;
                }
                else
                {
                    m_rightCharGo.gameObject.SetActive(true);

                    //On change le nom du perso en question
                    m_leftNameText.transform.parent.gameObject.SetActive(false);
                    m_rightNameText.transform.parent.gameObject.SetActive(true);
                    m_rightNameText.text = characterName;
                    m_rightNameText.color = characterNameColor;
                }


            }


        }


        #endregion



        #endregion
    }
}