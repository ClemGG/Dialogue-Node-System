using System;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Project.NodeSystem.Editor
{
    /* Cette classe nous permet de r�f�rencer un DialogueEvent,
     * qui est un ScriptableObject nous permettant d'appeler des m�thodes 
     * depuis des scripts en jeu.
     */
    public class EventNode : BaseNode
    {
        #region Fields

        private EventData eventData = new EventData();

        public EventData EventData { get => eventData; set => eventData = value; }


        #endregion





        public EventNode() : base("Event", Vector2.zero)
        {

        }

        public EventNode(Vector2 position, DialogueEditorWindow window, DialogueGraphView graphView) : base("Event", position, window, graphView, "EventNodeStyleSheet")
        {
            NodeBuilder.AddPort(this, "Input", Direction.Input, Port.Capacity.Multi);
            NodeBuilder.AddPort(this, "Output", Direction.Output, Port.Capacity.Single);


            //Cr�e la barre d'outils
            TopButton();



            //On appelle ces fonctions pour mettre � jour le visuel de la Node
            RefreshExpandedState();
            RefreshPorts();
        }


        #region Methods



        private void TopButton()
        {
            ToolbarMenu tm = NodeBuilder.NewToolbar(this, "Add Event");
            tm.AddMenuActions
                (
                    ("String Event Modifier", new Action<DropdownMenuAction>(x => AddStringEvent())),
                    ("Scriptable Object", new Action<DropdownMenuAction>(x => AddScriptableEvent()))
                );
        }



        public void AddStringEvent(EventData_StringModifier stringEvent = null)
        {
            NodeBuilder.AddStringModifierEvent(this, eventData.stringEvents, stringEvent);
        }

        public void AddScriptableEvent(ContainerValue<DialogueEventSO> scriptableEvent = null)
        {
            NodeBuilder.AddScriptableEvent(this, EventData, scriptableEvent);
        }


        #endregion
    }
}