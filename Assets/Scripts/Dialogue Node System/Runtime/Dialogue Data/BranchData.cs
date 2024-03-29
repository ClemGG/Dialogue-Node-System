﻿using System.Collections.Generic;

namespace Project.NodeSystem
{
    [System.Serializable]
    public class BranchData : BaseData
    {
        public string TrueNodeGuid;
        public string FalseNodeGuid;
        public List<EventData_StringEventCondition> StringConditions = new List<EventData_StringEventCondition>();

    }

}