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

    float boostInput = 0;
    float strafeInput = 0;
    float pitchInput = 0;
    float yawInput = 0;
    EvadeDirection evasionInput = EvadeDirection.None;

    public void OnBoost(InputValue value)
    {
        boostInput = value.Get<float>();
    }

    public void OnStrafe(InputValue value)
    {
        strafeInput = value.Get<float>();
    }

    public void OnPitchYaw(InputValue value)
    {
        Vector2 pitchYaw = value.Get<Vector2>();
        yawInput = pitchYaw.x;
        pitchInput = pitchYaw.y;
    }

    public void OnRollEast()
    {
        if (shipState != ShipState.Normal)
        {
            Debug.Log("Ignoring evade input as already evading!");
            return;
        }
        evasionInput = EvadeDirection.East;
    }

    public void OnRollWest()
    {
        if (shipState != ShipState.Normal)
        {
            Debug.Log("Ignoring evade input as already evading!");
            return;
        }
        evasionInput = EvadeDirection.West;
    }

    public void OnRollVertical()
    {
        if (shipState != ShipState.Normal)
        {
            Debug.Log("Ignoring evade input as already evading!");
            return;
        }
        evasionInput = EvadeDirection.Vertical;
    }

    #endregion

    ShipState shipState = ShipState.Normal;
    float vRollAngle = 0;

    // Start is called before the first frame update
    void Start()
    {

    }

    void FixedUpdate()
    {
        if (shipState == ShipState.Normal)
        {
            if (evasionInput != EvadeDirection.None)
            {
                shipState = ShipState.Evading;

                if (evasionInput == EvadeDirection.East)
                {
                    tiltController.targetAngle = 360;
                }
                else if (evasionInput == EvadeDirection.West)
                {
                    tiltController.targetAngle = -360;
                }
                else
                {
                    tiltController.targetAngle = 0;
                    vRollAngle = 0;
                }
                tiltController.tiltSpeed = evadeTiltSpeed;

                return;
            }
            
            if (strafeInput != 0)
            {
                transform.Translate(-strafeInput * strafeSpeed * Time.fixedDeltaTime, 0, 0, Space.Self);
                
                tiltController.targetAngle = strafeTilt * Mathf.Sign(strafeInput);
                tiltController.tiltSpeed = strafeTiltSpeed;
            }
            else
            {
                transform.Rotate(pitchInput * pitchSensitivity, 0, 0, Space.Self);
                transform.Rotate(0, yawInput * yawSensitivity, 0, Space.World);

                tiltController.targetAngle = yawInput == 0 ? 0 : yawTilt * Mathf.Sign(yawInput);
                tiltController.tiltSpeed = yawTiltSpeed;
            }

            float multiplier = Mathf.Lerp(slowMultiplier, boostMultiplier, (1 + boostInput) / 2);
            transform.Translate(0, 0, -thrustSpeed * multiplier * Time.fixedDeltaTime, Space.Self);
        }

        if (shipState == ShipState.Evading)
        {
            if (evasionInput == EvadeDirection.East || evasionInput == EvadeDirection.West)
            {
                if (tiltController.isTilting == false)
                {
                    shipState = ShipState.Normal;
                    evasionInput = EvadeDirection.None;
                    return;
                }

                float direction = evasionInput == EvadeDirection.East ? 1 : -1;
                transform.Translate(-direction * evadeStrafeSpeed * Time.fixedDeltaTime, 0, 0, Space.Self);
            }
            else if (evasionInput == EvadeDirection.Vertical)
            {
                float nextAngle = Mathf.MoveTowards(vRollAngle, 360, evadePitchSpeed * Time.fixedDeltaTime);
                transform.RotateAround(
                    transform.position + (evadeRadius * transform.up),
                    transform.right,
                    nextAngle - vRollAngle
                );
                vRollAngle = nextAngle;

                if (vRollAngle == 360)
                {
                    shipState = ShipState.Normal;
                    evasionInput = EvadeDirection.None;
                    return;
                }
            }
        }
    }
}
