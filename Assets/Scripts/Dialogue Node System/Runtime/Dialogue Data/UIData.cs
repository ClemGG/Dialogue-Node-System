#if UNITY_EDITOR
using UnityEngine.UIElements;
#endif

namespace Project.NodeSystem
{
    [System.Serializable]
    public class UIData : BaseData
    {

#if UNITY_EDITOR
        public Toggle Toggle { get; set; }
#endif

        //Displays the UI if true, hides it otherwise
        public ContainerValue<bool> show = new ContainerValue<bool>();


    }

}