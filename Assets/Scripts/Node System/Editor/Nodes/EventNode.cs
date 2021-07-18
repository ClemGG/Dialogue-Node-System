using System;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Project.NodeSystem.Editor
{
    /* Cette classe nous permet de référencer un DialogueEvent,
     * qui est un ScriptableObject nous permettant d'appeler des méthodes 
     * depuis des scripts en jeu.
     */
    public class EventNode : BaseNode
    {
        #region Fields

        private EventData eventData = new EventData();

        public EventData EventData { get => eventData; set => eventData = value; }


        #endregion





        public EventNode() : base("Event", Vector2.zero, null, null)
        {

        }

        public EventNode(Vector2 position, DialogueEditorWindow window, DialogueGraphView graphView) : base("Event", position, window, graphView)
        {

            StyleSheet styleSheet = Resources.Load<StyleSheet>("USS/Nodes/EventNodeStyleSheet");
            styleSheets.Add(styleSheet);


            AddPort("Input", Direction.Input, Port.Capacity.Multi);
            AddPort("Output", Direction.Output, Port.Capacity.Single);



            //Crée la barre d'outils
            TopButton();



            //On appelle ces fonctions pour mettre à jour le visuel de la Node
            RefreshExpandedState();
            RefreshPorts();
        }


        #region Methods



        private void TopButton()
        {
            ToolbarMenu menu = new ToolbarMenu();
            menu.text = "Add Event";

            menu.menu.AppendAction("String Event Modifier", new Action<DropdownMenuAction>(x => AddStringEvent()));
            menu.menu.AppendAction("Scriptable Object", new Action<DropdownMenuAction>(x => AddScriptableEvent()));

            titleContainer.Add(menu);

        }



        public void AddStringEvent(EventData_StringModifier stringEvent = null)
        {
            AddStringModifierEventBuild(eventData.stringEvents, stringEvent);
        }

        public void AddScriptableEvent(ContainerValue<DialogueEventSO> scriptableEvent = null)
        {
            AddScriptableEventBuild(EventData, scriptableEvent);
        }


        #endregion
    }
}