using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum EvadeDirection
{
    None,
    East,
    West,
    Vertical
}

public enum ShipState
{
    Normal,
    Evading
}

public class ShipController : MonoBehaviour
{
    public TiltController tiltController;

    [Header("Thrust")]
    public float thrustSpeed = 60;
    public float boostMultiplier = 2f;
    public float slowMultiplier = 0.5f;

    [Header("Strafe")]
    public float strafeSpeed = 45;     // meters/second
    public float strafeTilt = 20;      // degrees
    public float strafeTiltSpeed = 90; // degrees/second

    [Header("Turn")]
    public float pitchSensitivity = 1;
    public float yawSensitivity = 1;
    public float yawTilt = 30;
    public float yawTiltSpeed = 90;

    [Header("Evasion")]
    public float evadeStrafeSpeed = 90;
    public float evadeTiltSpeed = 420;
    public float evadePitchSpeed = 360;
    public float evadeRadius = 8;

    #region User Inputs

    float _boostInput = 0;
    float _strafeInput = 0;
    float _pitchInput = 0;
    float _yawInput = 0;
    EvadeDirection _evasionInput = EvadeDirection.None;

    public void OnBoost(InputValue value)
    {
        _boostInput = value.Get<float>();
    }

    public void OnStrafe(InputValue value)
    {
        _strafeInput = value.Get<float>();
    }

    public void OnPitchYaw(InputValue value)
    {
        Vector2 pitchYaw = value.Get<Vector2>();
        _yawInput = pitchYaw.x;
        _pitchInput = pitchYaw.y;
    }

    public void OnRollEast()
    {
        if (_shipState != ShipState.Normal)
        {
            Debug.Log("Ignoring evade input as already evading!");
            return;
        }
        _evasionInput = EvadeDirection.East;
    }

    public void OnRollWest()
    {
        if (_shipState != ShipState.Normal)
        {
            Debug.Log("Ignoring evade input as already evading!");
            return;
        }
        _evasionInput = EvadeDirection.West;
    }

    public void OnRollVertical()
    {
        if (_shipState != ShipState.Normal)
        {
            Debug.Log("Ignoring evade input as already evading!");
            return;
        }
        _evasionInput = EvadeDirection.Vertical;
    }

    #endregion

    ShipState _shipState = ShipState.Normal;
    float _vRollAngle = 0;

    public ShipState shipState { get { return _shipState; } }

    // Start is called before the first frame update
    void Start()
    {

    }

    void FixedUpdate()
    {
        if (_shipState == ShipState.Normal)
        {
            if (_evasionInput != EvadeDirection.None)
            {
                _shipState = ShipState.Evading;

                if (_evasionInput == EvadeDirection.East)
                {
                    tiltController.targetAngle = -360;
                }
                else if (_evasionInput == EvadeDirection.West)
                {
                    tiltController.targetAngle = 360;
                }
                else
                {
                    tiltController.targetAngle = 0;
                    _vRollAngle = 0;
                }
                tiltController.tiltSpeed = evadeTiltSpeed;

                return;
            }
            
            if (_strafeInput != 0)
            {
                transform.Translate(_strafeInput * strafeSpeed * Time.fixedDeltaTime, 0, 0, Space.Self);
                
                tiltController.targetAngle = strafeTilt * -Mathf.Sign(_strafeInput);
                tiltController.tiltSpeed = strafeTiltSpeed;
            }
            else
            {
                transform.Rotate(-_pitchInput * pitchSensitivity, 0, 0, Space.Self);
                transform.Rotate(0, _yawInput * yawSensitivity, 0, Space.World);

                tiltController.targetAngle = _yawInput == 0 ? 0 : yawTilt * -Mathf.Sign(_yawInput);
                tiltController.tiltSpeed = yawTiltSpeed;
            }

            float multiplier = Mathf.Lerp(slowMultiplier, boostMultiplier, (1 + _boostInput) / 2);
            transform.Translate(0, 0, thrustSpeed * multiplier * Time.fixedDeltaTime, Space.Self);
        }

        if (_shipState == ShipState.Evading)
        {
            if (_evasionInput == EvadeDirection.East || _evasionInput == EvadeDirection.West)
            {
                if (tiltController.isTilting == false)
                {
                    _shipState = ShipState.Normal;
                    _evasionInput = EvadeDirection.None;
                    return;
                }

                float direction = _evasionInput == EvadeDirection.East ? 1 : -1;
                transform.Translate(direction * evadeStrafeSpeed * Time.fixedDeltaTime, 0, 0, Space.Self);
            }
            else if (_evasionInput == EvadeDirection.Vertical)
            {
                float nextAngle = Mathf.MoveTowards(_vRollAngle, 360, evadePitchSpeed * Time.fixedDeltaTime);
                transform.RotateAround(
                    transform.position + (evadeRadius * transform.up),
                    transform.right,
                    -1 * (nextAngle - _vRollAngle)
                );
                _vRollAngle = nextAngle;

                if (_vRollAngle == 360)
                {
                    _shipState = ShipState.Normal;
                    _evasionInput = EvadeDirection.None;
                    return;
                }
            }
        }
    }
}
