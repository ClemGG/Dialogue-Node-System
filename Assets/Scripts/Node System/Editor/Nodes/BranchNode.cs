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

        public BranchNode() : base("Branch", Vector2.zero, null, null)
        {

        }

        public BranchNode(Vector2 position, DialogueEditorWindow window, DialogueGraphView graphView) : base("Branch", position, window, graphView)
        {
            StyleSheet styleSheet = Resources.Load<StyleSheet>("USS/Nodes/BranchNodeStyleSheet");
            styleSheets.Add(styleSheet);


            AddPort("Input", Direction.Input, Port.Capacity.Multi);
            AddPort("True", Direction.Output);
            AddPort("False", Direction.Output);

            TopButton();

            //On appelle ces fonctions pour mettre à jour le visuel de la Node
            RefreshExpandedState();
            RefreshPorts();
        }


        #region Methods



        private void TopButton()
        {
            ToolbarMenu menu = new ToolbarMenu();
            menu.text = "Add Condition";

            menu.menu.AppendAction("String Event Condition", new Action<DropdownMenuAction>(x => AddCondition()));

            titleContainer.Add(menu);

        }


        public void AddCondition(EventData_StringCondition stringEvent = null)
        {
            AddStringConditionEventBuild(BranchData.stringConditions, stringEvent);
        }


        #endregion

    }
}