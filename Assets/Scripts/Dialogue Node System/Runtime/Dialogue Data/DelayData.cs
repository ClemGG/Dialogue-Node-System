#if UNITY_EDITOR
using UnityEditor.UIElements;
#endif

namespace Project.NodeSystem
{
    [System.Serializable]
    public class DelayData : BaseData
    {
#if UNITY_EDITOR

        public FloatField FloatField { get; set; }

#endif

        public ContainerValue<float> Delay = new ContainerValue<float>();
    }
}