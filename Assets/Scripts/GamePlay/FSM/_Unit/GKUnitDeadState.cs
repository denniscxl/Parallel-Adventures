using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GKStateMachine;
using GKRole;

// 角色死亡状态.
class GKUnitDeadState : GKStateMachineStateBase<MachineStateID> {

    private GKUnit _unit = null;
    private Animator _animator;
    private readonly string _deadStateName = "Base Layer.dead";
    private AnimatorStateInfo _stateInfo;

    public GKUnitDeadState(GKUnit unit) : base(MachineStateID.Dead)
    {
        _unit = unit;
        _animator = _unit.myAnimator;
        if(null != _animator)
            _stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
    }

    override public void Enter()
    {
        
    }

    override public void Exit()
    {
        
    }

    override public MachineStateID Update()
    {
        if (null != _animator)
        {
            // normalizedTime: 范围0 -- 1,  0是动作开始，1是动作结束.
            if ((_stateInfo.normalizedTime > 0.99f) && (_stateInfo.IsName(_deadStateName)))
            {
                // 播放完成后隐藏对象.
                _unit.gameObject.SetActive(false);
            }
        }

        return ID;
    }

}
