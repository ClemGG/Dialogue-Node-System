using System;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Project.NodeSystem.Editor
{
    public class BranchNode : BaseNode
    {
        private BranchData branchData = new BranchData();

        public BranchData BranchData { get => branchData; set => branchData = value; }

        public BranchNode() : base("Branch", Vector2.zero)
        {

        }

        public BranchNode(Vector2 position, DialogueEditorWindow window, DialogueGraphView graphView) : base("Branch", position, window, graphView, "BranchNodeStyleSheet")
        {

            NodeBuilder.AddPort(this, "Input", Direction.Input, Port.Capacity.Multi);
            NodeBuilder.AddPort(this, "True", Direction.Output);
            NodeBuilder.AddPort(this, "False", Direction.Output);

            TopButton();

            //On appelle ces fonctions pour mettre à jour le visuel de la Node
            RefreshExpandedState();
            RefreshPorts();
        }


        #region Methods



        /// <summary>
        /// Ajoute des conditions à la node
        /// </summary>
        private void TopButton()
        {
            NodeBuilder.NewTitleButton(this, "Add Condition", () => AddCondition(), "TopBtn");
        }

        /// <summary>
        /// Ajoute un champ contenant une nouvelle condition
        /// </summary>
        /// <param name="stringEvent"></param>
        public void AddCondition(EventData_StringEventCondition stringEvent = null)
        {
            NodeBuilder.AddStringConditionEvent(this, BranchData.stringConditions, stringEvent);
        }


        #endregion

    }
}