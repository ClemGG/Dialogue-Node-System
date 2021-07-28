using System.Collections.Generic;
using UnityEngine;


#if UNITY_EDITOR
using UnityEngine.UIElements;
#endif

namespace Project.NodeSystem
{
    [System.Serializable]
    public class StartData : BaseData
    {
#if UNITY_EDITOR
        public Toggle toggle;
#endif

        //Si à true, cette node sera la node de départ par défaut
        public ContainerValue<bool> isDefaultStartNode = new ContainerValue<bool>();

        //Si ses conditions sont réunies, cette StartNode pourra être choisie au lancement du dialogue
        public List<EventData_StringEventCondition> stringConditions = new List<EventData_StringEventCondition>();
    }
}