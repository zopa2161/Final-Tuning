// 2. [Run] 매 프레임 물리 연산 시 접근하는 인터페이스
public interface IVehiclePartRun
{
    // 센싱 (Read Physics)
    void OnSensing();
    // 물리 적용 (Write Physics)
    void OnActuate(VehicleContext context);
    
    void OnVisualUpdate();
}