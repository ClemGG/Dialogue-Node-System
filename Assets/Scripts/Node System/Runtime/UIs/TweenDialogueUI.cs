using UnityEngine;

namespace Project.NodeSystem
{
    public class TweenDialogueUI : DialogueUI
    {

        #region Fields


        #region Dialogue


        [Space(10)]
        [Header("Tween Settings :")]
        [Space(10)]



        [SerializeField, Tooltip("Les Tweens à jouer sur les persos quand le dialogue est terminé.")]
        TweenSettings[] TS_CharOnDialogueEnded;

        [SerializeField, Tooltip("Les Tweens à jouer sur le container quand le dialogue est terminé.")]
        TweenSettings[] TS_ContainerOnDialogueEnded;

        [SerializeField, Tooltip("Le Tween à jouer quand un perso doit se retourner.")]
        TweenSettings[] TS_CharOnRot;


        [SerializeField, Tooltip("Le Tween à jouer quand un perso commence à parle.")]
        TweenSettings[] TS_CharOnStartTalk;
        [SerializeField, Tooltip("Le Tween à jouer quand un perso finit de parler.")]
        TweenSettings[] TS_CharOnEndTalk;

        #endregion


        #region UIs


        #region Dialogue Characters

        [Space(10)]
        [Header("Dialogue Characters :")]
        [Space(10)]

        [SerializeField] ObjectTweener charLeftGoTweener;
        [SerializeField] ObjectTweener charLeftSpriteTweener;
        [SerializeField] ObjectTweener charRightGoTweener;
        [SerializeField] ObjectTweener charRightSpriteTweener;
        [SerializeField] ObjectTweener containerTweener;

        DialogueSide whoIsTalking;  //On grise le perso qui n'est pas en train de parler

        #endregion


        #endregion



        #endregion






        #region Dialogue

        public override void StartDialogue()
        {
            base.StartDialogue();

            charLeftGoTweener.gameObject.SetActive(true);
            charRightGoTweener.gameObject.SetActive(true);
            containerTweener.gameObject.SetActive(true);

            characterLeftImg.transform.rotation = characterRightImg.transform.rotation = Quaternion.identity;

        }

        public override void EndDialogue()
        {

            for (int i = 0; i < TS_CharOnDialogueEnded.Length; i++)
            {
                charLeftGoTweener.BeginTweens(TS_CharOnDialogueEnded);
                charRightGoTweener.BeginTweens(TS_CharOnDialogueEnded);
            }

            for (int i = 0; i < TS_ContainerOnDialogueEnded.Length; i++)
            {
                containerTweener.BeginTweenByCode(TS_ContainerOnDialogueEnded[i]).setOnComplete(() => 
                {
                    base.EndDialogue();
                    characterLeftImg.transform.rotation = characterRightImg.transform.rotation = Quaternion.identity;
                });
            }
        }



        public override void SetText(string replique)
        {
            base.SetText(replique);

            if (whoIsTalking == DialogueSide.Left)
            {
                charLeftSpriteTweener.BeginTweens(TS_CharOnStartTalk);
                charRightSpriteTweener.BeginTweens(TS_CharOnEndTalk);
            }
            else
            {
                charRightSpriteTweener.BeginTweens(TS_CharOnStartTalk);
                charLeftSpriteTweener.BeginTweens(TS_CharOnEndTalk);
            }
        }


        protected override void OnCharacterSet(Sprite characterSprite, DialogueSide faceDir, DialogueSide sidePlacement)
        {
            whoIsTalking = sidePlacement;   //On garde en mémoire qui est en train de parler pour griser le bon perso

            if (sidePlacement == DialogueSide.Left)
            {
                characterLeftImg.sprite = characterSprite;

                //Tourner le perso
                float rotY = characterLeftImg.transform.eulerAngles.y;
                bool rotYIsZero = Mathf.Approximately(rotY, 0f);
                if (faceDir == DialogueSide.Left ^ !rotYIsZero)
                {
                    charLeftSpriteTweener.BeginTweens(TS_CharOnRot);

                }

            }
            else
            {

                characterRightImg.sprite = characterSprite;

                //Tourner le perso
                float rotY = characterRightImg.transform.eulerAngles.y;
                bool rotYIsZero = Mathf.Approximately(rotY, 0f);
                if (faceDir == DialogueSide.Right ^ !rotYIsZero)
                {
                    charRightSpriteTweener.BeginTweens(TS_CharOnRot);
                }

            }
        }


        #endregion
    }
}