using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GKStateMachine;
using GKUI;

class GKCameraOverall : GKStateMachineStateBase<MachineStateID> {

    private CameraController _controller;
    private Vector3 targetPos = Vector3.zero;
    private Transform myControllerTransform = null;

    public GKCameraOverall() : base(MachineStateID.Overall)
    {
        
    }

    override public void Enter()
    {
        _controller = CameraController.Instance();
        _controller.ResetCameraParent(true);
        _controller.GetMainCamera().transform.localPosition = new Vector3(0, CalcCameraHeight() , 0);
        _controller.GetMainCamera().transform.localRotation = Quaternion.Euler(90, 0, 0);
        UIController.instance.ShowHUD(false);
        myControllerTransform = _controller.GetMainCamera().transform;
    }

    override public void Exit()
    {
        if(null != _controller.GetFocus())
            _controller.SetTargetPos(_controller.GetFocus().position.x, _controller.GetFocus().position.z);
    }

    override public MachineStateID Update()
    {
        myControllerTransform.localPosition = Vector3.Lerp(myControllerTransform.localPosition,  targetPos, Time.deltaTime * 5);
        return ID;
    }

    // 计算全局摄像机高度.
    private float CalcCameraHeight()
    {
        int level = 0;
        switch(MyGame.Instance.MapSize)
        {
            case GKMap.Size.Small:
                level = 0;
                break;
            case GKMap.Size.Normal:
                level = 0;
                break;
            case GKMap.Size.Large:
                level = 0;
                break;
            case GKMap.Size.World:
                level = 0;
                break;
            case GKMap.Size.Epic:
                level = 0;
                break;
        }
        return level * 50 + 100;
    }

    // 改变全局摄像机位置与高度.
    public void ChangePos(Vector3 pos, int adjustHeight = 0)
    {
        // 如果控制器为空. 跳出逻辑.
        // 存在尚未进入全局状态, 但设置全局行为.
        if (null == _controller)
            return;

        float height = (0 == adjustHeight) ? CalcCameraHeight() : adjustHeight;
        targetPos = new Vector3(pos.x, height, pos.z);
    }
}
