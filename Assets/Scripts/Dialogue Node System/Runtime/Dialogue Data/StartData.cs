using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEngine.UIElements;
#endif

namespace Project.NodeSystem
{
    [System.Serializable]
    public class StartData : BaseData
    {

#if UNITY_EDITOR
        public Toggle Toggle { get; set; }
#endif

        //If true, this StartNode will be read in priority if its conditons are met
        public ContainerValue<bool> isDefault = new ContainerValue<bool>();

        //If its conditions are met, that StartNode becomes eligible for reading in the DialogueManager
        public List<EventData_StringEventCondition> StringConditions = new List<EventData_StringEventCondition>();



    }

}