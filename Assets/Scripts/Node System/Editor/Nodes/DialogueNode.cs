using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Project.NodeSystem.Editor
{
    public class DialogueNode : BaseNode
    {
        #region Fields

        private DialogueData dialogueData = new DialogueData();
        public DialogueData DialogueData { get => dialogueData; set => dialogueData = value; }

        private List<Box> boxes = new List<Box>();  //On les garde en mémoire pour les réarranger avec les flèches

        #endregion


        #region Methods




        #region Constructor

        public DialogueNode() : base("Dialogue Node", Vector2.zero, null, null)
        {

        }


        public DialogueNode(Vector2 position, DialogueEditorWindow window, DialogueGraphView graphView) : base("Dialogue Node", position, window, graphView)
        {

            StyleSheet styleSheet = Resources.Load<StyleSheet>("USS/Nodes/DialogueNodeStyleSheet");
            styleSheets.Add(styleSheet);


            AddPort("Input", Direction.Input, Port.Capacity.Multi);
            AddPort("Output", Direction.Output, Port.Capacity.Single);


            TopButton();


            #region Display

            //elementsToGreyOut.Add(characterLabel);
            //elementsToGreyOut.Add(facePreview);
            //elementsToGreyOut.Add(moodLabel);
            //elementsToGreyOut.Add(faceDirectionLabel);
            //elementsToGreyOut.Add(sideLabel);
            //elementsToGreyOut.Add(textsLabel);
            //elementsToGreyOut.Add(clipsLabel);

            //elementsToGreyOut.Add(moodField);
            //elementsToGreyOut.Add(faceDirectionField);
            //elementsToGreyOut.Add(sidePlacementField);
            //elementsToGreyOut.Add(clipsField);
            //elementsToGreyOut.Add(textsField);
            //elementsToGreyOut.Add(choiceBtn);

            ////Grise les éléments tant qu'on n'a pas de DialogueCharacterSO à visualiser
            //GreyOutElements();

            //mainContainer.Add(characterLabel);
            //mainContainer.Add(characterField);

            //mainContainer.Add(nameLabel);
            //mainContainer.Add(nameField);

            //mainContainer.Add(faceLabel);
            //mainContainer.Add(faceField);
            //mainContainer.Add(facePreview);

            //mainContainer.Add(moodLabel);
            //mainContainer.Add(moodField);

            //mainContainer.Add(faceDirectionLabel);
            //mainContainer.Add(faceDirectionField);

            //mainContainer.Add(sideLabel);
            //mainContainer.Add(sidePlacementField);

            //mainContainer.Add(clipsLabel);
            //mainContainer.Add(clipsField);

            //mainContainer.Add(textsLabel);
            //mainContainer.Add(textsField);

            //titleButtonContainer.Add(choiceBtn);


            ////On appelle ces fonctions pour mettre à jour le visuel de la Node
            //RefreshExpandedState();
            //RefreshPorts();

            #endregion


        }


        private void TopButton()
        {
            AddPortButton();
            AddDropdownMenu();

        }

        private void AddPortButton()
        {
            Button btn = new Button(() => { AddChoicePort(this); }) { text = "Add Choice" };
            btn.AddToClassList("TopBtn");

            titleButtonContainer.Add(btn);
        }



        private void AddDropdownMenu()
        {
            ToolbarMenu Menu = new ToolbarMenu();
            Menu.text = "Add Content";

            Menu.menu.AppendAction("Character", new Action<DropdownMenuAction>(x => AddCharacter()));
            Menu.menu.AppendAction("Text", new Action<DropdownMenuAction>(x => AddTextLine()));

            titleButtonContainer.Add(Menu);
        }

        #endregion




        #region Ports

        // Port ---------------------------------------------------------------------------------------

        public Port AddChoicePort(BaseNode baseNode, DialogueData_Port dialogueData_Port = null)
        {
            Port port = GetPortInstance(Direction.Output);
            DialogueData_Port newDialogue_Port = new DialogueData_Port();

            // Check if we load it in with values
            if (dialogueData_Port != null)
            {
                newDialogue_Port.inputGuid = dialogueData_Port.inputGuid;
                newDialogue_Port.outputGuid = dialogueData_Port.outputGuid;
                newDialogue_Port.portGuid = dialogueData_Port.portGuid;
            }
            else
            {
                newDialogue_Port.portGuid = Guid.NewGuid().ToString();
            }



            // Delete button
            Button deleteButton = new Button(() => DeletePort(baseNode, port)) { text = "X" };
            port.contentContainer.Add(deleteButton);
            


            port.portName = newDialogue_Port.portGuid;                      // We use portName as port ID
            Label portNameLabel = port.contentContainer.Q<Label>("type");   // Get Labal in port that is used to contain the port name.
            portNameLabel.AddToClassList("PortName");                       // Here we add a uss class to it so we can hide it in the editor window.

            // Set color of the port.
            port.portColor = Color.yellow;

            DialogueData.ports.Add(newDialogue_Port);

            baseNode.outputContainer.Add(port);

            // Refresh
            baseNode.RefreshPorts();
            baseNode.RefreshExpandedState();

            return port;
        }



        private void DeletePort(BaseNode node, Port port)
        {
            DialogueData_Port tmp = DialogueData.ports.Find(findPort => findPort.portGuid == port.portName);
            DialogueData.ports.Remove(tmp);

            IEnumerable<Edge> portEdge = graphView.edges.ToList().Where(edge => edge.output == port);

            if (portEdge.Any())
            {
                Edge edge = portEdge.First();
                edge.input.Disconnect(edge);
                edge.output.Disconnect(edge);
                graphView.RemoveElement(edge);
            }

            node.outputContainer.Remove(port);

            // Refresh
            node.RefreshPorts();
            node.RefreshExpandedState();
        }


        #endregion



        #region Menu dropdown

        // Menu dropdown --------------------------------------------------------------------------------------


        //Appelé dans DialogueSaveLoad
        public void AddTextLine(DialogueData_Translation data_Translations = null)
        {
            DialogueData_Translation newDialogueBaseContainer_Translation = new DialogueData_Translation();
            DialogueData.baseContainers.Add(newDialogueBaseContainer_Translation);

            // Add Container Box
            Box boxContainer = new Box();
            boxContainer.AddToClassList("DialogueBox");

            // Add Fields
            AddLabelAndButton(newDialogueBaseContainer_Translation, boxContainer, "Text", "TextColor");
            AddAudioClips(newDialogueBaseContainer_Translation, boxContainer);
            AddTextField(newDialogueBaseContainer_Translation, boxContainer);

            // Load in data if it got any
            if (data_Translations != null)
            {
                // Guid ID
                newDialogueBaseContainer_Translation.guid = data_Translations.guid;

                // Text
                foreach (LanguageGeneric<string> data_text in data_Translations.texts)
                {
                    foreach (LanguageGeneric<string> text in newDialogueBaseContainer_Translation.texts)
                    {
                        if (text.language == data_text.language)
                        {
                            text.data = data_text.data;
                        }
                    }
                }

                // Audio
                foreach (LanguageGeneric<AudioClip> data_audioclip in data_Translations.audioClips)
                {
                    foreach (LanguageGeneric<AudioClip> audioclip in newDialogueBaseContainer_Translation.audioClips)
                    {
                        if (audioclip.language == data_audioclip.language)
                        {
                            audioclip.data = data_audioclip.data;
                        }
                    }
                }
            }
            else
            {
                // Make New Guid ID
                newDialogueBaseContainer_Translation.guid.value = Guid.NewGuid().ToString();
            }

            // Reaload the current selected language
            ReloadLanguage();


            mainContainer.Add(boxContainer);
        }

        //Appelé dans DialogueSaveLoad
        public void AddCharacter(DialogueData_CharacterSO data_Character = null)
        {
            DialogueData_CharacterSO dialogue_Character = new DialogueData_CharacterSO();
            if (data_Character != null)
            {
                dialogue_Character.character.value = data_Character.character.value;
                dialogue_Character.characterName.value = data_Character.characterName.value;
                dialogue_Character.mood.value = data_Character.mood.value;
                dialogue_Character.faceDirection.value = data_Character.faceDirection.value;
                dialogue_Character.sidePlacement.value = data_Character.sidePlacement.value;
                dialogue_Character.sprite.value = data_Character.sprite.value;
            }
            else
            {
                dialogue_Character.faceDirection.value = DialogueSide.Right;
                dialogue_Character.sidePlacement.value = DialogueSide.Left;
            }
            DialogueData.baseContainers.Add(dialogue_Character);

            Box boxContainer = new Box();
            boxContainer.AddToClassList("CharacterNameBox");


            AddLabelAndButton(dialogue_Character, boxContainer, "Image", "ImageColor");
            AddImages(dialogue_Character, boxContainer);
            AddTextField_CharacterInfo(dialogue_Character, boxContainer);

            mainContainer.Add(boxContainer);
        }


        //Appelé dans DialogueSaveLoad
        public void CharacterName(DialogueData_CharacterSO data_Name = null)
        {
            DialogueData_CharacterSO dialogue_Name = new DialogueData_CharacterSO();
            if (data_Name != null)
            {
                dialogue_Name.characterName.value = data_Name.characterName.value;
            }
            DialogueData.baseContainers.Add(dialogue_Name);

            Box boxContainer = new Box();
            boxContainer.AddToClassList("CharacterNameBox");

            AddLabelAndButton(dialogue_Name, boxContainer, "Name", "NameColor");
            AddTextField_CharacterInfo(dialogue_Name, boxContainer);

            mainContainer.Add(boxContainer);
        }


        #endregion


        #region Fields

        // Fields --------------------------------------------------------------------------------------

        private void AddLabelAndButton(DialogueData_BaseContainer container, Box boxContainer, string labelName, string uniqueUSS = "")
        {
            Box topBoxContainer = new Box();
            topBoxContainer.AddToClassList("TopBox");

            Box buttonsBox = new Box();
            buttonsBox.AddToClassList("BtnBox");



            // Label Name
            Label label_texts = NewLabel(labelName, "LabelText", uniqueUSS);

            // Move up button.
            Action onClicked = () =>
            {
                MoveBox(container, true);
            };
            Button moveUpBtn = NewButton("", onClicked, "MoveUpBtn");

            // Move down button.
            onClicked = () =>
            {
                MoveBox(container, false);
            };
            Button moveDownBtn = NewButton("", onClicked, "MoveDownBtn");

            // Remove button.
            onClicked = () =>
            {
                DeleteBox(boxContainer);
                boxes.Remove(boxContainer);
                DialogueData.baseContainers.Remove(container);
            };
            Button removeBtn = NewButton("X", onClicked, "RemoveBtn");

            boxes.Add(boxContainer);
            buttonsBox.Add(moveUpBtn);
            buttonsBox.Add(moveDownBtn);
            buttonsBox.Add(removeBtn);

            topBoxContainer.Add(label_texts);
            topBoxContainer.Add(buttonsBox);

            boxContainer.Add(topBoxContainer);
        }

        private void MoveBox(DialogueData_BaseContainer container, bool moveUp)
        {
            List<DialogueData_BaseContainer> tmpDialogue_BaseContainers = new List<DialogueData_BaseContainer>();
            tmpDialogue_BaseContainers.AddRange(dialogueData.baseContainers);

            foreach (Box item in boxes)
            {
                mainContainer.Remove(item);
            }

            boxes.Clear();

            for (int i = 0; i < tmpDialogue_BaseContainers.Count; i++)
            {
                tmpDialogue_BaseContainers[i].ID.value = i;
            }

            if (container.ID.value > 0 && moveUp)
            {
                DialogueData_BaseContainer tmp01 = tmpDialogue_BaseContainers[container.ID.value];
                DialogueData_BaseContainer tmp02 = tmpDialogue_BaseContainers[container.ID.value - 1];

                tmpDialogue_BaseContainers[container.ID.value] = tmp02;
                tmpDialogue_BaseContainers[container.ID.value - 1] = tmp01;
            }
            else if (container.ID.value < tmpDialogue_BaseContainers.Count - 1 && !moveUp)
            {
                DialogueData_BaseContainer tmp01 = tmpDialogue_BaseContainers[container.ID.value];
                DialogueData_BaseContainer tmp02 = tmpDialogue_BaseContainers[container.ID.value + 1];

                tmpDialogue_BaseContainers[container.ID.value] = tmp02;
                tmpDialogue_BaseContainers[container.ID.value + 1] = tmp01;
            }

            dialogueData.baseContainers.Clear();

            foreach (DialogueData_BaseContainer data in tmpDialogue_BaseContainers)
            {
                switch (data)
                {
                    case DialogueData_CharacterSO Character:
                        AddCharacter(Character);
                        break;
                    case DialogueData_Translation Translation:
                        AddTextLine(Translation);
                        break;
                    default:
                        break;
                }
            }
        }




        private void AddTextField_CharacterInfo(DialogueData_CharacterSO container, Box boxContainer)
        {
            ObjectField characterField = NewCharacterField(container, container.character, "Character");
            boxContainer.Add(characterField);

            TextField nameField = NewTextField(container.characterName, "Name", "CharacterName", "TextStretch");
            nameField.SetEnabled(false);
            container.nameField = nameField;

            EnumField moodEnumField = NewCharacterMoodField(container, container.mood, "EnumField", "CharacterMood");
            EnumField faceDirEnumField = NewDialogueSideField(container.faceDirection, "EnumField", "CharacterFaceDirection");
            EnumField sidePlacementEnumField = NewDialogueSideField(container.sidePlacement, "EnumField", "CharacterSidePlacement");


            Label moodLabel = NewLabel("Mood", "EnumLabel");
            Label faceDirLabel = NewLabel("Face Direction", "EnumLabel");
            Label sideLabel = NewLabel("Side Placement", "EnumLabel");

            boxContainer.Add(nameField);
            boxContainer.Add(moodLabel);
            boxContainer.Add(moodEnumField);
            boxContainer.Add(faceDirLabel);
            boxContainer.Add(faceDirEnumField);
            boxContainer.Add(sideLabel);
            boxContainer.Add(sidePlacementEnumField);
        }

        private void AddTextField(DialogueData_Translation container, Box boxContainer)
        {
            TextField textField = NewTextLanguagesField(container.texts, "Write your dialogue here...", "TextBox", "TextStretch");

            container.TextField = textField;

            boxContainer.Add(textField);
        }

        private void AddAudioClips(DialogueData_Translation container, Box boxContainer)
        {
            ObjectField objectField = NewAudioClipLanguagesField(container.audioClips, "AudioClip");

            container.AudioField = objectField;

            boxContainer.Add(objectField);
        }

        private void AddImages(DialogueData_CharacterSO container, Box boxContainer)
        {
            Box ImagePreviewBox = new Box();

            ImagePreviewBox.AddToClassList("BoxRow");

            // Set up Image Preview.
            Image faceImage = NewImage("ImagePreview");

            container.spriteField = faceImage;  //On le garde en mémoire pour quand on veut changer l'humeur du perso

            ImagePreviewBox.Add(faceImage);

            // Add to box container.
            boxContainer.Add(ImagePreviewBox);
        }


        #endregion





        // ------------------------------------------------------------------------------------------

        public override void ReloadLanguage()
        {
            base.ReloadLanguage();
        }

        public override void LoadValueIntoField()
        {

        }
    }



    #endregion
}
