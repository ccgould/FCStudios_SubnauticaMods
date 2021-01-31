using FCS_AlterraHub.Model;
using FCS_AlterraHub.Model.Effects;
using UnityEngine;

namespace FCS_ProductionSolutions.MatterAnalyzer.Mono
{
    internal class MessagePop : MonoBehaviour
    {
        private EffectBuilder _scaleEffect;

        internal void Initialize()
        {
            _scaleEffect = new EffectBuilder(this);
            _scaleEffect.AddEffect(new ScaleRectEffect(GetComponent<RectTransform>(), new Vector3(0.9530678f, 0.9530678f, 0.9530678f), 1f, new WaitForSeconds(5)));
        }

        internal void Show()
        {
            _scaleEffect.ExecuteEffects();
        }
    }
}