using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using static Project.Utilities.ValueTypes.Enums;

namespace Project.NodeSystem.Editor
{

    public class BaseNode : Node
    {
        #region Fields

        private string nodeGuid;
        private DialogueGraphView graphView;
        private DialogueEditorWindow window;
        protected Vector2 defaultNodeSize = new Vector2(200, 250);

        public List<NodeBuilder.LanguageGenericHolder_Text> textHolders = new List<NodeBuilder.LanguageGenericHolder_Text>();
        public List<NodeBuilder.LanguageGenericHolder_AudioClip> audioClipHolders = new List<NodeBuilder.LanguageGenericHolder_AudioClip>();


        public string NodeGuid { get => nodeGuid; set => nodeGuid = value; }
        public DialogueGraphView GraphView { get => graphView; set => graphView = value; }
        public DialogueEditorWindow Window { get => window; set => window = value; }

        #endregion


        /// <summary>
        /// Toutes les classes filles possèdent un constructeur vide pour pouvoir les créer depuis la barre de recherche du graphe.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="position"></param>
        /// <param name="window"></param>
        /// <param name="graphView"></param>
        /// <param name="styleSheetName"></param>
        public BaseNode(string name, Vector2 position, DialogueEditorWindow window = null, DialogueGraphView graphView = null, string styleSheetName = "")
        {
            this.Window = window;
            this.GraphView = graphView;

            //La StyleSheet de base
            styleSheets.Add(Resources.Load<StyleSheet>("USS/Nodes/NodeStyleSheet"));

            //La StyleSheet propre à chaque node (si elle en a une)
            if (styleSheetName != string.Empty)
                styleSheets.Add(Resources.Load<StyleSheet>($"USS/Nodes/{styleSheetName}"));


            title = name;
            SetPosition(new Rect(position, defaultNodeSize));
            NodeGuid = Guid.NewGuid().ToString();
        }



        #region Methods



        //Quand on charge les données de la node, on l'assigne à ses champs si elle en a
        public virtual void LoadValueIntoField() { }

        //Idem que LoadValueIntoField() mais recharge les champs dépendant de la langue
        public virtual void ReloadLanguage()
        {
            foreach (NodeBuilder.LanguageGenericHolder_Text textHolder in textHolders)
            {
                Reload_TextLanguage(textHolder.inputText, textHolder.textField, textHolder.placeholderText);
            }
            foreach (NodeBuilder.LanguageGenericHolder_AudioClip audioHolder in audioClipHolders)
            {
                Reload_AudioClipLanguage(audioHolder.inputAudioClip, audioHolder.objectField);
            }

        }


        /// <summary>
        /// Reload all the text in the TextField to the current selected language.
        /// </summary>
        /// <param name="inputText">List of LanguageGeneric<string></param>
        /// <param name="textField">The TextField that is to be reload</param>
        /// <param name="placeholderText">The text that will be displayed if the text field is empty</param>
        protected void Reload_TextLanguage(List<LanguageGeneric<string>> inputText, TextField textField, string placeholderText = "")
        {
            // Reload Text
            textField.RegisterValueChangedCallback(value =>
            {
                inputText.Find(text => text.language == Window.SelectedLanguage).data = value.newValue;
            });
            textField.SetValueWithoutNotify(inputText.Find(text => text.language == Window.SelectedLanguage).data);

            NodeBuilder.SetPlaceholderText(textField, placeholderText);
        }

        /// <summary>
        /// Reload all the AudioClip in the ObjectField to the current selected language.
        /// </summary>
        /// <param name="inputAudioClip">List of LanguageGeneric<AudioClip></param>
        /// <param name="objectField">The ObjectField that is to be reload</param>
        protected void Reload_AudioClipLanguage(List<LanguageGeneric<AudioClip>> inputAudioClip, ObjectField objectField)
        {
            // Reload Text
            objectField.RegisterValueChangedCallback(value =>
            {
                inputAudioClip.Find(text => text.language == Window.SelectedLanguage).data = value.newValue as AudioClip;
            });
            objectField.SetValueWithoutNotify(inputAudioClip.Find(text => text.language == Window.SelectedLanguage).data);
        }


        /// <summary>
        /// Remove box container.
        /// </summary>
        /// <param name="boxToRemove">desired box to delete and remove</param>
        public virtual void DeleteBox(Box boxToRemove)
        {
            mainContainer.Remove(boxToRemove);
            RefreshExpandedState();
        }

        /// <summary>
        /// Remove box container.
        /// </summary>
        /// <param name="elementToRemove">desired box to delete and remove</param>
        public virtual void DeleteBox(VisualElement container, VisualElement elementToRemove)
        {
            container.Remove(elementToRemove);
            RefreshExpandedState();
        }

        /// <summary>
        /// Si la node contient des BaseContainers, cette fonction permet de les réarranger selon leur ID
        /// </summary>
        /// <param name="itemToMove"></param>
        /// <param name="moveUp"></param>
        public virtual void MoveBox(NodeData_BaseContainer itemToMove, bool moveUp)
        {

        }


        #endregion





    }
}