using System;
using System.Collections.Generic;
using UnityEngine;

public class VehicleStateHub 
{
    
    public EngineState Engine;
    public TransmissionState Transmission;
    public SteeringState Steering;
    public BrakeState Brake;
    public SuspensionState Suspension;
    public DrivetrainState Drivetrain;
    
    public Dictionary<Type, BaseModuleState> ExtensionStates = new Dictionary<Type, BaseModuleState>();


    public void RegisterState(BaseModuleState state)
    {
        Type type = state.GetType();

        // 코어 모듈이면 필드에 연결, 아니면 딕셔너리에 넣음
        if (state is EngineState e) Engine = e;
        else if (state is TransmissionState t) Transmission = t;
        else if (state is SteeringState s) Steering = s;
        else if (state is BrakeState b) Brake = b;
        else if (state is SuspensionState ss) Suspension = ss;
        else if (state is DrivetrainState d) Drivetrain = d;
        else
        {
            if (!ExtensionStates.ContainsKey(type)) ExtensionStates.Add(type, state);
        }
    }

    public void ResetEpisode()
    {
        Engine.ResetEpisode();
        Transmission.ResetEpisode();
        Steering.ResetEpisode();
        Brake.ResetEpisode();
        Suspension.ResetEpisode();
    }
}
