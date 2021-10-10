using UnityEngine;
using Project.ScreenFader;

#if UNITY_EDITOR
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif


namespace Project.NodeSystem
{

    [System.Serializable]
    public class BackgroundData : BaseData
    {
        public ContainerValue<BackgroundData_Transition> Transition = new ContainerValue<BackgroundData_Transition>();
    }

    [System.Serializable]
    public class BackgroundData_Transition : NodeData_BaseContainer
    {
        public ContainerValue<Texture2D> BackgroundTex = new ContainerValue<Texture2D>();


        public ContainerValue<string> BackgroundName = new ContainerValue<string>();


        /// <summary>
        /// The transition before changing the background
        /// </summary>
        public ContainerValue<TransitionSettingsSO> StartSettings = new ContainerValue<TransitionSettingsSO>();


        /// <summary>
        /// The transition after changing the background
        /// </summary>
        public ContainerValue<TransitionSettingsSO> EndSettings = new ContainerValue<TransitionSettingsSO>();



#if UNITY_EDITOR

        /// <summary>
        /// Contains the background container to remove it more easily in DialogueSaveLoad
        /// </summary>
        public Box BoxContainer { get; set; }

        /// <summary>
        /// The background image field
        /// </summary>
        public Image TextureField { get; set; }

        public ObjectField ObjectField { get; set; }

        /// <summary>
        /// The background name field
        /// </summary>
        public TextField NameField { get; set; }


#endif
    }

}