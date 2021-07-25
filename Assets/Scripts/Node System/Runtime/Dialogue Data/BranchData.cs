using System.Collections.Generic;

namespace Project.NodeSystem
{
    [System.Serializable]
    public class BranchData : BaseData
    {
        public string trueNodeGuid;
        public string falseNodeGuid;
        public List<EventData_StringEventCondition> stringConditions = new List<EventData_StringEventCondition>();

    }

}