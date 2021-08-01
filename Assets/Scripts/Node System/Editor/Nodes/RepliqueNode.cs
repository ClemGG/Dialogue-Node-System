using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Project.NodeSystem.Editor
{
    public class RepliqueNode : BaseNode
    {
        #region Fields

        private RepliqueData repliqueData = new RepliqueData();
        public RepliqueData RepliqueData { get => repliqueData; set => repliqueData = value; }

        private List<Box> boxesButtons = new List<Box>();   //Pour cacher les boutons Up et Down

        #endregion


        #region Methods




        #region Constructor

        public RepliqueNode() : base("Replique Node", Vector2.zero)
        {

        }


        public RepliqueNode(Vector2 position, DialogueEditorWindow window, DialogueGraphView graphView) : base("Replique Node", position, window, graphView, "RepliqueNodeStyleSheet")
        {
            NodeBuilder.AddPort(this, "Input", Direction.Input, Port.Capacity.Multi);
            NodeBuilder.AddPort(this, "Output", Direction.Output, Port.Capacity.Single);

            TopButton();

            //Crée une réplique par défaut pour que le DialogueManager évite de planter si on n'a pas de choix.
            //Retiré au moment du chargement dans DialogueSaveLoad pour éviter de créer un choix supplémentaire.
            AddReplique();

        }


        private void TopButton()
        {
            NodeBuilder.NewTitleButton(this, "New Replique", () => AddReplique(), "TopBtn");
        }

        #endregion







        #region Menu dropdown

        // Menu dropdown --------------------------------------------------------------------------------------


        //Appelé dans DialogueSaveLoad
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
            AddLabelAndButton(newReplique, newReplique.BoxContainer, "Text", "TextColor");
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

        // Fields --------------------------------------------------------------------------------------

        private void AddLabelAndButton(RepliqueData_Replique replique, Box boxContainer, string labelName, string uniqueUSS = "")
        {
            Box topBoxContainer = NodeBuilder.NewBox(boxContainer, "TopBox");

            // Label Name
            NodeBuilder.NewLabel(topBoxContainer, labelName, "LabelText", uniqueUSS);

            //Le conteneur des boutons
            Box buttonsBox = NodeBuilder.NewBox(topBoxContainer, "BtnBox");



            //Si on n'a qu'une seule réplique, pas la peine d'afficher les petits boutons
            boxesButtons.Add(buttonsBox);
            for (int i = 0; i < boxesButtons.Count; i++)
            {
                NodeBuilder.ShowHide(RepliqueData.Repliques.Count > 1, boxesButtons[i]);
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
            tmp.AddRange(repliqueData.Repliques);


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

            repliqueData.Repliques.Clear();

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
            NodeBuilder.NewToggle(NodeBuilder.NewBox(boxContainer, "BoxRow"), replique.AppendToText, "Append To Previous Text", "Toggle");

            //Steal control from player
            NodeBuilder.NewToggle(NodeBuilder.NewBox(boxContainer, "BoxRow"), replique.CanClickOnContinue, "Can Click On Continue", "Toggle");


            //Override Write Speed
            NodeBuilder.AddToggleFloatField(this, boxContainer, replique.OverrideWriteSpeed, replique.WriteSpeed, "Override Write Speed");

            //Auto Delay Duration
            NodeBuilder.AddToggleFloatField(this, boxContainer, replique.UseAutoDelay, replique.AutoDelayDuration, "Use Auto Delay");

        }


        private void ShowHideMoveButtons()
        {
            //Si on n'a qu'une seule réplique, pas la peine d'afficher les petits boutons
            for (int i = 0; i < boxesButtons.Count; i++)
            {
                NodeBuilder.ShowHide(RepliqueData.Repliques.Count > 1, boxesButtons[i]);
            }
        }


        #endregion






    }



    #endregion
}
