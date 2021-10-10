using System;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Project.NodeSystem.Editor
{

    public class StartNode : BaseNode
    {
        #region Fields

        public StartData StartData { get; set; } = new StartData();

        #endregion


        #region Constructors

        public StartNode() : base("Start", Vector2.zero)
        {

        }

        public StartNode(Vector2 position, DialogueEditorWindow window, DialogueGraphView graphView) : base("Start", position, window, graphView, "StartNodeStyleSheet")
        {
            //The StartNode uses an etry port in case we want to restat the dialogue from that specific node
            //(including the cleanup in the DialogueManager)
            NodeBuilder.AddPort(this, "Input", Direction.Input, Port.Capacity.Multi);
            NodeBuilder.AddPort(this, "Start", Direction.Output);


            AddDefaultStartNodeMarker();
            TopButton();


            //Repaint
            RefreshExpandedState();
            RefreshPorts();
        }

        #endregion


        #region Fields


        /// <summary>
        /// Add isDefault toggle.
        /// </summary>
        private void AddDefaultStartNodeMarker()
        {
            Box boxContainer = NodeBuilder.NewBox(this, "BoxRow");

            //When set to true, all other StartNodes' isDefault fields are set to false so that
            //only one StartNode is marked as the default one.
            Action onToggled = () =>
            {
                GraphView.nodes.ToList().Where(node => node is StartNode).Cast<StartNode>().ToList().ForEach(startNode =>
                {
                    if(startNode != this)
                    {
                        startNode.StartData.isDefault.Value = false;
                        startNode.ReloadFields();
                    }
                });
            };
            StartData.Toggle = NodeBuilder.NewToggle("Default", boxContainer, StartData.isDefault, onToggled, "ToggleLabel");
        }



        /// <summary>
        /// Add Condition button
        /// </summary>
        private void TopButton()
        {
            NodeBuilder.NewTitleButton(this, "Add Condition", () => AddCondition(), "TopBtn");
        }


        /// <summary>
        /// Adds a field containing a new condition
        /// </summary>
        /// <param name="stringEvent"></param>
        public void AddCondition(EventData_StringEventCondition stringEvent = null)
        {
            NodeBuilder.AddStringConditionEvent(this, StartData.StringConditions, stringEvent);
        }

        #endregion



        #region On Load

        public override void ReloadFields()
        {
            if(StartData.Toggle != null)
            {
                StartData.Toggle.SetValueWithoutNotify(StartData.isDefault.Value);
            }
        }

        #endregion
    }
}