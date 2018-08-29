using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GKStateMachine;
using GKRole;

// 角色攻击状态.
// 攻击目标角色, 如果目标角色丢失或超出攻击半径变更为待机状态.
class GKUnitAttackState : GKStateMachineStateBase<MachineStateID> {
   
    // 自身对象.
    private GKUnit _selfUnit = null;
    // 目标对象.
    private GKUnit _targetUnit = null;
    private float _tmpDistance = 0;
    private float _lastTime = 0;
    private float _rotateSpeed = 0;

    public GKUnitAttackState(GKUnit unit) : base(MachineStateID.Attack)
    {
        _selfUnit = unit;
    }

    override public void Enter()
    {
        Debug.Log(string.Format("GKUnitAttackState Unit name: {0}", _selfUnit.GetAttribute(EObjectAttr.Name).stringValue));
        _selfUnit.myAnimator.SetTrigger("Attacking");
        _selfUnit.myAnimator.SetBool("IsAttacking", true);
        _rotateSpeed = _selfUnit.GetAttribute(EObjectAttr.RotationSpeed).floatValue;
    }

    override public void Exit()
    {
        _selfUnit.myAnimator.SetBool("IsAttacking", false);
    }

    override public MachineStateID Update()
    {
        if (null == _selfUnit || null == _targetUnit)
        {
            return MachineStateID.Idle;
        }
            
        // 判断是否超出攻击范围.
        _tmpDistance = Vector3.Distance(_selfUnit.myTransform.position, _targetUnit.myTransform.position);
        if(_tmpDistance > _selfUnit.GetAttribute(EObjectAttr.AttackRange).ValInt)
        {
            return MachineStateID.Idle;
        }
            
        Vector3 lookAt = _targetUnit.myTransform.position - _selfUnit.myTransform.position;
        if(lookAt != Vector3.zero)
        {
            // 旋转, 朝向目标.
            Quaternion rot = Quaternion.LookRotation(lookAt);
            rot.x = 0;
            rot.z = 0;
            _selfUnit.myTransform.rotation = Quaternion.Slerp(_selfUnit.myTransform.rotation, rot, Time.deltaTime * _rotateSpeed);
        }
        else 
        {
            Debug.Log("GKUnitAttackState lookAt is zero.");
        }

        // 根据攻击间隔计算攻击.
        if(Time.realtimeSinceStartup - _lastTime > _selfUnit.GetAttribute(EObjectAttr.AttackInterval).floatValue)
        {
            _lastTime = Time.realtimeSinceStartup;
            _targetUnit.Hit(_selfUnit.GetAtkDamage(), _selfUnit);
        }

        return ID;
    }

    // 设置攻击对象.
    public void SetTarget(GKUnit target)
    {
        _targetUnit = target;
    }

}
