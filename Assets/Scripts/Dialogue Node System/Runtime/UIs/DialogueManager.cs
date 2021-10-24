using Project.ScreenFader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Project.NodeSystem 
{

    /// <summary>
    /// Executes different actions per node type contained in a DialogueContainerSO
    /// </summary>
    public class DialogueManager : DialogueGetData
    {

        #region Fields

        #region Components

        [Tooltip("The script subscribing to the events.")]
        private DE_EventCaller _triggerScript;

        #endregion


        #region Node Info

        [Tooltip("The language of the dialogue. Can be modified while reading a dialogue to display a new translation at the next node.")]
        private LanguageType _selectedLanguage = LanguageType.French;

        private int _curIndex = 0;

        //Stores the choices before displaying them in the UI script
        private readonly List<DialogueButtonContainer> _dialogueButtonContainers = new List<DialogueButtonContainer>();
        private readonly List<StartData> _eligibleDatas = new List<StartData>();

        #endregion


        #region Subscription

        public Action OnStartDialogue { get; set; }
        public Action OnEndDialogue { get; set; }
        public Action OnRunStartNode { get; set; }
        public Action<UIData, Action> OnRunUINode { get; set; }
        public Action<CharacterData_CharacterSO, Action> OnRunCharacterNode { get; set; }
        public Action<BackgroundData_Transition, TransitionSettingsSO, TransitionSettingsSO> OnRunBackgroundNode { get; set; }
        public Action<RepliqueData_Replique, LanguageType> OnRunRepliqueNode { get; set; }

        public Action OnChoicesSet { get; set; }
        public Action<List<DialogueButtonContainer>> OnChoiceInfosSet { get; set; }
        public Action<UnityAction> OnContinueBtnReached { get; set; }
        public Action OnTransitionEnded { get; set; }

        #endregion



        #endregion


        #region Init

        /// <summary>
        /// The method to call in order to start a new dialogue.
        /// </summary>
        /// <param name="newDialogue"></param>
        /// <param name="newLanugage"></param>
        /// <param name="newTriggerScript"></param>
        public void InitAndStartNewDialogue(DialogueContainerSO newDialogue, LanguageType? newLanugage = null, DE_EventCaller newTriggerScript = null)
        {
            if (newLanugage.HasValue) _selectedLanguage = newLanugage.Value;
            _dialogue = newDialogue;
            _triggerScript = newTriggerScript;

            StartDialogue();
        }

        public void SetLanguage(LanguageType newLanguage)
        {
            _selectedLanguage = newLanguage;
        }


        #endregion


        #region Overrides

        /// <summary>
        /// Get the StartNode meeting all its conditions.
        /// </summary>
        protected override void StartDialogue()
        {

            //Opens the dialogue UI et starts the opening animations if any
            OnStartDialogue?.Invoke();


            //By default, Start Nodes are sorted depending on their Y coord.
            //Thus the default Start Node will always be the topmost eligible node in the graph.
            _dialogue.StartDatas.Sort(delegate (StartData x, StartData y)
            {
                return x.Position.y.CompareTo(y.Position.y);
            });

            //Collects all Start Nodes and sorts them depending on their conditions
            StartData selectedStartData = null;
            _eligibleDatas.Clear();

            //For each Start Node...
            foreach (StartData startData in _dialogue.StartDatas)
            {
                bool eligible = true;

                //For each condition, we check if the DE_EventCaller's value matches the node's.
                foreach (EventData_StringEventCondition item in startData.StringConditions)
                {
                    //If not, the Start Node isn't eligible
                    if (!UseStringEventCondition.DialogueConditionEvents(_triggerScript, item.StringEvent.Value, item.ConditionType.Value, item.Number.Value))
                    {
                        eligible = false;
                        break;
                    }
                }

                //If it is, we add the node to the eligible list
                if(eligible) _eligibleDatas.Add(startData);

            }


            // If all/none of them are eligible...
            if (_eligibleDatas.Count == 0 || _eligibleDatas.Count == _dialogue.StartDatas.Count)
            {
                //We use the first Start Node marked as default
                selectedStartData = _dialogue.StartDatas.FirstOrDefault(start => start.isDefault.Value == true);

                // If none are marked as default, we use the first in the list
                if (selectedStartData == null) selectedStartData = _dialogue.StartDatas[0];
            }
            else
            {
                // If only some are eligible, we use the first in the eligible list.
                selectedStartData = _eligibleDatas.FirstOrDefault(start => start.isDefault.Value == true);

                // Else, we take the first one in the list, marked as default or not.
                if (selectedStartData == null) selectedStartData = _eligibleDatas[0];
            }


            RunNode(selectedStartData);
        }

        protected override void EndDialogue()
        {
            //Starts the closing animations if any and closes the UI
            OnEndDialogue?.Invoke();
        }

        protected override void RunNode(StartData nodeData)
        {

            //As it's the StartNode, we hide the characters here to not have to do it manually at each restart.
            OnRunStartNode?.Invoke();

            //Then we get the next node.
            Action onStart = () =>
            {
                CheckNodeType(GetNextNode(nodeData));
            };
            StartCoroutine(DelayCo(.5f, onStart));
        }

        protected override void RunNode(UIData nodeData)
        {
            //Will first modify the UI before reaching the next node.
            Action onRunEnded = () => { CheckNodeType(GetNextNode(nodeData)); };
            OnRunUINode?.Invoke(nodeData, onRunEnded);
        }

        protected override void RunNode(BranchData nodeData)
        {
            bool checkBranch = true;
            foreach (EventData_StringEventCondition item in nodeData.StringConditions)
            {
                //For each condition, we check if the DE_EventCaller's value matches the node's.
                if (!UseStringEventCondition.DialogueConditionEvents(_triggerScript, item.StringEvent.Value, item.ConditionType.Value, item.Number.Value))
                {
                    checkBranch = false;
                    break;
                }
            }

            //Get the next node's Guid depending on the valid branch
            string nextNode = (checkBranch ? nodeData.TrueNodeGuid : nodeData.FalseNodeGuid);
            CheckNodeType(GetNodeByGuid(nextNode));
        }

        protected override void RunNode(ChoiceData nodeData)
        {
            //Obtains the choices contained in the node to assign them to the UI's buttons.
            AssignActionsToButtons(nodeData);
        }

        protected override void RunNode(EventData nodeData)
        {
            List<NodeData_BaseContainer> events = nodeData.SortedEvents;

            //For each event...
            foreach (NodeData_BaseContainer item in events)
            {
                //If a DialogueEventSO has methods subscribed to it, we call its delegate.
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
                    //If there is a stringEvent, we get the matching value in the DE_EventCaller and we modify it depending on the modifierType.
                    EventData_StringEventModifier stringEvent = item as EventData_StringEventModifier;
                    stringEvent.Invoke(_triggerScript);
                }
            }

            CheckNodeType(GetNextNode(nodeData));
        }

        protected override void RunNode(EndData nodeData)
        {
            //For the End Node, we hide the UI before restarting or ending the dialogue
            UIData uiData = new UIData();
            uiData.Show.Value = false;

            switch (nodeData.EndNodeType.Value)
            {
                case EndNodeType.End:
                    //The DialogueTrigger assigns a method to exit the scene, which is called before the DialogueUI's HideUI() method.
                    //So no choice, we have to call it manually.
                    OnRunUINode?.Invoke(uiData, EndDialogue);
                    //EndDialogue();
                    break;
                case EndNodeType.ReturnToStart:
                    OnRunUINode?.Invoke(uiData, StartDialogue);
                    //StartDialogue();
                    break;
                default:
                    Debug.Log($"End Node Type \"{nodeData.EndNodeType.Value}\" not implemented");
                    break;
            }

        }

        protected override void RunNode(DelayData nodeData)
        {
            //Starts une coroutine avant de passer à la node suivante.
            Action onDelayEnded = () => CheckNodeType(GetNextNode(nodeData));
            StartCoroutine(DelayCo(nodeData.Delay.Value, onDelayEnded));
        }

        protected override void RunNode(BackgroundData nodeData)
        {
            BackgroundData_Transition transition = nodeData.Transition.Value;
            TransitionSettingsSO startSettings = transition.StartSettings.Value;
            TransitionSettingsSO endSettings = transition.EndSettings.Value;

            //Once the transition ended, we call OnTransitionEnded from the UI to reach the next node.
            Action tmp = () =>
            {
                CheckNodeType(GetNextNode(nodeData));
            };
            tmp += () => OnTransitionEnded -= tmp;
            OnTransitionEnded += tmp;

            OnRunBackgroundNode?.Invoke(transition, startSettings, endSettings);
        }

        protected override void RunNode(CharacterData nodeData)
        {
            _curIndex = 0;

            // The CharacterNode's containers can change their execution order depending on their ID.
            // We make sure to sort them before analyzing them.
            nodeData.Characters.Sort(delegate (CharacterData_CharacterSO x, CharacterData_CharacterSO y)
            {
                return x.ID.Value.CompareTo(y.ID.Value);
            });

            DisplayCharacters(nodeData);
        }

        protected override void RunNode(RepliqueData nodeData)
        {
            _curIndex = 0;

            // The RepliqueNode's containers can change their execution order depending on their ID.
            // We make sure to sort them before analyzing them.
            nodeData.Repliques.Sort(delegate (RepliqueData_Replique x, RepliqueData_Replique y)
            {
                return x.ID.Value.CompareTo(y.ID.Value);
            });

            DisplayRepliques(nodeData);
        }


        #endregion




        #region Implementations

        private IEnumerator DelayCo(float delay, Action onDelayEnded)
        {
            WaitForSeconds wait = new WaitForSeconds(delay);
            yield return wait;
            onDelayEnded?.Invoke();
        }

        private void DisplayCharacters(CharacterData nodeData)
        {

            //For each container...
            for (int i = _curIndex; i < nodeData.Characters.Count;)
            {
                _curIndex = i + 1;

                //The check if the character has a SO or not...
                CharacterData_CharacterSO character = nodeData.Characters[i];
                if (character.Character.Value != null)
                {
                    character.CharacterName.Value = character.CharacterNames[(int)_selectedLanguage];
                }


                //We use the autoDelay of the nodeData to display the characters one after the other if needed
                Action onCharacterDisplayed;

                //If we're done, we go to the next node
                if (i == nodeData.Characters.Count - 1)
                {
                    onCharacterDisplayed = () => CheckNodeType(GetNextNode(nodeData));
                }
                //Otherwise, we keep defiling the list
                else
                {
                    onCharacterDisplayed = () => DisplayCharacters(nodeData);
                }

                if (nodeData.Characters[i].UseAutoDelay.Value)
                {
                    //If the character has a SO, we display it in the UI
                    OnRunCharacterNode?.Invoke(character, () => 
                    {
                        StartCoroutine(DelayCo(nodeData.Characters[i].AutoDelayDuration.Value, onCharacterDisplayed));
                    });

                    
                }
                else
                {
                    OnRunCharacterNode?.Invoke(character, onCharacterDisplayed);
                    //onCharacterDisplayed?.Invoke();
                }




                break;
            }
        }

        private void DisplayRepliques(RepliqueData nodeData)
        {
            //For each container...
            for (int i = _curIndex; i < nodeData.Repliques.Count;)
            {
                _curIndex = i + 1;
                //We get its text and audio depending on the language...
                RepliqueData_Replique replique = nodeData.Repliques[i];

                //And we assign it to the UI.
                OnRunRepliqueNode?.Invoke(replique, _selectedLanguage);




                //Once done, we reach the next node.
                if (i == nodeData.Repliques.Count - 1)
                {
                    UnityAction displayNextNode = () => CheckNodeType(GetNextNode(nodeData));
                    OnContinueBtnReached?.Invoke(displayNextNode);
                }
                //Otherwise, we keep defiling the list
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

            // Assign actions to the continue button and the choice slots.
            // The Choice Node makes sure we have at least 1 available choice to avoid any bugs.


            _dialogueButtonContainers.Clear();

            //We check if there's at least 1 connected choice
            bool hasChoices = false;

            foreach (ChoiceData_Container container in nodeData.Choices)
            {
                //If the port is connected, add it to the list
                if (!string.IsNullOrEmpty(container.LinkedPort.InputGuid))
                {
                    GetChoiceInfo(container);
                    hasChoices = true;
                }
            }

            //If we have any choices, display them on the UI.
            if (hasChoices)
            {
                OnChoiceInfosSet?.Invoke(_dialogueButtonContainers);
                OnChoicesSet?.Invoke();
            }
            //Otherwise, the ChoiceNode has no available choices.
            //In this case, we end the dialogue to avoid any bugs.
            else
            {
                EndDialogue();
            }


            
        }

        private void GetChoiceInfo(ChoiceData_Container choice)
        {
            bool checkBranch = true;
            foreach (ChoiceData_Condition condition in choice.Conditions)
            {
                if (!UseStringEventCondition.DialogueConditionEvents(_triggerScript, condition.StringCondition.StringEvent.Value, condition.StringCondition.ConditionType.Value, condition.StringCondition.Number.Value))
                {
                    checkBranch = false;
                    break;
                }
            }

            //We create a DialogueButtonContainer, which will indicate to the UI if a choice is available or not
            UnityAction onChoiceClicked = () => CheckNodeType(GetNodeByGuid(choice.LinkedPort.InputGuid));

            DialogueButtonContainer dialogueButtonContainer = new DialogueButtonContainer();

            dialogueButtonContainer.ChoiceState = choice.ChoiceStateType.Value;

            //As for the line, we write the base text + the descriptions of the conditions when not fulfilled.
            dialogueButtonContainer.Text = choice.Texts.Find(text => text.Language == _selectedLanguage).Data;
            if (!checkBranch)
            {
                foreach (ChoiceData_Condition condition in choice.Conditions)
                {
                    string desc = condition.DescriptionsIfNotMet.Find(desc => desc.Language == _selectedLanguage).Data;

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

        #endregion
    }
}
