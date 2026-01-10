using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Tuning/Brake Module")]
public class BrakeModule : BaseModule
{
    [BoxGroup("Performance")]
    [Tooltip("최대 제동 토크 (Nm). 캘리퍼 피스톤 수와 패드 마찰력의 총합.")]
    [Range(0f, 10000f)]
    [SerializeField]
    private float _maxBrakeTorque = 2500f;

    

    [BoxGroup("Setting")]
    [Range(0f, 1f)]
    [Tooltip("브레이크의 전륜 편향.")]
    [SerializeField]
    private float _frontBias = 0.65f;
    
    [BoxGroup("ABS Config")]
    [SerializeField] private bool _enableABS = true;
    
    [BoxGroup("ABS Config"), ShowIf("_enableABS")]
    [Range(0f, 1f)]
    [Tooltip("ABS 개입 슬립 한계점. (0.2 정도면 타이어가 미끄러지기 직전에 개입)")]
    [SerializeField]
    private float _absThreshold = 0.25f;
    
    public override BaseModuleState CreateState()
    {
        var state = new BrakeState();
        state.maxBrakeTorque = _maxBrakeTorque;
        return new BrakeState();
    }

    public override void Calculate(VehicleContext context)
    {
        BrakeState myState = context.StateHub.Brake;
        if (myState == null) return;

        float totalForce = _maxBrakeTorque * context.Brake;
        
        if (totalForce <= 0)
        {
            myState.torqueFL = 0; myState.torqueFR = 0;
            myState.torqueRL = 0; myState.torqueRR = 0;
            myState.isABSActive = false;
            return;
        }
        float frontPerWheel = totalForce * _frontBias * 0.5f;
        float rearPerWheel = totalForce * (1.0f - _frontBias) * 0.5f;
        
        bool absActive = false;

        myState.torqueFL = ApplyABS(frontPerWheel, context.SensorHub.wheelSensor.fl, ref absActive);
        myState.torqueFR = ApplyABS(frontPerWheel, context.SensorHub.wheelSensor.fr, ref absActive);
        myState.torqueRL = ApplyABS(rearPerWheel, context.SensorHub.wheelSensor.rl, ref absActive);
        myState.torqueRR = ApplyABS(rearPerWheel, context.SensorHub.wheelSensor.rr, ref absActive);
        myState.isABSActive = absActive;
    }

    public override void OnCalculate()
    {
        throw new System.NotImplementedException();
    }

    private float ApplyABS(float targetTorque, WheelSensorData sensor, ref bool absFlag)
    {
        if (!_enableABS) return targetTorque;
    
        // 전진 슬립(Forward Slip)의 절대값이 임계치를 넘으면 잠긴(Lock) 것임
        if (Mathf.Abs(sensor.forwardSlip) > _absThreshold)
        {
            absFlag = true;
            return 0f; // 브레이크를 풀어버림 (Pulse 효과)
            
            // (심화) 0으로 완전히 풀기보다 targetTorque * 0.1f 정도로 약하게 잡거나,
            // Time.time을 이용해 다다다닥(Pulse) 거리는 주기를 만들면 더 리얼함.
        }

        return targetTorque;
    }

    
}