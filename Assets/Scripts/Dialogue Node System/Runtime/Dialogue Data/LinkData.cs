
using UnityEngine.Serialization;

namespace Project.NodeSystem
{
    //Pour savoir quelles nodes sont connectées à quelles nodes
    [System.Serializable]
    public class LinkData
    {
        public string BaseNodeGuid;
        public string BasePortName;
        public string TargetNodeGuid;
        public string TargetPortName;
    }

}