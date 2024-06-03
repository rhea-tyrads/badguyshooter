using System.Collections.Generic;

namespace Watermelon
{
    public class TweenCaseCollection
    {
        List<TweenCase> tweenCases = new();
        public List<TweenCase> TweenCases => tweenCases;

        SimpleCallback tweensCompleted;

        int completedTweensCount = 0;
        int tweensCount = 0;

        public void AddTween(TweenCase tweenCase)
        {
            tweenCase.OnComplete(OnTweenCaseComplete);

            tweenCases.Add(tweenCase);
            tweensCount++;
        }

        public bool IsComplete()
        {
            for(var i = 0; i < tweensCount; i++)
            {
                if (!tweenCases[i].IsCompleted)
                    return false;
            }

            return true;
        }

        public void Complete()
        {
            for (var i = 0; i < tweensCount; i++)
            {
                tweenCases[i].Complete();
            }
        }

        public void Kill()
        {
            for (var i = 0; i < tweensCount; i++)
            {
                tweenCases[i].Kill();
            }
        }

        public void OnComplete(SimpleCallback callback)
        {
            tweensCompleted += callback;
        }

        void OnTweenCaseComplete()
        {
            completedTweensCount++;

            if (completedTweensCount == tweensCount)
            {
                if (tweensCompleted != null)
                    tweensCompleted.Invoke();
            }
        }

        public static TweenCaseCollection operator +(TweenCaseCollection caseCollection, TweenCase tweenCase)
        {
            caseCollection.AddTween(tweenCase);

            return caseCollection;
        }
    }
}
