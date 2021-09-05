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

        //Affiche le UI si à true, le cache sinon
        public ContainerValue<bool> show = new ContainerValue<bool>();


    }

}