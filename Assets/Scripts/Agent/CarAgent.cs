using System;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class CarAgent : Agent
{
    public  event Action OnEpisodeReset;
    private CarMainSystem _mainSystem;
    private VehicleContext _context;
    private SensorHub _sensorHub;

    private InputManager _inputManager;
    private AgentManager _agentManager;
    
    public void Setup(CarMainSystem mainSystem, AgentManager agentManager)
    {
        _mainSystem = mainSystem;
        var context = _mainSystem.GetCurrentContext();
        _context = context;
        _sensorHub = context.SensorHub;
        _inputManager = mainSystem.InputManager;
        _agentManager = agentManager;
    
    }
    public override void OnEpisodeBegin()
    {
        OnEpisodeReset?.Invoke();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //상태 입력받기
        sensor.AddObservation(_context.SteeringAngle);
        sensor.AddObservation(_context.Throttle);
        sensor.AddObservation(_context.Brake);
        sensor.AddObservation(_context.StateHub.Transmission.currentGearIndex/_context.StateHub.Transmission.gearCount);
        sensor.AddObservation(_context.StateHub.Engine.currentRPM/_context.StateHub.Engine.maxRPM);
        sensor.AddObservation(_context.StateHub.Transmission.isShifting);
        
        //===준비된 메트릭 값들 입력받기
        float[] bodyObs = _sensorHub.ChassisSensor.ChassisData.observationArray;
        foreach (float val in bodyObs)
        {
            sensor.AddObservation(val);
        }
        //===Slip값 8개 입력받기
        sensor.AddObservation(_sensorHub.wheelSensor.SensorDatas[0].forwardSlip/10);
        sensor.AddObservation(_sensorHub.wheelSensor.SensorDatas[0].sidewaysSlip/10);
        sensor.AddObservation(_sensorHub.wheelSensor.SensorDatas[1].forwardSlip/10);
        sensor.AddObservation(_sensorHub.wheelSensor.SensorDatas[1].sidewaysSlip/10);
        sensor.AddObservation(_sensorHub.wheelSensor.SensorDatas[2].forwardSlip/10);
        sensor.AddObservation(_sensorHub.wheelSensor.SensorDatas[2].sidewaysSlip/10);
        sensor.AddObservation(_sensorHub.wheelSensor.SensorDatas[3].forwardSlip/10);
        sensor.AddObservation(_sensorHub.wheelSensor.SensorDatas[3].sidewaysSlip/10);
        //===???
        float maxWheelRpm = 2000f;
        sensor.AddObservation(_sensorHub.wheelSensor.SensorDatas[0].isGrounded);
        sensor.AddObservation(_sensorHub.wheelSensor.SensorDatas[1].isGrounded);
        sensor.AddObservation(_sensorHub.wheelSensor.SensorDatas[2].isGrounded);
        sensor.AddObservation(_sensorHub.wheelSensor.SensorDatas[3].isGrounded);
        sensor.AddObservation(_sensorHub.wheelSensor.SensorDatas[0].rpm/maxWheelRpm);
        sensor.AddObservation(_sensorHub.wheelSensor.SensorDatas[1].rpm/maxWheelRpm);
        sensor.AddObservation(_sensorHub.wheelSensor.SensorDatas[2].rpm/maxWheelRpm);
        sensor.AddObservation(_sensorHub.wheelSensor.SensorDatas[3].rpm/maxWheelRpm);
        //35개.
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var action = actions.ContinuousActions.Array;
        _inputManager.Step(action);
        TakeRewardResult(_agentManager.Step());//보상값 처리
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        
    }

    private void TakeRewardResult(RewardResult result)
    {
        AddReward(result.reward);
        if (result.endEpisode)
        {
            EndEpisode();
        }
        
    }
}
