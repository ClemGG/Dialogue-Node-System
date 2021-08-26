using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Project.NodeSystem.Editor
{
    public class EndNode : BaseNode
    {
        #region Fields

        public EndData EndData { get; set; } = new EndData();


        #endregion


        #region Constructeurs

        public EndNode() : base("End", Vector2.zero)
        {

        }

        public EndNode(Vector2 position, DialogueEditorWindow window, DialogueGraphView graphView) : base("End", position, window, graphView, "EndNodeStyleSheet")
        {
            NodeBuilder.AddPort(this, "End", Direction.Input, Port.Capacity.Multi);
            outputContainer.AddStyle("Hide");
            MakeMainContainer();
        }

        #endregion


        #region New Fields

        /// <summary>
        /// Crée le champ pour changer le type de fin
        /// </summary>
        private void MakeMainContainer()
        {
            NodeBuilder.NewEnumField("", this, EndData.EndNodeType);
        }

        #endregion


        #region OnLoad

        public override void ReloadFields()
        {
            if (EndData.EndNodeType.EnumField != null)
            {
                EndData.EndNodeType.EnumField.SetValueWithoutNotify(EndData.EndNodeType.Value);
            }
        }

        #endregion
    }
}