using System;
using System.Collections.Generic;
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





        public EventNode() : base("Event", Vector2.zero)
        {

        }

        public EventNode(Vector2 position, DialogueEditorWindow window, DialogueGraphView graphView) : base("Event", position, window, graphView, "EventNodeStyleSheet")
        {
            NodeBuilder.AddPort(this, "Input", Direction.Input, Port.Capacity.Multi);
            NodeBuilder.AddPort(this, "Output", Direction.Output, Port.Capacity.Single);


            //Crée la barre d'outils
            TopButton();



            //On appelle ces fonctions pour mettre à jour le visuel de la Node
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



        public void AddStringEvent(EventData_StringEventModifier stringEvent = null)
        {
            NodeBuilder.AddStringModifierEvent(this, EventData, stringEvent);
            ShowHideButtons();
        }

        public void AddScriptableEvent(EventData_ScriptableEvent scriptableEvent = null)
        {
            NodeBuilder.AddScriptableEvent(this, EventData, scriptableEvent);
            ShowHideButtons();
        }





        #endregion



        #region UIs


        public override void DeleteBox(Box boxToRemove)
        {
            base.DeleteBox(boxToRemove);
            ShowHideButtons();
        }


        /// <summary>
        /// Déplace les boxContainers des choix ainsi que leurs ports associés
        /// </summary>
        /// <param name="eventToMove"></param>
        /// <param name="moveUp"></param>
        public override void MoveBox(NodeData_BaseContainer eventToMove, bool moveUp)
        {
            List<NodeData_BaseContainer> tmp = new List<NodeData_BaseContainer>();
            tmp.AddRange(EventData.Events);


            for (int i = 0; i < tmp.Count; i++)
            {
                DeleteBox(tmp[i].EventBox);
                tmp[i].ID.Value = i;
            }

            if (eventToMove.ID.Value > 0 && moveUp)
            {
                NodeData_BaseContainer tmp01 = tmp[eventToMove.ID.Value];
                NodeData_BaseContainer tmp02 = tmp[eventToMove.ID.Value - 1];

                tmp[eventToMove.ID.Value] = tmp02;
                tmp[eventToMove.ID.Value - 1] = tmp01;

            }
            else if (eventToMove.ID.Value < tmp.Count - 1 && !moveUp)
            {
                NodeData_BaseContainer tmp01 = tmp[eventToMove.ID.Value];
                NodeData_BaseContainer tmp02 = tmp[eventToMove.ID.Value + 1];

                tmp[eventToMove.ID.Value] = tmp02;
                tmp[eventToMove.ID.Value + 1] = tmp01;

            }


            EventData.Events.Clear();

            for (int i = 0; i < tmp.Count; i++)
            {
                switch (tmp[i])
                {
                    // Save Dialogue Event
                    case EventData_ScriptableEvent scriptableEvent:
                        AddScriptableEvent(scriptableEvent);
                        break;

                    // Save String Event
                    case EventData_StringEventModifier stringEvent:
                        AddStringEvent(stringEvent);
                        break;
                }
            }

        }


        private void ShowHideButtons()
        {
            for (int i = 0; i < EventData.Events.Count; i++)
            {
                NodeBuilder.ShowHide(EventData.Events.Count > 1, EventData.Events[i].BtnsBox);
            }
        }




        #endregion
    }
}