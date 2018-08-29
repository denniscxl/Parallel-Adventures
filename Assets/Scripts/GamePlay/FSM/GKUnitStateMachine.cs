using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GKStateMachine;
public class GKUnitStateMachine : GKStateMachineBase<MachineStateID>
{
    
}

public enum MachineStateID : byte
{
    // Unit.
    Idle = 0,
    Move,
    Attack,
    Ambush,
    Defense,
    Hit,
    Dead,
    // Camera.
    BirdsEye,
    Follow,
    Overall,
    Stop
}