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

        private List<Box> boxesButtons = new List<Box>();  //On les garde en mémoire pour les réarranger avec les flèches

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


            //Crée un perso par défaut pour que le DialogueManager évite de planter si on n'a pas de choix.
            //Retiré au moment du chargement dans DialogueSaveLoad pour éviter de créer un perso supplémentaire.
            AddCharacter();
        }


        private void TopButton()
        {
            NodeBuilder.NewTitleButton(this, "New Character", () => AddCharacter(), "TopBtn");
        }


        #endregion




        #region Menu dropdown

        // Menu dropdown --------------------------------------------------------------------------------------



        //Appelé dans DialogueSaveLoad
        public void AddCharacter(CharacterData_CharacterSO character = null)
        {
            CharacterData_CharacterSO newCharacter = new CharacterData_CharacterSO();
            if (character != null)
            {
                newCharacter.character.value = character.character.value;
                newCharacter.characterNames.AddRange(character.characterNames);
                newCharacter.characterName.value = character.characterName.value;
                newCharacter.mood.value = character.mood.value;
                newCharacter.faceDirection.value = character.faceDirection.value;
                newCharacter.sidePlacement.value = character.sidePlacement.value;
                newCharacter.sprite.value = character.sprite.value;
            }
            else
            {
                newCharacter.faceDirection.value = DialogueSide.Right;
                newCharacter.sidePlacement.value = DialogueSide.Left;
            }
            CharacterData.characters.Add(newCharacter);

            newCharacter.boxContainer = NodeBuilder.NewBox(mainContainer, "CharacterNameBox");


            AddLabelAndButton(newCharacter, newCharacter.boxContainer, "Image", "ImageColor");
            AddImages(newCharacter, newCharacter.boxContainer);
            AddCharacterInfo(newCharacter, newCharacter.boxContainer);

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
                NodeBuilder.ShowHide(CharacterData.characters.Count > 1, boxesButtons[i]);
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
                CharacterData.characters.Remove(character);
                ShouldShowHideMoveButtons();
            };
            Button removeBtn = NodeBuilder.NewButton(buttonsBox, "X", onClicked, "RemoveBtn");
        }

        public override void MoveBox(NodeData_BaseContainer character, bool moveUp)
        {
            List<NodeData_BaseContainer> tmp = new List<NodeData_BaseContainer>();
            tmp.AddRange(CharacterData.characters);


            for (int i = 0; i < tmp.Count; i++)
            {
                DeleteBox(CharacterData.characters[i].boxContainer);
                tmp[i].ID.value = i;
            }

            if (character.ID.value > 0 && moveUp)
            {
                NodeData_BaseContainer tmp01 = tmp[character.ID.value];
                NodeData_BaseContainer tmp02 = tmp[character.ID.value - 1];

                tmp[character.ID.value] = tmp02;
                tmp[character.ID.value - 1] = tmp01;
            }
            else if (character.ID.value < tmp.Count - 1 && !moveUp)
            {
                NodeData_BaseContainer tmp01 = tmp[character.ID.value];
                NodeData_BaseContainer tmp02 = tmp[character.ID.value + 1];

                tmp[character.ID.value] = tmp02;
                tmp[character.ID.value + 1] = tmp01;
            }

            CharacterData.characters.Clear();

            foreach (NodeData_BaseContainer data in tmp)
            {
                AddCharacter(data as CharacterData_CharacterSO);
            }
        }




        private void AddCharacterInfo(CharacterData_CharacterSO character, Box boxContainer)
        {

            TextField nameField = NodeBuilder.NewTextField(character.characterName, "Name", "CharacterName", "TextStretch");
            nameField.SetEnabled(false);
            character.nameField = nameField;

            boxContainer.Add(NodeBuilder.NewCharacterField(this, character, character.character, "Character"));
            boxContainer.Add(nameField);
            boxContainer.Add(NodeBuilder.NewLabel("Mood", "EnumLabel"));
            boxContainer.Add(NodeBuilder.NewCharacterMoodField(character, character.mood, "EnumField", "CharacterMood"));
            boxContainer.Add(NodeBuilder.NewLabel("Face Direction", "EnumLabel"));
            boxContainer.Add(NodeBuilder.NewDialogueSideField(character.faceDirection, "EnumField", "CharacterFaceDirection"));
            boxContainer.Add(NodeBuilder.NewLabel("Side Placement", "EnumLabel"));
            boxContainer.Add(NodeBuilder.NewDialogueSideField(character.sidePlacement, "EnumField", "CharacterSidePlacement"));
        }



        private void AddImages(CharacterData_CharacterSO character, Box boxContainer)
        {
            Box ImagePreviewBox = NodeBuilder.NewBox(boxContainer, "BoxRow");

            // Set up Image Preview.
            Image faceImage = NodeBuilder.NewImage(ImagePreviewBox, "ImagePreview");

            character.spriteField = faceImage;  //On le garde en mémoire pour quand on veut changer l'humeur du perso

        }



        private void ShouldShowHideMoveButtons()
        {
            //Si on n'a qu'un seul perso, pas la peine d'afficher les petits boutons
            for (int i = 0; i < boxesButtons.Count; i++)
            {
                NodeBuilder.ShowHide(CharacterData.characters.Count > 1, boxesButtons[i]);
            }
        }


        #endregion






        // ------------------------------------------------------------------------------------------

        public override void ReloadLanguage()
        {
            base.ReloadLanguage();

            for (int i = 0; i < CharacterData.characters.Count; i++)
            {
                var tmp = CharacterData.characters[i];
                if (tmp.nameField != null && tmp.characterNames.Count > 0)
                {
                    tmp.nameField.SetValueWithoutNotify(tmp.characterNames[(int)Window.SelectedLanguage]);
                }
            }
        }

        public override void LoadValueIntoField()
        {

        }


    }



    #endregion
}
