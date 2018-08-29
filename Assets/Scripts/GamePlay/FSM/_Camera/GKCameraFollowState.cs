using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GKStateMachine;
using GKUI;

class GKCameraFollowState : GKStateMachineStateBase<MachineStateID> {

    private CameraController _controller;
    private Vector3 _lookAt = Vector3.zero;


    public GKCameraFollowState() : base(MachineStateID.Follow)
    {

    }

    override public void Enter()
    {
        _controller = CameraController.Instance();
        _controller.ResetCameraParent(false);
        UIController.instance.ShowHUD(false);
        _lookAt = new Vector3(0, -10, 0);

    }

    override public void Exit()
    {
        if(null != _controller.GetFocus())
           _controller.SetTargetPos(_controller.GetFocus().position.x, _controller.GetFocus().position.z);
    }

    override public MachineStateID Update()
    {
        
        if (null == _controller.GetMainCamera() || null == _controller.GetFocus())
            return ID;


        // <版本一> 摄像机位置以鸟瞰位置朝向角色.
        //Vector3 pos = _controller.GetFocus().position;
        //Quaternion rot = Quaternion.LookRotation(pos - _controller.GetMainCameraTransform().position);
        //_controller.GetMainCameraTransform().rotation = Quaternion.Slerp(_controller.GetMainCameraTransform().rotation, rot, Time.deltaTime * _controller.GetRotationSpeed());
        //Vector3 forward = _controller.GetForward();
        //pos -= forward * _controller.GetDistance();
        //_controller.GetMainCameraTransform().position = Vector3.Lerp(_controller.GetMainCameraTransform().position, pos, Time.deltaTime * _controller.GetMoveSpeed());

        // <版本二> 摄像机位置为世界中心点到目标角色的延长线. 朝向为世界中心点.
        Vector3 pos = _controller.GetFocus().position;
        Vector3 unitVector = pos.normalized;
        pos += (unitVector * _controller.GetDistance());
        pos.y = _controller.GetDistance();
        _controller.GetMainCameraTransform().position = Vector3.Lerp(_controller.GetMainCameraTransform().position, pos, Time.deltaTime * _controller.GetMoveSpeed());
        Quaternion rot = Quaternion.LookRotation(_lookAt - pos);
        _controller.GetMainCameraTransform().rotation = Quaternion.Slerp(_controller.GetMainCameraTransform().rotation, rot, Time.deltaTime * _controller.GetRotationSpeed());

        return ID;
    }

}
