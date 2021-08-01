using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Project.NodeSystem 
{

    /// <summary>
    /// Le gestionnaire de dialogue. Charg� d'ex�cuter des actions en fonction du contenu des nodes d'un dialogueSO.
    /// </summary>
    public class DialogueManager : DialogueGetData
    {

        #region Fields

        #region Components

        [SerializeField, Tooltip("Le script qui s'abonnera aux fonctions des events.")]
        DE_Trigger m_triggerScript;

        #endregion


        #region Node Info

        [Tooltip("La langue du dialogue. Peut �tre modifi�e en cours de dialogue pour afficher une traduction � la prochaine r�plique.")]
        public LanguageType SelectedLanguage = LanguageType.French;

        int _curIndex = 0;

        //Pour stocker les choix avant de les assigner dans le dialogueUI
        readonly List<DialogueButtonContainer> _dialogueButtonContainers = new List<DialogueButtonContainer>();
        readonly List<StartData> _eligibleDatas = new List<StartData>();

        #endregion


        #region Subscription

        public Action OnStartDialogue { get; set; }
        public Action OnEndDialogue { get; set; }
        public Action OnRunStartNode { get; set; }
        public Action OnChoicesSet { get; set; }
        public Action<List<DialogueButtonContainer>> OnChoiceInfosSet { get; set; }
        public Action<CharacterData_CharacterSO> OnRunCharacterNode { get; set; }
        public Action<RepliqueData_Replique, LanguageType> OnRunRepliqueNode { get; set; }

        public Action<UnityAction> OnContinueBtnReached { get; set; }

        #endregion



        #endregion


        #region Init

        void Start()
        {
            //On affiche l'interface de dialogue
            StartDialogue();
        }


        //Pour lui assigner un triggerScript depuis un autre script
        public void SetDialogueTriggerSript(DE_Trigger newTriggerScript)
        {
            m_triggerScript = newTriggerScript;
        }


        //Pour lui assigner un dialogue depuis un autre script
        public void SetDialogueContainer(DialogueContainerSO newDialogue)
        {
            dialogue = newDialogue;
        }



        #endregion


        /// <summary>
        /// R�cup�re la node de d�part.
        /// </summary>
        protected override void StartDialogue(DialogueContainerSO newDialogue = null, DE_Trigger newTriggerScript = null)
        {
            if (newDialogue) SetDialogueContainer(newDialogue);
            if (newTriggerScript) SetDialogueTriggerSript(newTriggerScript);



            //Ouvre le ui du dialogue et lance les anims d'ouverture s'il y en a
            OnStartDialogue?.Invoke();



            //Par d�faut, on trie les nodes selon leur position en Y.
            //Ainsi, la node de d�part par d�faut sera toujours la node �ligible la plus haute dans le graphe.
            dialogue.StartDatas.Sort(delegate (StartData x, StartData y)
            {
                return x.Position.y.CompareTo(y.Position.y);
            });

            // On r�cup�re toutes les StartNodes du dialogue et on s�lectionne celle � suivre en fonction de ses conditions.
            StartData selectedStartData = null;
            _eligibleDatas.Clear();

            //Pour chaque StartNode du dialogue...
            foreach (StartData startData in dialogue.StartDatas)
            {
                bool eligible = true;

                //Pour chaque condition de la node, on regarde si la valeur du DE_Trigger et celle de la node correspondent
                foreach (EventData_StringEventCondition item in startData.StringConditions)
                {
                    //Si ce n'est pas le cas, la StartNode n'est pas �ligible
                    if (!UseStringEventCondition.DialogueConditionEvents(m_triggerScript, item.StringEvent.Value, item.ConditionType.Value, item.Number.Value))
                    {
                        eligible = false;
                        break;
                    }
                }

                //On ajoute la node � la liste si elle est �ligible
                if(eligible) _eligibleDatas.Add(startData);

            }


            // Si aucune n'est �ligible ou que toutes sont �ligibles... 
            if (_eligibleDatas.Count == 0 || _eligibleDatas.Count == dialogue.StartDatas.Count)
            {
                // On prend la premi�re de la liste marqu�e par d�faut
                selectedStartData = dialogue.StartDatas.FirstOrDefault(start => start.isDefault.Value == true);

                // S'il n'y a pas de node par d�faut, on prend la premi�re qui vient
                if (selectedStartData == null) selectedStartData = dialogue.StartDatas[0];
            }
            else
            {
                // Si seulement certaines sont �ligibles, on r�cup�re la premi�re par d�faut si celle-ci est dans la liste �ligible.
                selectedStartData = _eligibleDatas.FirstOrDefault(start => start.isDefault.Value == true);

                // Sinon, on prend la premi�re �ligible, qu'elle soit par d�faut ou non
                if (selectedStartData == null) selectedStartData = _eligibleDatas[0];
            }


            RunNode(selectedStartData);
        }

        protected override void EndDialogue()
        {
            //On lance les anims de fermeture s'il y en a et on ferme le ui
            OnEndDialogue?.Invoke();
        }

        protected override void RunNode(StartData nodeData)
        {

            //Comme c'est la StartNode, on se contente de cacher les sprites des persos pour ne pas avoir � le faire manuellement.
            OnRunStartNode?.Invoke();

            //On r�cup�re la node suivante.
            Action onStart = () =>
            {
                CheckNodeType(GetNextNode(nodeData));
            };
            StartCoroutine(DelayCo(.5f, onStart));
        }

        protected override void RunNode(BranchData nodeData)
        {
            bool checkBranch = true;
            foreach (EventData_StringEventCondition item in nodeData.StringConditions)
            {
                //Pour chaque condition de la BranchNode, on regarde si la valeur du DE_Trigger et celle de la node correspondent
                if (!UseStringEventCondition.DialogueConditionEvents(m_triggerScript, item.StringEvent.Value, item.ConditionType.Value, item.Number.Value))
                {
                    checkBranch = false;
                    break;
                }
            }

            string nextNode = (checkBranch ? nodeData.TrueNodeGuid : nodeData.FalseNodeGuid);
            CheckNodeType(GetNodeByGuid(nextNode));
        }

        protected override void RunNode(ChoiceData nodeData)
        {
            //On doit maintenant r�cup�rer les choix de la node et leurs conditions
            //pour les assigner aux boutons de l'interface
            AssignActionsToButtons(nodeData);
        }

        protected override void RunNode(EventData nodeData)
        {
            List<NodeData_BaseContainer> events = nodeData.SortedEvents;

            foreach (NodeData_BaseContainer item in events)
            {
                //Si on a un DialogueEventSO � jouer, celui-ci va appeler toutes les m�thodes
                //qui se sont abonn�es � lui
                if (item is EventData_ScriptableEvent)
                {
                    EventData_ScriptableEvent scriptableEvent = item as EventData_ScriptableEvent;
                    if (scriptableEvent.ScriptableObject.Value != null)
                    {
                        scriptableEvent.ScriptableObject.Value.Invoke();
                    }
                }
                else
                {
                    //Pour chaque stringEvent, on r�cup�re la variable correspondante dans DE_Trigger et on la modifie selon le modifierType
                    EventData_StringEventModifier stringEvent = item as EventData_StringEventModifier;
                    stringEvent.Invoke(m_triggerScript);

                }
            }

            CheckNodeType(GetNextNode(nodeData));
        }

        protected override void RunNode(EndData nodeData)
        {
            switch (nodeData.EndNodeType.Value)
            {
                case EndNodeType.End:
                    EndDialogue();
                    break;
                case EndNodeType.ReturnToStart:
                    StartDialogue();
                    break;
                default:
                    Debug.Log($"End Node Type \"{nodeData.EndNodeType.Value}\" not implemented");
                    break;
            }

        }

        protected override void RunNode(CharacterData nodeData)
        {
            /* Les conteneurs de la CharacterNode peuvent changer d'ordre d'ex�cution selon leur ID.
             * On s'assure de les r�cup�rer dans le bon ordre avant de les analyser.
             */

            _curIndex = 0;

            nodeData.Characters.Sort(delegate (CharacterData_CharacterSO x, CharacterData_CharacterSO y)
            {
                return x.ID.Value.CompareTo(y.ID.Value);
            });

            DisplayCharacters(nodeData);
        }

        protected override void RunNode(RepliqueData nodeData)
        {
            /* Les conteneurs de la RepliqueNode peuvent changer d'ordre d'ex�cution selon leur ID.
             * On s'assure de les r�cup�rer dans le bon ordre avant de les analyser.
             */

            _curIndex = 0;

            nodeData.Repliques.Sort(delegate (RepliqueData_Replique x, RepliqueData_Replique y)
            {
                return x.ID.Value.CompareTo(y.ID.Value);
            });

            DisplayRepliques(nodeData);
        }





        private void DisplayCharacters(CharacterData nodeData)
        {
            //Pour chaque conteneur de la CharacterNode...
            for (int i = _curIndex; i < nodeData.Characters.Count; i++)
            {
                _curIndex = i + 1;

                //On regarde si le perso a un SO ou pas
                CharacterData_CharacterSO tmp = nodeData.Characters[i];
                if (tmp.Character.Value != null)
                {
                    tmp.CharacterName.Value = tmp.CharacterNames[(int)SelectedLanguage];
                }

                OnRunCharacterNode?.Invoke(tmp);


                //Utilise l'autoDelay de la nodeData pour afficher les persos les uns apr�s les autres si besoin
                Action onCharacterDisplayed;

                //Quand on a fini de parcourir la node, on passe � la suivante
                if (i == nodeData.Characters.Count - 1)
                {
                    onCharacterDisplayed = () => CheckNodeType(GetNextNode(nodeData));
                }
                //Sinon, on continue de d�filer les personnages
                else
                {
                    onCharacterDisplayed = () => DisplayCharacters(nodeData);
                }

                if (nodeData.Characters[i].useAutoDelay.Value)
                {
                    StartCoroutine(DelayCo(nodeData.Characters[i].autoDelayDuration.Value, onCharacterDisplayed));
                }
                else
                {
                    onCharacterDisplayed?.Invoke();
                }

                break;
            }
        }

        private IEnumerator DelayCo(float delay, Action onCharacterDisplayed)
        {
            WaitForSeconds wait = new WaitForSeconds(delay);
            yield return wait;
            onCharacterDisplayed?.Invoke();
        }

        private void DisplayRepliques(RepliqueData nodeData)
        {
            //Pour chaque conteneur de la RepliqueNode...
            for (int i = _curIndex; i < nodeData.Repliques.Count; i++)
            {
                _curIndex = i + 1;
                //On r�cup�re son texte et son audio en fonction de la langue...
                RepliqueData_Replique tmp = nodeData.Repliques[i];

                //Et on l'assigne � l'ui
                OnRunRepliqueNode?.Invoke(tmp, SelectedLanguage);




                //Quand on a fini de parcourir la node, on passe � la suivante
                if (i == nodeData.Repliques.Count - 1)
                {
                    UnityAction displayNextNode = () => CheckNodeType(GetNextNode(nodeData));
                    OnContinueBtnReached?.Invoke(displayNextNode);
                }
                //Sinon, on continue de d�filer les r�pliqus
                else
                {
                    UnityAction redo = () => DisplayRepliques(nodeData);
                    OnContinueBtnReached?.Invoke(redo);
                }

                break;
            }
        }

        private void AssignActionsToButtons(ChoiceData nodeData)
        {

            // Assigne les fonctions du bouton Continuer et des boutons de choix.
            // La ChoiceNode s'assure qu'on ait au moins 1 choix disponible pour �viter les bugs.
            // Sinon, on parcourt la liste


            _dialogueButtonContainers.Clear();

            //On v�rifie qu'il y a au moins un choix de connect�
            bool hasChoices = false;

            foreach (ChoiceData_Container container in nodeData.Choices)
            {
                //Si le port de choix est bien connect�, on peut ajouter le choix � la liste
                if (!string.IsNullOrEmpty(container.LinkedPort.InputGuid))
                {
                    AssignChoice(container);
                    hasChoices = true;
                }
            }

            //Si on a des choix, on affiche le panel des choix.
            if (hasChoices)
            {
                OnChoiceInfosSet?.Invoke(_dialogueButtonContainers);
                OnChoicesSet?.Invoke();
            }
            //Sinon, la ChoiceNode n'a pas de points de sortie.
            //Dans ce cas, pour �viter les erreurs, on arr�te le dialogue.
            else
            {
                OnEndDialogue?.Invoke();
            }


            
        }

        private void AssignChoice(ChoiceData_Container choice)
        {
            bool checkBranch = true;
            foreach (ChoiceData_Condition condition in choice.Conditions)
            {
                if (!UseStringEventCondition.DialogueConditionEvents(m_triggerScript, condition.StringCondition.StringEvent.Value, condition.StringCondition.ConditionType.Value, condition.StringCondition.Number.Value))
                {
                    checkBranch = false;
                    break;
                }
            }

            //On cr�e un DialogueButtonContainer, qui indiquera au UI si un choix doit �tre disponible ou pas
            UnityAction onChoiceClicked = () => CheckNodeType(GetNodeByGuid(choice.LinkedPort.InputGuid));

            DialogueButtonContainer dialogueButtonContainer = new DialogueButtonContainer();

            dialogueButtonContainer.ChoiceState = choice.ChoiceStateType.Value;

            //Pour le texte du choix, on met le texte de base + les descriptions si les conditions ne sont pas remplies.
            dialogueButtonContainer.Text = choice.Texts.Find(text => text.Language == SelectedLanguage).Data;
            if (!checkBranch)
            {
                foreach (ChoiceData_Condition condition in choice.Conditions)
                {
                    string desc = condition.DescriptionsIfNotMet.Find(desc => desc.Language == SelectedLanguage).Data;

                    if (!string.IsNullOrEmpty(desc))
                    {
                        dialogueButtonContainer.Text = string.Format("{0}   <size=30><i><color=#FF999999>({1})</color></i></size>",
                                                                     dialogueButtonContainer.Text,
                                                                     desc);
                    }
                }
            }


            dialogueButtonContainer.OnChoiceClicked = onChoiceClicked;
            dialogueButtonContainer.ConditionCheck = checkBranch;

            _dialogueButtonContainers.Add(dialogueButtonContainer);
        }
    }
}
