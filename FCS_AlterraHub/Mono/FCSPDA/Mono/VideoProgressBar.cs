using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;

namespace FCS_AlterraHub.Mono.FCSPDA.Mono
{
    public class VideoProgressBar : MonoBehaviour, IDragHandler, IPointerDownHandler
    {
        internal VideoPlayer VideoPlayer { get; set; }
        [SerializeField]
        private Camera camera;

        private Image progress;
        private void Awake()
        {
            camera = Player.main.viewModelCamera;
            progress = GetComponent<Image>();
        }

        void Update()
        {
            if(VideoPlayer == null) return;
            
            VideoPlayer.playbackSpeed = Mathf.Approximately(DayNightCycle.main.deltaTime, 0f) ? 0 : 1;
            
            if (VideoPlayer.frameCount > 0)
                progress.fillAmount = (float)VideoPlayer.frame / (float)VideoPlayer.frameCount;
        }

        public void OnDrag(PointerEventData eventData)
        {
            TrySkip(eventData);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            TrySkip(eventData);
        }

        private void TrySkip(PointerEventData eventData)
        {
            if(progress == null) return;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(progress.rectTransform, eventData.position, camera,
                out var localPoint))
            {
                float pct = Mathf.InverseLerp(progress.rectTransform.rect.xMin, progress.rectTransform.rect.xMax,localPoint.x);
                SkipToPercent(pct);
            }
        }

        private void SkipToPercent(float pct)
        {
            if (VideoPlayer == null) return;
            var frame = VideoPlayer.frameCount * pct;
            VideoPlayer.frame = (long)frame;
        }

        public void Stop()
        {
            SkipToPercent(0);
            progress.fillAmount = 0;
        }
    }
}