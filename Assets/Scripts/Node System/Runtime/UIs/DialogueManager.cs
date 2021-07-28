using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Project.NodeSystem 
{

    /// <summary>
    /// Le gestionnaire de dialogue. Chargé d'exécuter des actions en fonction du contenu des nodes d'un dialogueSO.
    /// </summary>
    public class DialogueManager : DialogueGetData
    {

        #region Fields

        [Tooltip("L'interface visuelle du dialogue.")]
        DialogueUI dialogueUi;

        [SerializeField, Tooltip("Le script qui s'abonnera aux fonctions des events.")]
        DE_Trigger triggerScript;

        [Tooltip("La langue du dialogue. Peut être modifiée en cours de dialogue pour afficher une traduction à la prochaine réplique.")]
        public LanguageType selectedLanguage = LanguageType.French;


        //Pour stocker les choix avant de les assigner dans le dialogueUI
        readonly List<DialogueButtonContainer> dialogueButtonContainers = new List<DialogueButtonContainer>();
        readonly List<StartData> eligibleDatas = new List<StartData>();


        #endregion


        #region Init

        void Start()
        {
            //On affiche l'interface de dialogue
            dialogueUi = GetComponent<DialogueUI>();

            StartDialogue();
        }

        //Pour lui assigner un triggerScript depuis un autre script
        public void SetDialogueTriggerSript(DE_Trigger newTriggerScript)
        {
            triggerScript = newTriggerScript;
        }



        #endregion


        /// <summary>
        /// Récupère la node de départ.
        /// </summary>
        protected override void StartDialogue()
        {
            dialogueUi.StartDialogue();

            //Par défaut, on trie les nodes selon leur position en Y.
            //Ainsi, la node de départ par défaut sera toujours la node éligible la plus haute dans le graphe.
            dialogue.startDatas.Sort(delegate (StartData x, StartData y)
            {
                return x.position.y.CompareTo(y.position.y);
            });

            // On récupère toutes les StartNodes du dialogue et on sélectionne celle à suivre en fonction de ses conditions.
            StartData selectedStartData = null;
            eligibleDatas.Clear();

            //Pour chaque StartNode du dialogue...
            foreach (StartData startData in dialogue.startDatas)
            {
                bool eligible = true;

                //Pour chaque condition de la node, on regarde si la valeur du DE_Trigger et celle de la node correspondent
                foreach (EventData_StringEventCondition item in startData.stringConditions)
                {
                    //Si ce n'est pas le cas, la StartNode n'est pas éligible
                    if (!UseStringEventCondition.DialogueConditionEvents(triggerScript, item.stringEvent.value, item.conditionType.value, item.number.value))
                    {
                        eligible = false;
                        break;
                    }
                }

                //On ajoute la node à la liste si elle est éligible
                if(eligible) eligibleDatas.Add(startData);

            }


            // Si aucune n'est éligible ou que toutes sont éligibles... 
            if (eligibleDatas.Count == 0 || eligibleDatas.Count == dialogue.startDatas.Count)
            {
                // On prend la première de la liste marquée par défaut
                selectedStartData = dialogue.startDatas.FirstOrDefault(start => start.isDefaultStartNode.value == true);

                // S'il n'y a pas de node par défaut, on prend la première qui vient
                if (selectedStartData == null) selectedStartData = dialogue.startDatas[0];
            }
            else
            {
                // Si seulement certaines sont éligibles, on récupère la première par défaut si celle-ci est dans la liste éligible.
                selectedStartData = eligibleDatas.FirstOrDefault(start => start.isDefaultStartNode.value == true);

                // Sinon, on prend la première éligible, qu'elle soit par défaut ou non
                if (selectedStartData == null) selectedStartData = eligibleDatas[0];
            }


            RunNode(selectedStartData);
        }

        protected override void RunNode(StartData nodeData)
        {
            //Comme c'est la StartNode, on se contente de cacher les sprites des persos pour ne pas avoir à le faire manuellement.
            dialogueUi.SetCharacter("", new Color(252, 3, 252, 1), null, DialogueSide.Right, DialogueSide.Left);    //Perso de gauche
            dialogueUi.SetCharacter("", new Color(252, 3, 252, 1), null, DialogueSide.Left, DialogueSide.Right);    //Perso de droite

            //On récupère la node suivante.
            CheckNodeType(GetNextNode(nodeData));
        }

        protected override void RunNode(BranchData nodeData)
        {
            bool checkBranch = true;
            foreach (EventData_StringEventCondition item in nodeData.stringConditions)
            {
                //Pour chaque condition de la BranchNode, on regarde si la valeur du DE_Trigger et celle de la node correspondent
                if (!UseStringEventCondition.DialogueConditionEvents(triggerScript, item.stringEvent.value, item.conditionType.value, item.number.value))
                {
                    checkBranch = false;
                    break;
                }
            }

            string nextNode = (checkBranch ? nodeData.trueNodeGuid : nodeData.falseNodeGuid);
            CheckNodeType(GetNodeByGuid(nextNode));
        }

        protected override void RunNode(ChoiceData nodeData)
        {
            //On doit maintenant récupérer les choix de la node et leurs conditions
            //pour les assigner aux boutons de l'interface
            AssignActionsToButtons(nodeData);
        }

        protected override void RunNode(EventData nodeData)
        {
            List<NodeData_BaseContainer> events = nodeData.SortedEvents;

            foreach (NodeData_BaseContainer item in events)
            {
                //Si on a un DialogueEventSO à jouer, celui-ci va appeler toutes les méthodes
                //qui se sont abonnées à lui
                if (item is EventData_ScriptableEvent)
                {
                    EventData_ScriptableEvent scriptableEvent = item as EventData_ScriptableEvent;
                    if (scriptableEvent.scriptableObject.value != null)
                    {
                        scriptableEvent.scriptableObject.value.Invoke();
                    }
                }
                else
                {
                    //Pour chaque stringEvent, on récupère la variable correspondante dans DE_Trigger et on la modifie selon le modifierType
                    EventData_StringEventModifier stringEvent = item as EventData_StringEventModifier;
                    stringEvent.Invoke(triggerScript);

                }
            }

            CheckNodeType(GetNextNode(nodeData));
        }

        protected override void RunNode(EndData nodeData)
        {
            switch (nodeData.endNodeType.value)
            {
                case EndNodeType.End:
                    dialogueUi.EndDialogue();
                    break;
                case EndNodeType.ReturnToStart:
                    StartDialogue();
                    break;
                default:
                    Debug.Log($"End Node Type \"{nodeData.endNodeType.value}\" not implemented");
                    break;
            }

        }

        protected override void RunNode(CharacterData nodeData)
        {
            /* Les conteneurs de la CharacterNode peuvent changer d'ordre d'exécution selon leur ID.
             * On s'assure de les récupérer dans le bon ordre avant de les analyser.
             */

            nodeData.characters.Sort(delegate (CharacterData_CharacterSO x, CharacterData_CharacterSO y)
            {
                return x.ID.value.CompareTo(y.ID.value);
            });

            DisplayCharacters(nodeData);

            //Quand on a fini de parcourir la node, on passe à la suivante
            CheckNodeType(GetNextNode(nodeData));
        }

        protected override void RunNode(RepliqueData nodeData)
        {
            /* Les conteneurs de la RepliqueNode peuvent changer d'ordre d'exécution selon leur ID.
             * On s'assure de les récupérer dans le bon ordre avant de les analyser.
             */

            nodeData.repliques.Sort(delegate (RepliqueData_Replique x, RepliqueData_Replique y)
            {
                return x.ID.value.CompareTo(y.ID.value);
            });

            DisplayRepliques(nodeData);
        }





        private void DisplayCharacters(CharacterData nodeData)
        {
            //Pour chaque conteneur de la CharacterNode...
            for (int i = 0; i < nodeData.characters.Count; i++)
            {
                //On regarde si le perso a un SO ou pas
                CharacterData_CharacterSO tmp = nodeData.characters[i];
                if (tmp.character.value == null)
                {
                    //Si on n'a pas de personnage, on n'affiche pas de perso à l'emplacement "sidePlacement"
                    dialogueUi.SetCharacter("", new Color(252, 3, 252, 1), null, tmp.faceDirection.value, tmp.sidePlacement.value);
                }
                else
                {
                    //Sinon on l'affiche normalement
                    tmp.characterName.value = tmp.characterNames[(int)selectedLanguage];
                    dialogueUi.SetCharacter(tmp.characterName.value, tmp.character.value.characterNameColor, tmp.sprite.value, tmp.faceDirection.value, tmp.sidePlacement.value);
                }

            }
        }

        private void DisplayRepliques(RepliqueData nodeData)
        {
            //Pour chaque conteneur de la RepliqueNode...
            for (int i = 0; i < nodeData.repliques.Count; i++)
            {
                //On récupère son texte et son audio en fonction de la langue...
                RepliqueData_Replique tmp = nodeData.repliques[i];
                dialogueUi.SetText(tmp.texts.Find(text => text.language == selectedLanguage).data);
                dialogueUi.PlaySound(tmp.audioClips.Find(text => text.language == selectedLanguage).data);



                //Quand on a fini de parcourir la node, on passe à la suivante
                if (i == nodeData.repliques.Count - 1)
                {
                    UnityAction displayNextNode = () => CheckNodeType(GetNextNode(nodeData));
                    dialogueUi.SetContinueBtn(displayNextNode);
                }
                //Sinon, on continue de défiler les répliqus
                else
                {
                    UnityAction redo = () => DisplayRepliques(nodeData);
                    dialogueUi.SetContinueBtn(redo);
                }
            }
        }

        private void AssignActionsToButtons(ChoiceData nodeData)
        {

            // Assigne les fonctions du bouton Continuer et des boutons de choix.
            // La ChoiceNode s'assure qu'on ait au moins 1 choix disponible pour éviter les bugs.
            // Sinon, on parcourt la liste


            dialogueButtonContainers.Clear();

            //On vérifie qu'il y a au moins un choix de connecté
            bool hasChoices = false;

            foreach (ChoiceData_Container container in nodeData.choices)
            {
                //Si le port de choix est bien connecté, on peut ajouter le choix à la liste
                if (!string.IsNullOrEmpty(container.linkedPort.inputGuid))
                {
                    AssignChoice(container);
                    hasChoices = true;
                }
            }

            //Si on a des choix, on affiche le panel des choix.
            if (hasChoices)
            {
                dialogueUi.SetChoices(dialogueButtonContainers);
                dialogueUi.ShowChoicesPanel();
            }
            //Sinon, la ChoiceNode n'a pas de points de sortie.
            //Dans ce cas, pour éviter les erreurs, on arrête le dialogue.
            else
            {
                dialogueUi.EndDialogue();
            }


            
        }

        private void AssignChoice(ChoiceData_Container choice)
        {
            bool checkBranch = true;
            foreach (ChoiceData_Condition condition in choice.conditions)
            {
                if (!UseStringEventCondition.DialogueConditionEvents(triggerScript, condition.stringCondition.stringEvent.value, condition.stringCondition.conditionType.value, condition.stringCondition.number.value))
                {
                    checkBranch = false;
                    break;
                }
            }

            //On crée un DialogueButtonContainer, qui indiquera au UI si un choix doit être disponible ou pas
            UnityAction onChoiceClicked = () => CheckNodeType(GetNodeByGuid(choice.linkedPort.inputGuid));

            DialogueButtonContainer dialogueButtonContainer = new DialogueButtonContainer();

            dialogueButtonContainer.ChoiceState = choice.choiceStateType.value;

            //Pour le texte du choix, on met le texte de base + les descriptions si les conditions ne sont pas remplies.
            dialogueButtonContainer.Text = choice.texts.Find(text => text.language == selectedLanguage).data;
            if (!checkBranch)
            {
                foreach (ChoiceData_Condition condition in choice.conditions)
                {
                    string desc = condition.descriptionsIfNotMet.Find(desc => desc.language == selectedLanguage).data;

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

            dialogueButtonContainers.Add(dialogueButtonContainer);
        }
    }
}
