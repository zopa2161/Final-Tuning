using System;
using UnityEngine;

public class TriggerDetector : IVehiclePartConfig, IVehiclePartRun, IDisposable
{
    private readonly CollisionProxy _proxy;
    private readonly string _partName;
    private TriggerResult _triggerResult;

    public TriggerDetector(CollisionProxy proxy, string partName)
    {
        _proxy = proxy;
        _proxy.OnTrigger += OnTrigger;
        _partName = partName;
        _triggerResult = new TriggerResult();
    }

    public void LinkSensor(SensorHub sensorHub)
    {
        // SensorHub에 TriggerResult를 직접 연결하도록 변경
        sensorHub.TriggerResultByName[_partName] = _triggerResult;
    }

    public void ApplySettings(VehicleStateHub stateHub)
    {
        // 필요 시 설정 적용 로직 구현
    }

    public void OnSensing()
    {
    }

    public void OnActuate(VehicleContext context)
    {
        // 필요 시 물리 연산 로직 구현
    }

    public void OnVisualUpdate()
    {
        // 필요 시 시각적 업데이트 로직 구현
    }

    public void Dispose()
    {
        _proxy.OnTrigger -= OnTrigger;
    }

    private void OnTrigger(CollisionProxy proxy, Collider other)
    {
        var checker = other.GetComponent<Checker>();
        if (checker != null)
        {
            _triggerResult.isPass = true;
            _triggerResult.passedCheckerNumber = checker.GetCheckerNumber();
        }
    }
}