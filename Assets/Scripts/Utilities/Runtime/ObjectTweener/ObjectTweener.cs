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

        #endregion


        #region Private

        private List<LTDescr> _tweeners = new List<LTDescr>();
        private LTSeq _sequence;
        private TweenSettings _settings;
        private CanvasGroup cg;
        private Vector3 startPos, startRot, startScale;

        #endregion



        #region Accessors

        public Transform ObjectToAnimate
        {
            get { if (_objectToAnimate == null) _objectToAnimate = gameObject.transform; return _objectToAnimate; }
            set => _objectToAnimate = value;
        }
        public TweenSettings Settings { get => _settings; set => _settings = value; }
        public Action<float> OnValueUpdated { get; set; }
        public CanvasGroup Cg
        {
            get
            {
                if (!ObjectToAnimate.TryGetComponent(out cg))
                {
                    return ObjectToAnimate.gameObject.AddComponent<CanvasGroup>();
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
            Stop();

            if (resetTransformOnDisable)
            {
                Disable();
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


        #endregion



        #region Tween

        public void BeginTween(TweenSettings newSettings = null)
        {
            SetSettings(newSettings);

            LTDescr _tweener = new LTDescr();

            switch (Settings.animationType)
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


            _tweener.setDelay(Settings.delay);

            if (Settings.curveType == LeanTweenType.animationCurve)
                _tweener.setEase(Settings.curve);
            else
                _tweener.setEase(Settings.curveType);


            if (Settings.loop)
            {
                _tweener.loopCount = int.MaxValue;
                _tweener.setLoopClamp();
            }
            if (Settings.pingPong)
            {
                _tweener.loopCount = int.MaxValue;
                _tweener.setLoopPingPong();
            }

            _tweener.setOnComplete(() =>
            {

                if (Settings.loop)
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

                if (!Settings.loop && !Settings.pingPong)
                {
                    for (int i = 0; i < Settings.OnComplete.Length; i++)
                    {
                        BeginTween(Settings.OnComplete[i]);
                    }
                }
            });

            _tweeners.Add(_tweener);
        }

        //Si on veut l'appeler par code et lui ajouter des fonctionnalités supplémentaires
        public LTDescr BeginTweenByCode(TweenSettings newSettings = null)
        {
            SetSettings(newSettings);

            LTDescr _tweener = new LTDescr();

            switch (Settings.animationType)
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


            _tweener.setDelay(Settings.delay);

            if (Settings.curveType == LeanTweenType.animationCurve)
                _tweener.setEase(Settings.curve);
            else
                _tweener.setEase(Settings.curveType);


            if (Settings.loop)
            {
                _tweener.loopCount = int.MaxValue;
                _tweener.setLoopClamp();
            }
            if (Settings.pingPong)
            {
                _tweener.loopCount = int.MaxValue;
                _tweener.setLoopPingPong();
            }

            _tweener.setOnComplete(() =>
            {

                if (Settings.loop)
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


                if (!Settings.loop && !Settings.pingPong)
                {
                    for (int i = 0; i < Settings.OnComplete.Length; i++)
                    {
                        BeginTween(Settings.OnComplete[i]);
                    }
                }
            });

            _tweeners.Add(_tweener);
            return _tweener;
        }

        //Si on veut l'appeler par code et lui ajouter des fonctionnalités supplémentaires
        public LTDescr[] BeginTweens(TweenSettings[] newSettings = null)
        {
            LTDescr[] tweens = new LTDescr[newSettings.Length];

            for (int i = 0; i < tweens.Length; i++)
            {
                tweens[i] = BeginTweenByCode(newSettings[i]);
                _tweeners.Add(tweens[i]);
            }

            return tweens;
        }

        private LTDescr Move()
        {
            Vector3 to = Settings.RelativeTo(ObjectToAnimate);

            if (ObjectToAnimate.TryGetComponent(out RectTransform rt))
            {
                if (Settings.useFromAsStart)
                {
                    rt.position = Settings.RelativeFrom(rt);

                }
            }
            else
            {
                if (Settings.useFromAsStart)
                {
                    ObjectToAnimate.position = Settings.RelativeFrom(ObjectToAnimate);
                }
            }


            return LeanTween.move(ObjectToAnimate.gameObject, to, Settings.duration);

        }

        private LTDescr MoveX()
        {
            Vector3 to = Settings.RelativeTo(ObjectToAnimate);

            if (ObjectToAnimate.TryGetComponent(out RectTransform rt))
            {
                if (Settings.useFromAsStart)
                {
                    rt.position = Settings.RelativeFrom(rt);
                }
            }
            else
            {
                if (Settings.useFromAsStart)
                {
                    ObjectToAnimate.position = Settings.RelativeFrom(ObjectToAnimate);
                }
            }

            return LeanTween.moveX(ObjectToAnimate.gameObject, to.x, Settings.duration);

        }

        private LTDescr MoveY()
        {
            Vector3 to = Settings.RelativeTo(ObjectToAnimate);

            if (ObjectToAnimate.TryGetComponent(out RectTransform rt))
            {
                if (Settings.useFromAsStart)
                {
                    rt.position = Settings.RelativeFrom(rt);

                }
            }
            else
            {
                if (Settings.useFromAsStart)
                {
                    ObjectToAnimate.position = Settings.RelativeFrom(ObjectToAnimate);
                }
            }


            return LeanTween.moveY(ObjectToAnimate.gameObject, to.y, Settings.duration);

        }

        private LTDescr MoveZ()
        {
            Vector3 to = Settings.RelativeTo(ObjectToAnimate);

            if (ObjectToAnimate.TryGetComponent(out RectTransform rt))
            {
                if (Settings.useFromAsStart)
                {
                    rt.position = Settings.RelativeFrom(rt);

                }
            }
            else
            {
                if (Settings.useFromAsStart)
                {
                    ObjectToAnimate.position = Settings.RelativeFrom(ObjectToAnimate);
                }
            }


            return LeanTween.moveZ(ObjectToAnimate.gameObject, to.z, Settings.duration);

        }

        private LTDescr Rot()
        {
            Vector3 to = Settings.RelativeTo(ObjectToAnimate);


            if (ObjectToAnimate.TryGetComponent(out RectTransform rt))
            {
                if (Settings.useFromAsStart)
                {
                    rt.eulerAngles = Settings.RelativeFrom(rt);
                }
            }
            else
            {
                if (Settings.useFromAsStart)
                {
                    ObjectToAnimate.eulerAngles = Settings.RelativeFrom(ObjectToAnimate);
                }
            }


            return LeanTween.rotate(ObjectToAnimate.gameObject, to, Settings.duration);
        }

        private LTDescr RotX()
        {
            Vector3 to = Settings.RelativeTo(ObjectToAnimate);


            if (ObjectToAnimate.TryGetComponent(out RectTransform rt))
            {
                if (Settings.useFromAsStart)
                {
                    rt.eulerAngles = Settings.RelativeFrom(rt);
                }
            }
            else
            {
                if (Settings.useFromAsStart)
                {
                    ObjectToAnimate.eulerAngles = Settings.RelativeFrom(ObjectToAnimate);
                }
            }


            return LeanTween.rotateX(ObjectToAnimate.gameObject, to.x, Settings.duration);
        }

        private LTDescr RotY()
        {
            Vector3 to = Settings.RelativeTo(ObjectToAnimate);


            if (ObjectToAnimate.TryGetComponent(out RectTransform rt))
            {
                if (Settings.useFromAsStart)
                {
                    rt.eulerAngles = Settings.RelativeFrom(rt);
                }
            }
            else
            {
                if (Settings.useFromAsStart)
                {
                    ObjectToAnimate.eulerAngles = Settings.RelativeFrom(ObjectToAnimate);
                }
            }


            return LeanTween.rotateY(ObjectToAnimate.gameObject, to.y, Settings.duration);
        }

        private LTDescr RotZ()
        {
            Vector3 to = Settings.RelativeTo(ObjectToAnimate);


            if (ObjectToAnimate.TryGetComponent(out RectTransform rt))
            {
                if (Settings.useFromAsStart)
                {
                    rt.eulerAngles = Settings.RelativeFrom(rt);
                }
            }
            else
            {
                if (Settings.useFromAsStart)
                {
                    ObjectToAnimate.eulerAngles = Settings.RelativeFrom(ObjectToAnimate);
                }
            }


            return LeanTween.rotateZ(ObjectToAnimate.gameObject, to.z, Settings.duration);
        }

        private LTDescr Scale()
        {
            Vector3 to = Settings.RelativeTo(ObjectToAnimate);


            if (ObjectToAnimate.TryGetComponent(out RectTransform rt))
            {
                if (Settings.useFromAsStart)
                {
                    rt.localScale = Settings.RelativeFrom(rt);
                }
            }
            else
            {
                ObjectToAnimate.localScale = Settings.RelativeFrom(ObjectToAnimate);
            }


            return LeanTween.scale(ObjectToAnimate.gameObject, to, Settings.duration);
        }

        private LTDescr ScaleX()
        {
            Vector3 to = Settings.RelativeTo(ObjectToAnimate);


            if (ObjectToAnimate.TryGetComponent(out RectTransform rt))
            {
                if (Settings.useFromAsStart)
                {
                    rt.localScale = Settings.RelativeFrom(rt);
                }
            }
            else
            {
                ObjectToAnimate.localScale = Settings.RelativeFrom(ObjectToAnimate);
            }


            return LeanTween.scaleX(ObjectToAnimate.gameObject, to.x, Settings.duration);
        }

        private LTDescr ScaleY()
        {
            Vector3 to = Settings.RelativeTo(ObjectToAnimate);


            if (ObjectToAnimate.TryGetComponent(out RectTransform rt))
            {
                if (Settings.useFromAsStart)
                {
                    rt.localScale = Settings.RelativeFrom(rt);
                }
            }
            else
            {
                ObjectToAnimate.localScale = Settings.RelativeFrom(ObjectToAnimate);
            }


            return LeanTween.scaleY(ObjectToAnimate.gameObject, to.y, Settings.duration);
        }

        private LTDescr ScaleZ()
        {
            Vector3 to = Settings.RelativeTo(ObjectToAnimate);


            if (ObjectToAnimate.TryGetComponent(out RectTransform rt))
            {
                if (Settings.useFromAsStart)
                {
                    rt.localScale = Settings.RelativeFrom(rt);
                }
            }
            else
            {
                ObjectToAnimate.localScale = Settings.RelativeFrom(ObjectToAnimate);
            }


            return LeanTween.scaleZ(ObjectToAnimate.gameObject, to.z, Settings.duration);
        }

        private LTDescr Fade()
        {

            if (ObjectToAnimate.TryGetComponent(out RectTransform rt))
            {
                if (Settings.useFromAsStart)
                {
                    Cg.alpha = Settings.from.x;
                }
            }
            return LeanTween.alphaCanvas(Cg, Settings.to.x, Settings.duration);
        }

        private LTDescr Color()
        {
            //Graphic = Image, TextMeshPro, càd les Components qui portent une couleur
            if (ObjectToAnimate.TryGetComponent(out Graphic ui))
            {
                if (Settings.useFromAsStart)
                {
                    ui.color = Settings.fromColor;
                }


                return LeanTween.colorText(ui.GetComponent<RectTransform>(), Settings.toColor, Settings.duration);
            }
            return null;
        }

        private LTDescr Value()
        {
            return LeanTween.value(ObjectToAnimate.gameObject, Settings.from.x, Settings.to.x, Settings.duration);
        }



        private LTDescr Width()
        {
            Vector3 to = Settings.RelativeTo(ObjectToAnimate);


            if (ObjectToAnimate.TryGetComponent(out RectTransform rt))
            {
                if (Settings.useFromAsStart)
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
                if (Settings.useFromAsStart)
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



        //Lance une succession de tweens à la suite
        private void Sequence(List<LTDescr> tweensToRecord)
        {
            _sequence = LeanTween.sequence();
            for (int i = 0; i < tweensToRecord.Count; i++)
            {
                _sequence.append(tweensToRecord[i]);
            }
        }

        //Lance une succession de tweens à la suite
        private void Sequence(params LTDescr[] tweensToRecord)
        {
            _sequence = LeanTween.sequence();
            for (int i = 0; i < tweensToRecord.Length; i++)
            {
                _sequence.append(tweensToRecord[i]);
            }
        }

        //Lance une succession de tweens à la suite
        private void Sequence(params TweenSettings[] tweensToRecord)
        {
            LTDescr[] tmp = new LTDescr[tweensToRecord.Length];
            _tweeners.CopyTo(tmp);
            _tweeners.Clear();

            _sequence = LeanTween.sequence();
            for (int i = 0; i < tweensToRecord.Length; i++)
            {
                BeginTween(tweensToRecord[i]);
                _sequence.append(_tweeners[i]);
            }
            _tweeners.Clear();
            _tweeners = tmp.ToList();
        }
        //Lance une succession de tweens à la suite
        private void Sequence(Action tweensToRecord)
        {
            _sequence = LeanTween.sequence();
            _sequence.append(tweensToRecord);
        }

        private void RewindSequence()
        {
            if (_sequence != null) _sequence.reverse();
        }

        public void Stop()
        {
            LeanTween.cancel(ObjectToAnimate.gameObject);
            if (_sequence != null) LeanTween.cancel(_sequence.id);
            _tweeners.Clear();
        }


        #endregion


    }
}