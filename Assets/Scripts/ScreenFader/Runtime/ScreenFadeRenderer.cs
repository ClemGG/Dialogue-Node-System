using System.Linq;
using UnityEngine;




/* Le ScreenFader étant un SO, il ne peut pas appeler OnRenderImage pour exécuter le fade.
 * Quand on ajoute ce Component sur la Camera.current, on lui ajoute un ScreenFadeRenderer
 * pour réaliser le OnRenderImage spécifié dans le ScreenFader.
 * Une fois la Coroutine du SO terminée, on peut détruire ce Component.
 * 
 * Ce script est aussi en charge de trouver tous les Canvas en mode Overlay et de les passer en mode Caméra.
 * (Car les canvas en Overlay ne sont pas affectés par le ScreenFader)
 */

namespace Project.ScreenFader
{
    [RequireComponent(typeof(Camera))]
    [ExecuteInEditMode]
    public class ScreenFadeRenderer : MonoBehaviour
    {
        [Tooltip("Le SO en charge de la transition.")]
        private ScreenFaderSO _screenFader;

        //Variables utilisées pour stocker les canvas au début de la transition pour les réinitialiser une fois celle-ci terminée.
        private bool _setToCamera = false;
        private Canvas[] _overlayCanvases;
        private Camera _mainCam;
        private Camera MainCam { get => _mainCam ??= GetComponent<Camera>(); set => _mainCam = value; }




        public void SetScreenFader(ScreenFaderSO screenFader, bool shouldDestroyRendererOnEnded)
        {
            _screenFader = screenFader;

            //Au début de la transition, on récupère tous les canvas en Overlay et on les passe en mode Camera.
            SetCanvases();

            //Si shouldDestroy est à false, on garde ce Component sur l'objet jusqu'à
            //ce qu'une transition future le détruise.

            //Si à true, on abonne et désabonne automatiquement ce renderer au ScreenFader
            //pour détruire ce Component une fois la transition terminée.
            Subscribe(shouldDestroyRendererOnEnded);
        }


        private void Subscribe(bool shouldDestroyRendererOnEnded)
        {

            if (shouldDestroyRendererOnEnded)
            {
                _screenFader.OnTransitionEnded += CleanUp;
                _screenFader.OnTransitionEnded += SetCanvases;  //Vu que _setToCamera = true à ce moment, il repassera tous nos canvas en mode Overlay
            }
        }

        private void CleanUp()
        {
            Destroy(this);
            _screenFader.OnTransitionEnded -= CleanUp;
            _screenFader.OnTransitionEnded -= SetCanvases;
        }



        public void SetCanvases()
        {
            _setToCamera = !_setToCamera;

            if (_setToCamera)
            {
                //Si à true, on doit changer tous les canvas en mode Caméra et leur assigner la caméra avec laquelle on affiche la scène
                _overlayCanvases = FindObjectsOfType<Canvas>().Where(canvas => canvas.renderMode == RenderMode.ScreenSpaceOverlay).ToArray();

                for (int i = 0; i < _overlayCanvases.Length; i++)
                {
                    _overlayCanvases[i].renderMode = RenderMode.ScreenSpaceCamera;
                    _overlayCanvases[i].worldCamera = MainCam;
                    _overlayCanvases[i].planeDistance = MainCam.nearClipPlane + .1f;
                }
            }
            else
            {
                //Si à false, on ramène tous les canvas stockés à leur état initial
                for (int i = 0; i < _overlayCanvases.Length; i++)
                {
                    _overlayCanvases[i].renderMode = RenderMode.ScreenSpaceOverlay;
                }
            }
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            _screenFader.OnRenderImage(source, destination);
        }
    }
}