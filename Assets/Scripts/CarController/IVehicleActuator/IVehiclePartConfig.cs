using UnityEngine;

public interface IVehiclePartConfig 
{
    // 센서 연결
    void LinkSensor(SensorHub sensorHub);
    // 물리값 세팅 (SO -> State 변환값 적용)
    void ApplySettings(VehicleStateHub stateHub);
}
