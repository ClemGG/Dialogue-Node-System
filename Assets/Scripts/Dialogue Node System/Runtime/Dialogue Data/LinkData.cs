
using UnityEngine.Serialization;

namespace Project.NodeSystem
{
    //To keep in memory which nodes are connected to which nodes
    [System.Serializable]
    public class LinkData
    {
        public string BaseNodeGuid;
        public string BasePortName;
        public string TargetNodeGuid;
        public string TargetPortName;
    }

}