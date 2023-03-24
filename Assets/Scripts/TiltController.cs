using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TiltController : MonoBehaviour
{
    public float tiltSpeed = 30f;

    // Use Euler angle values for rotation, to
    // support longer paths such as a 360-spin.
    float _currentAngle = 0;
    float _targetAngle = 0;

    private float currentAngle
    {
        get { return _currentAngle; }
        set
        {
            _currentAngle = value;
            transform.localRotation = Quaternion.Euler(0, 0, _currentAngle);
        }
    }

    public float targetAngle
    {
        get { return _targetAngle; }
        set
        {
            _targetAngle = value;
            // If a new rotation is requested, wrap start value to 
            // prevent error accumulation from multiple 360-spins.
            _currentAngle = _currentAngle % 360;
        }
    }

    public bool isTilting
    {
        get { return _currentAngle != _targetAngle; }
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    void FixedUpdate()
    {
        if (isTilting)
        {
            float delta = tiltSpeed * Time.fixedDeltaTime;
            currentAngle = Mathf.MoveTowards(currentAngle, targetAngle, delta);
        }
    }
}
