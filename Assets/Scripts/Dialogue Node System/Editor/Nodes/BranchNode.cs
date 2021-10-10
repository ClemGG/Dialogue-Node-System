using System;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Project.NodeSystem.Editor
{
    public class BranchNode : BaseNode
    {
        #region Fields

        public BranchData BranchData { get; set; } = new BranchData();

        #endregion



        #region Constructors

        //Empty constructor for the NodeSearchWindow
        public BranchNode() : base("Branch", Vector2.zero)
        {

        }

        public BranchNode(Vector2 position, DialogueEditorWindow window, DialogueGraphView graphView) : base("Branch", position, window, graphView, "BranchNodeStyleSheet")
        {

            NodeBuilder.AddPort(this, "Input", Direction.Input, Port.Capacity.Multi);
            NodeBuilder.AddPort(this, "True", Direction.Output);
            NodeBuilder.AddPort(this, "False", Direction.Output);

            TopButton();

            //Repaint
            RefreshExpandedState();
            RefreshPorts();
        }

        #endregion


        #region Methods



        /// <summary>
        /// Creates a button to add conditions
        /// </summary>
        private void TopButton()
        {
            NodeBuilder.NewTitleButton(this, "Add Condition", () => AddCondition(), "TopBtn");
        }

        /// <summary>
        /// Adds a new field containing a condition
        /// </summary>
        /// <param name="stringEvent">The event to load (if null, the script will create a new one)</param>
        public void AddCondition(EventData_StringEventCondition stringEvent = null)
        {
            NodeBuilder.AddStringConditionEvent(this, BranchData.StringConditions, stringEvent);
        }


        #endregion

    }
}