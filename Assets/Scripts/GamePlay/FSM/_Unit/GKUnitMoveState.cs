using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GKStateMachine;
using GKPathFinding;
using GKRole;
using GKMap;

class GKUnitMoveState : GKStateMachineStateBase<MachineStateID> {

    private List<AStarPoint> _path = null;
    private AStarPoint _curNode = null;
    private GKUnit _unit = null;
    private Vector3 _target = Vector3.zero;
    private Vector3 _start = Vector3.zero;
    private float _moveSpeed = 0;
    private float _moveTime = 0;
    private float _moveLastTime = 0;
    private bool _moveStepA = true;
    private float _percentage = 0;
    private float _rotateSpeed = 0;

    public GKUnitMoveState(GKUnit unit) : base(MachineStateID.Move)
    {
        _unit = unit;
        _rotateSpeed = _unit.GetAttribute(EObjectAttr.RotationSpeed).floatValue;
    }

    override public void Enter()
    {
        //Debug.Log(string.Format("GKUnitMoveState Unit name: {0}", _unit.GetAttribute(EObjectAttr.Name).stringValue));
        _unit.myAnimator.SetBool("IsWalk", true);
        _unit.myAnimator.SetBool("IsRun", false);
    }

    override public void Exit()
    {
        _unit.myAnimator.SetBool("IsWalk", false);
        _unit.myAnimator.SetBool("IsRun", false);
    }


    override public MachineStateID Update()
    {
        _moveLastTime += Time.deltaTime;
        _percentage = _moveLastTime / _moveTime;

        //Debug.Log(string.Format("_moveLastTime: {0}, _moveTime:{1}, _percentage:{2}", _moveLastTime, _moveTime, _percentage));

        // 移动完毕检测.
        if(1 <= _percentage)
        {
            _percentage = 0;
            _moveLastTime = 0;

            if(_moveStepA)
            {
                _moveStepA = false;
                if(null != _curNode)
                {
                    _start = _unit.myTransform.position;
                    _unit.Grid = _curNode.grid;
                    _target = _curNode.grid.transform.position;
                }
            }
            else
                NextNode();
        }

        if (null == _curNode)
        {
            return MachineStateID.Idle;
        }

        Quaternion rot = Quaternion.LookRotation(_curNode.grid.gameObject.transform.position - _unit.myTransform.position);
        rot.x = 0;
        rot.z = 0;
        _unit.myTransform.rotation = Quaternion.Slerp(_unit.myTransform.rotation, rot, Time.deltaTime * _rotateSpeed);
        _unit.myTransform.position = Vector3.Lerp(_start, _target, _percentage);

        return ID;
    }

    public void SetPath(List<AStarPoint> path)
    {
        _path = path;
        // 当前目标移动节点不为空.
        // 当前移动中, 并变更移动目标.
        // 角色寻路需要剔除当前所在节点.
        // 否则在反向移动时先移动至当前地块中心再折返现象.
        NextNode();
        if(null != _curNode)
        {
            NextNode();
        }
    }

    private void NextNode()
    {
        if(null == _path || 0 == _path.Count)
        {
            _curNode = null;
            return;
        }

        _moveStepA = true;

        // 获取下一个移动节点.
        _curNode = _path[_path.Count - 1];
        _path.RemoveAt(_path.Count - 1);

        // 计算移动时间与速度.
        CalcMoveSpeed(_curNode.grid.tile.terrain);
        // 设置移动目标.
        _target = (_curNode.grid.transform.position - _unit.myTransform.position) * 0.5f;
        _target = _unit.myTransform.position + _target;
        _start = _unit.myTransform.position;
        CalcMoveTime(_target);
    }

    // 计算当前移动速度.
    // 移动速度为 英雄速度 * 地形加成.
    private void CalcMoveSpeed(TerrainType type)
    {
        _moveSpeed = 0;
        _moveSpeed = _unit.GetAttribute(EObjectAttr.MoveSpeed).floatValue * DataController.Data.GetTerrainData((int)type).speed;
    }

    // 计算移动时间.
    private void CalcMoveTime(Vector3 pos)
    {
        float distance = Vector3.Distance(_unit.myTransform.position, pos);
        _moveTime = distance / _moveSpeed;
    }
}
