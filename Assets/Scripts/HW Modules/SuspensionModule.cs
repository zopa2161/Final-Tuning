using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;


public class SuspensionModule : BaseModule
{
    [BoxGroup("Springs")]
    [Tooltip("스프링 강성 (N/m). 일반차: 30,000, 레이싱: 80,000+")]
    [SerializeField, Range(0f, 20f)]
    private float _springRateKgf = 10f;

    [BoxGroup("Springs")]
    [Tooltip("차고 조절 (기본값 대비 오프셋, m). -0.05면 5cm 다운.")]
    [SerializeField]
    private float _rideHeightMm = 0.0f;


    [BoxGroup("Dampers")]
    [Tooltip("범프 (눌릴 때 저항). 단위: Ns/m")]
    [SerializeField]
    private float _damperBump = 3000f;

    [BoxGroup("Dampers")]
    [Tooltip("리바운드 (펴질 때 저항). 보통 범프보다 높게 설정.")]
    [SerializeField]
    private float _damperForce = 5000f;


    // [BoxGroup("Stabilizers")]
    // [Tooltip("스태빌라이저 강성 (Nm). 높을수록 좌우 롤링 억제.")]
    // [FormerlySerializedAs("antiRollBarForce")]
    // [SerializeField]
    // private float _antiRollBarForce = 5000f;


    // [BoxGroup("Geometry")]
    // [Tooltip("캠버 각도 (음수면 안쪽으로 기울어짐). 예: -2.5")]
    // [Range(-10f, 0f)]
    // [FormerlySerializedAs("camberAngle")]
    // [SerializeField]
    private float _camberAngle = -1.5f;
    
    // 서스펜션 총 이동 거리 (유니티 WheelCollider 설정용)
    [FormerlySerializedAs("suspensionDistance")]
    [SerializeField]
    private float _suspensionDistance = 0.2f;

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
        SuspensionState state = new SuspensionState();

        // [Logic] 현실 단위 -> 유니티 단위 변환
        
        // 1. Spring: kgf/mm -> N/m
        // 1 kgf ≈ 9.8 N, 1 mm = 0.001 m
        // Value * 9.8 * 1000 = N/m
        state.finalSpringRate = _springRateKgf * 9.81f * 1000f;

        // 2. Damper: 값 그대로 적용 (단위가 같다면)
        state.finalDamper = _damperForce;

        // 3. Target Position (0.0 ~ 1.0)
        // 차고(mm)를 거리(m)로 변환해서 오프셋 적용
        float heightOffsetM = _rideHeightMm * 0.001f;
        // 기본 0.5에서 오프셋만큼 뺌 (낮추려면 타겟포지션을 올려야 함? 아니면 스프링을 줄이나? 
        // 보통 targetPosition 1.0이 완전 압축, 0.0이 완전 이완이라 가정 시 튜닝에 맞게 조정)
        state.finalTargetPos = 0.5f - (heightOffsetM / _suspensionDistance);

        state.finalDistance = _suspensionDistance;

        return state;
    }
}