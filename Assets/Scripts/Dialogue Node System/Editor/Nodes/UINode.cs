using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Project.NodeSystem.Editor
{

    public class UINode : BaseNode
    {
        #region Fields

        public UIData UIData { get; set; } = new UIData();

        #endregion


        #region Constructors

        public UINode() : base("UI", Vector2.zero)
        {

        }

        public UINode(Vector2 position, DialogueEditorWindow window, DialogueGraphView graphView) : base("UI", position, window, graphView, "UINodeStyleSheet")
        {
            NodeBuilder.AddPort(this, "Input", Direction.Input, Port.Capacity.Multi);
            NodeBuilder.AddPort(this, "Output", Direction.Output);

            AddFields();


            //Repaint
            RefreshExpandedState();
            RefreshPorts();
        }

        #endregion


        #region Fields

        private void AddFields()
        {
            Box boxContainer = NodeBuilder.NewBox(this, "BoxRow");
            UIData.ShowToggle = NodeBuilder.NewToggle("Show Dialogue UI", mainContainer, UIData.Show, "ToggleLabel");
            UIData.ClearToggle = NodeBuilder.NewToggle("Clear Sprites On Show", mainContainer, UIData.ClearCharSprites, "ToggleLabel");
        }


        #endregion



        #region On Load

        public override void ReloadFields()
        {
            if (UIData.ShowToggle != null)
            {
                UIData.ShowToggle.SetValueWithoutNotify(UIData.Show.Value);
            }
            if (UIData.ClearToggle != null)
            {
                UIData.ClearToggle.SetValueWithoutNotify(UIData.ClearCharSprites.Value);
            }
        }

        #endregion
    }
}