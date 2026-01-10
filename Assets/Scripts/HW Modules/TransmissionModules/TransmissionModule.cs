using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Module/Base Transmission Module")]
public class TransmissionModule : BaseModule
{
    
    [Tooltip("종감속비 (Final Drive). 이 숫자가 클수록 가속형, 작을수록 최고속형."),BoxGroup("GearInfo")]
    [Range(0,10f), PropertyOrder(0)]
    [SerializeField]
    private float _finalDriveRatio;
    
    [ShowInInspector, ReadOnly, BoxGroup("GearInfo"), PropertyOrder(0)]
    public int GearCount => _gearInfos.Count; 
    [SerializeField,BoxGroup("GearInfo"), PropertyOrder(1)]
    private List<GearInfo> _gearInfos;
    
    [Tooltip("기어 변속에 걸리는 시간 (초)"),BoxGroup("GearInfo")]
    [Range(0, 2f)]
    [SerializeField, PropertyOrder(1)]
    private float _shiftTime;
    
    
    public override void Calculate(VehicleContext context)
    {
        TransmissionState myState = context.StateHub.Transmission;
        EngineState engineState = context.StateHub.Engine;
        if (myState == null) return;
        myState.isShifting = false;
        myState.clutchRatio = 1.0f;
        if (myState.shiftTimer > 0)
        {
            myState.shiftTimer -= context.deltaTime;
            myState.clutchRatio = Mathf.Lerp(0.2f, 1, myState.shiftTimer / _shiftTime);
            myState.isShifting = true;
        }
        else
        {
            // 엔진 RPM을 보고 변속 결정 (엔진 정보가 있어야 가능)
            if (engineState != null)
            {
                float engineRPM = engineState.currentRPM;
                int maxGear = _gearInfos.Count;

                // 1) Upshift 체크 (마지막 단 아님 + RPM 높음 + 악셀 밟는 중일 때 주로 발생)
                if (myState.currentGearIndex < maxGear && engineRPM > _gearInfos[myState.currentGearIndex-1].upshiftRpmPercent * engineState.maxRPM)
                {
                    ChangeGear(myState, 1); // +1단
                }
                // 2) Downshift 체크 (1단 아님 + RPM 낮음)
                else if (myState.currentGearIndex > 1 && engineRPM < _gearInfos[myState.currentGearIndex - 1].downshiftRpmPercent * engineState.maxRPM)
                {
                    ChangeGear(myState, -1); // -1단
                }
            }
        }
        float gearRatio = _gearInfos[myState.currentGearIndex-1].gearRatio;
        myState.currentRatio = gearRatio;

        // 바퀴 속도 가져오기 (Observations)
        float wheelRPM = context.SensorHub.wheelSensor.GetAvgRPM();
        
        // 엔진 예상 RPM 역산: WheelRPM * Gear * Final
        float calcRPM = wheelRPM * myState.currentRatio * _finalDriveRatio;
        myState.feedbackRPM = Mathf.Abs(calcRPM);
        
        var calculatedEngineRPM = wheelRPM * gearRatio * _finalDriveRatio;
        myState.feedbackRPM = calculatedEngineRPM;
        if (engineState != null)
        {
            // 쿨타임 중(변속 중)에는 동력을 잠시 끊어서 충격 완화 (Torque Cut)
            // (0.1초 정도 짧게 끊어주면 '철컥' 하는 느낌이 남)
            float shiftFactor = (myState.shiftTimer > (_shiftTime * 0.5f)) ? 0.2f : 1.0f; 

            // 엔진토크 * 기어비 * 종감속비 * 변속충격계수
            float output = engineState.generatedTorque * myState.currentRatio * _finalDriveRatio * shiftFactor;
            
            myState.finalOutputTorque = output;
        }
        else
        {
            myState.finalOutputTorque = 0f;
        }
        
    }
    
    

    public override void OnCalculate()
    {
        throw new System.NotImplementedException();
    }

    public override BaseModuleState CreateState()
    {
        var state = new TransmissionState();
        state.gearCount = GearCount;
        state.shiftTime = _shiftTime;
        state.gearInfos = _gearInfos;
        return state;
    }
    private void ChangeGear(TransmissionState state, int direction)
    {
        state.currentGearIndex += direction;
        state.shiftTimer = _shiftTime; // 쿨타임 리셋
        // (옵션) 여기서 "철컥" 사운드 재생 요청 가능
    }
    
}
[System.Serializable]
public class GearInfo
{
    [Tooltip("기어비")]
    public float gearRatio;
    [Tooltip("상한 RPM 비율 (Upshift). 이 비율을 넘으면 다음 단으로 변경.")]
    [ValidateInput("ValidateUpshiftRpm","upShiftRPM은 downShiftRPM보다는 커야합니다.")]
    [Range(0f, 1.0f)]
    public float upshiftRpmPercent;
    [Tooltip("하한 RPM 비율 (Downshift). 이 비율보다 떨어지면 이전 단으로 변경.")]
    [Range(0f, 1.0f)]
    public float downshiftRpmPercent;

    private bool ValidateUpshiftRpm(float value) => value > downshiftRpmPercent;
}