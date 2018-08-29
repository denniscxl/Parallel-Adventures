using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GKStateMachine;
using GKRole;

class GKUnitDefenseState : GKStateMachineStateBase<MachineStateID> {

    private GKUnit _unit;

    // 如果指令为防御, 下一帧仍需进行指令运算. 得到新指令.
    public GKUnitDefenseState(GKUnit unit) : base(MachineStateID.Defense)
    {
        _unit = unit;
    }

    override public void Enter()
    {
        Debug.Log(string.Format("GKUnitDefenseState Unit name: {0}", _unit.GetAttribute(EObjectAttr.Name).stringValue));
        _unit.myAnimator.SetTrigger("Defense");
        _unit.myAnimator.SetBool("IsDefense", true);
    }

    override public void Exit()
    {
        _unit.myAnimator.SetBool("IsDefense", false);
    }

    override public MachineStateID Update()
    {
        return ID;
    }

}
