namespace Project.NodeSystem
{
    //Pour savoir quelles nodes sont connectées à quelles nodes
    [System.Serializable]
    public class LinkData
    {
        public string baseNodeGuid;
        public string basePortName;
        public string targetNodeGuid;
        public string targetPortName;
    }

}