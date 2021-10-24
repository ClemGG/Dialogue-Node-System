#if UNITY_EDITOR
using UnityEngine.UIElements;
#endif

namespace Project.NodeSystem
{
    [System.Serializable]
    public class UIData : BaseData
    {

#if UNITY_EDITOR

        public Toggle ClearToggle { get; set; }
        public Toggle ShowToggle { get; set; }
#endif

        //Displays the UI if true, hides it otherwise
        public ContainerValue<bool> Show = new ContainerValue<bool>();

        //If true, clears the characters' sprites before showing the new ones.
        //Use this if you want to immediately show new characters after the ui was hidden,
        //so that the old ones don't briefly flash on screen before dissapearing
        public ContainerValue<bool> ClearCharSprites = new ContainerValue<bool>();



    }

}