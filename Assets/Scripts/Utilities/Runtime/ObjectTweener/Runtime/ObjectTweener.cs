using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Utilities.Tween
{

    public class ObjectTweener : MonoBehaviour
    {

        #region Fields


        #region Public

        [SerializeField] private Transform _objectToAnimate;

        [Tooltip("A remplir si on veut lancer plusieurs tweens à la fois depuis OnEnable.")]
        [SerializeField] private TweenSettings[] tweensOnEnable;

        [Tooltip("Si à true, lance les tweensOnEnable à chaque appel d'OnEnable().")]
        public bool startOnEnable;
        [Tooltip("Si à true, désactive l'objet en fin de tween.")]
        public bool disableOnStop;
        [Tooltip("Si à true, détruit l'objet en fin de tween.")]
        public bool destroyOnStop;

        [Tooltip("Si à true, OnEnable() appelle l'Awake() pour la remise à 0 de la transform.")]
        public bool saveTransformOnEnable;
        [Tooltip("Si à true, la transform est remise à zéro dans OnDisable().")]
        public bool resetTransformOnDisable = true;
        [Tooltip("Si à true, la transform est remise à zéro quand le tween sera terminé.")]
        public bool resetTransformOnTweenEnded = false;

        #endregion


        #region Private

        private List<LTDescr> _tweeners = new List<LTDescr>();
        private CanvasGroup cg;
        private Vector3 startPos, startRot, startScale;

        #endregion



        #region Accessors

        public Transform ObjectToAnimate
        {
            get { if (_objectToAnimate == null) _objectToAnimate = transform; return _objectToAnimate; }
            set => _objectToAnimate = value;
        }
        public TweenSettings Settings { get; set; }
        public Action<float> OnValueUpdated { get; set; }
        public CanvasGroup Cg
        {
            get
            {
                if (!ObjectToAnimate.TryGetComponent(out cg))
                {
                    cg = ObjectToAnimate.gameObject.AddComponent<CanvasGroup>();
                }
                return cg;
            }
            set => cg = value;
        }

        #endregion

        #endregion



        #region Mono

        private void Awake()
        {
            startPos = ObjectToAnimate.localPosition;
            startRot = ObjectToAnimate.eulerAngles;
            startScale = ObjectToAnimate.localScale;
        }

        private void OnEnable()
        {
            if (saveTransformOnEnable)
            {
                Awake();
            }

            if (startOnEnable)
            {
                for (int i = 0; i < tweensOnEnable.Length; i++)
                {
                    BeginTween(tweensOnEnable[i]);
                }
            }
        }

        private void OnDisable()
        {

            if (resetTransformOnDisable)
            {
                Disable();
            }
            else
            {
                Stop();
            }
        }

        private void OnDestroy()
        {
            Disable();
        }


        #endregion




        #region Settings

        public void SetTarget(Transform target)
        {
            ObjectToAnimate = target;
        }

        public void SetSettings(TweenSettings newSettings)
        {
            if (newSettings)
                Settings = newSettings;
        }

        private void Disable()
        {
            Stop();

            ObjectToAnimate.localPosition = startPos;
            ObjectToAnimate.eulerAngles = startRot;
            ObjectToAnimate.localScale = startScale;
        }

        public void Stop()
        {
            LeanTween.cancel(ObjectToAnimate.gameObject);
            _tweeners.Clear();
        }

        public void ResetAlpha(float alpha = 1f)
        {
            Cg.alpha = alpha;
        }

        #endregion



        #region Tween

        public void BeginTween(TweenSettings newSettings)
        {
            SetSettings(newSettings);

            LTDescr _tweener = new LTDescr();

            switch (Settings.AnimationType)
            {
                case TweenAnimationType.Move:
                    _tweener = Move();
                    break;
                case TweenAnimationType.MoveX:
                    _tweener = MoveX();
                    break;
                case TweenAnimationType.MoveY:
                    _tweener = MoveY();
                    break;
                case TweenAnimationType.MoveZ:
                    _tweener = MoveZ();
                    break;



                case TweenAnimationType.Rot:
                    _tweener = Rot();
                    break;
                case TweenAnimationType.RotX:
                    _tweener = RotX();
                    break;
                case TweenAnimationType.RotY:
                    _tweener = RotY();
                    break;
                case TweenAnimationType.RotZ:
                    _tweener = RotZ();
                    break;



                case TweenAnimationType.Scale:
                    _tweener = Scale();
                    break;
                case TweenAnimationType.ScaleX:
                    _tweener = ScaleX();
                    break;
                case TweenAnimationType.ScaleY:
                    _tweener = ScaleY();
                    break;
                case TweenAnimationType.ScaleZ:
                    _tweener = ScaleZ();
                    break;



                case TweenAnimationType.Fade:
                    _tweener = Fade();
                    break;
                case TweenAnimationType.Color:
                    _tweener = Color();
                    break;
                case TweenAnimationType.Value:
                    _tweener = Value();
                    _tweener.setOnUpdate(OnValueUpdated);
                    break;
                case TweenAnimationType.Width:
                    _tweener = Width();
                    break;
                case TweenAnimationType.Height:
                    _tweener = Height();
                    break;
            }


            _tweener.setDelay(Settings.Delay);

            if (Settings.CurveType == LeanTweenType.animationCurve)
                _tweener.setEase(Settings.Curve);
            else
                _tweener.setEase(Settings.CurveType);


            if (Settings.Loop)
            {
                _tweener.loopCount = int.MaxValue;
                _tweener.setLoopClamp();
            }
            if (Settings.PingPong)
            {
                _tweener.loopCount = int.MaxValue;
                _tweener.setLoopPingPong();
            }

            _tweener.setOnComplete(() =>
            {

                if (Settings.Loop)
                {
                    _tweener.reset();
                    _tweener.resume();
                }
                else if (destroyOnStop)
                {
                    Destroy(ObjectToAnimate);
                    _tweener.destroyOnComplete = true;
                }
                else if (disableOnStop)
                {
                    ObjectToAnimate.gameObject.SetActive(false);
                    _tweener.reset();
                }


                //Ce trub bugue et peut répéter des tweens en boucle, alors je le désactive
                //jusqu'à ce que je me penche dessus

                //if (!Settings.Loop && !Settings.PingPong)
                //{
                //    for (int i = 0; i < Settings.OnComplete.Length; i++)
                //    {
                //        BeginTween(Settings.OnComplete[i]);
                //    }
                //}
            });

            _tweeners.Add(_tweener);
        }

        //Si on veut l'appeler par code et lui ajouter des fonctionnalités supplémentaires
        public LTDescr BeginTweenByCode(TweenSettings newSettings)
        {
            SetSettings(newSettings);

            LTDescr _tweener = new LTDescr();

            switch (Settings.AnimationType)
            {
                case TweenAnimationType.Move:
                    _tweener = Move();
                    break;
                case TweenAnimationType.MoveX:
                    _tweener = MoveX();
                    break;
                case TweenAnimationType.MoveY:
                    _tweener = MoveY();
                    break;
                case TweenAnimationType.MoveZ:
                    _tweener = MoveZ();
                    break;



                case TweenAnimationType.Rot:
                    _tweener = Rot();
                    break;
                case TweenAnimationType.RotX:
                    _tweener = RotX();
                    break;
                case TweenAnimationType.RotY:
                    _tweener = RotY();
                    break;
                case TweenAnimationType.RotZ:
                    _tweener = RotZ();
                    break;



                case TweenAnimationType.Scale:
                    _tweener = Scale();
                    break;
                case TweenAnimationType.ScaleX:
                    _tweener = ScaleX();
                    break;
                case TweenAnimationType.ScaleY:
                    _tweener = ScaleY();
                    break;
                case TweenAnimationType.ScaleZ:
                    _tweener = ScaleZ();
                    break;



                case TweenAnimationType.Fade:
                    _tweener = Fade();
                    break;
                case TweenAnimationType.Color:
                    _tweener = Color();
                    break;
                case TweenAnimationType.Value:
                    _tweener = Value();
                    _tweener.setOnUpdate(OnValueUpdated);
                    break;
                case TweenAnimationType.Width:
                    _tweener = Width();
                    break;
                case TweenAnimationType.Height:
                    _tweener = Height();
                    break;
            }


            _tweener.setDelay(Settings.Delay);

            if (Settings.CurveType == LeanTweenType.animationCurve)
                _tweener.setEase(Settings.Curve);
            else
                _tweener.setEase(Settings.CurveType);


            if (Settings.Loop)
            {
                _tweener.loopCount = int.MaxValue;
                _tweener.setLoopClamp();
            }
            if (Settings.PingPong)
            {
                _tweener.loopCount = int.MaxValue;
                _tweener.setLoopPingPong();
            }

            _tweener.setOnComplete(() =>
            {

                if (Settings.Loop)
                {
                    _tweener.reset();
                    _tweener.resume();
                }
                else if (destroyOnStop)
                {
                    Destroy(ObjectToAnimate);
                    _tweener.destroyOnComplete = true;
                }
                else if (disableOnStop)
                {
                    ObjectToAnimate.gameObject.SetActive(false);
                    _tweener.reset();
                }

                //Ce trub bugue et peut répéter des tweens en boucle, alors je le désactive
                //jusqu'à ce que je me penche dessus

                //if (!Settings.Loop && !Settings.PingPong)
                //{
                //    for (int i = 0; i < Settings.OnComplete.Length; i++)
                //    {
                //        BeginTween(Settings.OnComplete[i]);
                //    }
                //}
            });

            _tweeners.Add(_tweener);
            return _tweener;
        }

        //Si on veut l'appeler par code et lui ajouter des fonctionnalités supplémentaires
        public LTDescr[] BeginTweens(TweenSettings[] newSettings, Action onComplete = null)
        {
            LTDescr[] tweens = new LTDescr[newSettings.Length];

            for (int i = 0; i < tweens.Length; i++)
            {
                tweens[i] = BeginTweenByCode(newSettings[i]);
                _tweeners.Add(tweens[i]);
            }

            if (resetTransformOnTweenEnded)
            {
                onComplete += Disable;
            }

            if(onComplete != null)
            {
                tweens.SetOnTweensComplete(onComplete);
            }

            return tweens;
        }

        private LTDescr Move()
        {
            Vector3 to = Settings.RelativeTo(ObjectToAnimate);

            if (ObjectToAnimate.TryGetComponent(out RectTransform rt))
            {
                if (Settings.UseFromAsStart)
                {
                    rt.position = Settings.RelativeFrom(rt);

                }
            }
            else
            {
                if (Settings.UseFromAsStart)
                {
                    ObjectToAnimate.position = Settings.RelativeFrom(ObjectToAnimate);
                }
            }


            return LeanTween.move(ObjectToAnimate.gameObject, to, Settings.Duration);

        }

        private LTDescr MoveX()
        {
            Vector3 to = Settings.RelativeTo(ObjectToAnimate);

            if (ObjectToAnimate.TryGetComponent(out RectTransform rt))
            {
                if (Settings.UseFromAsStart)
                {
                    rt.position = Settings.RelativeFrom(rt);
                }
            }
            else
            {
                if (Settings.UseFromAsStart)
                {
                    ObjectToAnimate.position = Settings.RelativeFrom(ObjectToAnimate);
                }
            }

            return LeanTween.moveX(ObjectToAnimate.gameObject, to.x, Settings.Duration);

        }

        private LTDescr MoveY()
        {
            Vector3 to = Settings.RelativeTo(ObjectToAnimate);

            if (ObjectToAnimate.TryGetComponent(out RectTransform rt))
            {
                if (Settings.UseFromAsStart)
                {
                    rt.position = Settings.RelativeFrom(rt);

                }
            }
            else
            {
                if (Settings.UseFromAsStart)
                {
                    ObjectToAnimate.position = Settings.RelativeFrom(ObjectToAnimate);
                }
            }


            return LeanTween.moveY(ObjectToAnimate.gameObject, to.y, Settings.Duration);

        }

        private LTDescr MoveZ()
        {
            Vector3 to = Settings.RelativeTo(ObjectToAnimate);

            if (ObjectToAnimate.TryGetComponent(out RectTransform rt))
            {
                if (Settings.UseFromAsStart)
                {
                    rt.position = Settings.RelativeFrom(rt);

                }
            }
            else
            {
                if (Settings.UseFromAsStart)
                {
                    ObjectToAnimate.position = Settings.RelativeFrom(ObjectToAnimate);
                }
            }

            return LeanTween.moveZ(ObjectToAnimate.gameObject, to.z, Settings.Duration);

        }

        private LTDescr Rot()
        {
            Vector3 to = Settings.RelativeTo(ObjectToAnimate);


            if (ObjectToAnimate.TryGetComponent(out RectTransform rt))
            {
                if (Settings.UseFromAsStart)
                {
                    rt.eulerAngles = Settings.RelativeFrom(rt);
                }
            }
            else
            {
                if (Settings.UseFromAsStart)
                {
                    ObjectToAnimate.eulerAngles = Settings.RelativeFrom(ObjectToAnimate);
                }
            }


            return LeanTween.rotate(ObjectToAnimate.gameObject, to, Settings.Duration);
        }

        private LTDescr RotX()
        {
            Vector3 to = Settings.RelativeTo(ObjectToAnimate);


            if (ObjectToAnimate.TryGetComponent(out RectTransform rt))
            {
                if (Settings.UseFromAsStart)
                {
                    rt.eulerAngles = Settings.RelativeFrom(rt);
                }
            }
            else
            {
                if (Settings.UseFromAsStart)
                {
                    ObjectToAnimate.eulerAngles = Settings.RelativeFrom(ObjectToAnimate);
                }
            }


            return LeanTween.rotateX(ObjectToAnimate.gameObject, to.x, Settings.Duration);
        }

        private LTDescr RotY()
        {
            Vector3 to = Settings.RelativeTo(ObjectToAnimate);


            if (ObjectToAnimate.TryGetComponent(out RectTransform rt))
            {
                if (Settings.UseFromAsStart)
                {
                    rt.eulerAngles = Settings.RelativeFrom(rt);
                }
            }
            else
            {
                if (Settings.UseFromAsStart)
                {
                    ObjectToAnimate.eulerAngles = Settings.RelativeFrom(ObjectToAnimate);
                }
            }


            return LeanTween.rotateY(ObjectToAnimate.gameObject, to.y, Settings.Duration);
        }

        private LTDescr RotZ()
        {
            Vector3 to = Settings.RelativeTo(ObjectToAnimate);


            if (ObjectToAnimate.TryGetComponent(out RectTransform rt))
            {
                if (Settings.UseFromAsStart)
                {
                    rt.eulerAngles = Settings.RelativeFrom(rt);
                }
            }
            else
            {
                if (Settings.UseFromAsStart)
                {
                    ObjectToAnimate.eulerAngles = Settings.RelativeFrom(ObjectToAnimate);
                }
            }


            return LeanTween.rotateZ(ObjectToAnimate.gameObject, to.z, Settings.Duration);
        }

        private LTDescr Scale()
        {
            Vector3 to = Settings.RelativeTo(ObjectToAnimate);


            if (ObjectToAnimate.TryGetComponent(out RectTransform rt))
            {
                if (Settings.UseFromAsStart)
                {
                    rt.localScale = Settings.RelativeFrom(rt);
                }
            }
            else
            {
                ObjectToAnimate.localScale = Settings.RelativeFrom(ObjectToAnimate);
            }


            return LeanTween.scale(ObjectToAnimate.gameObject, to, Settings.Duration);
        }

        private LTDescr ScaleX()
        {
            Vector3 to = Settings.RelativeTo(ObjectToAnimate);


            if (ObjectToAnimate.TryGetComponent(out RectTransform rt))
            {
                if (Settings.UseFromAsStart)
                {
                    rt.localScale = Settings.RelativeFrom(rt);
                }
            }
            else
            {
                ObjectToAnimate.localScale = Settings.RelativeFrom(ObjectToAnimate);
            }


            return LeanTween.scaleX(ObjectToAnimate.gameObject, to.x, Settings.Duration);
        }

        private LTDescr ScaleY()
        {
            Vector3 to = Settings.RelativeTo(ObjectToAnimate);


            if (ObjectToAnimate.TryGetComponent(out RectTransform rt))
            {
                if (Settings.UseFromAsStart)
                {
                    rt.localScale = Settings.RelativeFrom(rt);
                }
            }
            else
            {
                ObjectToAnimate.localScale = Settings.RelativeFrom(ObjectToAnimate);
            }


            return LeanTween.scaleY(ObjectToAnimate.gameObject, to.y, Settings.Duration);
        }

        private LTDescr ScaleZ()
        {
            Vector3 to = Settings.RelativeTo(ObjectToAnimate);


            if (ObjectToAnimate.TryGetComponent(out RectTransform rt))
            {
                if (Settings.UseFromAsStart)
                {
                    rt.localScale = Settings.RelativeFrom(rt);
                }
            }
            else
            {
                ObjectToAnimate.localScale = Settings.RelativeFrom(ObjectToAnimate);
            }


            return LeanTween.scaleZ(ObjectToAnimate.gameObject, to.z, Settings.Duration);
        }

        private LTDescr Fade()
        {
            if (ObjectToAnimate.TryGetComponent(out RectTransform rt))
            {
                if (Settings.UseFromAsStart)
                {
                    Cg.alpha = Settings.From.x;
                }
            }
            return LeanTween.alphaCanvas(Cg, Settings.To.x, Settings.Duration);
        }

        private LTDescr Color()
        {
            //Graphic = Image, TextMeshPro, càd les Components qui portent une couleur
            if (ObjectToAnimate.TryGetComponent(out Graphic ui))
            {
                if (Settings.UseFromAsStart)
                {
                    ui.color = Settings.FromColor;
                }


                return LeanTween.colorText(ui.GetComponent<RectTransform>(), Settings.ToColor, Settings.Duration);
            }
            return null;
        }

        private LTDescr Value()
        {
            return LeanTween.value(ObjectToAnimate.gameObject, Settings.From.x, Settings.To.x, Settings.Duration);
        }



        private LTDescr Width()
        {
            Vector3 to = Settings.RelativeTo(ObjectToAnimate);


            if (ObjectToAnimate.TryGetComponent(out RectTransform rt))
            {
                if (Settings.UseFromAsStart)
                {
                    rt.localScale = Settings.RelativeFrom(rt);
                }

                return Value().setOnUpdate(ChangeWidthOnValueUpdate).setOnUpdateParam(rt);
            }

            return new LTDescr();
        }

        private void ChangeWidthOnValueUpdate(float value, object obj)
        {
            RectTransform rt = obj as RectTransform;
            rt.sizeDelta = new Vector2(value, rt.sizeDelta.y);
        }

        private LTDescr Height()
        {
            Vector3 to = Settings.RelativeTo(ObjectToAnimate);


            if (ObjectToAnimate.TryGetComponent(out RectTransform rt))
            {
                if (Settings.UseFromAsStart)
                {
                    rt.localScale = Settings.RelativeFrom(rt);
                }

                return Value().setOnUpdate(ChangeHeightOnValueUpdate).setOnUpdateParam(rt);
            }

            return new LTDescr();
        }


        private void ChangeHeightOnValueUpdate(float value, object obj)
        {
            RectTransform rt = obj as RectTransform;
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, value);
        }




        #endregion


    }
}