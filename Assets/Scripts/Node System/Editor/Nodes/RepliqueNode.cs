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


            RepliqueData.repliques.Add(newReplique);

            // Add Container Box
            newReplique.boxContainer = NodeBuilder.NewBox(mainContainer, "DialogueBox");


            // Add Fields
            AddLabelAndButton(newReplique, newReplique.boxContainer, "Text", "TextColor");
            AddAudioClips(newReplique, newReplique.boxContainer);
            AddTextField(newReplique, newReplique.boxContainer);


            // Load in data if it got any
            if (replique != null)
            {
                // Guid ID
                newReplique.guid = replique.guid;


                // Text
                foreach (LanguageGeneric<string> data_text in replique.texts)
                {
                    foreach (LanguageGeneric<string> text in newReplique.texts)
                    {
                        if (text.language == data_text.language)
                        {
                            text.data = data_text.data;
                        }
                    }
                }

                // Audio
                foreach (LanguageGeneric<AudioClip> data_audioclip in replique.audioClips)
                {
                    foreach (LanguageGeneric<AudioClip> audioclip in newReplique.audioClips)
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
                newReplique.guid.value = Guid.NewGuid().ToString();
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
                NodeBuilder.ShowHide(RepliqueData.repliques.Count > 1, boxesButtons[i]);
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
                RepliqueData.repliques.Remove(replique);
                ShouldShowHideMoveButtons();
            };
            Button removeBtn = NodeBuilder.NewButton(buttonsBox, "X", onClicked, "RemoveBtn");


        }

        public override void MoveBox(NodeData_BaseContainer replique, bool moveUp)
        {
            List<NodeData_BaseContainer> tmp = new List<NodeData_BaseContainer>();
            tmp.AddRange(repliqueData.repliques);


            for (int i = 0; i < tmp.Count; i++)
            {
                DeleteBox(RepliqueData.repliques[i].boxContainer);
                tmp[i].ID.value = i;
            }

            if (replique.ID.value > 0 && moveUp)
            {
                NodeData_BaseContainer tmp01 = tmp[replique.ID.value];
                NodeData_BaseContainer tmp02 = tmp[replique.ID.value - 1];

                tmp[replique.ID.value] = tmp02;
                tmp[replique.ID.value - 1] = tmp01;
            }
            else if (replique.ID.value < tmp.Count - 1 && !moveUp)
            {
                NodeData_BaseContainer tmp01 = tmp[replique.ID.value];
                NodeData_BaseContainer tmp02 = tmp[replique.ID.value + 1];

                tmp[replique.ID.value] = tmp02;
                tmp[replique.ID.value + 1] = tmp01;
            }

            repliqueData.repliques.Clear();

            foreach (NodeData_BaseContainer data in tmp)
            {
                AddReplique(data as RepliqueData_Replique);
            }
        }



        private void AddTextField(RepliqueData_Replique replique, Box boxContainer)
        {
            TextField textField = NodeBuilder.NewTextLanguagesField(this, boxContainer, replique.texts, "Write your dialogue here...", "TextBox", "TextStretch");
            replique.TextField = textField;
        }

        private void AddAudioClips(RepliqueData_Replique replique, Box boxContainer)
        {
            ObjectField objectField = NodeBuilder.NewAudioClipLanguagesField(this, boxContainer, replique.audioClips, "AudioClip");
            replique.AudioField = objectField;
        }



        private void ShouldShowHideMoveButtons()
        {
            //Si on n'a qu'une seule réplique, pas la peine d'afficher les petits boutons
            for (int i = 0; i < boxesButtons.Count; i++)
            {
                NodeBuilder.ShowHide(RepliqueData.repliques.Count > 1, boxesButtons[i]);
            }
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
