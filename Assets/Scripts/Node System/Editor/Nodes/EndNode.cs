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

        public EndNode() : base("End", Vector2.zero, null, null)
        {

        }

        public EndNode(Vector2 position, DialogueEditorWindow window, DialogueGraphView graphView) : base("End", position, window, graphView)
        {

            StyleSheet styleSheet = Resources.Load<StyleSheet>("USS/Nodes/EndNodeStyleSheet");
            styleSheets.Add(styleSheet);


            AddPort("End", Direction.Input, Port.Capacity.Multi);

            MakeMainContainer();
        }


        private void MakeMainContainer()
        {
            EnumField enumField = NewEndNodeTypeField(endData.endNodeType);
            mainContainer.Add(enumField);
        }

        public override void LoadValueIntoField()
        {
            if(EndData.endNodeType.enumField != null)
            {
                EndData.endNodeType.enumField.SetValueWithoutNotify(EndData.endNodeType.value);
            }
        }
    }
}