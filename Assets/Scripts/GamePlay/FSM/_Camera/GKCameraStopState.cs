using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GKStateMachine;
using GKUI;

class GKCameraStopState : GKStateMachineStateBase<MachineStateID> {

    private CameraController _controller;

    public GKCameraStopState() : base(MachineStateID.Stop)
    {
        
    }

    override public void Enter()
    {
        _controller = CameraController.Instance();
        UIController.instance.ClearHUD();
        UIController.instance.ShowHUD(false);
    }

    override public void Exit()
    {
        
    }

    override public MachineStateID Update()
    {
        return ID;
    }

}
