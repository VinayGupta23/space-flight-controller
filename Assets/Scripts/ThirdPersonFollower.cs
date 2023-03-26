using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FollowerSettings
{
    public ShipState stateMask;
    public float moveResponseTime;
    public float moveMaxSpeed;
    public float rotateResponseTime;
    public float rotateMaxSpeed;

    public FollowerSettings(ShipState mask, float moveTime, float moveSpeed, float rotateTime, float rotateSpeed)
    {
        stateMask = mask;
        moveResponseTime = moveTime;
        moveMaxSpeed = moveSpeed;
        rotateResponseTime = rotateTime;
        rotateMaxSpeed = rotateSpeed;
    }
}

public class ThirdPersonFollower : MonoBehaviour, IShipStateListener
{
    // Note: Changing these at runtime is not supported
    public GameObject targetObject;
    public ShipStateRelay shipStateSource;

    public Vector3 cameraOffset = Vector3.zero;
    public FollowerSettings[] settings =
    {
        new FollowerSettings(ShipState.Everything, 0.1f, float.PositiveInfinity, 0.1f, float.PositiveInfinity)
    };

    int _currentIndex = 0;
    float _rotateVelocity = 0;
    Vector3 _moveVelocity = Vector3.zero;
    Vector3 _originPosition = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        if (targetObject != null)
        {
            transform.rotation = targetObject.transform.rotation;
            transform.position = targetObject.transform.TransformPoint(cameraOffset);
            _originPosition = targetObject.transform.position;
        }
        else
        {
            Debug.LogError("No target to follow!");
        }

        if (settings.Length == 0)
        {
            Debug.LogError("No follow settings provided!");
        }

        if (shipStateSource != null)
        {
            shipStateSource.OnShipStateChanged += ShipStateChangedHandler;
        }
        else
        {
            Debug.LogWarning("No ship state notifier found, follow settings may be incorrect.");
        }
    }

    public void ShipStateChangedHandler(ShipState targetState)
    {
        for (int i = 0; i < settings.Length; i++)
        {
            if (settings[i].stateMask.HasFlag(targetState))
            {
                _currentIndex = i;
                return;
            }
        }

        Debug.LogWarning("No follower settings found for ship state: " + targetState.ToString());
    }
    
    void FixedUpdate()
    {
        if (targetObject == null || settings.Length == 0)
        {
            return;
        }

        FollowerSettings curSetting = settings[_currentIndex];

        // Note: We can visualize the TPS camera as an object that rotates
        // around the ship (imagine a sphere with the ship as the center).

        // Let the ship turn causing the forward vector to rotate by some angle.
        // We need to move the camera along this very spherical path, as defined
        // by the forward vectors before and after.
        
        if (transform.rotation != targetObject.transform.rotation)
        {
            float errorAngle = Quaternion.Angle(transform.rotation, targetObject.transform.rotation);
            float currentAngle = Mathf.SmoothDamp(
                0, errorAngle, ref _rotateVelocity, 
                curSetting.rotateResponseTime, curSetting.rotateMaxSpeed);

            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetObject.transform.rotation, currentAngle);
        }

        // Perform similar smoothing on movement, ideally with faster response time.
        // This creates better "feel" as user can see the ship leaning towards the desired axis.
        //
        // Note: The smoothing is done assuming no offset. Otherwise, the offset vector
        // gets rotated causing unintended jitter.

        _originPosition = Vector3.SmoothDamp(
            _originPosition, targetObject.transform.position, ref _moveVelocity, 
            curSetting.moveResponseTime, curSetting.moveMaxSpeed);

        transform.position = _originPosition;
        transform.position = transform.TransformPoint(cameraOffset);  // Apply offset, if any
    }

    void OnDestroy()
    {
        if (shipStateSource != null)
        {
            shipStateSource.OnShipStateChanged -= ShipStateChangedHandler;
        }
    }
}
