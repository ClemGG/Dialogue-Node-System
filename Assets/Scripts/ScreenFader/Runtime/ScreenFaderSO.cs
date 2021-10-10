// This code is related to an answer I provided in the Unity forums at:
// http://forum.unity3d.com/threads/circular-fade-in-out-shader.344816/

using UnityEngine;
using System.Collections;

using Project.Enums;
using System;
using UnityEngine.UI;




/* Cette classe sert uniquement à gérer l'effet de transition sur la caméra.
 * On en fait un ScriptableObject pour permettre la persistence entre les scènes sans avoir à tout réassigner.
 * 
 * Pour exécuter des actions au début ou à la fin d'une transition (changement de niveau, etc.),
 * abonner ces actions aux delegates appropriés.
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
        [SerializeField, Range(0f, 2f)] private float _maskValue = 1f; //Mis à 1 pour qu'on puisse voir la scène


        private Material _fadeMaterial;
        private Material _blendMaterial;
        private bool _enabled;
        private Camera _currentCam;

        //Contient toutes les caméras de la scène. Celle qui fera office de Camera.current
        //sera celle avec la depth la plus élevée.
        private Camera[] allCamsInScene;

        #endregion


        #region Actions

        public Action OnTransitionStarted { get; set; }         //Quand le fade démarre
        public Action<float> OnTransitionUpdated { get; set; }  //Quand le fade met à jour m_maskValue (passée en paramètre pour que les autres scripts puissent faire leurs changements)
        public Action OnTransitionEnded { get; set; }           //Quand le fade se termine
        public Action OnCompleteTransitionMiddle { get; set; }  //Quand le double fade réalise son premier fade

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

            //Récupère le shader pour les fondus en noir et avec mask
            //(le fade transparent est géré par les scripts abonnés aux delegates pour récupérer la maskValue)

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
            Array.Sort(allCamsInScene);
            _currentCam = allCamsInScene[allCamsInScene.Length - 1];
        }


        /// <summary>
        /// Crée une Coroutine de fade sur le GameObject en paramètre. (le ScreenFader étant un SO, il doit passer la Coroutine à un MonoBehaviour)
        /// </summary>
        /// <param name="target">L'objet dans la scène qui doit porter la Coroutine</param>
        /// <param name="show">Doit-on afficher ou masquer la scène ?</param>
        /// <param name="shouldDestroyRendererOnEnded">Doit-on garder le renderer jusqu'à la prochaine transition ou le détruire ?</param>
        /// <param name="deltaTime">Permet de changer le Time.deltaTime en fixed ou unscaled si besoin</param>
        /// <param name="parameters">Les paramètres de transition. Si null, on utilise les paramètres déjà assignés.</param>
        public Coroutine StartFade(MonoBehaviour target, bool show, bool shouldDestroyRendererOnEnded, float deltaTime, TransitionSettingsSO parameters = null)
        {
            //_currentCam = Camera.current;
            GetCurrentCamera();
            ScreenFadeRenderer sfr = _currentCam.gameObject.GetComponent<ScreenFadeRenderer>();
            if (!sfr)
            {
                sfr = _currentCam.gameObject.AddComponent<ScreenFadeRenderer>();
            }
            sfr.SetScreenFader(this, shouldDestroyRendererOnEnded);   //Lie le Renderer à ce SO et s'abonne automatiquement pour être détruit à la fin de la transition


            if (parameters != null)
                _params = parameters;

            return target.StartCoroutine(FadeCo(show, deltaTime));
        }
        
            
            
        /// <summary>
        /// Crée une Coroutine de fade sur le GameObject en paramètre. (le ScreenFader étant un SO, il doit passer la Coroutine à un MonoBehaviour)
        /// </summary>
        /// <param name="target">L'objet dans la scène qui doit porter la Coroutine</param>
        /// <param name="show">Doit-on afficher ou masquer la scène ?</param>
        /// <param name="shouldDestroyRendererOnEnded">Doit-on garder le renderer jusqu'à la prochaine transition ou le détruire ?</param>
        /// <param name="deltaTime">Permet de changer le Time.deltaTime en fixed ou unscaled si besoin</param>
        /// <param name="endParams">Les paramètres de la 1è transition. Si null, on utilise les paramètres déjà assignés.</param>
        /// <param name="endParams">Les paramètres de la 2è transition. Si null, on utilise les paramètres déjà assignés.</param>
        public Coroutine StartCompleteFade(MonoBehaviour target, bool show, bool shouldDestroyRendererOnEnded, float deltaTime, TransitionSettingsSO startParams = null, TransitionSettingsSO endParams = null)
        {
            //_currentCam = Camera.current;
            GetCurrentCamera();
            ScreenFadeRenderer sfr = _currentCam.gameObject.GetComponent<ScreenFadeRenderer>();
            if (!sfr)
            {
                sfr = _currentCam.gameObject.AddComponent<ScreenFadeRenderer>();
            }
            sfr.SetScreenFader(this, shouldDestroyRendererOnEnded);   //Lie le Renderer à ce SO et s'abonne automatiquement pour être détruit à la fin de la transition

            return target.StartCoroutine(CompleteFadeCo(show, deltaTime, startParams, endParams));

        }




        /// <summary>
        /// Crée une Coroutine de blend sur l'image en paramètre. (le ScreenFader étant un SO, il doit passer la Coroutine à un MonoBehaviour)
        /// </summary>
        /// <param name="target">L'objet dans la scène qui doit porter la Coroutine</param>
        /// <param name="deltaTime">Permet de changer le Time.deltaTime en fixed ou unscaled si besoin</param>
        /// <param name="secTex">La texture vers laquelle transitionner.</param>
        /// <param name="parameters">Les paramètres de transition. Si null, on utilise les paramètres déjà assignés.</param>
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
        /// Diminue l'alpha du fondu pour faire apparaître la scène
        /// </summary>
        /// <returns></returns>
        public IEnumerator FadeCo(bool show, float deltaTime)
        {
            OnTransitionStarted?.Invoke();

            //La valeur de départ du masque pour montrer ou cacher la scène au début de la transition
            float t = _maskValue = show ? 0f : Mathf.Lerp(1f, 2f, _params.MaskSpread);

            //Si on veut montrer la scène, on augmente m_maskValue, sinon on la diminue
            float coef = show ? 1f : -1f;

            while (show ? t < 1f : t > 0f)
            {
                t += deltaTime * _params.Speed * coef;
                float value = _params.FadeCurve.Evaluate(t);
                _maskValue = Mathf.Lerp(value, value * 2f, _params.MaskSpread); //Pour le fade out, on ajoute le spread pour compenser l'ajout de noir sur le masques

                OnTransitionUpdated?.Invoke(_maskValue);

                yield return null;
            }

            WaitForSeconds wait = new WaitForSeconds(_params.DelayAfterTransition);
            yield return wait;

            OnTransitionEnded?.Invoke();
        }


        /// <summary>
        /// Diminue l'alpha du fondu pour faire apparaître la scène (utilise 2 transitions)
        /// </summary>
        /// <returns></returns>
        public IEnumerator CompleteFadeCo(bool show, float deltaTime, TransitionSettingsSO startParams, TransitionSettingsSO endParams)
        {

            OnTransitionStarted?.Invoke();


            //--------------------------- 1è transition -----------------------

            if (startParams != null)
                _params = startParams;

            //La valeur de départ du masque pour montrer ou cacher la scène au début de la transition
            float t = _maskValue = show ? 0f : Mathf.Lerp(1f, 2f, _params.MaskSpread);

            //Si on veut montrer la scène, on augmente m_maskValue, sinon on la diminue
            float coef = show ? 1f : -1f;

            while (show ? t < 1f : t > 0f)
            {
                t += deltaTime * _params.Speed * coef;
                float value = _params.FadeCurve.Evaluate(t);
                _maskValue = Mathf.Lerp(value, value * 2f, _params.MaskSpread); //Pour le fade out, on ajoute le spread pour compenser l'ajout de noir sur le masques

                OnTransitionUpdated?.Invoke(_maskValue);

                yield return null;
            }


            OnCompleteTransitionMiddle?.Invoke();

            WaitForSeconds wait = new WaitForSeconds(_params.DelayAfterTransition);
            yield return wait;



            //--------------------------- 2è transition -----------------------


            if (endParams != null)
                _params = endParams;

            //La 2è transition est l'inverse de la première, donc on se contente juste d'inverser le bool
            show = !show;

            //La valeur de départ du masque pour montrer ou cacher la scène au début de la transition
            t = _maskValue = show ? 0f : Mathf.Lerp(1f, 2f, _params.MaskSpread);

            //Si on veut montrer la scène, on augmente m_maskValue, sinon on la diminue
            coef = show ? 1f : -1f;

            while (show ? t < 1f : t > 0f)
            {
                t += deltaTime * _params.Speed * coef;
                float value = _params.FadeCurve.Evaluate(t);
                _maskValue = Mathf.Lerp(value, value * 2f, _params.MaskSpread); //Pour le fade out, on ajoute le spread pour compenser l'ajout de noir sur le masques

                OnTransitionUpdated?.Invoke(_maskValue);

                yield return null;
            }

            wait = new WaitForSeconds(_params.DelayAfterTransition);
            yield return wait;



            OnTransitionEnded?.Invoke();
        }



        /// <summary>
        /// Transitionne entre la 1è et la 2è texture du material de l'Image
        /// </summary>
        /// <returns></returns>
        public IEnumerator BlendCo(Image target, float deltaTime)
        {
            OnTransitionStarted?.Invoke();


            //La valeur de départ du masque pour montrer ou cacher la scène au début de la transition
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
            // Pour le TransparentFade, c'est un autre shader qui se charge du fade des textures 
            // depuis un autre script qui se sera abonné aux delegates du ScreenFader.
            // Le ScreenFader n'a rien d'autre à faire que de mettre à jour la maskValue dans ce cas.

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