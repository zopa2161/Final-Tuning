using System.Collections.Generic;
using System.Linq;


public class RewardInputData
{
    // 1. 차량의 지속적인 상태 (Context 참조)
    public VehicleContext VehicleCtx;
    public Dictionary<string, CollisionResult> CollisionResultByName;
    public int CurrentCheckerNumber;
    public RewardInputData(VehicleContext vehicleContext)
    {
        VehicleCtx = vehicleContext;
        CollisionResultByName = VehicleCtx.SensorHub.CollisionResultByName;
        CurrentCheckerNumber = 0;
    }
    public void Reset()
    {
        if (CollisionResultByName == null) return;

        foreach (var collision in CollisionResultByName.Values)
        {
            collision?.Reset();
        }
    }
}