using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Project.NodeSystem.Editor
{
    public class EndNode : BaseNode
    {
        #region Fields

        private EndData endData = new EndData();
        public EndData EndData { get => endData; set => endData = value; }


        #endregion

        public EndNode() : base("End", Vector2.zero)
        {

        }

        public EndNode(Vector2 position, DialogueEditorWindow window, DialogueGraphView graphView) : base("End", position, window, graphView, "EndNodeStyleSheet")
        {
            NodeBuilder.AddPort(this, "End", Direction.Input, Port.Capacity.Multi);
            outputContainer.AddStyle("Hide");
            MakeMainContainer();
        }


        private void MakeMainContainer()
        {
            NodeBuilder.NewEndNodeTypeField(this, endData.endNodeType);
        }

        public override void LoadValueIntoField()
        {
            if (EndData.endNodeType.enumField != null)
            {
                EndData.endNodeType.enumField.SetValueWithoutNotify(EndData.endNodeType.value);
            }
        }
    }
}