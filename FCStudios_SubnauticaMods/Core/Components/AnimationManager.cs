using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Core.Components;
public class AnimationManager : MonoBehaviour
{
    [SerializeField] private Animator animator;
    #region Unity Methods   

    private void Start()
    {
        if (animator == null || animator.enabled) return;
        QuickLogger.Debug("animator was disabled and now has been enabled");
        animator.enabled = true;
    }

    #endregion

    #region public Methods

    /// <summary>
    /// Sets the an animator float to a certain value (For use with setting the page on the screen)
    /// </summary>
    /// <param name="stateHash">The hash of the parameter</param>
    /// <param name="value">Float to set</param>
    public void SetFloatHash(int stateHash, float value)
    {
        animator.SetFloat(stateHash, value);
    }

    /// <summary>
    /// Sets the an animator boolean to a certain value
    /// </summary>
    /// <param name="stateHash">The hash of the parameter</param>
    /// <param name="value">Float to set</param>
    public void SetBoolHash(int stateHash, bool value)
    {
        animator.SetBool(stateHash, value);
    }

    /// <summary>
    /// Sets the an animator integer to a certain value
    /// </summary>
    /// <param name="stateHash">The hash of the parameter</param>
    /// <param name="value">Float to set</param>
    public void SetIntHash(int stateHash, int value)
    {
        animator.SetInteger(stateHash, value);
    }

    public int GetIntHash(int hash)
    {
        if (animator != null)
        {
            return animator.GetInteger(hash);
        }
        
       return 0;
    }

    public bool GetBoolHash(int hash)
    {
        if (animator != null) return animator.GetBool(hash);

        return animator.GetBool(hash);
    }

    public float AnimationLength(string animationName, int fps = 30)
    {
        if (animator == null)
        {
             return 0f;
        }

        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        {
            if (clip.name == animationName)
            {
                return clip.length / fps;
            }
        }

        return 0f;

        #endregion
    }
}
