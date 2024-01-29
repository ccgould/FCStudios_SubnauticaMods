using FCS_AlterraHub.Core.Helpers;
using System.Collections;
using UnityEngine;

namespace FCS_AlterraHub.Core.Components;
public class SlerpHelper : MonoBehaviour
{
    [SerializeField] private AnimationCurve openingCurve;
    [SerializeField] private AnimationCurve closingingCurve;
    [SerializeField] private float playerDistance = 2f;
    [SerializeField] private Constructable constructable;
    [SerializeField] private FMOD_CustomEmitter openSound;
    [SerializeField] private FMOD_CustomEmitter closeSound;

    [SerializeField] private Vector3 openRotEuler;
    [SerializeField] private Vector3 closedRotEuler;

    private Quaternion openRot;
    private Quaternion closedRot;

    private float _animationTimePosition;
    private bool _isOpen;
    private bool _notTransitioning;


    private void Awake()
    {
        openRot = Quaternion.Euler(openRotEuler);
        closedRot = Quaternion.Euler(closedRotEuler);
    }


    private void Update()
    {
        if (!constructable.constructed) return;

        if (WorldHelpers.CheckIfInRange(gameObject, Player.main.gameObject, playerDistance) && !_isOpen)
        {
            if (!_notTransitioning)
            {
                StartCoroutine(Transition(true, openRot,openingCurve));
                openSound.Play();
            }
        }
        
        if(!WorldHelpers.CheckIfInRange(gameObject, Player.main.gameObject, playerDistance) && _isOpen)
        {
            if (!_notTransitioning)
            {
                StartCoroutine(Transition(false, closedRot, closingingCurve));
                closeSound.Play();
            }
        }
    }

    private IEnumerator Transition(bool v, Quaternion pos, AnimationCurve curve)
    {
        _notTransitioning = true;

        while (transform.localEulerAngles.x != pos.eulerAngles.x)
        {
            _animationTimePosition += DayNightCycle.main.deltaTime;
            transform.localRotation = Quaternion.Slerp(transform.localRotation, pos, curve.Evaluate(_animationTimePosition));
            yield return null;
        }
        _animationTimePosition = 0;
        _isOpen = v;
        _notTransitioning = false;
        yield break;
    }
}
