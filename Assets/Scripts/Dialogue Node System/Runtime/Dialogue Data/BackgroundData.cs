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


        public ContainerValue<string> BackgroundName = new ContainerValue<string>();    //Grisé sur la node


        /// <summary>
        /// La transition avant de changer de décor
        /// </summary>
        public ContainerValue<TransitionSettingsSO> StartSettings = new ContainerValue<TransitionSettingsSO>();


        /// <summary>
        /// La transition après avoir changé de décor
        /// </summary>
        public ContainerValue<TransitionSettingsSO> EndSettings = new ContainerValue<TransitionSettingsSO>();



#if UNITY_EDITOR

        /// <summary>
        /// Contient le conteneur de l'arrière-plan pour le supprimer plus facilement dans DialogueSaveLoad
        /// </summary>
        public Box BoxContainer { get; set; }

        /// <summary>
        /// Le champ pour l'image de l'arrière-plan
        /// </summary>
        public Image TextureField { get; set; }

        public ObjectField ObjectField { get; set; }

        /// <summary>
        /// Le champ pour le nom de l'arrière-plan
        /// </summary>
        public TextField NameField { get; set; }


#endif
    }

}