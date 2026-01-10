using System.Collections.Generic;
using UnityEngine;

public class WheelActuator : IVehiclePartConfig, IVehiclePartRun 
{
    private readonly WheelCollider _collider;
    private readonly Transform _mesh;

    private WheelLoc _loc;

    // 생성자: 나는 이것들이 없으면 존재할 수 없어! (강제성 부여)
    public WheelActuator(WheelCollider col,Transform mesh,VehicleContext context ,WheelLoc loc)
    {
        _collider = col;
        _loc = loc;
        _mesh = mesh;
        
        LinkSensor(context.SensorHub);
        ApplySettings(context.StateHub);
    }
    

    public void OnSensing()
    {
    }
    public void OnActuate(VehicleContext context)
    {
        var drivetrain = context.StateHub.Drivetrain;
        var steering = context.StateHub.Steering;
        var brake = context.StateHub.Brake;
        
        if (drivetrain == null) return;
        if (steering == null) return;
        if (brake == null) return;
        
        float motorVal = 0f;
        float brakeVal = 0f;
        float steerVal = 0f; // [New]
        
        _collider.motorTorque = drivetrain.Torques[(int)_loc];

        switch (_loc)
        {
            case WheelLoc.FL :
                motorVal =drivetrain.Torques[(int)(_loc)];
                brakeVal = brake.torqueFL;
                steerVal = steering.finalSteerAngleFL;
                break;
            case WheelLoc.FR:
                motorVal = drivetrain.Torques[(int)(_loc)];
                brakeVal = brake.torqueFR;
                steerVal = steering.finalSteerAngleFR;
                break;
            case WheelLoc.RL:
                motorVal = drivetrain.Torques[(int)(_loc)];
                brakeVal = brake.torqueRL;
                steerVal = steering.finalSteerAngleRL;
                break;
            case WheelLoc.RR:
                motorVal = drivetrain.Torques[(int)(_loc)];
                brakeVal = brake.torqueRR;
                steerVal = steering.finalSteerAngleRR;
                break;
        }

        _collider.motorTorque = motorVal;
        _collider.brakeTorque = brakeVal;
        _collider.steerAngle = steerVal;
    }
    public void OnVisualUpdate()
    {
        // 여기서는 오직 "비주얼(Mesh)"만 다룹니다.
        // 물리에 힘을 가하지 않고, 위치만 읽어옵니다.
        
        if (_mesh == null || _collider == null) return;

        _collider.GetWorldPose(out Vector3 pos, out Quaternion rot);
        _mesh.position = pos;
        _mesh.rotation = rot;
    
    }

    public void ApplySettings(VehicleStateHub stateHub)
    {
        var tireState = stateHub.ExtensionStates[typeof(TireState)] as TireState;
        var suspensionState = stateHub.Suspension;
        _collider.mass = tireState.totalMass;
        _collider.radius = tireState.calculatedRadius;
        //지금 당장은 전후륜 같은 세팅
        _collider.forwardFriction = tireState.forwardFriction;
        _collider.sidewaysFriction = tireState.sidewaysFriction;
        
        //=== 서스펜션
        var spring = _collider.suspensionSpring;
        spring.spring = suspensionState.finalSpringRate;
        spring.damper = suspensionState.finalDamper;
        spring.targetPosition = suspensionState.finalTargetPos;
        _collider.suspensionSpring = spring;
        _collider.suspensionDistance = suspensionState.finalDistance;

    }
    public void LinkSensor(SensorHub sensorHub)
    {
  
    }
   
}
