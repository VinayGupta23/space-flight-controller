using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class ShipController : MonoBehaviour, IShipStateNotifier
{
    [Header("Thrust")]
    public float thrustSpeed = 20f;
    public float boostMultiplier = 2f;
    public float slowMultiplier = 0.5f;

    [Header("Drift")]
    public float driftSpeed = 10f;

    [Header("Turn")]
    public float pitchSensitivity = 1f;
    public float yawSensitivity = 1f;

    [Header("Evade: East/West")]
    public float evadeDriftSpeed = 60f;
    public float evadeRollSpeed = 720f;

    [Header("Evade: Vertical")]
    public float evadePitchSpeed = 720f;
    public float evadeRadius = 4.5f;

    #region User Inputs

    float _boostInput = 0;
    float _driftInput = 0;
    Vector2 _turnInput = Vector2.zero;
    ShipState _evasionInput = ShipState.Normal;

    public void OnBoost(InputValue value)
    {
        _boostInput = value.Get<float>();
    }

    public void OnDrift(InputValue value)
    {
        _driftInput = value.Get<float>();
    }

    public void OnPitchYaw(InputValue value)
    {
        _turnInput = value.Get<Vector2>();
    }

    public void OnRollEast()
    {
        if (shipState != ShipState.Normal)
        {
            Debug.Log("Ignoring evade input as already evading!");
            return;
        }
        _evasionInput = ShipState.EvadeEast;
    }

    public void OnRollWest()
    {
        if (shipState != ShipState.Normal)
        {
            Debug.Log("Ignoring evade input as already evading!");
            return;
        }
        _evasionInput = ShipState.EvadeWest;
    }

    public void OnRollVertical()
    {
        if (shipState != ShipState.Normal)
        {
            Debug.Log("Ignoring evade input as already evading!");
            return;
        }
        _evasionInput = ShipState.EvadeVertical;
    }

    #endregion

    ShipState _shipState = ShipState.Normal;

    // For evasion
    float _startRollAngle = 0f;
    float _endRollAngle = 0f;
    Vector3 _evadeDirection = Vector3.zero;

    public Action<ShipState> OnShipStateChanged { get; set; }
    public ShipState shipState
    { 
        get { return _shipState; }
        set
        {
            _shipState = value;
            OnShipStateChanged(value);
        }
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    void FixedUpdate()
    {
        if (shipState == ShipState.Normal)
        {
            if (_evasionInput != ShipState.Normal)
            {
                SetupEvade(_evasionInput);
                shipState = _evasionInput;
            }
            else
            {
                ProcessNormalInput();
            }
        }
        else
        {
            bool evadeDone = 
                shipState == ShipState.EvadeVertical 
                ? PerformEvadeVertical() 
                : PerformEvadeHorizontal();

            if (evadeDone)
            {
                shipState = ShipState.Normal;
                _evasionInput = ShipState.Normal;
            }
        }

        float multiplier = Mathf.Lerp(slowMultiplier, boostMultiplier, (1 + _boostInput) / 2);
        transform.Translate(0, 0, thrustSpeed * multiplier * Time.fixedDeltaTime, Space.Self);
    }

    void ProcessNormalInput()
    {
        float _yawInput = _turnInput.x;
        float _pitchInput = _turnInput.y;

        transform.Rotate(-_pitchInput * pitchSensitivity, 0, 0, Space.Self);
        transform.Rotate(0, _yawInput * yawSensitivity, 0, Space.World);

        if (_turnInput == Vector2.zero)
        {
            // Only do drift if ship is not turning
            transform.Translate(_driftInput * driftSpeed * Time.fixedDeltaTime, 0, 0, Space.Self);
        }
    }

    void SetupEvade(ShipState evadeState)
    {
        switch (evadeState)
        {
            case ShipState.EvadeEast:
                _startRollAngle = 0;
                _endRollAngle = -360;
                _evadeDirection = transform.right;
                break;
            case ShipState.EvadeWest:
                _startRollAngle = 0;
                _endRollAngle = 360;
                _evadeDirection = -transform.right;
                break;
            case ShipState.EvadeVertical:
                _startRollAngle = 0;
                _endRollAngle = 360;
                break;
            default:
                break;
        }
    }

    bool PerformEvadeHorizontal()
    {
        float nextAngle = Mathf.MoveTowards(_startRollAngle, _endRollAngle, evadeRollSpeed * Time.fixedDeltaTime);
        transform.Rotate(0, 0, nextAngle - _startRollAngle, Space.Self);
        _startRollAngle = nextAngle;

        transform.position += _evadeDirection * evadeDriftSpeed * Time.fixedDeltaTime;

        return _startRollAngle == _endRollAngle;
    }

    bool PerformEvadeVertical()
    {
        float nextAngle = Mathf.MoveTowards(_startRollAngle, _endRollAngle, evadePitchSpeed * Time.fixedDeltaTime);
        transform.RotateAround(
            transform.position + (evadeRadius * transform.up),
            transform.right,
            -1 * (nextAngle - _startRollAngle)
        );
        _startRollAngle = nextAngle;

        return _startRollAngle == _endRollAngle;
    }
}
