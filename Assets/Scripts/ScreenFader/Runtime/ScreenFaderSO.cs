// This code is related to an answer I provided in the Unity forums at:
// http://forum.unity3d.com/threads/circular-fade-in-out-shader.344816/

using UnityEngine;
using System.Collections;

using Project.Enums;
using System;
using UnityEngine.UI;
using System.Linq;




/* Executes the transition effect on the specified camera.
 * We made it a SO to allow reusability between scenes.
 * 
 * To execute actions at the beginning/end of a transition (changing level, etc.),
 * please subscribe those actions to their appropriate delegates.
 */


namespace Project.ScreenFader
{


    [CreateAssetMenu(fileName = "New Screen Fader", menuName = "Scriptable Objects/Screen Fader/New Screen Fader")]
    public class ScreenFaderSO : ScriptableObject
    {

        #region Fields


        #region Transition Settings

        [Space(10)]
        [Header("Transition Settings : ")]
        [Space(10)]


        [SerializeField] private Shader _fadeShader;
        [SerializeField] private Shader _blendShader;
        [SerializeField] private TransitionSettingsSO _params;
        [SerializeField, Range(0f, 2f)] private float _maskValue = 1f; //Set to 1 so we can see the scene in Edit mode


        private Material _fadeMaterial;
        private Material _blendMaterial;
        private bool _enabled;
        private Camera _currentCam;

        //Contains all cameras in the scene. The one acting as Camera.current
        //will be the one with the highest depth.
        private Camera[] allCamsInScene;

        #endregion


        #region Actions

        public Action OnTransitionStarted { get; set; }         //When the fade starts
        public Action<float> OnTransitionUpdated { get; set; }  //When the fade updates m_maskValue (passed as a parameter so that other scripts can make their own changes)
        public Action OnTransitionEnded { get; set; }           //When the fade ends
        public Action OnCompleteTransitionMiddle { get; set; }  //When the double fade completes its first fade

        #endregion



        #endregion








        #region Mono

#if UNITY_EDITOR
        private void OnValidate()
        {
            OnEnable();
        }
#endif



        void OnEnable()
        {
            _enabled = true;

            //Retrieves the shader for the Fade and Mask modes
            //(the Blend mode is run by the scripts subscribed to retrieve the maskValue)

            if(!_blendShader) _blendShader = Shader.Find("Custom/Effects/UI/BlendTexture");
            if(!_fadeShader) _fadeShader = Shader.Find("Custom/Effects/UI/ScreenTransitionImageEffect");

            // Disable the image effect if the shader can't
            // run on the users graphics card
            if (_fadeShader == null || !_fadeShader.isSupported)
            {
                Debug.LogError("Le shader \"ScreenTransitionImageEffect\" est introuvable.");
                _enabled = false;
            }

        }



        #endregion






        #region Screen Fader

        public void GetAllCamerasInScene()
        {
            allCamsInScene = FindObjectsOfType<Camera>(true);
        }

        public void GetCurrentCamera()
        {
            //Sorts all cameras bu depth; the highest one will be the first
            allCamsInScene.ToList().Sort(delegate (Camera x, Camera y)
            {
                return x.depth.CompareTo(y.depth);
            });
            _currentCam = allCamsInScene[0];
        }


        /// <summary>
        /// Creates a fade Coroutine on the target. (the ScreenFader being a SO, it must pass the Coroutine to a MonoBehaviour)
        /// </summary>
        /// <param name="target">To object to start the Coroutine on.</param>
        /// <param name="show">Shouldwe display or hide the scene?</param>
        /// <param name="shouldDestroyRendererOnEnded">Keep the renderer until the next transition or destroy it?</param>
        /// <param name="deltaTime">Allows to change Time.deltaTime into fixed or unscaled if needed.</param>
        /// <param name="parameters">The Transition Settings. If null, it uses the one already assigned.</param>
        public Coroutine StartFade(MonoBehaviour target, bool show, bool shouldDestroyRendererOnEnded, float deltaTime, TransitionSettingsSO parameters = null)
        {
            //_currentCam = Camera.current;
            GetCurrentCamera();
            ScreenFadeRenderer sfr = _currentCam.gameObject.GetComponent<ScreenFadeRenderer>();
            if (!sfr)
            {
                sfr = _currentCam.gameObject.AddComponent<ScreenFadeRenderer>();
            }
            sfr.SetScreenFader(this, shouldDestroyRendererOnEnded);   //Links the Renderer to this SO and subscribes automatically to be destroyed at the end of the transition


            if (parameters != null)
                _params = parameters;

            return target.StartCoroutine(FadeCo(show, deltaTime));
        }



        /// <summary>
        /// Creates a fade Coroutine on the target. (the ScreenFader being a SO, it must pass the Coroutine to a MonoBehaviour)
        /// </summary>
        /// <param name="target">To object to start the Coroutine on.</param>
        /// <param name="show">Shouldwe display or hide the scene?</param>
        /// <param name="shouldDestroyRendererOnEnded">Keep the renderer until the next transition or destroy it?</param>
        /// <param name="deltaTime">Allows to change Time.deltaTime into fixed or unscaled if needed.</param>
        /// <param name="startParams">The parameters of the 1st transition. If null, it uses the one already assigned.</param>
        /// <param name="endParams">The parameters of the 2ns transition. If null, it uses the one already assigned.</param>
        public Coroutine StartCompleteFade(MonoBehaviour target, bool show, bool shouldDestroyRendererOnEnded, float deltaTime, TransitionSettingsSO startParams = null, TransitionSettingsSO endParams = null)
        {
            //_currentCam = Camera.current;
            GetCurrentCamera();
            ScreenFadeRenderer sfr = _currentCam.gameObject.GetComponent<ScreenFadeRenderer>();
            if (!sfr)
            {
                sfr = _currentCam.gameObject.AddComponent<ScreenFadeRenderer>();
            }
            sfr.SetScreenFader(this, shouldDestroyRendererOnEnded);   //Links the Renderer to this SO and subscribes automatically to be destroyed at the end of the transition

            return target.StartCoroutine(CompleteFadeCo(show, deltaTime, startParams, endParams));

        }




        /// <summary>
        /// Creates a blend Coroutine on the target. (the ScreenFader being a SO, it must pass the Coroutine to a MonoBehaviour)
        /// </summary>
        /// <param name="target">To object to start the Coroutine on.</param>
        /// <param name="deltaTime">Allows to change Time.deltaTime into fixed or unscaled if needed.</param>
        /// <param name="secTex">The texture to transition to.</param>
        /// <param name="parameters">The Transition Settings. If null, it uses the one already assigned.</param>
        public Coroutine StartBlend(Image target, float deltaTime, Texture2D secTex, TransitionSettingsSO parameters = null)
        {
            if (!target.material)
            {
                Debug.Log("Erreur : L'image doit avoir un material avec le Shader BlendTexture pour fonctionner.");
                return null;
            }


            _blendMaterial = target.materialForRendering;

            _blendMaterial.SetTexture("_SecTex", secTex);
            _blendMaterial.SetFloat("_Blend", 0f);
            target.SetMaterialDirty();

            if (parameters != null)
                _params = parameters;


            return target.StartCoroutine(BlendCo(target, deltaTime));
        }


        /// <summary>
        /// Reduces the fade of the alpha to make the scene appear
        /// </summary>
        /// <returns></returns>
        public IEnumerator FadeCo(bool show, float deltaTime)
        {
            OnTransitionStarted?.Invoke();

            //init maskValue
            float t = _maskValue = show ? 0f : Mathf.Lerp(1f, 2f, _params.MaskSpread);

            //If we want to show the scene, we increase m_maskValue, otherwise we decrease it
            float coef = show ? 1f : -1f;

            while (show ? t < 1f : t > 0f)
            {
                t += deltaTime * _params.Speed * coef;
                float value = _params.FadeCurve.Evaluate(t);
                _maskValue = Mathf.Lerp(value, value * 2f, _params.MaskSpread); //For the fade out, we add the spread of the mask to compensate the dark scattering on the masks

                OnTransitionUpdated?.Invoke(_maskValue);

                yield return null;
            }

            WaitForSeconds wait = new WaitForSeconds(_params.DelayAfterTransition);
            yield return wait;

            OnTransitionEnded?.Invoke();
        }


        /// <summary>
        /// Reduces the fade of the alpha to make the scene appear (2 transitions)
        /// </summary>
        /// <returns></returns>
        public IEnumerator CompleteFadeCo(bool show, float deltaTime, TransitionSettingsSO startParams, TransitionSettingsSO endParams)
        {

            OnTransitionStarted?.Invoke();


            //--------------------------- 1st transition -----------------------

            if (startParams != null)
                _params = startParams;

            //init maskValue
            float t = _maskValue = show ? 0f : Mathf.Lerp(1f, 2f, _params.MaskSpread);

            //If we want to show the scene, we increase m_maskValue, otherwise we decrease it
            float coef = show ? 1f : -1f;

            while (show ? t < 1f : t > 0f)
            {
                t += deltaTime * _params.Speed * coef;
                float value = _params.FadeCurve.Evaluate(t);
                _maskValue = Mathf.Lerp(value, value * 2f, _params.MaskSpread); //For the fade out, we add the spread of the mask to compensate the dark scattering on the masks

                OnTransitionUpdated?.Invoke(_maskValue);

                yield return null;
            }


            OnCompleteTransitionMiddle?.Invoke();

            WaitForSeconds wait = new WaitForSeconds(_params.DelayAfterTransition);
            yield return wait;



            //--------------------------- 2è transition -----------------------


            if (endParams != null)
                _params = endParams;

            //The 2nd transition is the opposite of the first, so we just invert the bool
            show = !show;


            //init maskValue
            t = _maskValue = show ? 0f : Mathf.Lerp(1f, 2f, _params.MaskSpread);


            //If we want to show the scene, we increase m_maskValue, otherwise we decrease it
            coef = show ? 1f : -1f;

            while (show ? t < 1f : t > 0f)
            {
                t += deltaTime * _params.Speed * coef;
                float value = _params.FadeCurve.Evaluate(t);
                _maskValue = Mathf.Lerp(value, value * 2f, _params.MaskSpread); //For the fade out, we add the spread of the mask to compensate the dark scattering on the masks

                OnTransitionUpdated?.Invoke(_maskValue);

                yield return null;
            }

            wait = new WaitForSeconds(_params.DelayAfterTransition);
            yield return wait;



            OnTransitionEnded?.Invoke();
        }



        /// <summary>
        /// Transitions between the 1st and 2nd texture of the image's material
        /// </summary>
        /// <returns></returns>
        public IEnumerator BlendCo(Image target, float deltaTime)
        {
            OnTransitionStarted?.Invoke();


            //init maskValue
            float t = _maskValue = 0f;


            while (t < 1f)
            {
                t += deltaTime * _params.Speed;
                _maskValue = _params.FadeCurve.Evaluate(t);

                _blendMaterial.SetFloat("_Blend", _maskValue);

                OnTransitionUpdated?.Invoke(_maskValue);

                yield return null;
            }

            _blendMaterial.SetTexture("_MainTex", _blendMaterial.GetTexture("_SecTex"));
            _blendMaterial.SetFloat("_Blend", 0f);
            target.SetMaterialDirty();

            WaitForSeconds wait = new WaitForSeconds(_params.DelayAfterTransition);
            yield return wait;

            OnTransitionEnded?.Invoke();
        }










        #endregion









        #region Shader

        Material Material
        {
            get
            {
                if (_fadeMaterial == null)
                {
                    _fadeMaterial = new Material(_fadeShader)
                    {
                        hideFlags = HideFlags.HideAndDontSave
                    };
                }
                return _fadeMaterial;
            }
        }

        private void OnDisable()
        {
            DestroyImmediate(_fadeMaterial);
        }

        public void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            //The Blend mode is managed by another material,
            //all we do in this case is updating the maskVlue.

            if (!_enabled || _params == null || _params.TransitionType == FaderTransitionType.TextureBlend)
            {
                Graphics.Blit(source, destination);
                return;
            }

            Material.SetColor("_MaskColor", _params.FadeColor);
            Material.SetFloat("_MaskValue", _maskValue);
            Material.SetFloat("_MaskSpread", _params.MaskSpread);
            Material.SetFloat("_FadeMode", (float)_params.TransitionType);
            Material.SetTexture("_MainTex", source);
            Material.SetTexture("_MaskTex", _params.MaskTexture);

            if (Material.IsKeywordEnabled("INVERT_MASK") != _params.InvertMask)
            {
                if (_params.InvertMask)
                    Material.EnableKeyword("INVERT_MASK");
                else
                    Material.DisableKeyword("INVERT_MASK");
            }

            Graphics.Blit(source, destination, Material);
        }

        #endregion
    }
}