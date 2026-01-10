using System;
using System.Collections.Generic;

public class MetricHub : IMetricHub
{
    private VehicleContext _context;
    
    public void Setup(VehicleContext context)
    {
        _context = context;
    }

    public void RegisterSession(MetricSession session)
    {
         session.metricEntries.Add(CreateMetricEntry("speed","km/h", 0f,200f,() => _context.SensorHub.ChassisSensor.ChassisData.speedKmh));
         //이런식으로 하나씩 추가.
         session.metricEntries.Add(CreateMetricEntry("RPM","RPM", 0,10000, () => _context.StateHub.Engine.currentRPM));
         session.metricEntries.Add(CreateMetricEntry("GearIndex","",0,10, () => _context.StateHub.Transmission.currentGearIndex));
        
         session.metricEntries.Add(CreateMetricEntry("SlipAngle", "deg", -10,10,() => _context.SensorHub.ChassisSensor.ChassisData.slipAngle));
         session.metricEntries.Add(CreateMetricEntry("YawRate", "deg/s", -10,10,() => _context.SensorHub.ChassisSensor.ChassisData.angularVelocity.z));
         session.metricEntries.Add(CreateMetricEntry("RollAngle", "deg", -10,10,() => _context.SensorHub.ChassisSensor.ChassisData.rollAngle));
         session.metricEntries.Add(CreateMetricEntry("PitchAngle", "deg", -10,10,() => _context.SensorHub.ChassisSensor.ChassisData.pitchAngle));
         
    }
    public void RegisterSource(string id, string unit, Func<float> provider)
    {
        throw new NotImplementedException();
    }
    public float GetValue(string id)
    {
        throw new NotImplementedException();
    }
    public IEnumerable<string> GetAllIds()
    {
        throw new NotImplementedException();
    }

    private MetricEntry CreateMetricEntry(string id, string unit,float min, float max, Func<float> provider)
    {
        var entry = new MetricEntry(id,unit,min,max,provider);
        return entry;
    }
}