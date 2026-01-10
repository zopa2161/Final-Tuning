using Sirenix.OdinInspector;
using UnityEngine;

public enum TireType { Street, Sport, Slick, OffRoad, Drift }

[CreateAssetMenu(menuName = "Tuning/Tire Module")]
public class TireModule : BaseModule
{
    [BoxGroup("Dimensions")]
    [Tooltip("단면폭 (mm). 예: 245")]
    [SerializeField]
    private float _widthMm = 245f;

    [BoxGroup("Dimensions")]
    [Tooltip("편평비 (%). 예: 45")]
    [SerializeField]
    private float _aspectRatio = 45f;

    [BoxGroup("Dimensions")]
    [Tooltip("타이어 무게 (kg). 휠 무게와 합쳐져서 현가하질량(Unsprung Mass)이 됨.")]
    [SerializeField]
    private float _tireWeightKg = 10f;

    [BoxGroup("Performance")]
    [SerializeField]
    private TireType _tireType = TireType.Street;

    [BoxGroup("Performance")]
    [Tooltip("전반적인 그립 계수. 1.0 = 표준, 1.5 = 레이싱 슬릭.")]
    [Range(0.5f, 2.0f)]
    [SerializeField]
    private float _gripFactor = 1.0f;

    [BoxGroup("Performance")]
    [Tooltip("타이어 강성 (Stiffness). 높으면 칼같은 코너링, 낮으면 물렁함/드리프트.")]
    [Range(0.5f, 5.0f)]
    [SerializeField]
    private float _stiffness = 1.0f;

    

    // --------------------------------------------------------
    // 유니티 WheelCollider용 마찰 곡선 프리셋 생성
    // --------------------------------------------------------
    // public WheelFrictionCurve GetFrictionCurve(bool isSideways)
    // {
    //     WheelFrictionCurve curve = new WheelFrictionCurve();
    //
    //     // 튜닝값에 따른 마찰력 설정
    //     // 미끄러지기 시작하는 지점 (Extremum)
    //     curve.extremumSlip = isSideways ? 0.2f : 0.4f; 
    //     curve.extremumValue = 1.0f * _gripMultiplier; 
    //
    //     // 완전히 미끄러질 때 (Asymptote)
    //     curve.asymptoteSlip = isSideways ? 0.5f : 0.8f;
    //     curve.asymptoteValue = 0.8f * _gripMultiplier; // 한계 그립 이후 떨어지는 정도
    //
    //     // 타이어 강성 적용
    //     curve.stiffness = _stiffness;
    //
    //     return curve;
    // }

    public override void Calculate(VehicleContext context)
    {
        //throw new System.NotImplementedException();
    }

    public override void OnCalculate()
    {
        throw new System.NotImplementedException();
    }
    public override BaseModuleState CreateState()
    {
        var state = new TireState();
        state.widthMm = _widthMm;
        state.tireWeightKg = _tireWeightKg;
        return state;
    }

    public override void InitializeState(BaseModuleState state, VehicleStateHub hub)
    {
        TireState myState = state as TireState;
        
        // RimState는 반지름/무게 계산에만 필요 (없어도 마찰력은 계산 가능하지만 구조상 여기서 처리)
        RimState rimState = hub.ExtensionStates[typeof(RimState)] as RimState; 
    
        if (myState != null)
        {
            // A. [반지름 & 무게 계산] (Rim이 있을 때만)
            if (rimState != null)
            {
                float wallHeightM = (_widthMm * 0.001f) * (_aspectRatio * 0.01f);
      
                myState.calculatedRadius = (rimState.radiusM) + wallHeightM;
                
                myState.totalMass = rimState.massKg + myState.tireWeightKg;
            }

            // B. [마찰력 계산] (Rim 없어도 계산 가능)
            // 전진(Forward)과 횡(Sideways) 마찰 곡선을 각각 생성하여 저장
            myState.forwardFriction = CreateFrictionCurve(false);
            myState.sidewaysFriction = CreateFrictionCurve(true);
        }
    }
    
    private WheelFrictionCurve CreateFrictionCurve(bool isSideways)
    {
        WheelFrictionCurve curve = new WheelFrictionCurve();

        // [1] Stiffness (강성) 설정
        // Drift 타이어면 횡강성을 낮춰서 잘 미끄러지게 보정
        float stiffnessMult = 1.0f;
        if (_tireType == TireType.Drift && isSideways) stiffnessMult = 0.8f;
        
        curve.stiffness = _stiffness * stiffnessMult;


        // [2] Slip (미끄러짐 발생 지점) 설정
        // OffRoad는 흙길이라 슬립이 더 길게 늘어짐
        float slipMult = (_tireType == TireType.OffRoad) ? 1.5f : 1.0f;

        // isSideways: 횡방향은 보통 전진보다 슬립 한계가 낮음 (0.2 vs 0.4)
        curve.extremumSlip = (isSideways ? 0.2f : 0.4f) * slipMult;
        curve.asymptoteSlip = (isSideways ? 0.5f : 0.8f) * slipMult;


        // [3] Value (마찰 계수 = Grip) 설정
        // _gripFactor가 메인. asymptote(한계 이후)는 보통 피크의 70~80% 수준
        curve.extremumValue = 1.0f * _gripFactor;
        
        // Drift 타이어는 미끄러진 후(Asymptote)에도 그립을 어느 정도 유지해야 컨트롤이 됨
        float dropOff = (_tireType == TireType.Drift) ? 0.9f : 0.7f; 
        curve.asymptoteValue = curve.extremumValue * dropOff;

        return curve;
    }
}
