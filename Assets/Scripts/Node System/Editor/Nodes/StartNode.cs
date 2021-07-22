using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Project.NodeSystem.Editor
{

    public class StartNode : BaseNode
    {
        private StartData startData = new StartData();
        public StartData StartData { get => startData; set => startData = value; }

        public StartNode() : base("Start", Vector2.zero)
        {

        }

        public StartNode(Vector2 position, DialogueEditorWindow window, DialogueGraphView graphView) : base("Start", position, window, graphView, "StartNodeStyleSheet")
        {

            NodeBuilder.AddPort(this, "Start", Direction.Output);

            //On appelle ces fonctions pour mettre à jour le visuel de la Node
            RefreshExpandedState();
            RefreshPorts();
        }

    }
}