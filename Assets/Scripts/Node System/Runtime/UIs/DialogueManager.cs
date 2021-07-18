using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Project.NodeSystem 
{

    /// <summary>
    /// Le gestionnaire de dialogue. Chargé d'exécuter des actions en fonction du contenu des nodes d'un dialogueSO.
    /// </summary>
    public class DialogueManager : DialogueGetData
    {
        [Tooltip("La langue du dialogue. Peut être modifiée en cours de dialogue pour afficher une traduction à la prochaine réplique.")]
        public LanguageType curLanguage = LanguageType.French;

        [Tooltip("L'interface visuelle du dialogue.")]
        DialogueUI dialogueUi;

        DialogueData curData;   //la DialogueNode en cours
        DialogueData lastData;  //Si on veut revenir en arrière

        readonly List<DialogueButtonContainer> dialogueButtonContainers = new List<DialogueButtonContainer>();
        readonly List<DialogueData_BaseContainer> baseContainers = new List<DialogueData_BaseContainer>();
        private int curIndex = 0;


        void Start()
        {
            //On affiche l'interface de dialogue
            dialogueUi = GetComponent<DialogueUI>();
            dialogueUi.StartDialogue();

            StartDialogue();
        }

        /// <summary>
        /// Récupère la node de départ.
        /// </summary>
        public void StartDialogue()
        {
            //A faire : Récupérer plusieurs StartNodes et sélectionner celle à suivre en fonction de ses conditions.
            //La raison pour laquelle on peut avoir plusieurs StartNodes est dans le cas où l'on veut choisir une node
            //de départ en fonction de certaines conditions (Une BranchNode ne peut prendre que 2 chemins, "true" ou "false").
            CheckNodeType(GetNextNode(dialogue.startDatas[0]));
        }


        /// <summary>
        /// Récupère l'action à exécuter en fonction du type de node atteint.
        /// </summary>
        /// <param name="inputData">La node à convertir.</param>
        private void CheckNodeType(BaseData inputData)
        {
            switch (inputData)
            {
                case StartData nodeData:
                    RunNode(nodeData);
                    break;
                case DialogueData nodeData:
                    RunNode(nodeData);
                    break;
                case BranchData nodeData:
                    RunNode(nodeData);
                    break;
                case EventData nodeData:
                    RunNode(nodeData);
                    break;
                case EndData nodeData:
                    RunNode(nodeData);
                    break;
                default:
                    break;
            }
        }





        private void RunNode(StartData startNodeData)
        {
            //C'est une StartNode, on n'a rien à faire. On récupère la node suivante.
            CheckNodeType(GetNextNode(dialogue.startDatas[0]));
        }

        private void RunNode(BranchData nodeData)
        {
            bool checkBranch = true;
            foreach (EventData_StringCondition item in nodeData.stringConditions)
            {
                //Pour chaque condition de la BranchNode, on regarde si la valeur du DE_Trigger et celle de la node correspondent
                if (!GameEvents.DialogueConditionEvents(item.stringEvent.value, item.conditionType.value, item.number.value))
                {
                    checkBranch = false;
                    break;
                }
            }

            string nextNode = (checkBranch ? nodeData.trueNodeGuid : nodeData.falseNodeGuid);
            CheckNodeType(GetNodeByGuid(nextNode));
        }

        private void RunNode(EventData nodeData)
        {
            //Si on a un DialogueEvent à jouer, celui-ci va appeler la classe statique GameEvents
            //pour lier l'event au DE_Trigger en question
            foreach (ContainerValue<DialogueEventSO> item in nodeData.scriptableEvents)
            {
                if (item.value != null)
                {
                    item.value.Invoke();
                }
            }
            //Pour chaque stringEvent, on récupère la variable correspondante dans DE_Trigger et on la modifie selon le modifierType
            foreach (EventData_StringModifier item in nodeData.stringEvents)
            {
                GameEvents.DialogueModifierEvents(item.stringEvent.value, item.modifierType.value, item.number.value);
            }
            CheckNodeType(GetNextNode(nodeData));
        }

        private void RunNode(EndData nodeData)
        {
            switch (nodeData.endNodeType.value)
            {
                //On termine le dialogue et on désactive le UI pour rendre le contrôle au joueur.
                case EndNodeType.End:
                    dialogueUi.EndDialogue();
                    break;

                //Rejoue la dernière DialogueNode lue.
                case EndNodeType.Repeat:
                    CheckNodeType(GetNodeByGuid(curData.nodeGuid));
                    break;

                //Si on a une lastNode, on revient à celle-ci, sinon on peut revenir plus loin en arrière.
                case EndNodeType.GoBack:
                    if (lastData != null)
                        CheckNodeType(GetNodeByGuid(lastData.nodeGuid));
                    else
                        goto case EndNodeType.Repeat;
                    break;

                //Rejoue le dialogue depuis le début
                case EndNodeType.ReturnToStart:
                    StartDialogue();
                    break;
                default:
                    break;
            }
        }

        private void RunNode(DialogueData nodeData)
        {
            //Assigne les nodes pour la EndNode
            lastData = curData;
            curData = nodeData;

            /* Les conteneurs de la DialogueNode (Persos et textes) peuvent changer d'ordre d'exécution selon leur ID.
             * On s'assure de les récupérer dans le bon ordre avant de les analyser.
             */
            baseContainers.Clear();
            baseContainers.AddRange(nodeData.characters);
            baseContainers.AddRange(nodeData.translations);

            curIndex = 0;

            baseContainers.Sort(delegate (DialogueData_BaseContainer x, DialogueData_BaseContainer y)
            {
                return x.ID.value.CompareTo(y.ID.value);
            });

            DisplayDialogueAndCharacters();
        }





        private void DisplayDialogueAndCharacters()
        {
            //Pour chaque conteneur de la DialogueNode, on regarde s'il s'agit d'un perso ou d'une réplique
            for (int i = curIndex; i < baseContainers.Count; i++)
            {
                curIndex = i + 1;
                if (baseContainers[i] is DialogueData_CharacterSO)
                {
                    //Si c'est un perso, on regarde s'il a un SO ou pas
                    DialogueData_CharacterSO tmp = baseContainers[i] as DialogueData_CharacterSO;
                    if(tmp.character.value == null)
                    {
                        //Si on n'a pas de personnage, on n'affiche pas de perso à l'emplacement "sidePlacement"
                        dialogueUi.SetCharacter("", new Color(252, 3, 252, 1), null, tmp.faceDirection.value, tmp.sidePlacement.value);
                    }
                    else
                    {
                        dialogueUi.SetCharacter(tmp.characterName.value, tmp.character.value.characterNameColor, tmp.sprite.value, tmp.faceDirection.value, tmp.sidePlacement.value);
                    }
                }
                if (baseContainers[i] is DialogueData_Translation)
                {
                    //Si c'est une réplique, on récupère son texte et son audio en fonction de la langue...
                    DialogueData_Translation tmp = baseContainers[i] as DialogueData_Translation;
                    dialogueUi.SetText(tmp.texts.Find(text => text.language == curLanguage).data);
                    dialogueUi.PlaySound(tmp.audioClips.Find(text => text.language == curLanguage).data);

                    //Puis on assigne les fonctions aux boutons de l'interface.
                    AssignActionsToButtons();
                    break;
                }
            }
        }



        private void AssignActionsToButtons()
        {

            ////Assigne les fonctions du bouton Continuer et des boutons de choix.
            ////Si la node n'a qu'un seul port, on passe à la réplique suivante
            //UnityAction nextReplique = null;
            //nextReplique += () => CheckNodeType(GetNodeByGuid(curData.dialogueNodePorts.First(port => port.inputGuid != string.Empty).inputGuid)); //Pour le bouton continuer, on récupère le premier port menant à une réplique
            //dialogueUi.SetContinueBtn(curData.dialogueNodePorts.Where(port => port.inputGuid != string.Empty).Cast<ChoicePort>().ToList(), nextReplique);    //On ne passe que les ports branchés
            //AssignChoiceButtons(curData.dialogueNodePorts);

            //Si on atteint la dernière ligne de texte et qu'on n'a pas de choix, le bouton continue joue la node suivante
            if (curIndex == baseContainers.Count && curData.ports.Count == 0)
            {
                UnityAction displayNextNode = () => CheckNodeType(GetNextNode(curData));
                dialogueUi.SetContinueBtn(displayNextNode);
            }
            else
            {
                if (curIndex == baseContainers.Count)
                {
                    dialogueButtonContainers.Clear();

                    foreach (DialogueData_Port port in curData.ports)
                    {
                        //Si le port de choix est bien connecté, on peut ajouter le choix à la liste
                        if(!string.IsNullOrEmpty(port.inputGuid))
                            AssignChoice(port.inputGuid);
                    }

                    //On indique au bouton continuer qu'au lieu de jouer la réplique suivante, on affiche la liste des choix
                    UnityAction showChoices = () =>
                    {
                        dialogueUi.SetChoices(dialogueButtonContainers);
                        dialogueUi.ShowChoicesPanel();
                    };
                    dialogueUi.SetContinueBtn(showChoices);
                }
                else
                {
                    UnityAction redo = () => DisplayDialogueAndCharacters();
                    dialogueUi.SetContinueBtn(redo);
                }
            }
        }

        private void AssignChoice(string guidID)
        {
            BaseData asd = GetNodeByGuid(guidID);
            ChoiceData choiceNode = asd as ChoiceData;
            DialogueButtonContainer dialogueButtonContainer = new DialogueButtonContainer();

            bool checkBranch = true;
            foreach (EventData_StringCondition item in choiceNode.stringConditions)
            {
                if (!GameEvents.DialogueConditionEvents(item.stringEvent.value, item.conditionType.value, item.number.value))
                {
                    checkBranch = false;
                    break;
                }
            }

            //On crée un DialogueButtonContainer, qui indiquera au UI si un choix doit être disponible ou pas
            UnityAction unityAction = () => CheckNodeType(GetNextNode(choiceNode));

            dialogueButtonContainer.ChoiceState = choiceNode.choiceStateType.value;
            dialogueButtonContainer.Text = choiceNode.texts.Find(text => text.language == curLanguage).data;
            dialogueButtonContainer.OnChoiceClicked = unityAction;
            dialogueButtonContainer.ConditionCheck = checkBranch;

            dialogueButtonContainers.Add(dialogueButtonContainer);
        }
    }
}
