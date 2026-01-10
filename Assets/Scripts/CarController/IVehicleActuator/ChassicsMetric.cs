using UnityEngine;

public class ChassisMetric : IVehiclePartRun, IVehiclePartConfig
{
    private readonly Rigidbody _rb;
    private ChassisSensorData _targetData;
    private Vector3 _lastVelocity; // 가속도 계산용 캐시

    private IVehiclePartConfig _vehiclePartConfigImplementation;

    public ChassisMetric(Rigidbody rb)
    {
        _rb = rb;
    }

    public void LinkSensor(SensorHub hub)
    {
        _targetData = hub.ChassisSensor.ChassisData; // 연결
    }
    public void ApplySettings(VehicleStateHub stateHub)
    {
        
    }

    public void OnSensing()
    {
        if (_targetData == null) return;

        // 1. 기본 속도
        Vector3 currentVel = _rb.linearVelocity;
        _targetData.velocityVector = currentVel;
        _targetData.speedMs = currentVel.magnitude;
        _targetData.speedKmh = _targetData.speedMs * 3.6f;

        // 2. 로컬 속도 (Local Velocity)
        Vector3 localVel = _rb.transform.InverseTransformDirection(currentVel);
        _targetData.localVelocity = localVel;
        _targetData.forwardSpeed = localVel.z;
        _targetData.sideSpeed = localVel.x;

        // 3. 각속도 (Angular Velocity - Local)
        // 유니티의 rb.angularVelocity는 World 기준일 수 있으므로 로컬로 변환 권장
        _targetData.angularVelocity = _rb.transform.InverseTransformDirection(_rb.angularVelocity);

        // 4. 가속도 (Acceleration & G-Force)
        if (Time.fixedDeltaTime > 0)
        {
            Vector3 worldAccel = (currentVel - _lastVelocity) / Time.fixedDeltaTime;
            _targetData.worldAcceleration = worldAccel;
            
            // [핵심] 로컬 가속도: 운전자가 느끼는 쏠림 (우회전하면 몸이 왼쪽으로 쏠리는 힘)
            _targetData.localAcceleration = _rb.transform.InverseTransformDirection(worldAccel);
        }
        _lastVelocity = currentVel;

        // 5. 자세 (Orientation)
        _targetData.upVector = _rb.transform.up;
        
        // Euler Angle은 짐벌락 이슈가 있지만, 차량 시뮬에선 직관적이라 쓸만함
        // -180 ~ 180 범위로 변환해주면 좋음
        Vector3 euler = _rb.rotation.eulerAngles;
        _targetData.pitchAngle = WrapAngle(euler.x);
        _targetData.rollAngle = WrapAngle(euler.z);

        // 6. 슬립 앵글 (Side Slip Angle)
        // 차가 바라보는 방향(Z)과 실제 이동 방향 사이의 각도
        // 속도가 너무 느리면(2m/s 이하) 계산 의미 없음
        if (_targetData.speedMs > 2.0f)
        {
            _targetData.slipAngle = Mathf.Atan2(localVel.x, localVel.z) * Mathf.Rad2Deg;
        }
        else
        {
            _targetData.slipAngle = 0f;
        }

        // [마지막] ML-Agents용 배열 갱신
        _targetData.UpdateObservationArray();
    }
    
    private float WrapAngle(float angle)
    {
        if (angle > 180f) angle -= 360f;
        return angle;
    }

    public void OnActuate(VehicleContext context) { } // Actuate 없음
    public void OnVisualUpdate() { }
}