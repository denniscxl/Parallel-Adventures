using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GKStateMachine;
using GKUI;

class GKCameraBirdsEyeState : GKStateMachineStateBase<MachineStateID> {

    private CameraController _controller;
    //private UIVirtualJoyStick _uiVirtualJoyStick;
    private Vector3 _tempTarget = Vector3.zero;

    public GKCameraBirdsEyeState() : base(MachineStateID.BirdsEye)
    {
        
    }

    override public void Enter()
    {
        _controller = CameraController.Instance();
        _controller.ResetCameraParent(true);
        UIController.instance.ShowHUD(true);
        //_uiVirtualJoyStick = UIVirtualJoyStick.instance;
    }

    override public void Exit()
    {
        //if(null == _uiVirtualJoyStick)
        //    _uiVirtualJoyStick = UIVirtualJoyStick.instance;
        
        //if (null != _uiVirtualJoyStick)
            //_uiVirtualJoyStick.Reset();


        if(null != _controller.GetFocus())
            _controller.SetTargetPos(_controller.GetFocus().position.x, _controller.GetFocus().position.z);
    }

    override public MachineStateID Update()
    {
        if (null == _controller.GetFocus())
            return ID;

        _tempTarget.x = _controller.GetFocus().position.x;
        _tempTarget.y = _controller.GetZoomVal();
        _tempTarget.z = _controller.GetFocus().position.z;

        // 不跟随玩家.
        //_controller.GetMainCameraTransform().localPosition = Vector3.Lerp(_controller.GetMainCameraTransform().localPosition, target, Time.deltaTime * _controller.GetMoveSpeed());
        // 跟随玩家.
        _controller.GetMainCameraTransform().localPosition = Vector3.Lerp(_controller.GetMainCameraTransform().localPosition, _tempTarget, Time.deltaTime * _controller.GetMoveSpeed());
        _controller.GetMainCameraTransform().localRotation = Quaternion.Slerp(_controller.GetMainCameraTransform().localRotation, Quaternion.Euler(90,0,0), Time.deltaTime * _controller.GetRotationSpeed());

        return ID;
    }

}
