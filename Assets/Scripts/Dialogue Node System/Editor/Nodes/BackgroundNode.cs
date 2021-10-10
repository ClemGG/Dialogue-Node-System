using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

using static Project.Utilities.Assets.AssetFinderUtilities;

namespace Project.NodeSystem.Editor
{
    public class BackgroundNode : BaseNode
    {
        #region Fields

        public BackgroundData BackgroundData { get; set; } = new BackgroundData();

        #endregion



        #region Constructors

        public BackgroundNode() : base("Background", Vector2.zero)
        {

        }

        public BackgroundNode(Vector2 position, DialogueEditorWindow window, DialogueGraphView graphView) : base("Background", position, window, graphView, "BackgroundNodeStyleSheet")
        {
            NodeBuilder.AddPort(this, "Input", Direction.Input, Port.Capacity.Multi);
            NodeBuilder.AddPort(this, "Output", Direction.Output, Port.Capacity.Single);


            //Creates a default background so that the DialogueManager doesn't creash.
            //Remove at loading time in the DialogueSaveLoad script.
            AddBackground();
        }

        #endregion




        #region Methods



        #region New Fields


        public void AddBackground(BackgroundData_Transition transition = null)
        {
            BackgroundData_Transition newTransition = new BackgroundData_Transition();
            if (transition != null)
            {
                newTransition.BackgroundTex.Value = transition.BackgroundTex.Value;
                newTransition.BackgroundName.Value = transition.BackgroundName.Value;
                newTransition.StartSettings.Value = transition.StartSettings.Value;
                newTransition.EndSettings.Value = transition.EndSettings.Value;
            }

            BackgroundData.Transition.Value = newTransition;


            newTransition.BoxContainer = NodeBuilder.NewBox(mainContainer);

            //Add Fields
            AddLabel(newTransition, newTransition.BoxContainer);
            AddImage(newTransition, newTransition.BoxContainer);
            AddFields(newTransition, newTransition.BoxContainer);

        }

        private void AddLabel(BackgroundData_Transition newTransition, Box boxContainer)
        {
            Box topBoxContainer = NodeBuilder.NewBox(boxContainer, "TopBox");

            // Label Name
            NodeBuilder.NewLabel(topBoxContainer, "Background", "LabelText", "LabelColor");

        }

        private void AddImage(BackgroundData_Transition newTransition, Box boxContainer)
        {
            Box ImagePreviewBox = NodeBuilder.NewBox(boxContainer, "BoxRow");

            // Set up Image Preview.
            Image faceImage = NodeBuilder.NewImage(ImagePreviewBox, "ImagePreview");

            newTransition.TextureField = faceImage;  //Kept in memory to change texture when we change the field

        }



        private void AddFields(BackgroundData_Transition newTransition, Box boxContainer)
        {
            //CharacterName
            TextField nameField = NodeBuilder.NewTextField(newTransition.BackgroundName, "", "BackgroundName", "TextStretch");
            nameField.SetEnabled(false);
            newTransition.NameField = nameField;
            boxContainer.Add(nameField);

            //Background ObjectField
            Action<UnityEngine.Object> onBackgroundChanged = (newValue) =>
            {
                //The field must never be null; there must be at least a transparent texture
                //so that the shader doesn't break
                if(newValue == null)
                {
                    newTransition.BackgroundTex.Value = FindInResourcesByName<Texture2D>("tex_emptyTransp");
                }
                newTransition.TextureField.image = newTransition.BackgroundTex.Value;
                if(newTransition.ObjectField != null)
                {
                    newTransition.ObjectField.SetValueWithoutNotify(newTransition.BackgroundTex.Value);
                }


                newTransition.BackgroundName.Value = newTransition.BackgroundTex.Value.name;
                newTransition.NameField.SetValueWithoutNotify(newTransition.BackgroundName.Value);
            };
            newTransition.ObjectField = NodeBuilder.NewObjectField("Background Texture", boxContainer, newTransition.BackgroundTex, onBackgroundChanged, "ObjectField");

            //TransitionSettingsSO
            NodeBuilder.NewObjectField("Start Settings", boxContainer, newTransition.StartSettings, "ObjectField");
            NodeBuilder.NewObjectField("End Settings", boxContainer, newTransition.EndSettings, "ObjectField");

        }

        #endregion



        public override void ReloadFields()
        {
            BackgroundData_Transition tmp = BackgroundData.Transition.Value;
            if (tmp != null)
            {
                tmp.TextureField.image = tmp.BackgroundTex.Value == null ? null : tmp.BackgroundTex.Value;
            }
        }




        #endregion
    }
}