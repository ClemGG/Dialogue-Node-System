using System.Collections.Generic;

namespace Project.NodeSystem
{


    [System.Serializable]
    public class EventData : BaseData
    {
        public List<EventData_StringModifier> stringEvents = new List<EventData_StringModifier>();
        public List<ContainerValue<DialogueEventSO>> scriptableEvents = new List<ContainerValue<DialogueEventSO>>();
    }


}