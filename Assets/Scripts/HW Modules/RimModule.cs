using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Tuning/Rim Module")]
public class 
    RimModule : BaseModule
{
    [BoxGroup("Specs")]
    [Tooltip("림 지름 (인치). 예: 18, 19")]
    [SerializeField]
    private float _diameterInch = 18f;

    [BoxGroup("Specs")]
    [Tooltip("림 폭 (J). 타이어 폭과 매칭 확인용.")]
    [SerializeField]
    private float _widthJ = 8.5f;

    [BoxGroup("Specs")]
    [Tooltip("림 무게 (kg). 가벼울수록 서스펜션 반응이 좋음.")]
    [SerializeField]
    private float _weightKg = 9.5f; // 경량 휠은 8kg, 순정은 12kg 등

    [BoxGroup("Specs")]
    [Tooltip("오프셋 (mm). 휀더 밖으로 얼마나 튀어나오나 (비주얼용).")]
    [SerializeField]
    private float _offset = 35f;

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
        RimState state = new RimState();
        
        // 인치 -> 미터 반지름 변환 (1 inch = 0.0254 m)
        state.radiusM = (_diameterInch * 0.0254f) * 0.5f;
        state.massKg = _weightKg;

        return state;
    }
}