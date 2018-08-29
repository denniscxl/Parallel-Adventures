using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GKStateMachine;
using GKRole;

class GKUnitAmbushState : GKStateMachineStateBase<MachineStateID> {
    
    public GKUnitAmbushState(GKUnit unit) : base(MachineStateID.Ambush)
    {
        
    }

    override public void Enter()
    {
        
    }

    override public void Exit()
    {
        
    }

    override public MachineStateID Update()
    {
        return ID;
    }

}
