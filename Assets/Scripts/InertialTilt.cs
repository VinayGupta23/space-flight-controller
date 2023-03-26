using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InertialTilt : MonoBehaviour, IShipStateListener
{
    // Note: Changing these at runtime is not supported
    public GameObject targetObject;
    public ShipStateRelay shipStateSource;

    public float tiltMaxSpeed = float.PositiveInfinity;
    public float tiltResponseTime = 0.1f;
    public float velocityWeight = 3f;
    public float angularWeight = 0.3f;

    float _curTilt = 0f;
    float _tiltVelocity = 0f;
    Vector3 _targetLastPos = Vector3.zero;
    Vector3 _targetRight = Vector3.right;

    // Start is called before the first frame update
    void Start()
    {
        if (targetObject != null)
        {
            RecordTargetTransform();
        }
        else
        {
            Debug.LogError("No target object to track!");
        }

        if (shipStateSource != null)
        {
            shipStateSource.OnShipStateChanged += ShipStateChangedHandler;
        }
        else
        {
            Debug.LogWarning("No ship state notifier found, tilt settings may be incorrect.");
        }
    }

    void RecordTargetTransform()
    {
        _targetLastPos = targetObject.transform.position;
        _targetRight = targetObject.transform.right;
    }

    public void ShipStateChangedHandler(ShipState targetState)
    {
        enabled = !ShipState.EvadeHorizontal.HasFlag(targetState);
        if (enabled)
        {
            // Re-center to target to compute proper tilt next frame
            RecordTargetTransform();
        }
    }

    void FixedUpdate()
    {
        if (targetObject == null)
        {
            return;
        }

        // Note: This will not work for very sudden turns with angle exceeding 
        // 180 degrees between 2 frames. This shouldn't happen anyway!

        float velocityRight = Vector3.Dot((targetObject.transform.position - _targetLastPos) / Time.fixedDeltaTime, _targetRight);
        float angularRight = Vector3.SignedAngle(_targetRight, targetObject.transform.right, Vector3.up);
        angularRight /= Time.fixedDeltaTime;
        
        float targetTilt = -1 * (velocityWeight * velocityRight + angularWeight * angularRight);
        _curTilt = Mathf.SmoothDamp(_curTilt, targetTilt, ref _tiltVelocity, tiltResponseTime, tiltMaxSpeed);
        transform.localRotation = Quaternion.Euler(0, 0, _curTilt);

        // Update target's last transform, for next frame
        RecordTargetTransform();
    }

    void OnDestroy()
    {
        if (shipStateSource != null)
        {
            shipStateSource.OnShipStateChanged -= ShipStateChangedHandler;
        }
    }
}
