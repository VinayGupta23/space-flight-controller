using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThrusterManager : MonoBehaviour, IShipStateListener
{
    // Note: Changing these at runtime is not supported
    public ParticleSystem targetSystem;
    public ShipStateRelay shipStateSource;

    public float minSpeed;
    public float maxSpeed;
    public AnimationCurve speedCurve;
    public float smoothTime;

    private float _currentSpeed = 0;
    private float _targetSpeed = 0;
    private float _boostInput = 0;
    private float _smoothingVelocity = 0;

    public void EvaluateSpeed(float time)
    {
        float speedPerc = speedCurve.Evaluate(time);
        _targetSpeed = minSpeed + (maxSpeed - minSpeed) * speedPerc;
    }

    public void OnBoost(InputValue value)
    {
        _boostInput = value.Get<float>();
        _boostInput = 0.5f + 0.5f * Mathf.Clamp(_boostInput, -1f, 1f);
        EvaluateSpeed(_boostInput);
    }

    // Start is called before the first frame update
    void Start()
    {
        if (targetSystem == null)
        {
            Debug.LogWarning("No particle system attached, manager will be inactive!");
        }

        if (shipStateSource != null)
        {
            shipStateSource.OnShipStateChanged += ShipStateChangedHandler;
        }
        else
        {
            Debug.LogWarning("No ship state notifier found, thruster settings may be incorrect.");
        }

        EvaluateSpeed(0.5f);
    }

    public void ShipStateChangedHandler(ShipState targetState)
    {
        // Nothing as of now
        // Maybe show full velocity thrusters when doing an evasive maneuver?
    }
    
    void Update()
    {
        if (targetSystem == null)
            return;

        if (_currentSpeed != _targetSpeed)
        {
            _currentSpeed = Mathf.SmoothDamp(_currentSpeed, _targetSpeed, ref _smoothingVelocity, smoothTime);
            var mainFX = targetSystem.main;
            mainFX.startSpeed = _currentSpeed;
        }
    }

    void OnDestroy()
    {
        if (shipStateSource != null)
        {
            shipStateSource.OnShipStateChanged -= ShipStateChangedHandler;
        }
    }
}
