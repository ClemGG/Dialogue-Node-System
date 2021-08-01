using System;
using System.Collections.Generic;

namespace Project.Utilities.Tween
{
    public static class TweenUtilities
    {

        public static void SetOnTweensComplete(this LTDescr[] tweensList, Action onComplete)
        {
            if(tweensList.Length > 0)
            {
                tweensList[tweensList.Length - 1].setOnComplete(onComplete);
            }
        }

        public static void SetOnTweensComplete(this List<LTDescr> tweensList, Action onComplete)
        {
            if (tweensList.Count > 0)
            {
                tweensList[tweensList.Count - 1].setOnComplete(onComplete);
            }
        }

    }
}