using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipStateRelay : MonoBehaviour, IShipStateNotifier, IShipStateListener
{
    // Note: Changing these at runtime is not supported
    public GameObject shipStateSource;

    public Action<ShipState> OnShipStateChanged { get; set; }

    private IShipStateNotifier _shipStateNotifier = null;

    // Start is called before the first frame update
    void Start()
    {
        _shipStateNotifier = shipStateSource.GetComponent<IShipStateNotifier>();
        if (_shipStateNotifier != null)
        {
            _shipStateNotifier.OnShipStateChanged += ShipStateChangedHandler;
        }
        else
        {
            Debug.LogError("No ship state source found, relay will not work!");
        }
    }

    public void ShipStateChangedHandler(ShipState newState)
    {
        // Relay to other objects that are listening
        OnShipStateChanged(newState);
    }

    void OnDestroy()
    {
        if (_shipStateNotifier != null)
        {
            _shipStateNotifier.OnShipStateChanged -= ShipStateChangedHandler;
        }
    }
}
