using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Project.NodeSystem.Editor
{
    public class RepliqueNode : BaseNode
    {
        #region Fields

        public RepliqueData RepliqueData { get; set; } = new RepliqueData();

        private List<Box> _boxesButtons = new List<Box>();   //To hide the Up and Down buttons

        #endregion


        #region Methods




        #region Constructor

        public RepliqueNode() : base("Replique", Vector2.zero)
        {

        }


        public RepliqueNode(Vector2 position, DialogueEditorWindow window, DialogueGraphView graphView) : base("Replique", position, window, graphView, "RepliqueNodeStyleSheet")
        {
            NodeBuilder.AddPort(this, "Input", Direction.Input, Port.Capacity.Multi);
            NodeBuilder.AddPort(this, "Output", Direction.Output, Port.Capacity.Single);

            TopButton();

            //Creates a text field by default so that the DialogueManager doesn't crash.
            //Removed in DialogueSaveLoad to avoid creating an extra field.
            AddReplique();

        }


        #endregion







        #region Add Replique

        /// <summary>
        /// Add New Replique Button
        /// </summary>
        private void TopButton()
        {
            NodeBuilder.NewTitleButton(this, "New Replique", () => AddReplique(), "TopBtn");
        }


        /// <summary>
        /// Creates a new replique
        /// </summary>
        /// <param name="replique">If null, creates an empty replique, otherwise, assign it to the created field.</param>
        public void AddReplique(RepliqueData_Replique replique = null)
        {
            RepliqueData_Replique newReplique = new RepliqueData_Replique();

            if(replique != null)
            {
                newReplique.AppendToText.Value = replique.AppendToText.Value;
                newReplique.CanClickOnContinue.Value = replique.CanClickOnContinue.Value;
                newReplique.OverrideWriteSpeed.Value = replique.OverrideWriteSpeed.Value;
                newReplique.WriteSpeed.Value = replique.WriteSpeed.Value;
                newReplique.UseAutoDelay.Value = replique.UseAutoDelay.Value;
                newReplique.AutoDelayDuration.Value = replique.AutoDelayDuration.Value;
            }
            else
            {
                newReplique.CanClickOnContinue.Value = true;
            }


            RepliqueData.Repliques.Add(newReplique);

            // Add Container Box
            newReplique.BoxContainer = NodeBuilder.NewBox(mainContainer, "DialogueBox");


            // Add Fields
            AddLabelAndButton(newReplique, newReplique.BoxContainer);
            AddAudioClips(newReplique, newReplique.BoxContainer);
            AddTextField(newReplique, newReplique.BoxContainer);
            AddWriteInfo(newReplique, newReplique.BoxContainer);


            // Load in data if it got any
            if (replique != null)
            {
                // Guid ID
                newReplique.Guid = replique.Guid;

                // Text
                foreach (LanguageGeneric<string> data_text in replique.Texts)
                {
                    foreach (LanguageGeneric<string> text in newReplique.Texts)
                    {
                        if (text.Language == data_text.Language)
                        {
                            text.Data = data_text.Data;
                        }
                    }
                }

                // Audio
                foreach (LanguageGeneric<AudioClip> data_audioclip in replique.AudioClips)
                {
                    foreach (LanguageGeneric<AudioClip> audioclip in newReplique.AudioClips)
                    {
                        if (audioclip.Language == data_audioclip.Language)
                        {
                            audioclip.Data = data_audioclip.Data;
                        }
                    }
                }
            }
            else
            {
                // Make New Guid ID
                newReplique.Guid.Value = Guid.NewGuid().ToString();
            }


            // Reaload the current selected language fields
            ReloadLanguage();
        }

      
        #endregion




        #region Fields
        


        private void AddLabelAndButton(RepliqueData_Replique replique, Box boxContainer)
        {
            Box topBoxContainer = NodeBuilder.NewBox(boxContainer, "TopBox");

            // Label Name
            NodeBuilder.NewLabel(topBoxContainer, "Text", "Label", "LabelColor");

            //button container
            Box buttonsBox = NodeBuilder.NewBox(topBoxContainer, "BtnBox", "LabelBtn");



            //If there is only one text line, no need to display the buttons
            _boxesButtons.Add(buttonsBox);
            for (int i = 0; i < _boxesButtons.Count; i++)
            {
                NodeBuilder.ShowHide(RepliqueData.Repliques.Count > 1, _boxesButtons[i]);
            }


            // Move up button.
            Action onClicked = () =>
            {
                MoveBox(replique, true);
            };
            Button moveUpBtn = NodeBuilder.NewButton(buttonsBox, "", onClicked, "MoveUpBtn");

            // Move down button.
            onClicked = () =>
            {
                MoveBox(replique, false);
            };
            Button moveDownBtn = NodeBuilder.NewButton(buttonsBox, "", onClicked, "MoveDownBtn");

            // Remove button.
            onClicked = () =>
            {
                DeleteBox(boxContainer);
                RepliqueData.Repliques.Remove(replique);
                ShowHideMoveButtons();
            };
            Button removeBtn = NodeBuilder.NewButton(buttonsBox, "X", onClicked, "RemoveBtn");


        }

        public override void MoveBox(NodeData_BaseContainer replique, bool moveUp)
        {
            List<NodeData_BaseContainer> tmp = new List<NodeData_BaseContainer>();
            tmp.AddRange(RepliqueData.Repliques);


            for (int i = 0; i < tmp.Count; i++)
            {
                DeleteBox(RepliqueData.Repliques[i].BoxContainer);
                tmp[i].ID.Value = i;
            }

            if (replique.ID.Value > 0 && moveUp)
            {
                NodeData_BaseContainer tmp01 = tmp[replique.ID.Value];
                NodeData_BaseContainer tmp02 = tmp[replique.ID.Value - 1];

                tmp[replique.ID.Value] = tmp02;
                tmp[replique.ID.Value - 1] = tmp01;
            }
            else if (replique.ID.Value < tmp.Count - 1 && !moveUp)
            {
                NodeData_BaseContainer tmp01 = tmp[replique.ID.Value];
                NodeData_BaseContainer tmp02 = tmp[replique.ID.Value + 1];

                tmp[replique.ID.Value] = tmp02;
                tmp[replique.ID.Value + 1] = tmp01;
            }

            RepliqueData.Repliques.Clear();

            foreach (NodeData_BaseContainer data in tmp)
            {
                AddReplique(data as RepliqueData_Replique);
            }
        }



        private void AddTextField(RepliqueData_Replique replique, Box boxContainer)
        {
            TextField textField = NodeBuilder.NewTextLanguagesField(this, boxContainer, replique.Texts, "Write your dialogue here...", "TextBox", "TextStretch");
            replique.TextField = textField;
        }

        private void AddAudioClips(RepliqueData_Replique replique, Box boxContainer)
        {
            ObjectField objectField = NodeBuilder.NewAudioClipLanguagesField(this, boxContainer, replique.AudioClips, "AudioClip");
            replique.AudioField = objectField;
        }

        private void AddWriteInfo(RepliqueData_Replique replique, Box boxContainer)
        {
            //Append to text
            NodeBuilder.NewToggle("Append To Previous Text", NodeBuilder.NewBox(boxContainer, "BoxRow"), replique.AppendToText, "Toggle");

            //Steal control from player
            NodeBuilder.NewToggle("Can Click On Continue", NodeBuilder.NewBox(boxContainer, "BoxRow"), replique.CanClickOnContinue, "Toggle");


            //Override Write Speed
            NodeBuilder.AddToggleFloatField("Override Write Speed", this, boxContainer, replique.OverrideWriteSpeed, replique.WriteSpeed);

            //Auto Delay Duration
            NodeBuilder.AddToggleFloatField("Use Auto Delay", this, boxContainer, replique.UseAutoDelay, replique.AutoDelayDuration);

        }


        private void ShowHideMoveButtons()
        {
            //If there is only one text line, no need to display the buttons
            for (int i = 0; i < _boxesButtons.Count; i++)
            {
                NodeBuilder.ShowHide(RepliqueData.Repliques.Count > 1, _boxesButtons[i]);
            }
        }


        #endregion






    }



    #endregion
}
