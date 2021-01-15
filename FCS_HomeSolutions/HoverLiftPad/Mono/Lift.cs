using FCS_HomeSolutions.HoverLiftPad.Enums;
using UnityEngine;

public class Lift : MonoBehaviour
{

    private Vector3 _pos;

    public bool _isLiftMoving;
    public bool goToFloor;
    public int floor;
    private SpeedModes _currentSpeed = SpeedModes.Min;
    private Transform _myTransform;
    private Vector3 _basePos;

    void Start()
    {
        _myTransform = transform;
        _basePos = transform.position;
    }
    void Update()
    {
        if (goToFloor)
        {
            GoToFloor();
        }
    }
    void OnTriggerStay(Collider coll)
    {
        if (coll.gameObject.name.Equals("Player"))
        {
            Debug.Log("Player stay");
            //Player.parent = this.transform;
        }
    }
    void OnTriggerExit(Collider coll)
    {
        if (coll.gameObject.name.Equals("Player"))
        {
            Debug.Log("Player exit");
            // Player.parent = null;
        }
    }

    void MoveLift(bool isGoingUp)
    {
        Debug.Log(" movelift");
        if (!isGoingUp)
        {
            if (_myTransform.position == _basePos) return;
            _myTransform.Translate(Vector3.down * (int)_currentSpeed * Time.deltaTime);
            _isLiftMoving = true;
        }
        if (isGoingUp)
        {
            _myTransform.Translate(Vector3.up * (int)_currentSpeed * Time.deltaTime);
            _isLiftMoving = true;
        }
    }

    private void GoToFloor()
    {
        _myTransform.position = Vector3.MoveTowards(_myTransform.position, _pos, (int)_currentSpeed * DayNightCycle.main.deltaTime);
        if (_myTransform.position == _pos)
        {
            goToFloor = false;
        }
    }

    public void LiftUp()
    {
        if (goToFloor) return;
        _isLiftMoving = true;
        MoveLift(true);
    }

    public void LiftDown()
    {
        if (goToFloor) return;
        _isLiftMoving = true;
        MoveLift(false);
    }

    public void SetIsMoving(bool value)
    {
        _isLiftMoving = false;
    }

    internal void GoToFloor(Vector3 pos)
    {
        _pos = pos;
        goToFloor = true;
    }

    internal void ChangeSpeed(SpeedModes speedMode)
    {
        _currentSpeed = speedMode;
    }

    public void Stop()
    {
        _isLiftMoving = false;
        goToFloor = false;
    }

    public void GoToPosition(Vector3 pos)
    {
        _pos = pos;
        goToFloor = true;
    }
}