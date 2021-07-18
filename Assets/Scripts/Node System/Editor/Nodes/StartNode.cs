using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Project.NodeSystem.Editor
{

    public class StartNode : BaseNode
    {
        public StartNode() : base("Start", Vector2.zero, null, null)
        {

        }

        public StartNode(Vector2 position, DialogueEditorWindow window, DialogueGraphView graphView) : base("Start", position, window, graphView)
        {

            StyleSheet styleSheet = Resources.Load<StyleSheet>("USS/Nodes/StartNodeStyleSheet");
            styleSheets.Add(styleSheet);


            AddPort("Start", Direction.Output);

            //On appelle ces fonctions pour mettre à jour le visuel de la Node
            RefreshExpandedState();
            RefreshPorts();
        }
    }
}