using System.Linq;
using UnityEngine;




/* The ScreenFader being an SO, it cannot call OnRenderImage to execute the fade.
 * We add this component to call OnRenderImage() from a MonoBehaviour.
 * Once the Coroutine finished, we can destroy this Component.
 * 
 * This script is also in charge of fiding all canvases in Overlay Mode and setting them to Camera Mode.
 * (Bc the canvas are only affected by the fade in this mode.)
 */

namespace Project.ScreenFader
{
    [RequireComponent(typeof(Camera))]
    [ExecuteInEditMode]
    public class ScreenFadeRenderer : MonoBehaviour
    {
        [Tooltip("Le SO en charge de la transition.")]
        private ScreenFaderSO _screenFader;

        //Temp. variable to store the canvases.
        private bool _setToCamera = false;
        private Canvas[] _overlayCanvases;
        private Camera _mainCam;
        private Camera MainCam { get => _mainCam ??= GetComponent<Camera>(); set => _mainCam = value; }




        public void SetScreenFader(ScreenFaderSO screenFader, bool shouldDestroyRendererOnEnded)
        {
            _screenFader = screenFader;

            //Finds all canvases in Overlay Mode and sets them to Camera Mode.
            SetCanvases();

            //If shouldDestroy = false, we keep this Component until a future transition gets rid of it.

            //If shouldDestroy = true, we automatically sub and unsub this to the ScreenFader
            //to destroy it once the transition completed.
            Subscribe(shouldDestroyRendererOnEnded);
        }


        private void Subscribe(bool shouldDestroyRendererOnEnded)
        {

            if (shouldDestroyRendererOnEnded)
            {
                _screenFader.OnTransitionEnded += CleanUp;
                _screenFader.OnTransitionEnded += SetCanvases;  //Since _setToCamera = true at this point, it will set all of them in Overlay mode
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
                //If true, we change all canvases in Camera Mode et assign them the camera used to draw the scene
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
                //If false, all canvases return to normal.
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