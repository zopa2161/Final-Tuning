using Sirenix.OdinInspector;
using UnityEngine;


public enum EngineType { I4, V6, V8, Flat6, Rotary, V10, V12 }

[CreateAssetMenu(menuName = "Module/Base Engine Module")]
public class EngineModule : BaseModule
{
    // --- Fields (SerializeField private fields grouped here) ---
    
    // === 실린더 데이터 ===
    [EnumToggleButtons, BoxGroup("Engine Cylinder Data" )]
    [SerializeField]
    private EngineType _engineType;

    [Tooltip("실린더 1개당 배기량 (cc). 보통 400~600cc 사이"),BoxGroup("Engine Cylinder Data" )]
    [Range(300f, 1000f)] 
    [SerializeField]
    private float _ccPerCylinder = 500f;

    [BoxGroup("Engine Cylinder Data")]
    [SerializeField]
    private float _torquePerLiter;
    
    [ShowInInspector, ReadOnly, LabelText("Total Displacement (cc)"),BoxGroup("Engine Cylinder Data" )]
    public float TotalDisplacement => CylinderCount * _ccPerCylinder;

    [ShowInInspector, ReadOnly, LabelText("Class"),BoxGroup("Engine Cylinder Data" )]
    public string EngineClassString => $"{TotalDisplacement / 1000f:F1}L {_engineType}";

    
    // === 퍼포먼스 데이터 ===
    [Range(0, 10000)]
    [BoxGroup("Performance Data" )]
    [ValidateInput("ValidateIdleRpm", "idle RPM must be less than max RPM.")]
    [SerializeField]
    private float _idleRPM = 800f;
    
    [BoxGroup("Performance Data" )]
    [Tooltip("X축: RPM (0~1 정규화), Y축: 토크 효율 (0~1)")]
    [SerializeField]
    private AnimationCurve _torqueCurve;
    [ShowInInspector,BoxGroup("Performance Data" )]
    public float MaxRPM =>9000f * Mathf.Pow(_ccPerCylinder/500f, 2);
    [ShowInInspector,BoxGroup("Performance Data" )]
    public float MaxTorque => TotalDisplacement * _torquePerLiter / 1000;
    [ShowInInspector,ReadOnly,BoxGroup("Performance Data" )]
    public float MaxHP { get; private set; }

    [SerializeField, BoxGroup("Performance Data")]
    [Range(0f,10f)]
    public float _response;

    [SerializeReference, BoxGroup("Aspiration Data")]
    private BaseAspirationModule _aspirationModule;
    
    //=== 프로퍼티 (코드끼리만 공유)
    public EngineType EngineType { get => _engineType; set => _engineType = value; }
    public float CcPerCylinder { get => _ccPerCylinder; set => _ccPerCylinder = value; }
    public float IdleRPM { get => _idleRPM; private set => _idleRPM = value; }
    public AnimationCurve TorqueCurve { get => _torqueCurve; set => _torqueCurve = value; }
    private int CylinderCount => GetCylinderCount(_engineType);

    //최대 마력값 계산.
    [Tooltip("X: RPM(0~1), Y: 토크 효율(0~1)")]
    [Button("Update Engine Specs", ButtonSizes.Large),BoxGroup("Performance Data")]
    private void CalculateSpecs()
    {
        float tempMaxHP = 0f;
        float tempMaxTorque = 0f;
 
        float step = 100f;
        for (float rpm = 0; rpm <= MaxRPM; rpm += step)
        {
            // 1. 현재 RPM에서의 토크값 가져오기 (Curve Evaluation)
            // curve의 X축이 0~1 정규화되어 있다면 rpm / maxRPM
            float torque = _torqueCurve.Evaluate(rpm / MaxRPM)* MaxTorque;

            // 2. 마력 계산 (공식: Torque * RPM / 5252 -> 단위에 따라 상수는 9549 등 변경)
            // 여기서는 예시로 9549 (Nm -> kW 변환 가정) 혹은 간략식 사용
            // 편의상 상수 C = 7000 (게임 밸런스용 임의 상수)라 가정
            float hp = (torque * rpm) / 7000f; 

            // 3. 최댓값 갱신 확인
            if (torque > tempMaxTorque) tempMaxTorque = torque;
            if (hp > tempMaxHP)
            {
                tempMaxHP = hp;
            }
        }
        MaxHP = tempMaxHP;
    }

    
    
    private int GetCylinderCount(EngineType type)
    {
        switch (type)
        {
            case EngineType.I4: return 4;
            case EngineType.V6: return 6;
            case EngineType.V8: return 8;
            case EngineType.V10: return 10;
            case EngineType.V12: return 12;
            case EngineType.Rotary: return 2; // 로터리는 보통 2로터
            default: return 4;
        }
    }
    
    private bool ValidateIdleRpm(float value) => value < MaxRPM;
    
    // EngineModule.cs의 Calculate 내부

    public override void Calculate(VehicleContext context)
    {
        EngineState myState = context.StateHub.Engine;
        if (myState == null) return;
        UpdateEngineRPM(context);
        // 1. RPM 제한 (Clamp)
        // (참고: 실제로는 RPM이 Idle 밑으로 떨어지려 할 때 토크를 확 줘서 살려내는 게 맞지만, 일단 Clamp 유지)
        myState.currentRPM = Mathf.Clamp(myState.currentRPM, _idleRPM, MaxRPM);

        
        float idleMaintainThrottle = Mathf.InverseLerp(_idleRPM * 1.5f, _idleRPM, myState.currentRPM);
        
        float baseCreepPower = 0.15f; 
        float autoThrottle = idleMaintainThrottle * baseCreepPower;
        
        float effectiveThrottle = Mathf.Max(context.Throttle, autoThrottle);
        
        // =================================================================
        // 3. 토크 계산
        // =================================================================

        float efficiency = _torqueCurve.Evaluate(myState.currentRPM / MaxRPM);

        // 수정: context.Throttle 대신 effectiveThrottle 사용
        float rawTorque = MaxTorque * efficiency * effectiveThrottle;

    
        // (디버깅)
        // if (effectiveThrottle > 0 && context.Throttle == 0)
        //     Debug.Log($"[Creep] RPM:{myState.currentRPM:F0}, AutoThrottle:{autoThrottle:F3}, Torque:{rawTorque:F1}");

        myState.generatedTorque = rawTorque;
    }

    public override void OnCalculate()
    {
        throw new System.NotImplementedException();
    }

    public override BaseModuleState CreateState()
    {
        var state = new EngineState();
        state.maxRPM = MaxRPM;
        state.maxTorque = MaxTorque;
        return state;
    }

    
    private void UpdateEngineRPM(VehicleContext context)
    {
        EngineState myState = context.StateHub.Engine;
        TransmissionState transState = context.StateHub.Transmission;
        // 1. 작성하신 코드 (엔진의 의지)
        float combustionRPM = Mathf.Lerp(
            myState.currentRPM, 
            MaxRPM * context.Throttle, 
            _response * context.deltaTime
        );

        // 2. Feedback RPM (바퀴의 강요)
        // (바퀴 RPM * 기어비 * 종감속비)
        float feedbackRPM = transState.feedbackRPM;

        // 3. 클러치 상태 (0: 끊김, 1: 연결)
        float clutch = transState.clutchRatio;

        // [결론] 클러치가 붙을수록 바퀴 속도(Feedback)를 따라가야 함!
        // 클러치가 떨어지면 엔진 의지(Combustion)대로 돎.
        myState.currentRPM = Mathf.Lerp(combustionRPM, feedbackRPM, clutch);
    
        // (최소 RPM 방어)
        myState.currentRPM = Mathf.Max(myState.currentRPM, _idleRPM);
    }
    
    
}
