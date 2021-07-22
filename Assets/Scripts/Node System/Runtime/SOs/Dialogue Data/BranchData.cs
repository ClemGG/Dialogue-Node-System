using System.Collections.Generic;

namespace Project.NodeSystem
{
    [System.Serializable]
    public class BranchData : BaseData
    {
        public string trueNodeGuid;
        public string falseNodeGuid;
        public List<EventData_StringCondition> stringConditions = new List<EventData_StringCondition>();

    }

}