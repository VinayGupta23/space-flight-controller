using System;
using System.Collections;
using System.Collections.Generic;

[System.Flags]
public enum ShipState
{
    Normal = 1,
    EvadeEast = 2,
    EvadeWest = 4,
    EvadeVertical = 8,

    EvadeHorizontal = EvadeEast | EvadeWest,
    EvadeAny = EvadeHorizontal | EvadeVertical,
    Everything = Normal | EvadeAny
}

public interface IShipStateListener
{
    public void ShipStateChangedHandler(ShipState newState);
}

public interface IShipStateNotifier
{
    public Action<ShipState> OnShipStateChanged { get; set; }
}
