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
        private ChoiceData choiceData = new ChoiceData();
        public ChoiceData ChoiceData { get => choiceData; set => choiceData = value; }

        private List<Box> boxesButtons = new List<Box>();
        private Button newChoiceBtn;

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




        private void AddTopButton()
        {
            newChoiceBtn = NodeBuilder.NewTitleButton(this, "New Choice", () => AddChoice(), "TopBtn");
        }



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
            LoadValueIntoField();
        }





        #region Fields

        private void AddLabelAndButton(ChoiceData_Container newChoice, string labelName)
        {
            Box labelContainer = NodeBuilder.NewBox(newChoice.BoxContainer, "TopBox");

            // Label Name
            Box buttonsBox = NodeBuilder.NewBox(labelContainer, "BtnBox");
            Label title = NodeBuilder.NewLabel(labelContainer, labelName, "LabelText", "TextColor");




            //Si on n'a qu'un seul choix, pas la peine d'afficher les petits boutons
            boxesButtons.Add(buttonsBox);
            for (int i = 0; i < boxesButtons.Count; i++)
            {
                NodeBuilder.ShowHide(ChoiceData.Choices.Count > 1, boxesButtons[i]);
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
            NodeBuilder.NewButton(labelContainer, "+ Condition", onAddConditionClicked, "AddConditionBtn");


            //Ajoute un bouton pour supprimer ce choix et son port associé
            Action onRemoveClicked = () =>
            {
                boxesButtons.Remove(buttonsBox);
                DeleteBox(newChoice.BoxContainer);
                NodeBuilder.DeleteChoicePort(this, newChoice.LinkedPort.Port);


                ChoiceData.Choices.Remove(newChoice);
                RenameChoices();
                ShowHideNewChoiceBtn();
            };
            Button removeBtn = NodeBuilder.NewButton(buttonsBox, "X", onRemoveClicked, "RemoveChoiceBtn");

        }






        //Crée un champ pour la réplique du choix et son audio
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

        //Permet de cacher ou griser le bouton choix correspondant 
        //si les conditions pour activer cette partie du dialogue ne sont pas remplies
        private void ChoiceStateEnum(ChoiceData_Container newChoice)
        {
            newChoice.ChoiceStateEnumBox = NodeBuilder.NewBox(newChoice.BoxContainer, "BoxRow");
            ShowHideChoiceEnum();

            // Add fields to box.
            newChoice.ChoiceStateEnumBox.Add(NodeBuilder.NewLabel( "If the conditions are not met", "ChoiceLabel"));
            NodeBuilder.NewEnumField("", newChoice.ChoiceStateEnumBox, newChoice.ChoiceStateType, "enumHide");
        }



        public ChoiceData_Condition AddCondition(ChoiceData_Container newChoice, EventData_StringEventCondition stringEvent = null)
        {
            Box boxContainer = NodeBuilder.NewBox(newChoice.BoxContainer, "TextLineBox");
            ChoiceData_Condition condition = NodeBuilder.AddStringConditionEvent(this, boxContainer, newChoice, stringEvent);

            ShowHideChoiceEnum();
            return condition;
        }


        #endregion




        #region Ports

        // Port ---------------------------------------------------------------------------------------

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
            boxesButtons.Clear();

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

        private void ShowHideChoiceEnum()
        {
            for (int i = 0; i < ChoiceData.Choices.Count; i++)
            {
                NodeBuilder.ShowHide(ChoiceData.Choices[i].Conditions.Count > 0, ChoiceData.Choices[i].ChoiceStateEnumBox);
            }
        }

        private void ShowHideNewChoiceBtn()
        {
            NodeBuilder.ShowHide(ChoiceData.Choices.Count < Window.NbMaxChoices, newChoiceBtn);

            //Si on n'a qu'un seul choix, pas la peine d'afficher les petits boutons
            for (int i = 0; i < boxesButtons.Count; i++)
            {
                NodeBuilder.ShowHide(ChoiceData.Choices.Count > 1, boxesButtons[i]);
            }
        }

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

        public override void LoadValueIntoField()
        {
            for (int i = 0; i < ChoiceData.Choices.Count; i++)
            {
                if (ChoiceData.Choices[i].ChoiceStateType.EnumField != null)
                    ChoiceData.Choices[i].ChoiceStateType.EnumField.SetValueWithoutNotify(ChoiceData.Choices[i].ChoiceStateType.Value);
            }

        }



        #endregion
    }
}