using System.Collections.Generic;

namespace Project.NodeSystem
{


    [System.Serializable]
    public class EventData : BaseData
    {
        //Contenir les StringEvents et les ScriptableEvents dans une seule liste nous permet de les trier par ID
        //et de les jouer dans un certain ordre
        public List<NodeData_BaseContainer> Events = new List<NodeData_BaseContainer>();
        public List<EventData_StringEventModifier> StringEvents = new List<EventData_StringEventModifier>();
        public List<EventData_ScriptableEvent> ScriptableEvents = new List<EventData_ScriptableEvent>();

        readonly List<NodeData_BaseContainer> m_sortedEvents = new List<NodeData_BaseContainer>();

        //Les events triés pour les MonoBehaviours
        public List<NodeData_BaseContainer> SortedEvents
        {
            get
            {
                m_sortedEvents.Clear();
                m_sortedEvents.AddRange(StringEvents);
                m_sortedEvents.AddRange(ScriptableEvents);

                m_sortedEvents.Sort(delegate (NodeData_BaseContainer x, NodeData_BaseContainer y)
                {
                    return x.ID.Value.CompareTo(y.ID.Value);
                });

                return m_sortedEvents;
            }
        }


    }


}
