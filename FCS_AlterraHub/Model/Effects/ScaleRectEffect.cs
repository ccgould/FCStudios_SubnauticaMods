using System;
using System.Collections;
using FCS_AlterraHub.Enumerators;
using UnityEngine;

namespace FCS_AlterraHub.Model.Effects
{
    public class ScaleRectEffect : IUiEffect
    {
        private RectTransform _rectTransform;
        private Vector3 MaxSize { get; }
        private float ScaleSpeed { get;}
        private YieldInstruction Wait { get; }
        public ScaleRectEffect(RectTransform rectTransform, Vector3 maxSize, float scaleSpeed, YieldInstruction wait, Action<IUiEffect> onComplete = null)
        {
            _rectTransform = rectTransform;
            MaxSize = maxSize;
            ScaleSpeed = scaleSpeed;
            Wait = wait;
            OnComplete += onComplete;
        }

        public IEnumerator Execute()
        {
           //Scale the rect using the rect transform passed in the constructor
           var time = 0f;
           var currentScale = _rectTransform.localScale;

           while (_rectTransform.localScale != MaxSize)
           {
               time += Time.deltaTime * ScaleSpeed;
               var scale = Vector3.Lerp(currentScale, MaxSize, time);
               _rectTransform.localScale = scale;
               yield return null;
           }
           yield return Wait;

           time = 0f;
           currentScale = _rectTransform.localScale;

           while (_rectTransform.localScale != Vector3.zero)
           {
               time += Time.deltaTime * ScaleSpeed;
               var scale = Vector3.Lerp(currentScale, Vector3.zero, time);
               _rectTransform.localScale = scale;
               yield return null;
           }

           OnComplete?.Invoke(this);
        }

        public event Action<IUiEffect> OnComplete;
    }
}
