using System.Collections.Generic;

public class SensorHub
{
    public WheelSensor wheelSensor = new WheelSensor();
    public ChassisSensor ChassisSensor = new ChassisSensor();

    public Dictionary<string, CollisionResult> CollisionResultByName = new Dictionary<string, CollisionResult>();
    public Dictionary<string, TriggerResult> TriggerResultByName = new Dictionary<string, TriggerResult>();
    public void Setup()
    {
       
    }
    
    public void ResetEpisode()
    {
        wheelSensor.ResetEpisode();
        ChassisSensor.ChassisData.ResetEpisode();
        
    }
}