using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPosition : MonoBehaviour
{
    [SerializeField]
    private Transform robotFocalPoint;
    public Transform RobotFocalPoint { get { return robotFocalPoint; } }

    [SerializeField]
    private Vector3 defaultPosition;
    public Vector3 DefaultPosition { get { return defaultPosition; } }

    [SerializeField]
    private float minFlySpeed;
    public float MinFlySpeed { get { return minFlySpeed; } }

    private CameraState currentState;

    void Awake()
    {
        currentState = new CameraFollowRobotState(this);
    }

    void Start()
    {
        currentState.Start();
    }

    void Update()
    {
        if (currentState != currentState.GetNextState())
        {
            currentState.End();
            currentState = currentState.GetNextState();
            currentState.Start();
        }
        currentState.Update();
    }
}

public abstract class CameraState
{
    protected CameraPosition cameraPosition;

    public CameraState(CameraPosition cameraPosition)
    {
        this.cameraPosition = cameraPosition;
    }

    public virtual void Start()
    {
        Debug.Log("Starting " + this.GetType().ToString());
    }

    public virtual void Update()
    {
        Debug.Log("Updating " + this.GetType().ToString());
    }

    public virtual void End()
    {
        Debug.Log("Ending " + this.GetType().ToString());
    }

    public abstract CameraState GetNextState();
}

public class CameraFollowRobotState : CameraState
{
    private CameraState nextState;

    public CameraFollowRobotState(CameraPosition cameraPosition) : base(cameraPosition)
    {
        nextState = this;
    }

    public override void Start()
    {
        cameraPosition.transform.position = cameraPosition.RobotFocalPoint.position + cameraPosition.DefaultPosition;
        cameraPosition.transform.LookAt(cameraPosition.RobotFocalPoint);
    }

    public override void Update()
    {
        float distanceToTarget = Vector3.Distance(cameraPosition.transform.position, cameraPosition.RobotFocalPoint.position + cameraPosition.DefaultPosition);
        if (distanceToTarget <= cameraPosition.MinFlySpeed * Time.deltaTime)
            cameraPosition.transform.position = cameraPosition.RobotFocalPoint.position + cameraPosition.DefaultPosition;
        else
            cameraPosition.transform.position = Vector3.MoveTowards(cameraPosition.transform.position, cameraPosition.RobotFocalPoint.position + cameraPosition.DefaultPosition, distanceToTarget * Time.deltaTime * cameraPosition.MinFlySpeed);
    }

    public override void End() {}

    public override CameraState GetNextState()
    {
        return nextState;
    }
}

