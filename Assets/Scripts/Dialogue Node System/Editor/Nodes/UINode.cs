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
            UIData.Toggle = NodeBuilder.NewToggle("Show Dialogue UI", boxContainer, UIData.show, "ToggleLabel");
        }


        #endregion



        #region On Load

        public override void ReloadFields()
        {
            if(UIData.Toggle != null)
            {
                UIData.Toggle.SetValueWithoutNotify(UIData.show.Value);
            }
        }

        #endregion
    }
}