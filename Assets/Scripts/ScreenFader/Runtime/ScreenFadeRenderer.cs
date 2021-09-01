using System.Linq;
using UnityEngine;




/* Le ScreenFader �tant un SO, il ne peut pas appeler OnRenderImage pour ex�cuter le fade.
 * Quand on ajoute ce Component sur la Camera.current, on lui ajoute un ScreenFadeRenderer
 * pour r�aliser le OnRenderImage sp�cifi� dans le ScreenFader.
 * Une fois la Coroutine du SO termin�e, on peut d�truire ce Component.
 * 
 * Ce script est aussi en charge de trouver tous les Canvas en mode Overlay et de les passer en mode Cam�ra.
 * (Car les canvas en Overlay ne sont pas affect�s par le ScreenFader)
 */

namespace Project.ScreenFader
{
    [RequireComponent(typeof(Camera))]
    [ExecuteInEditMode]
    public class ScreenFadeRenderer : MonoBehaviour
    {
        [Tooltip("Le SO en charge de la transition.")]
        private ScreenFaderSO _screenFader;

        //Variables utilis�es pour stocker les canvas au d�but de la transition pour les r�initialiser une fois celle-ci termin�e.
        private bool _setToCamera = false;
        private Canvas[] _overlayCanvases;
        private Camera _mainCam;
        private Camera MainCam { get => _mainCam ??= GetComponent<Camera>(); set => _mainCam = value; }




        public void SetScreenFader(ScreenFaderSO screenFader, bool shouldDestroyRendererOnEnded)
        {
            _screenFader = screenFader;

            //Au d�but de la transition, on r�cup�re tous les canvas en Overlay et on les passe en mode Camera.
            SetCanvases();

            //Si shouldDestroy est � false, on garde ce Component sur l'objet jusqu'�
            //ce qu'une transition future le d�truise.

            //Si � true, on abonne et d�sabonne automatiquement ce renderer au ScreenFader
            //pour d�truire ce Component une fois la transition termin�e.
            Subscribe(shouldDestroyRendererOnEnded);
        }


        private void Subscribe(bool shouldDestroyRendererOnEnded)
        {

            if (shouldDestroyRendererOnEnded)
            {
                _screenFader.OnTransitionEnded += CleanUp;
                _screenFader.OnTransitionEnded += SetCanvases;  //Vu que _setToCamera = true � ce moment, il repassera tous nos canvas en mode Overlay
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
                //Si � true, on doit changer tous les canvas en mode Cam�ra et leur assigner la cam�ra avec laquelle on affiche la sc�ne
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
                //Si � false, on ram�ne tous les canvas stock�s � leur �tat initial
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