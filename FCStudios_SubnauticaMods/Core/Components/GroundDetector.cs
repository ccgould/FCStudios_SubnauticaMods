using System.Collections.Generic;
using UnityEngine;

namespace FCS_AlterraHub.Core.Components;
public class GroundDetector : MonoBehaviour
{
    public enum AxisToRotate { Back, Down, Forward, Left, Right, Up, Zero };
    static readonly Vector3[] vectorAxes = new Vector3[] {
        Vector3.back,
        Vector3.down,
        Vector3.forward,
        Vector3.left,
        Vector3.right,
        Vector3.up,
        Vector3.zero
    };

    [SerializeField] private AxisToRotate direction;
    [SerializeField] private float raycastLength;


    public Vector3 GetAxis(AxisToRotate axis)
    {
        return vectorAxes[(int)axis];
    }

    private void Awake()
    {
        IsGroundVisible();
    }

    private void Update()
    {
        //IsGroundVisible();
    }

    public bool IsGroundVisible()
    {
        var hit = Physics.Raycast(transform.position, transform.TransformDirection(GetAxis(direction)), raycastLength,LayerMask.GetMask("TerrainCollider"));
        Debug.DrawRay(transform.position, transform.TransformDirection(GetAxis(direction)), Color.red , raycastLength);
        
        return hit;
    }
}
