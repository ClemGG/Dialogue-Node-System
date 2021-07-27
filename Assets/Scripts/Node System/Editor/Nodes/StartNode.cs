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

            //La StartNode poss�de un port d'entr�e dans le cas o� l'on voudrait relancer le dialogue depuis le d�but 
            //(incluant le nettoyage dans la Start() du DialogueManager)
            NodeBuilder.AddPort(this, "Input", Direction.Input, Port.Capacity.Multi);
            NodeBuilder.AddPort(this, "Start", Direction.Output);

            //On appelle ces fonctions pour mettre � jour le visuel de la Node
            RefreshExpandedState();
            RefreshPorts();
        }

    }
}