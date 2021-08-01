using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Project.NodeSystem.Editor
{
    public class CharacterNode : BaseNode
    {
        #region Fields

        private CharacterData characterData = new CharacterData();
        public CharacterData CharacterData { get => characterData; set => characterData = value; }

        private List<Box> boxesButtons = new List<Box>();  //On les garde en m�moire pour les r�arranger avec les fl�ches

        #endregion


        #region Methods




        #region Constructor

        public CharacterNode() : base("Character Node", Vector2.zero)
        {

        }


        public CharacterNode(Vector2 position, DialogueEditorWindow window, DialogueGraphView graphView) : base("Character Node", position, window, graphView, "CharacterNodeStyleSheet")
        {
            NodeBuilder.AddPort(this, "Input", Direction.Input, Port.Capacity.Multi);
            NodeBuilder.AddPort(this, "Output", Direction.Output, Port.Capacity.Single);

            TopButton();


            //Cr�e un perso par d�faut pour que le DialogueManager �vite de planter si on n'a pas de choix.
            //Retir� au moment du chargement dans DialogueSaveLoad pour �viter de cr�er un perso suppl�mentaire.
            AddCharacter();
        }


        private void TopButton()
        {
            NodeBuilder.NewTitleButton(this, "New Character", () => AddCharacter(), "TopBtn");
        }


        #endregion




        #region Menu dropdown

        // Menu dropdown --------------------------------------------------------------------------------------



        //Appel� dans DialogueSaveLoad
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
                newCharacter.useAutoDelay.Value = character.useAutoDelay.Value;
                newCharacter.autoDelayDuration.Value = character.autoDelayDuration.Value;
            }
            else
            {
                newCharacter.FaceDirection.Value = DialogueSide.Right;
                newCharacter.SidePlacement.Value = DialogueSide.Left;
            }
            CharacterData.Characters.Add(newCharacter);

            newCharacter.BoxContainer = NodeBuilder.NewBox(mainContainer, "CharacterNameBox");


            AddLabelAndButton(newCharacter, newCharacter.BoxContainer, "Image", "ImageColor");
            AddImages(newCharacter, newCharacter.BoxContainer);
            AddCharacterInfo(newCharacter, newCharacter.BoxContainer);

        }


        #endregion


        #region Fields

        // Fields --------------------------------------------------------------------------------------

        private void AddLabelAndButton(CharacterData_CharacterSO character, Box boxContainer, string labelName, string uniqueUSS = "")
        {
            Box topBoxContainer = NodeBuilder.NewBox(boxContainer, "TopBox");

            // Label Name
            Label label_texts = NodeBuilder.NewLabel(topBoxContainer, labelName, "LabelText", uniqueUSS);

            //Le conteneur des boutons
            Box buttonsBox = NodeBuilder.NewBox(topBoxContainer, "BtnBox");



            //Si on n'a qu'un seul perso, pas la peine d'afficher les petits boutons
            boxesButtons.Add(buttonsBox);
            for (int i = 0; i < boxesButtons.Count; i++)
            {
                NodeBuilder.ShowHide(CharacterData.Characters.Count > 1, boxesButtons[i]);
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

            //Character SO
            boxContainer.Add(NodeBuilder.NewCharacterField(this, character, character.Character, "Character"));
            boxContainer.Add(nameField);


            //Mood
            Action onMoodChanged = () =>
            {
                if (character.Character.Value != null)
                {
                    //Quand on change d'humeur, on affiche le sprite correspondant
                    character.Sprite.Value = character.Character.Value.GetFaceFromMood(character.Mood.Value);
                    character.SpriteField.image = character.Sprite.Value.texture;
                }
            };
            NodeBuilder.NewEnumField("Mood", boxContainer, character.Mood, onMoodChanged, "EnumField", "CharacterMood");

            //Dialogue Sides
            NodeBuilder.NewEnumField("Face Direction", boxContainer, character.FaceDirection, "EnumField", "CharacterFaceDirection");
            NodeBuilder.NewEnumField("Side Placement", boxContainer, character.SidePlacement, "EnumField", "CharacterSidePlacement");


            //Auto Delay Duration
            NodeBuilder.AddToggleFloatField(this, boxContainer, character.useAutoDelay, character.autoDelayDuration, "Use Auto Delay");
        }



        private void AddImages(CharacterData_CharacterSO character, Box boxContainer)
        {
            Box ImagePreviewBox = NodeBuilder.NewBox(boxContainer, "BoxRow");

            // Set up Image Preview.
            Image faceImage = NodeBuilder.NewImage(ImagePreviewBox, "ImagePreview");

            character.SpriteField = faceImage;  //On le garde en m�moire pour quand on veut changer l'humeur du perso

        }



        private void ShouldShowHideMoveButtons()
        {
            //Si on n'a qu'un seul perso, pas la peine d'afficher les petits boutons
            for (int i = 0; i < boxesButtons.Count; i++)
            {
                NodeBuilder.ShowHide(CharacterData.Characters.Count > 1, boxesButtons[i]);
            }
        }


        #endregion






        // ------------------------------------------------------------------------------------------

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

        public override void LoadValueIntoField()
        {

        }


    }



    #endregion
}
