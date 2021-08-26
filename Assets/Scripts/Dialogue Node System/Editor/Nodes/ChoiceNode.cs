using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Project.NodeSystem.Editor
{
    public class ChoiceNode : BaseNode
    {

        #region Fields

        public ChoiceData ChoiceData { get; set; } = new ChoiceData();

        private readonly List<Box> _boxesButtons = new List<Box>();
        private Button _newChoiceBtn;

        #endregion


        #region Constructors

        public ChoiceNode() : base("Choice", Vector2.zero)
        {

        }

        public ChoiceNode(Vector2 position, DialogueEditorWindow window, DialogueGraphView graphView) : base("Choice", position, window, graphView, "ChoiceNodeStyleSheet")
        {

            NodeBuilder.AddPort(this, "Input", Direction.Input, Port.Capacity.Multi);

            AddTopButton();

            //Crée un choix par défaut pour que le DialogueManager évite de planter si on n'a pas de choix.
            //Retiré au moment du chargement dans DialogueSaveLoad pour éviter de créer un choix supplémentaire.
            AddChoice();

            //On appelle ces fonctions pour mettre à jour le visuel de la Node
            RefreshExpandedState();
            RefreshPorts();
        }

        #endregion


        #region Methods


        #region Add Choice

        /// <summary>
        /// Crée un bouton en haut de la node pour ajouter un choix
        /// </summary>
        private void AddTopButton()
        {
            _newChoiceBtn = NodeBuilder.NewTitleButton(this, "New Choice", () => AddChoice(), "TopBtn");
        }


        /// <summary>
        /// Crée un choix et l'ajoute à la node
        /// </summary>
        /// <param name="loadedChoice">Si null, crée un choix vide. Sinon, assigne ses données aux champs du choix crée.</param>
        public void AddChoice(ChoiceData_Container loadedChoice = null)
        {
            ChoiceData_Container newChoice = new ChoiceData_Container();
            ChoiceData.Choices.Add(newChoice);
            newChoice.BoxContainer = NodeBuilder.NewBox(this, "ChoiceBox");

            AddLabelAndButton(newChoice, $"Choice n°{(ChoiceData.Choices.IndexOf(newChoice)+1).ToString()}");
            AddTextLine(newChoice);
            ChoiceStateEnum(newChoice);
            

            // Load in data if it got any
            if (loadedChoice != null)
            {
                // Guid ID
                newChoice.Guid = loadedChoice.Guid;
                newChoice.ChoiceStateType.Value = loadedChoice.ChoiceStateType.Value;

                // Text
                foreach (LanguageGeneric<string> data_text in loadedChoice.Texts)
                {
                    foreach (LanguageGeneric<string> text in newChoice.Texts)
                    {
                        if (text.Language == data_text.Language)
                        {
                            text.Data = data_text.Data;
                        }
                    }
                }


                // Audio
                foreach (LanguageGeneric<AudioClip> data_audioclip in loadedChoice.AudioClips)
                {
                    foreach (LanguageGeneric<AudioClip> audioclip in newChoice.AudioClips)
                    {
                        if (audioclip.Language == data_audioclip.Language)
                        {
                            audioclip.Data = data_audioclip.Data;
                        }
                    }
                }


                //Conditions
                foreach (ChoiceData_Condition loadedCondition in loadedChoice.Conditions)
                {
                    ChoiceData_Condition newCondition = AddCondition(newChoice, loadedCondition.StringCondition);
                    newCondition.Guid.Value = loadedCondition.Guid.Value;

                    // Descriptions de chaque condition
                    foreach (LanguageGeneric<string> loaded_desc in loadedCondition.DescriptionsIfNotMet)
                    {

                        foreach (LanguageGeneric<string> desc in newCondition.DescriptionsIfNotMet)
                        {
                            if (desc.Language == loaded_desc.Language)
                            {
                                desc.Data = loaded_desc.Data;
                            }
                        }
                    }
                }

                
            }
            else
            {
                newChoice.Guid.Value = Guid.NewGuid().ToString();
            }



            //Crée un port quand un nouveau choix est créé
            newChoice.LinkedPort = AddChoicePort(this, loadedChoice);



            // Reaload the current selected language fields
            ReloadLanguage();
            ReloadFields();
        }


        #endregion



        #region Fields

        private void AddLabelAndButton(ChoiceData_Container newChoice, string labelName)
        {
            Box labelContainer = NodeBuilder.NewBox(newChoice.BoxContainer, "TopBox");

            // Label Name
            Box buttonsBox = NodeBuilder.NewBox(labelContainer, "BtnBox", "LabelBtn");
            Label title = NodeBuilder.NewLabel(labelContainer, labelName, "Label", "LabelColor");




            //Si on n'a qu'un seul choix, pas la peine d'afficher les petits boutons
            _boxesButtons.Add(buttonsBox);
            for (int i = 0; i < _boxesButtons.Count; i++)
            {
                NodeBuilder.ShowHide(ChoiceData.Choices.Count > 1, _boxesButtons[i]);
            }


            // Move up button.
            Action onClicked = () =>
            {
                MoveBox(newChoice, true);
            };
            Button moveUpBtn = NodeBuilder.NewButton(buttonsBox, "", onClicked, "MoveUpBtn");

            // Move down button.
            onClicked = () =>
            {
                MoveBox(newChoice, false);
            };
            Button moveDownBtn = NodeBuilder.NewButton(buttonsBox, "", onClicked, "MoveDownBtn");


            //Ajoute un bouton pour ajouter une condition à ce choix
            Action onAddConditionClicked = () => AddCondition(newChoice);
            NodeBuilder.NewButton(labelContainer, "+ Condition", onAddConditionClicked, "AddConditionBtn", "LabelBtn");


            //Ajoute un bouton pour supprimer ce choix et son port associé
            Action onRemoveClicked = () =>
            {
                _boxesButtons.Remove(buttonsBox);
                DeleteBox(newChoice.BoxContainer);
                NodeBuilder.DeleteChoicePort(this, newChoice.LinkedPort.Port);


                ChoiceData.Choices.Remove(newChoice);
                RenameChoices();
                ShowHideNewChoiceBtn();
            };
            Button removeBtn = NodeBuilder.NewButton(buttonsBox, "X", onRemoveClicked, "RemoveChoiceBtn");

        }






        /// <summary>
        /// Crée un champ pour la réplique du choix et son audio
        /// </summary>
        /// <param name="newChoice"></param>
        public void AddTextLine(ChoiceData_Container newChoice)
        {
            // Make Container Box
            Box boxContainer = NodeBuilder.NewBox(newChoice.BoxContainer, "TextLineBox");

            // Text
            TextField textField = NodeBuilder.NewTextLanguagesField(this, boxContainer, newChoice.Texts, "Text", "TextBox");
            newChoice.TextField = textField;

            // Audio
            ObjectField objectField = NodeBuilder.NewAudioClipLanguagesField(this, boxContainer, newChoice.AudioClips, "AudioClip");
            newChoice.ObjectField = objectField;

            // Reload the current selected language
            ReloadLanguage();
        }


        /// <summary>
        /// Permet de cacher ou griser le bouton choix correspondant. Si les conditions pour activer cette partie du dialogue ne sont pas remplies
        /// </summary>
        /// <param name="newChoice"></param>
        private void ChoiceStateEnum(ChoiceData_Container newChoice)
        {
            newChoice.ChoiceStateEnumBox = NodeBuilder.NewBox(newChoice.BoxContainer, "BoxRow");
            ShowHideChoiceEnum();

            // Add fields to box.
            newChoice.ChoiceStateEnumBox.Add(NodeBuilder.NewLabel( "If the conditions are not met", "ChoiceStateTypeLabel"));
            NodeBuilder.NewEnumField("", newChoice.ChoiceStateEnumBox, newChoice.ChoiceStateType, "ChoiceStateTypeEnum");
        }


        /// <summary>
        /// Ajoute un champ de condition au choix en cours d'édition.
        /// </summary>
        /// <param name="newChoice">Le choix auquel ajouter la condition.</param>
        /// <param name="stringEvent">La condition à ajouter ainsi que sa description.</param>
        /// <returns></returns>
        public ChoiceData_Condition AddCondition(ChoiceData_Container newChoice, EventData_StringEventCondition stringEvent = null)
        {
            Box boxContainer = NodeBuilder.NewBox(newChoice.BoxContainer, "TextLineBox");
            ChoiceData_Condition condition = NodeBuilder.AddStringConditionEvent(this, boxContainer, newChoice, stringEvent);

            ShowHideChoiceEnum();
            return condition;
        }


        #endregion



        #region Ports


        /// <summary>
        /// Crée un port de sortie à chaque fois qu'un choix est créé
        /// </summary>
        /// <param name="baseNode"></param>
        /// <param name="loadedChoice"></param>
        /// <returns></returns>
        public NodeData_Port AddChoicePort(BaseNode baseNode, ChoiceData_Container loadedChoice = null)
        {
            NodeData_Port newPort = new NodeData_Port();

            // Check if we load it in with values
            if (loadedChoice != null)
            {
                newPort.InputGuid = loadedChoice.LinkedPort.InputGuid;
                newPort.OutputGuid = loadedChoice.LinkedPort.OutputGuid;
                newPort.PortGuid = loadedChoice.LinkedPort.PortGuid;
                newPort.Port = loadedChoice.LinkedPort.Port;
            }
            else
            {
                newPort.PortGuid = Guid.NewGuid().ToString();
            }

            if(newPort.Port == null)
            {
                Port port = NodeBuilder.GetPortInstance(this, Direction.Output);

                port.portName = newPort.PortGuid;                      // We use portName as port ID
                Label portNameLabel = port.contentContainer.Q<Label>("type");   // Get Labal in port that is used to contain the port name.
                portNameLabel.AddStyle("PortName");                       // Here we add a uss class to it so we can hide it in the editor window.


                baseNode.outputContainer.Add(port);
                newPort.Port = port;

                // Refresh
                ShowHideNewChoiceBtn();
                baseNode.RefreshPorts();
                baseNode.RefreshExpandedState();

            }

            return newPort;
        }



        #endregion



        #region UI

        /// <summary>
        /// Déplace les boxContainers des choix ainsi que leurs ports associés
        /// </summary>
        /// <param name="choiceToMove"></param>
        /// <param name="moveUp"></param>
        public override void MoveBox(NodeData_BaseContainer choiceToMove, bool moveUp)
        {
            List<NodeData_BaseContainer> tmp = new List<NodeData_BaseContainer>();
            List<VisualElement> ports = new List<VisualElement>();

            tmp.AddRange(ChoiceData.Choices);
            ports.AddRange(outputContainer.Children());


            for (int i = 0; i < tmp.Count; i++)
            {
                DeleteBox(ChoiceData.Choices[i].BoxContainer);
                tmp[i].ID.Value = i;
            }

            if (choiceToMove.ID.Value > 0 && moveUp)
            {
                NodeData_BaseContainer tmp01 = tmp[choiceToMove.ID.Value];
                NodeData_BaseContainer tmp02 = tmp[choiceToMove.ID.Value - 1];

                tmp[choiceToMove.ID.Value] = tmp02;
                tmp[choiceToMove.ID.Value - 1] = tmp01;

                VisualElement p01 = ports[choiceToMove.ID.Value];
                VisualElement p02 = ports[choiceToMove.ID.Value - 1];

                ports[choiceToMove.ID.Value] = p02;
                ports[choiceToMove.ID.Value - 1] = p01;

            }
            else if (choiceToMove.ID.Value < tmp.Count - 1 && !moveUp)
            {
                NodeData_BaseContainer tmp01 = tmp[choiceToMove.ID.Value];
                NodeData_BaseContainer tmp02 = tmp[choiceToMove.ID.Value + 1];

                tmp[choiceToMove.ID.Value] = tmp02;
                tmp[choiceToMove.ID.Value + 1] = tmp01;

                VisualElement p01 = ports[choiceToMove.ID.Value];
                VisualElement p02 = ports[choiceToMove.ID.Value + 1];

                ports[choiceToMove.ID.Value] = p02;
                ports[choiceToMove.ID.Value + 1] = p01;
            }

            ChoiceData.Choices.Clear();
            outputContainer.Clear();
            _boxesButtons.Clear();

            for (int i = 0; i < ports.Count; i++)
            {
                outputContainer.Add(ports[i]);
            }




            foreach (NodeData_BaseContainer data in tmp)
            {
                AddChoice(data as ChoiceData_Container);
            }
        }


        public override void DeleteBox(Box boxToRemove)
        {
            base.DeleteBox(boxToRemove);
            RenameChoices();
        }

        public override void DeleteBox(VisualElement container, VisualElement elementToRemove)
        {
            base.DeleteBox(container, elementToRemove);
            ShowHideChoiceEnum();
        }

        /// <summary>
        /// Affiche l'énum Hide/GreyOut si le choix a des conditions
        /// </summary>
        private void ShowHideChoiceEnum()
        {
            for (int i = 0; i < ChoiceData.Choices.Count; i++)
            {
                NodeBuilder.ShowHide(ChoiceData.Choices[i].Conditions.Count > 0, ChoiceData.Choices[i].ChoiceStateEnumBox);
            }
        }


        /// <summary>
        /// Affiche le bouton AddChoice si la node n'a pas atteint la limite max de choix possibles
        /// </summary>
        private void ShowHideNewChoiceBtn()
        {
            NodeBuilder.ShowHide(ChoiceData.Choices.Count < Window.NbMaxChoices, _newChoiceBtn);

            //Si on n'a qu'un seul choix, pas la peine d'afficher les petits boutons
            for (int i = 0; i < _boxesButtons.Count; i++)
            {
                NodeBuilder.ShowHide(ChoiceData.Choices.Count > 1, _boxesButtons[i]);
            }
        }


        /// <summary>
        /// Renomme les choix par ID
        /// </summary>
        private void RenameChoices()
        {
            for (int i = 0; i < ChoiceData.Choices.Count; i++)
            {
                ChoiceData.Choices[i].BoxContainer.Q<Label>().text = $"Choice n°{(i + 1).ToString()}";
            }
        }

        #endregion



        #region On Load


        public override void ReloadLanguage()
        {
            base.ReloadLanguage();
        }

        public override void ReloadFields()
        {
            for (int i = 0; i < ChoiceData.Choices.Count; i++)
            {
                if (ChoiceData.Choices[i].ChoiceStateType.EnumField != null)
                    ChoiceData.Choices[i].ChoiceStateType.EnumField.SetValueWithoutNotify(ChoiceData.Choices[i].ChoiceStateType.Value);
            }

        }



        #endregion



        #endregion
    }
}