using Sirenix.OdinInspector;
using UnityEngine;
public enum DriveType { FWD, RWD, AWD }
//public enum DifferentialType { Open, Locked, LSD_1Way, LSD_1_5Way, LSD_2Way, ActiveLSD }
[CreateAssetMenu(menuName = "Module/Drivetrain Module")]
public class DrivetrainModule : BaseModule
{
    [Header("Drive Type")]
    [Tooltip("구동방식")]
    [SerializeField,EnumToggleButtons]
    private DriveType _driveType = DriveType.RWD;
    
    [ShowIf(nameof(_driveType), DriveType.AWD)]
    [SerializeField]
    [Range(0f,1f)]
    private float _frontTorqueSplit = 0.3f;
    
    [Header("Differential (LSD)")]
    [Tooltip("오픈 디퍼렌셜 시뮬레이션: 한쪽이 미끄러지면 반대쪽 힘도 빠짐 (0: Open ~ 1: Locked)")]
    [Range(0f, 1f)]
    [SerializeField] private float _lockingRatio = 0.5f;
    
    public override void Calculate(VehicleContext context)
    {
        DrivetrainState myState = context.StateHub.Drivetrain;
        TransmissionState transState = context.StateHub.Transmission;
        
        if (myState == null || transState == null) return;
        
        float inputTorque = transState.finalOutputTorque;
        
        //초기화
        myState.torqueFL = 0; myState.torqueFR = 0;
        myState.torqueRL = 0; myState.torqueRR = 0;
        
        float frontTotal = 0f;
        float rearTotal = 0f;

        switch (_driveType)
        {
            case DriveType.FWD:
                frontTotal = inputTorque;
                break;
            case DriveType.RWD:
                rearTotal = inputTorque;
                break;
            case DriveType.AWD:
                frontTotal = inputTorque * _frontTorqueSplit;
                rearTotal = inputTorque * (1.0f - _frontTorqueSplit);
                break;
        }
        myState.Torques[(int)WheelLoc.FL] = frontTotal / 2f;
        myState.Torques[(int)WheelLoc.FR] = frontTotal/ 2f;
        myState.Torques[(int)WheelLoc.RL] = rearTotal / 2f;
        myState.Torques[(int)WheelLoc.RR] = rearTotal / 2f;

        ApplyDifferential(context.SensorHub.wheelSensor.fl, context.SensorHub.wheelSensor.fr, frontTotal, out myState.Torques[(int)WheelLoc.FL], out myState.Torques[(int)WheelLoc.FR]);
        ApplyDifferential(context.SensorHub.wheelSensor.rl, context.SensorHub.wheelSensor.rr, rearTotal, out myState.Torques[(int)WheelLoc.RL], out myState.Torques[(int)WheelLoc.RR]);
        
    }

    public override void OnCalculate()
    {
        //throw new System.NotImplementedException();
    }

    public override BaseModuleState CreateState()
    {
        return new DrivetrainState();
    }
    
    private void ApplyDifferential(WheelSensorData leftObs, WheelSensorData rightObs, float axleTorque, out float torqueL, out float torqueR)
    {
        if (axleTorque == 0) 
        {
            torqueL = 0; torqueR = 0;
            return;
        }

        // 기본은 50:50 배분 (Locked Diff)
        torqueL = axleTorque * 0.5f;
        torqueR = axleTorque * 0.5f;

        // [간이 오픈 디퍼렌셜 로직]
        // 한쪽 바퀴가 공중에 뜨거나 미끄러지면(Slip 높음), 
        // 반대쪽 바퀴로 가는 토크도 같이 줄어들어버리는 현상 구현
        
        float slipL = Mathf.Abs(leftObs.forwardSlip);
        float slipR = Mathf.Abs(rightObs.forwardSlip);
        
        // 더 미끄러지는 쪽의 슬립량 찾기
        float maxSlip = Mathf.Max(slipL, slipR);

        // 슬립이 임계치(예: 0.5)를 넘으면 토크 손실 발생
        // _lockingRatio가 1이면(LSD) 손실 없음, 0이면(Open) 토크 다 빠짐
        if (maxSlip > 0.5f)
        {
            // 미끄러짐에 비례해 토크 삭감 계수 계산
            float slipFactor = 1.0f - ((maxSlip - 0.5f) * 2.0f); // 0.5~1.0 슬립 -> 1.0~0.0 계수
            slipFactor = Mathf.Clamp01(slipFactor);
            
            // LSD 세팅에 따라 방어 (LockingRatio가 높으면 슬립해도 토크 유지)
            float effectiveFactor = Mathf.Lerp(slipFactor, 1.0f, _lockingRatio);

            torqueL *= effectiveFactor;
            torqueR *= effectiveFactor;
        }
    }
}
