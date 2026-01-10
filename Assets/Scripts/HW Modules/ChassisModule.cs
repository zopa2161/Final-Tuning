using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Tuning/Chassis Module")]
public class ChassisModule : BaseModule
{
    [BoxGroup("Mass & Balance")]
    [Tooltip("차량 총 중량 (kg). 경량화 튜닝 시 감소.")]
    [SerializeField]
    private float _totalMass = 1500f;

    [BoxGroup("Mass & Balance")]
    [Tooltip("전륜 무게 배분율 (0.0 ~ 1.0). 0.54면 앞이 54%.")]
    [Range(0.3f, 0.7f)]
    [SerializeField]
    private float _frontWeightBias = 0.54f;

    [BoxGroup("Mass & Balance")]
    [Tooltip("무게중심 높이 오프셋 (m). 바닥 기준 높이.")]
    [SerializeField]
    private float _centerOfMassHeight = 0.4f;


    [BoxGroup("Aerodynamics (Body Kit)")]
    [Tooltip("공기저항 계수 (Cd). 낮을수록 공기를 잘 가름.")]
    [SerializeField]
    private float _dragCoefficient = 0.3f;

    [BoxGroup("Aerodynamics (Body Kit)")]
    [Tooltip("다운포스 계수. 속도에 비례해 차를 누르는 힘.")]
    [SerializeField]
    private float _downforceFactor = 0.5f;

    [BoxGroup("Aerodynamics (Body Kit)")]
    [Tooltip("차량 전면 투영 면적 (m^2). 공기저항 계산용.")]
    [SerializeField]
    private float _frontalArea = 2.2f;

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
        return new ChassisState();
    }
}