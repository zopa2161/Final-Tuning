// "바퀴의 상태를 측정하는 부품"

using UnityEngine;

public class WheelMetric : IVehiclePartConfig, IVehiclePartRun
{
    private readonly WheelCollider _collider;
    

    private readonly WheelLoc _loc;
    private WheelSensorData _mySensorData; // 여기에 측정값을 적음

    public WheelMetric(WheelCollider col, WheelLoc loc)
    {
        _collider = col;
        _loc = loc;
    }

    // [Config] 데이터 기록할 곳 연결
    public void LinkSensor(SensorHub sensorHub)
    {
        _mySensorData = sensorHub.wheelSensor.SensorDatas[(int)_loc];
    }
    
    public void ApplySettings(VehicleStateHub hub) { /* 측정기는 세팅할 게 딱히 없음 */ }

    // [Run] Sensing (FixedUpdate)
    public void OnSensing()
    {
        if (_mySensorData == null) return;
     

        // 물리 엔진에서 값 추출
        _mySensorData.rpm = _collider.rpm;

        _mySensorData.isGrounded = _collider.GetGroundHit(out WheelHit hit);
        
        if (_mySensorData.isGrounded)
        {
            _mySensorData.forwardSlip= hit.forwardSlip;
            _mySensorData.sidewaysSlip = hit.sidewaysSlip;
        }
        else
        {
            _mySensorData.forwardSlip = 0f;
            _mySensorData.sidewaysSlip = 0f;

            // ...
        }
        
    }

    // 측정기는 행동(Actuate)이나 비주얼(Visual)에 관여하지 않음
    public void OnActuate(VehicleContext context) { }
    public void OnVisualUpdate() { }
}