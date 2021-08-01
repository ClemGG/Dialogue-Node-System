using UnityEngine;
using Project.Utilities.Tween;
using static Project.Utilities.Tween.TweenUtilities;

namespace Project.NodeSystem
{
    public class TweenDialogueUI : DialogueUI
    {

        #region Fields


        #region Tween Settings


        [Space(10)]
        [Header("Tween Settings :")]
        [Space(10)]



        [SerializeField, Tooltip("Les Tweens � jouer quand un perso appara�t � l'�cran (et que l'emplacement �tait libre).")]
        TweenSettings[] TS_CharAppears;

        [SerializeField, Tooltip("Les Tweens � jouer quand un perso se retire de la conversation.")]
        TweenSettings[] TS_CharDisappears;




        [SerializeField, Tooltip("Le Tween � jouer quand un perso doit se retourner.")]
        TweenSettings[] TS_CharRotates;

        [SerializeField, Tooltip("Le Tween � jouer quand un perso doit se retourner.")]
        TweenSettings[] TS_CharRotatesNegative;

        [SerializeField, Tooltip("Le Tween � jouer quand un perso d�j� pr�sent commence � parler.")]
        TweenSettings[] TS_CharTalks;

        [SerializeField, Tooltip("Le Tween � jouer quand ce n'est plus au tour du perso de parler.")]
        TweenSettings[] TS_CharMuted;




        [SerializeField, Tooltip("Les Tweens � jouer sur les persos quand le dialogue est termin�.")]
        TweenSettings[] TS_CharOnDialogueEnded;

        [SerializeField, Tooltip("Les Tweens � jouer sur le container quand le dialogue est termin�.")]
        TweenSettings[] TS_ContainerOnDialogueEnded;


        #endregion


        #region Tweeners

        [Space(10)]
        [Header("Tweeners :")]
        [Space(10)]

        [SerializeField] ObjectTweener m_leftCharGoTweener;
        [SerializeField] ObjectTweener m_leftCharSpriteTweener;
        [SerializeField] ObjectTweener m_rightCharGoTweener;
        [SerializeField] ObjectTweener m_rightCharSpriteTweener;
        [SerializeField] ObjectTweener m_containerTweener;

        DialogueSide? _whoIsTalking;  //On grise le perso qui n'est pas en train de parler
        Vector2 _leftStartPos, _rightStartPos;

        #endregion



        #endregion




        #region Mono

        protected override void Awake()
        {
            base.Awake();

            //On garde les positions des sprites en m�moire pour les r�initialiser au prochain dialogue
            _leftStartPos = m_leftCharGo.transform.position;
            _rightStartPos = m_rightCharGo.transform.position;

            //Cache les persos pour les v�rifs auto du script
            m_leftCharGo.SetActive(false);
            m_rightCharGo.SetActive(false);

            //On emp�che la remise � z�ro auto des tweeners pour replacer les sprites nous-m�mes quand ils dispara�ssent
            m_leftCharSpriteTweener.resetTransformOnDisable = m_rightCharSpriteTweener.resetTransformOnDisable = false;
        }

        #endregion




        #region Init

        //Ram�ne les valeurs internes � leur d�faut pour la prochaine lecture d'un dialogue
        protected override void ResetUI()
        {
            _whoIsTalking = null;
            m_leftCharSpriteTweener.Cg.alpha = m_leftCharSpriteTweener.Cg.alpha = 1f;
            m_leftCharGo.transform.position = _leftStartPos;
            m_rightCharGo.transform.position = _rightStartPos;
            m_leftCharImg.transform.rotation = m_rightCharImg.transform.rotation = Quaternion.identity;
        }

        protected override void ShowUI()
        {
            base.ShowUI();


            //On active le tweener du container pour le faire appara�tre
            m_containerTweener.gameObject.SetActive(true);

            //On ram�ne les positions et rotations des persos � z�ro                    
            m_leftCharImg.transform.rotation = m_rightCharImg.transform.rotation = Quaternion.identity;

        }


        protected override void HideUI(bool endDialogue = false)
        {
            //Pour chaque perso, on joue le tween de fin de dialogue pour les faire dispara�tre
            //(S'il le perso n'est pas visible alors il est d�sactiv�, donc �a craint rien)
            for (int i = 0; i < TS_CharOnDialogueEnded.Length; i++)
            {
                m_leftCharGoTweener.BeginTweens(TS_CharOnDialogueEnded);
                m_rightCharGoTweener.BeginTweens(TS_CharOnDialogueEnded);
            }

            //Pour le container, on ferme la fen�tre de dialogue lorsque le tween de fin du container a termin�
            for (int i = 0; i < TS_ContainerOnDialogueEnded.Length; i++)
            {
                m_containerTweener.BeginTweenByCode(TS_ContainerOnDialogueEnded[i]).setOnComplete(() =>
                {
                    m_leftCharImg.transform.rotation = m_rightCharImg.transform.rotation = Quaternion.identity;
                    base.HideUI(endDialogue);
                });
            }
        }

        #endregion





        #region Characters


        protected override void DisplayCharacterAndName(string characterName, Color characterNameColor, Sprite characterSprite, DialogueSide faceDir, DialogueSide sidePlacement)
        {

            if (sidePlacement == DialogueSide.Left)
            {
                //S'il n'y a pas de sprite en param�tre, on veut cacher le personnage de ce c�t�
                if (!characterSprite && m_leftCharGo.activeSelf)
                {
                    m_leftCharGoTweener.BeginTweens(TS_CharDisappears).SetOnTweensComplete(() => m_leftCharGo.gameObject.SetActive(false));
                    m_leftNameText.transform.parent.gameObject.SetActive(false);
                    return;
                }
                //Sinon, on l'active
                else if (characterSprite && !m_leftCharGo.activeSelf)
                {
                    m_leftCharGo.gameObject.SetActive(true);
                    m_leftCharGoTweener.BeginTweens(TS_CharAppears);

                    //On change le nom du perso en question
                    m_leftNameText.transform.parent.gameObject.SetActive(true);
                    m_rightNameText.transform.parent.gameObject.SetActive(false);


                }


            }
            else
            {
                //S'il n'y a pas de sprite en param�tre, on veut cacher le personnage de ce c�t�
                if (!characterSprite && m_rightCharGo.activeSelf)
                {
                    m_rightCharGoTweener.BeginTweens(TS_CharDisappears).SetOnTweensComplete(() => m_rightCharGo.gameObject.SetActive(false));
                    m_rightNameText.transform.parent.gameObject.SetActive(false);
                    return;
                }
                //Sinon, on l'active
                else if (characterSprite && !m_rightCharGo.activeSelf)
                {
                    m_rightCharGo.gameObject.SetActive(true);
                    m_rightCharGoTweener.BeginTweens(TS_CharAppears);

                    //On change le nom du perso en question
                    m_leftNameText.transform.parent.gameObject.SetActive(false);
                    m_rightNameText.transform.parent.gameObject.SetActive(true);
                }


            }


            m_leftNameText.text = m_rightNameText.text = characterName;
            m_leftNameText.color = m_rightNameText.color = characterNameColor;
        }

        protected override void OnCharacterSet(string characterName, Sprite characterSprite, DialogueSide faceDir, DialogueSide sidePlacement)
        {
            _whoIsTalking = sidePlacement;
            Sprite lastSprite;
            bool shouldRotate;

            if (sidePlacement == DialogueSide.Left)
            {
                lastSprite = m_leftCharImg.sprite;
                m_leftCharImg.sprite = characterSprite;

                //Tourner le perso
                float rotY = m_leftCharImg.transform.eulerAngles.y;
                bool rotYIsZero = Mathf.Approximately(rotY, 0f);
                shouldRotate = faceDir == DialogueSide.Left ^ !rotYIsZero;
                if (shouldRotate)
                {
                    //Si on doit changer de direction et que le perso est visible, on peut l'animer
                    if (m_leftCharGo.activeSelf)
                    {
                        m_leftCharSpriteTweener.BeginTweens(rotYIsZero ? TS_CharRotates : TS_CharRotatesNegative);
                    }
                    //Sinon, �a veut dire que notre perso a �t� d�sactiv� ; on doit le tourner dans la bonne direction avant de l'afficher 
                    else
                    {
                        Vector3 euler = m_leftCharImg.transform.eulerAngles;
                        euler.y = faceDir == DialogueSide.Left ? 180f : 0f;
                        m_leftCharImg.transform.eulerAngles = euler;
                    }
                }


            }
            else
            {
                lastSprite = m_rightCharImg.sprite;
                m_rightCharImg.sprite = characterSprite;



                //Tourner le perso
                float rotY = m_rightCharImg.transform.eulerAngles.y;
                bool rotYIsZero = Mathf.Approximately(rotY, 0f);
                shouldRotate = faceDir == DialogueSide.Right ^ !rotYIsZero;
                if (shouldRotate)
                {
                    //Si on doit changer de direction et que le perso est visible, on peut l'animer
                    if (m_rightCharGo.activeSelf)
                    {
                        m_rightCharSpriteTweener.BeginTweens(rotYIsZero ? TS_CharRotates : TS_CharRotatesNegative);
                    }
                    //Sinon, �a veut dire que notre perso a �t� d�sactiv� ; on doit le tourner dans la bonne direction avant de l'afficher 
                    else
                    {
                        Vector3 euler = m_rightCharImg.transform.eulerAngles;
                        euler.y = faceDir == DialogueSide.Right ? 180f : 0f;
                        m_rightCharImg.transform.eulerAngles = euler;
                    }
                }


            }






            if (_whoIsTalking == DialogueSide.Left)
            {
                //Animer le perso s'il affiche un sprite diff�rent du pr�c�dent
                if (lastSprite != m_leftCharImg.sprite && !shouldRotate)
                    m_leftCharSpriteTweener.BeginTweens(TS_CharTalks);

                m_rightCharSpriteTweener.BeginTweens(TS_CharMuted);
            }
            else
            {
                //Animer le perso s'il affiche un sprite diff�rent du pr�c�dent
                if (lastSprite != m_rightCharImg.sprite && !shouldRotate)
                    m_rightCharSpriteTweener.BeginTweens(TS_CharTalks);

                m_leftCharSpriteTweener.BeginTweens(TS_CharMuted);
            }
        }


        #endregion

    }
}