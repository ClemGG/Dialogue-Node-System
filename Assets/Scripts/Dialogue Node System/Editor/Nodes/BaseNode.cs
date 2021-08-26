using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Project.NodeSystem.Editor
{

    public class BaseNode : Node
    {
        #region Fields

            
        protected Vector2 _defaultNodeSize = new Vector2(200, 250);

        public List<NodeBuilder.LanguageGenericHolder_Text> TextHolders = new List<NodeBuilder.LanguageGenericHolder_Text>();
        public List<NodeBuilder.LanguageGenericHolder_AudioClip> AudioClipHolders = new List<NodeBuilder.LanguageGenericHolder_AudioClip>();


        public string NodeGuid { get; set; }
        public DialogueGraphView GraphView { get; set; }
        public DialogueEditorWindow Window { get; set; }


        #endregion


        #region Constructor

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
            SetPosition(new Rect(position, _defaultNodeSize));
            NodeGuid = Guid.NewGuid().ToString();


        }

        #endregion



        #region Methods



        #region On Load

        /// <summary>
        /// Quand on charge les données de la node, on les assigne à ses champs si elle en a
        /// </summary>
        public virtual void ReloadFields() { }

        /// <summary>
        /// Quand on charge les données de la node, recharge les champs dépendant de la langue
        /// </summary>
        public virtual void ReloadLanguage()
        {
            foreach (NodeBuilder.LanguageGenericHolder_Text textHolder in TextHolders)
            {
                Reload_TextLanguage(textHolder.inputText, textHolder.textField, textHolder.placeholderText);
            }
            foreach (NodeBuilder.LanguageGenericHolder_AudioClip audioHolder in AudioClipHolders)
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
                inputText.Find(text => text.Language == Window.SelectedLanguage).Data = value.newValue;
            });
            textField.SetValueWithoutNotify(inputText.Find(text => text.Language == Window.SelectedLanguage).Data);

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
                inputAudioClip.Find(text => text.Language == Window.SelectedLanguage).Data = value.newValue as AudioClip;
            });
            objectField.SetValueWithoutNotify(inputAudioClip.Find(text => text.Language == Window.SelectedLanguage).Data);
        }


        #endregion



        #region UIs

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

        #endregion





    }
}