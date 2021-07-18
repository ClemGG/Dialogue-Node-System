using System;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Project.NodeSystem.Editor
{
    public class ChoiceNode : BaseNode
    {
        private ChoiceData choiceData = new ChoiceData();
        public ChoiceData ChoiceData { get => choiceData; set => choiceData = value; }

        private Box choiceStateEnumBox;

        public ChoiceNode() : base("Choice", Vector2.zero, null, null)
        {

        }

        public ChoiceNode(Vector2 position, DialogueEditorWindow window, DialogueGraphView graphView) : base("Choice", position, window, graphView)
        {
            StyleSheet styleSheet = Resources.Load<StyleSheet>("USS/Nodes/ChoiceNodeStyleSheet");
            styleSheets.Add(styleSheet);


            Port inputPort = AddPort("Input", Direction.Input, Port.Capacity.Multi);
            AddPort("True", Direction.Output);

            inputPort.portColor = Color.yellow;

            TopButton();

            TextLine();

            ChoiceStateEnum();

            //On appelle ces fonctions pour mettre à jour le visuel de la Node
            RefreshExpandedState();
            RefreshPorts();
        }


        private void TopButton()
        {
            ToolbarMenu menu = new ToolbarMenu();
            menu.text = "Add Condition";

            menu.menu.AppendAction("String Event Condition", new Action<DropdownMenuAction>(x => AddCondition()));

            titleContainer.Add(menu);

        }


        public void AddCondition(EventData_StringCondition stringEvent = null)
        {
            AddStringConditionEventBuild(ChoiceData.stringConditions, stringEvent);
            ShowHideChoiceEnum();
        }




        //Crée un champ pour la réplique du choix et son audio
        public void TextLine()
        {
            // Make Container Box
            Box boxContainer = new Box();
            boxContainer.AddToClassList("TextLineBox");

            // Text
            TextField textField = NewTextLanguagesField(ChoiceData.texts, "Text", "TextBox");
            ChoiceData.TextField = textField;
            boxContainer.Add(textField);

            // Audio
            ObjectField objectField = NewAudioClipLanguagesField(ChoiceData.audioClips, "AudioClip");
            ChoiceData.ObjectField = objectField;
            boxContainer.Add(objectField);

            // Reload the current selected language
            ReloadLanguage();

            mainContainer.Add(boxContainer);
        }

        //Permet de cacher ou griser le bouton choix correspondant 
        //si les conditions pour activer cette partie du dialogue ne sont pas remplies
        private void ChoiceStateEnum()
        {
            choiceStateEnumBox = new Box();
            choiceStateEnumBox.AddToClassList("BoxRow");
            ShowHideChoiceEnum();

            // Make fields.
            Label enumLabel = NewLabel("If the conditions are not met", "ChoiceLabel");
            EnumField choiceStateEnumField = NewChoiceStateTypeField(ChoiceData.choiceStateType, "enumHide");

            // Add fields to box.
            choiceStateEnumBox.Add(choiceStateEnumField);
            choiceStateEnumBox.Add(enumLabel);

            mainContainer.Add(choiceStateEnumBox);
        }





        protected override void DeleteBox(Box boxContainer)
        {
            base.DeleteBox(boxContainer);
            ShowHideChoiceEnum();
        }

        private void ShowHideChoiceEnum()
        {
            ShowHide(ChoiceData.stringConditions.Count > 0, choiceStateEnumBox);
        }


        public override void LoadValueIntoField()
        {
            if (ChoiceData.choiceStateType.enumField != null)
                ChoiceData.choiceStateType.enumField.SetValueWithoutNotify(ChoiceData.choiceStateType.value);
        }
    }
}