using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GKStateMachine;
using GKRole;

// 角色受到伤害时状态. 动画播放完毕后返回上一个状态.
class GKUnitHitState : GKStateMachineStateBase<MachineStateID> {
    
    private GKUnit _unit = null;
    private GKUnit _attacker = null;
    private int _damager = 0;
    private Animator _animator;
    private readonly string _hitStateName = "Base Layer.damage";
    private GKUnitStateMachine _stateMachine = null;
    private MachineStateID _lastState = MachineStateID.Idle;

    public GKUnitHitState(GKUnit unit) : base(MachineStateID.Hit)
    {
        _unit = unit;
        _stateMachine = _unit.GetStateMachine();
        _animator = _unit.myAnimator;
    }

    override public void Enter()
    {
        Debug.Log(string.Format("GKUnitHitState Unit name: {0}", _unit.GetAttribute(EObjectAttr.Name).stringValue));
        _unit.myAnimator.SetTrigger("Damage");
        // 设置完结时状态跳转ID.
        if (MachineStateID.Hit != _stateMachine.GetLastState().ID)
        {
            _lastState = _stateMachine.GetLastState().ID;
        }
        // 计算伤害.
        int hp = _unit.GetAttribute(EObjectAttr.Hp).ValInt;
        hp -= _damager;
        //Debug.Log(string.Format("Current Hp: {0}, Damage: {1}", hp, _damager));
        // 如果血量低于0. 变更为死亡状态.
        if(hp <= 0)
        {
            hp = 0;
            _lastState = MachineStateID.Dead;
            // 更新攻击者击退数.
            if (null != _attacker)
            {
                int kill = _attacker.GetAttribute(EObjectAttr.KillCount).ValInt;
                _attacker.SetAttribute(EObjectAttr.KillCount, kill + 1, true);
            }
        }
        _unit.SetAttribute(EObjectAttr.Hp, hp, true);
    }

    override public void Exit()
    {
        
    }

    override public MachineStateID Update()
    {
        if(!_animator.GetCurrentAnimatorStateInfo(0).IsName(_hitStateName))
        {
            // 返回之前状态.
            Debug.Log(string.Format("Return to State: {0}", _lastState));
            return _lastState;
        }
        return ID;
    }

    public void SetDamage(int damager, GKUnit attacker)
    {
        _damager = damager;
        _attacker = attacker;
    }
}
