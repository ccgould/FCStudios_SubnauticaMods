using System;
using System.Collections;
using FCS_AlterraHub.Enumerators;
using UnityEngine;

namespace FCS_AlterraHub.Model.Effects
{
    public class SlideRectEffect : IUiEffect
    {
        private RectTransform _rectTransform;
        private Vector3 MaxPosition { get; }
        private Vector3 MinPosition { get; }
        private float MoveSpeed { get;}
        private YieldInstruction Wait { get; }
        public SlideRectEffect(RectTransform rectTransform, Vector3 minPosition,Vector3 maxPosition, float moveSpeed, YieldInstruction wait, Action<IUiEffect> onComplete = null)
        {
            _rectTransform = rectTransform;
            MaxPosition = maxPosition;
            MoveSpeed = moveSpeed;
            MinPosition = minPosition;
            Wait = wait;
            OnComplete += onComplete;
        }

        public IEnumerator Execute()
        {
           //Scale the rect using the rect transform passed in the constructor
           var time = 0f;
           var currentPosition = _rectTransform.localPosition;

           while (_rectTransform.localPosition != MaxPosition)
           {
               time += Time.deltaTime * MoveSpeed;
               var position = Vector3.Lerp(currentPosition, MaxPosition, time);
               _rectTransform.localPosition = position;
               yield return null;
           }
           yield return Wait;

           time = 0f;
           currentPosition = _rectTransform.localPosition;

           while (_rectTransform.localPosition != MinPosition)
           {
               time += Time.deltaTime * MoveSpeed;
               var position = Vector3.Lerp(currentPosition, MinPosition, time);
               _rectTransform.localPosition = position;
               yield return null;
           }

           OnComplete?.Invoke(this);
        }

        public event Action<IUiEffect> OnComplete;
    }
}
