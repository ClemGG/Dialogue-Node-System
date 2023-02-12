#if UNITY_EDITOR

#endif

namespace Project.NodeSystem
{
    [System.Serializable]
    public class DelayData : BaseData
    {
#if UNITY_EDITOR

        public UnityEngine.UIElements.FloatField FloatField { get; set; }

#endif

        public ContainerValue<float> Delay = new ContainerValue<float>();
    }
}