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

        public DialogueNode() : base("Dialogue Node", Vector2.zero)
        {

        }


        public DialogueNode(Vector2 position, DialogueEditorWindow window, DialogueGraphView graphView) : base("Dialogue Node", position, window, graphView, "DialogueNodeStyleSheet")
        {
            NodeBuilder.AddPort(this, "Input", Direction.Input, Port.Capacity.Multi);
            NodeBuilder.AddPort(this, "Output", Direction.Output, Port.Capacity.Single);

            TopButton();
        }


        private void TopButton()
        {
            AddPortButton();
            AddDropdownMenu();

        }

        private void AddPortButton()
        {
            Button btn = NodeBuilder.NewTitleButton(this, "Add Choice", () => AddChoicePort(this), "TopBtn");
        }



        private void AddDropdownMenu()
        {
            ToolbarMenu tm = NodeBuilder.NewToolbar(this, "Add Content");
            tm.AddMenuActions
                (
                    ("Character", new Action<DropdownMenuAction>(x => AddCharacter())),
                    ("Text", new Action<DropdownMenuAction>(x => AddTextLine()))
                );
        }

        #endregion




        #region Ports

        // Port ---------------------------------------------------------------------------------------

        public Port AddChoicePort(BaseNode baseNode, NodeData_Port dialogueData_Port = null)
        {
            Port port = NodeBuilder.GetPortInstance(this, Direction.Output);
            NodeData_Port newDialogue_Port = new NodeData_Port();

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
            Button deleteButton = NodeBuilder.NewButton(port.contentContainer, "X", () => 
            {
                NodeBuilder.DeleteChoicePort(this, port);
                NodeData_Port tmp = DialogueData.ports.Find(findPort => findPort.portGuid == port.portName);
                DialogueData.ports.Remove(tmp);
            });
            


            port.portName = newDialogue_Port.portGuid;                      // We use portName as port ID
            Label portNameLabel = port.contentContainer.Q<Label>("type");   // Get Labal in port that is used to contain the port name.
            portNameLabel.AddStyle("PortName");                       // Here we add a uss class to it so we can hide it in the editor window.


            DialogueData.ports.Add(newDialogue_Port);

            baseNode.outputContainer.Add(port);

            // Refresh
            baseNode.RefreshPorts();
            baseNode.RefreshExpandedState();

            return port;
        }



        #endregion



        #region Menu dropdown

        // Menu dropdown --------------------------------------------------------------------------------------


        //Appelé dans DialogueSaveLoad
        public void AddTextLine(DialogueData_Repliques translation = null)
        {
            DialogueData_Repliques newDialogueBaseContainer_Translation = new DialogueData_Repliques();


            DialogueData.baseContainers.Add(newDialogueBaseContainer_Translation);

            // Add Container Box
            Box boxContainer = NodeBuilder.NewBox(mainContainer, "DialogueBox");


            // Add Fields
            AddLabelAndButton(newDialogueBaseContainer_Translation, boxContainer, "Text", "TextColor");
            AddAudioClips(newDialogueBaseContainer_Translation, boxContainer);
            AddTextField(newDialogueBaseContainer_Translation, boxContainer);


            // Load in data if it got any
            if (translation != null)
            {
                // Guid ID
                newDialogueBaseContainer_Translation.guid = translation.guid;


                // Text
                foreach (LanguageGeneric<string> data_text in translation.texts)
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
                foreach (LanguageGeneric<AudioClip> data_audioclip in translation.audioClips)
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


            // Reaload the current selected language fields
            ReloadLanguage();
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

            Box boxContainer = NodeBuilder.NewBox(mainContainer, "CharacterNameBox");


            AddLabelAndButton(dialogue_Character, boxContainer, "Image", "ImageColor");
            AddImages(dialogue_Character, boxContainer);
            AddTextField_CharacterInfo(dialogue_Character, boxContainer);

        }


        #endregion


        #region Fields

        // Fields --------------------------------------------------------------------------------------

        private void AddLabelAndButton(NodeData_BaseContainer container, Box boxContainer, string labelName, string uniqueUSS = "")
        {
            Box topBoxContainer = NodeBuilder.NewBox(boxContainer, "TopBox");

            // Label Name
            Label label_texts = NodeBuilder.NewLabel(topBoxContainer, labelName, "LabelText", uniqueUSS);

            //Le conteneur des boutons
            Box buttonsBox = NodeBuilder.NewBox(topBoxContainer, "BtnBox");




            // Move up button.
            Action onClicked = () =>
            {
                MoveBox(container, true);
            };
            Button moveUpBtn = NodeBuilder.NewButton(buttonsBox, "", onClicked, "MoveUpBtn");

            // Move down button.
            onClicked = () =>
            {
                MoveBox(container, false);
            };
            Button moveDownBtn = NodeBuilder.NewButton(buttonsBox, "", onClicked, "MoveDownBtn");

            // Remove button.
            onClicked = () =>
            {
                DeleteBox(boxContainer);
                boxes.Remove(boxContainer);
                DialogueData.baseContainers.Remove(container);
            };
            Button removeBtn = NodeBuilder.NewButton(buttonsBox, "X", onClicked, "RemoveBtn");

            boxes.Add(boxContainer);

        }

        private void MoveBox(NodeData_BaseContainer container, bool moveUp)
        {
            List<NodeData_BaseContainer> tmpDialogue_BaseContainers = new List<NodeData_BaseContainer>();
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
                NodeData_BaseContainer tmp01 = tmpDialogue_BaseContainers[container.ID.value];
                NodeData_BaseContainer tmp02 = tmpDialogue_BaseContainers[container.ID.value - 1];

                tmpDialogue_BaseContainers[container.ID.value] = tmp02;
                tmpDialogue_BaseContainers[container.ID.value - 1] = tmp01;
            }
            else if (container.ID.value < tmpDialogue_BaseContainers.Count - 1 && !moveUp)
            {
                NodeData_BaseContainer tmp01 = tmpDialogue_BaseContainers[container.ID.value];
                NodeData_BaseContainer tmp02 = tmpDialogue_BaseContainers[container.ID.value + 1];

                tmpDialogue_BaseContainers[container.ID.value] = tmp02;
                tmpDialogue_BaseContainers[container.ID.value + 1] = tmp01;
            }

            dialogueData.baseContainers.Clear();

            foreach (NodeData_BaseContainer data in tmpDialogue_BaseContainers)
            {
                switch (data)
                {
                    case DialogueData_CharacterSO Character:
                        AddCharacter(Character);
                        break;
                    case DialogueData_Repliques Translation:
                        AddTextLine(Translation);
                        break;
                    default:
                        break;
                }
            }
        }




        private void AddTextField_CharacterInfo(DialogueData_CharacterSO container, Box boxContainer)
        {

            TextField nameField = NodeBuilder.NewTextField(container.characterName, "Name", "CharacterName", "TextStretch");
            nameField.SetEnabled(false);
            container.nameField = nameField;

            boxContainer.Add(NodeBuilder.NewCharacterField(container, container.character, "Character"));
            boxContainer.Add(nameField);
            boxContainer.Add(NodeBuilder.NewLabel("Mood", "EnumLabel"));
            boxContainer.Add(NodeBuilder.NewCharacterMoodField(container, container.mood, "EnumField", "CharacterMood"));
            boxContainer.Add(NodeBuilder.NewLabel("Face Direction", "EnumLabel"));
            boxContainer.Add(NodeBuilder.NewDialogueSideField(container.faceDirection, "EnumField", "CharacterFaceDirection"));
            boxContainer.Add(NodeBuilder.NewLabel("Side Placement", "EnumLabel"));
            boxContainer.Add(NodeBuilder.NewDialogueSideField(container.sidePlacement, "EnumField", "CharacterSidePlacement"));
        }

        private void AddTextField(DialogueData_Repliques container, Box boxContainer)
        {
            TextField textField = NodeBuilder.NewTextLanguagesField(this, boxContainer, container.texts, "Write your dialogue here...", "TextBox", "TextStretch");
            container.TextField = textField;
        }

        private void AddAudioClips(DialogueData_Repliques container, Box boxContainer)
        {
            ObjectField objectField = NodeBuilder.NewAudioClipLanguagesField(this, boxContainer, container.audioClips, "AudioClip");
            container.AudioField = objectField;
        }

        private void AddImages(DialogueData_CharacterSO container, Box boxContainer)
        {
            Box ImagePreviewBox = NodeBuilder.NewBox(boxContainer, "BoxRow");

            // Set up Image Preview.
            Image faceImage = NodeBuilder.NewImage(ImagePreviewBox, "ImagePreview");

            container.spriteField = faceImage;  //On le garde en mémoire pour quand on veut changer l'humeur du perso

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
