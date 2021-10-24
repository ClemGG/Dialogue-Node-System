using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Project.NodeSystem.Editor
{
    public class CharacterNode : BaseNode
    {
        #region Fields

        public CharacterData CharacterData { get; set; } = new CharacterData();

        private List<Box> _boxesButtons = new List<Box>();  //To hide the Up and Down buttons

        #endregion


        #region Methods




        #region Constructor

        public CharacterNode() : base("Character", Vector2.zero)
        {

        }


        public CharacterNode(Vector2 position, DialogueEditorWindow window, DialogueGraphView graphView) : base("Character", position, window, graphView, "CharacterNodeStyleSheet")
        {
            NodeBuilder.AddPort(this, "Input", Direction.Input, Port.Capacity.Multi);
            NodeBuilder.AddPort(this, "Output", Direction.Output, Port.Capacity.Single);

            TopButton();

            //Creates a character slot by default so that the DialogueManager doesn't crash.
            //Removed in DialogueSaveLoad to avoid creating an extra field.
            AddCharacter();
        }



        #endregion




        #region Add Character


        /// <summary>
        /// Add New Character button
        /// </summary>
        private void TopButton()
        {
            NodeBuilder.NewTitleButton(this, "New Character", () => AddCharacter(), "TopBtn");
        }



        //Called in DialogueSaveLoad
        public void AddCharacter(CharacterData_CharacterSO character = null)
        {
            CharacterData_CharacterSO newCharacter = new CharacterData_CharacterSO();
            if (character != null)
            {
                newCharacter.Character.Value = character.Character.Value;
                newCharacter.CharacterNames.AddRange(character.CharacterNames);
                newCharacter.CharacterName.Value = character.CharacterName.Value;
                newCharacter.Mood.Value = character.Mood.Value;
                newCharacter.FaceDirection.Value = character.FaceDirection.Value;
                newCharacter.SidePlacement.Value = character.SidePlacement.Value;
                newCharacter.Sprite.Value = character.Sprite.Value;
                newCharacter.UseAutoDelay.Value = character.UseAutoDelay.Value;
                newCharacter.AutoDelayDuration.Value = character.AutoDelayDuration.Value;
            }
            else
            {
                newCharacter.FaceDirection.Value = DialogueSide.Right;
                newCharacter.SidePlacement.Value = DialogueSide.Left;
            }
            CharacterData.Characters.Add(newCharacter);

            newCharacter.BoxContainer = NodeBuilder.NewBox(mainContainer);


            AddLabelAndButton(newCharacter, newCharacter.BoxContainer);
            AddImage(newCharacter, newCharacter.BoxContainer);
            AddCharacterInfo(newCharacter, newCharacter.BoxContainer);

        }


        #endregion




        #region Fields

        // Fields --------------------------------------------------------------------------------------

        private void AddLabelAndButton(CharacterData_CharacterSO character, Box boxContainer)
        {
            Box topBoxContainer = NodeBuilder.NewBox(boxContainer, "TopBox");

            // Label Name
            NodeBuilder.NewLabel(topBoxContainer, "Character", "Label", "LabelColor");

            //Buttons container
            Box buttonsBox = NodeBuilder.NewBox(topBoxContainer, "BtnBox", "LabelBtn");



            //If there's only one character, no need to show the buttons
            _boxesButtons.Add(buttonsBox);
            for (int i = 0; i < _boxesButtons.Count; i++)
            {
                NodeBuilder.ShowHide(CharacterData.Characters.Count > 1, _boxesButtons[i]);
            }

            // Move up button.
            Action onClicked = () =>
            {
                MoveBox(character, true);
            };
            Button moveUpBtn = NodeBuilder.NewButton(buttonsBox, "", onClicked, "MoveUpBtn");

            // Move down button.
            onClicked = () =>
            {
                MoveBox(character, false);
            };
            Button moveDownBtn = NodeBuilder.NewButton(buttonsBox, "", onClicked, "MoveDownBtn");

            // Remove button.
            onClicked = () =>
            {
                DeleteBox(boxContainer);
                CharacterData.Characters.Remove(character);
                ShouldShowHideMoveButtons();
            };
            Button removeBtn = NodeBuilder.NewButton(buttonsBox, "X", onClicked, "RemoveBtn");
        }

        public override void MoveBox(NodeData_BaseContainer character, bool moveUp)
        {
            List<NodeData_BaseContainer> tmp = new List<NodeData_BaseContainer>();
            tmp.AddRange(CharacterData.Characters);


            for (int i = 0; i < tmp.Count; i++)
            {
                DeleteBox(CharacterData.Characters[i].BoxContainer);
                tmp[i].ID.Value = i;
            }

            if (character.ID.Value > 0 && moveUp)
            {
                NodeData_BaseContainer tmp01 = tmp[character.ID.Value];
                NodeData_BaseContainer tmp02 = tmp[character.ID.Value - 1];

                tmp[character.ID.Value] = tmp02;
                tmp[character.ID.Value - 1] = tmp01;
            }
            else if (character.ID.Value < tmp.Count - 1 && !moveUp)
            {
                NodeData_BaseContainer tmp01 = tmp[character.ID.Value];
                NodeData_BaseContainer tmp02 = tmp[character.ID.Value + 1];

                tmp[character.ID.Value] = tmp02;
                tmp[character.ID.Value + 1] = tmp01;
            }

            CharacterData.Characters.Clear();

            foreach (NodeData_BaseContainer data in tmp)
            {
                AddCharacter(data as CharacterData_CharacterSO);
            }
        }




        private void AddCharacterInfo(CharacterData_CharacterSO character, Box boxContainer)
        {
            //CharacterName
            TextField nameField = NodeBuilder.NewTextField(character.CharacterName, "", "CharacterName", "TextStretch");
            nameField.SetEnabled(false);
            character.NameField = nameField;
            boxContainer.Add(nameField);

            //Character SO
            boxContainer.Add(NodeBuilder.NewCharacterField(this, character, character.Character, "Character"));


            //Mood
            Action onMoodChanged = () =>
            {
                if (character.Character.Value != null)
                {
                    //When the mood is changed, we retrieve the corresponding sprite
                    character.Sprite.Value = character.Character.Value.GetFaceFromMood(character.Mood.Value);
                    character.SpriteField.image = character.Sprite.Value.texture;
                }
            };
            NodeBuilder.NewEnumField("Mood", boxContainer, character.Mood, onMoodChanged, "EnumField", "CharacterMood");

            //Dialogue Sides
            NodeBuilder.NewEnumField("Face Direction", boxContainer, character.FaceDirection, "EnumField", "CharacterFaceDirection");
            NodeBuilder.NewEnumField("Side Placement", boxContainer, character.SidePlacement, "EnumField", "CharacterSidePlacement");


            //Auto Delay Duration
            NodeBuilder.AddToggleFloatField("Use Auto Delay", this, boxContainer, character.UseAutoDelay, character.AutoDelayDuration);
        }



        private void AddImage(CharacterData_CharacterSO character, Box boxContainer)
        {
            Box ImagePreviewBox = NodeBuilder.NewBox(boxContainer, "BoxRow");

            // Set up Image Preview.
            Image faceImage = NodeBuilder.NewImage(ImagePreviewBox, "ImagePreview");

            character.SpriteField = faceImage;  //Kept in memory in case we change the mood of the character

        }



        private void ShouldShowHideMoveButtons()
        {
            //If there's only one character, no need to show the buttons

            for (int i = 0; i < _boxesButtons.Count; i++)
            {
                NodeBuilder.ShowHide(CharacterData.Characters.Count > 1, _boxesButtons[i]);
            }
        }


        #endregion




        #region On Load

        public override void ReloadLanguage()
        {
            base.ReloadLanguage();

            for (int i = 0; i < CharacterData.Characters.Count; i++)
            {
                var tmp = CharacterData.Characters[i];
                if (tmp.NameField != null && tmp.CharacterNames.Count > 0)
                {
                    tmp.NameField.SetValueWithoutNotify(tmp.CharacterNames[(int)Window.SelectedLanguage]);
                }
            }

        }

        #endregion

    }



    #endregion
}
