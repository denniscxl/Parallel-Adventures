using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GKStateMachine;
using GKRole;

class GKUnitIdleState : GKStateMachineStateBase<MachineStateID> {

    private GKUnit _unit = null;

    public GKUnitIdleState(GKUnit unit) : base(MachineStateID.Idle)
    {
        _unit = unit;
    }

    override public void Enter()
    {
        Debug.Log(string.Format("GKUnitIdleState Unit name: {0}", _unit.GetAttribute(EObjectAttr.Name).stringValue));
        _unit.myAnimator.SetTrigger("Idle");
    }

    override public void Exit()
    {
        
    }

    override public MachineStateID Update()
    {
        return ID;
    }

}
