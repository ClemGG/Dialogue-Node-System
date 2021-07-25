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
            ChoiceData.choices.Add(newChoice);
            newChoice.choiceContainer = NodeBuilder.NewBox(this, "ChoiceBox");

            AddLabelAndButton(newChoice, $"Choice n°{(ChoiceData.choices.IndexOf(newChoice)+1).ToString()}");
            AddTextLine(newChoice);
            ChoiceStateEnum(newChoice);
            

            // Load in data if it got any
            if (loadedChoice != null)
            {
                // Guid ID
                newChoice.guid = loadedChoice.guid;
                newChoice.choiceStateType.value = loadedChoice.choiceStateType.value;

                // Text
                foreach (LanguageGeneric<string> data_text in loadedChoice.texts)
                {
                    foreach (LanguageGeneric<string> text in newChoice.texts)
                    {
                        if (text.language == data_text.language)
                        {
                            text.data = data_text.data;
                        }
                    }
                }


                // Audio
                foreach (LanguageGeneric<AudioClip> data_audioclip in loadedChoice.audioClips)
                {
                    foreach (LanguageGeneric<AudioClip> audioclip in newChoice.audioClips)
                    {
                        if (audioclip.language == data_audioclip.language)
                        {
                            audioclip.data = data_audioclip.data;
                        }
                    }
                }


                //Conditions
                foreach (ChoiceData_Condition loadedCondition in loadedChoice.conditions)
                {
                    ChoiceData_Condition newCondition = AddCondition(newChoice, loadedCondition.stringCondition);
                    newCondition.guid.value = loadedCondition.guid.value;

                    // Descriptions de chaque condition
                    foreach (LanguageGeneric<string> loaded_desc in loadedCondition.descriptionsIfNotMet)
                    {

                        foreach (LanguageGeneric<string> desc in newCondition.descriptionsIfNotMet)
                        {
                            if (desc.language == loaded_desc.language)
                            {
                                desc.data = loaded_desc.data;
                            }
                        }
                    }
                }

                
            }
            else
            {
                newChoice.guid.value = Guid.NewGuid().ToString();
            }



            //Crée un port quand un nouveau choix est créé
            newChoice.linkedPort = AddChoicePort(this, loadedChoice);



            // Reaload the current selected language fields
            ReloadLanguage();
            LoadValueIntoField();
        }





        #region Fields

        private void AddLabelAndButton(ChoiceData_Container newChoice, string labelName)
        {
            Box labelContainer = NodeBuilder.NewBox(newChoice.choiceContainer, "TopBox");

            // Label Name
            Box buttonsBox = NodeBuilder.NewBox(labelContainer, "BtnBox");
            Label title = NodeBuilder.NewLabel(labelContainer, labelName, "LabelText", "TextColor");




            //Si on n'a qu'un seul choix, pas la peine d'afficher les petits boutons
            boxesButtons.Add(buttonsBox);
            for (int i = 0; i < boxesButtons.Count; i++)
            {
                NodeBuilder.ShowHide(ChoiceData.choices.Count > 1, boxesButtons[i]);
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
                DeleteBox(newChoice.choiceContainer);
                NodeBuilder.DeleteChoicePort(this, newChoice.linkedPort.port);


                ChoiceData.choices.Remove(newChoice);
                RenameChoices();
                ShowHideNewChoiceBtn();
            };
            Button removeBtn = NodeBuilder.NewButton(buttonsBox, "X", onRemoveClicked, "RemoveChoiceBtn");

        }






        //Crée un champ pour la réplique du choix et son audio
        public void AddTextLine(ChoiceData_Container newChoice)
        {
            // Make Container Box
            Box boxContainer = NodeBuilder.NewBox(newChoice.choiceContainer, "TextLineBox");

            // Text
            TextField textField = NodeBuilder.NewTextLanguagesField(this, boxContainer, newChoice.texts, "Text", "TextBox");
            newChoice.TextField = textField;

            // Audio
            ObjectField objectField = NodeBuilder.NewAudioClipLanguagesField(this, boxContainer, newChoice.audioClips, "AudioClip");
            newChoice.ObjectField = objectField;

            // Reload the current selected language
            ReloadLanguage();
        }

        //Permet de cacher ou griser le bouton choix correspondant 
        //si les conditions pour activer cette partie du dialogue ne sont pas remplies
        private void ChoiceStateEnum(ChoiceData_Container newChoice)
        {
            newChoice.choiceStateEnumBox = NodeBuilder.NewBox(newChoice.choiceContainer, "BoxRow");
            ShowHideChoiceEnum();

            // Add fields to box.
            newChoice.choiceStateEnumBox.Add(NodeBuilder.NewLabel( "If the conditions are not met", "ChoiceLabel"));
            newChoice.choiceStateEnumBox.Add(NodeBuilder.NewChoiceStateTypeField(newChoice.choiceStateType, "enumHide"));
        }



        public ChoiceData_Condition AddCondition(ChoiceData_Container newChoice, EventData_StringEventCondition stringEvent = null)
        {
            Box boxContainer = NodeBuilder.NewBox(newChoice.choiceContainer, "TextLineBox");
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
                newPort.inputGuid = loadedChoice.linkedPort.inputGuid;
                newPort.outputGuid = loadedChoice.linkedPort.outputGuid;
                newPort.portGuid = loadedChoice.linkedPort.portGuid;
                newPort.port = loadedChoice.linkedPort.port;
            }
            else
            {
                newPort.portGuid = Guid.NewGuid().ToString();
            }

            if(newPort.port == null)
            {
                Port port = NodeBuilder.GetPortInstance(this, Direction.Output);

                port.portName = newPort.portGuid;                      // We use portName as port ID
                Label portNameLabel = port.contentContainer.Q<Label>("type");   // Get Labal in port that is used to contain the port name.
                portNameLabel.AddStyle("PortName");                       // Here we add a uss class to it so we can hide it in the editor window.


                baseNode.outputContainer.Add(port);
                newPort.port = port;

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

            tmp.AddRange(ChoiceData.choices);
            ports.AddRange(outputContainer.Children());


            for (int i = 0; i < tmp.Count; i++)
            {
                DeleteBox(ChoiceData.choices[i].choiceContainer);
                tmp[i].ID.value = i;
            }

            if (choiceToMove.ID.value > 0 && moveUp)
            {
                NodeData_BaseContainer tmp01 = tmp[choiceToMove.ID.value];
                NodeData_BaseContainer tmp02 = tmp[choiceToMove.ID.value - 1];

                tmp[choiceToMove.ID.value] = tmp02;
                tmp[choiceToMove.ID.value - 1] = tmp01;

                VisualElement p01 = ports[choiceToMove.ID.value];
                VisualElement p02 = ports[choiceToMove.ID.value - 1];

                ports[choiceToMove.ID.value] = p02;
                ports[choiceToMove.ID.value - 1] = p01;

            }
            else if (choiceToMove.ID.value < tmp.Count - 1 && !moveUp)
            {
                NodeData_BaseContainer tmp01 = tmp[choiceToMove.ID.value];
                NodeData_BaseContainer tmp02 = tmp[choiceToMove.ID.value + 1];

                tmp[choiceToMove.ID.value] = tmp02;
                tmp[choiceToMove.ID.value + 1] = tmp01;

                VisualElement p01 = ports[choiceToMove.ID.value];
                VisualElement p02 = ports[choiceToMove.ID.value + 1];

                ports[choiceToMove.ID.value] = p02;
                ports[choiceToMove.ID.value + 1] = p01;
            }

            ChoiceData.choices.Clear();
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
            for (int i = 0; i < ChoiceData.choices.Count; i++)
            {
                NodeBuilder.ShowHide(ChoiceData.choices[i].conditions.Count > 0, ChoiceData.choices[i].choiceStateEnumBox);
            }
        }

        private void ShowHideNewChoiceBtn()
        {
            NodeBuilder.ShowHide(ChoiceData.choices.Count < Window.NbMaxChoices, newChoiceBtn);

            //Si on n'a qu'un seul choix, pas la peine d'afficher les petits boutons
            for (int i = 0; i < boxesButtons.Count; i++)
            {
                NodeBuilder.ShowHide(ChoiceData.choices.Count > 1, boxesButtons[i]);
            }
        }

        private void RenameChoices()
        {
            for (int i = 0; i < ChoiceData.choices.Count; i++)
            {
                ChoiceData.choices[i].choiceContainer.Q<Label>().text = $"Choice n°{(i + 1).ToString()}";
            }
        }

        public override void ReloadLanguage()
        {
            base.ReloadLanguage();
        }

        public override void LoadValueIntoField()
        {
            for (int i = 0; i < ChoiceData.choices.Count; i++)
            {
                if (ChoiceData.choices[i].choiceStateType.enumField != null)
                    ChoiceData.choices[i].choiceStateType.enumField.SetValueWithoutNotify(ChoiceData.choices[i].choiceStateType.value);
            }
            
        }


        #endregion
    }
}