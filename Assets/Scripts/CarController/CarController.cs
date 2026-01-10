using System.Collections.Generic;
using Sirenix.OdinInspector;
using System;
using System.Linq;
using UnityEngine;

public enum WheelLoc { FL, FR, RL, RR }

public class CarController : MonoBehaviour
{
 
    [SerializeField]
    private List<WheelSetupData> _wheelSetups = new List<WheelSetupData>();
    
    private CarMainSystem _mainSystem;

    private Rigidbody _rb;
    
    private List<IVehiclePartRun> _runners = new List<IVehiclePartRun>();
    private List<IVehiclePartRun> _metrics = new List<IVehiclePartRun>();
    
    
    public void Setup(CarMainSystem mainSystem, VehicleContext vehicleContext)
    {
        _mainSystem = mainSystem;
        _rb = GetComponent<Rigidbody>();
        _runners.Clear();
        _metrics.Clear();
        
        var proxies = GetComponentsInChildren<CollisionProxy>().ToList();
        if(proxies.Count==0) Debug.LogWarning("NoProxy");
        var bodyProxy = proxies.First(x => x.partID.Equals("Body"));
        var collsionDetector = new CollsionDetector(bodyProxy, "Body");
        
        collsionDetector.LinkSensor(vehicleContext.SensorHub);
        _metrics.Add(collsionDetector);
        
        var triggerDetetor = new TriggerDetector(bodyProxy, "Body");
        triggerDetetor.LinkSensor(vehicleContext.SensorHub);
        _metrics.Add(triggerDetetor);
        
        var chassisMetric = new ChassisMetric(_rb);
        chassisMetric.LinkSensor(vehicleContext.SensorHub);
        _metrics.Add(chassisMetric);
    
        
        
        foreach (var data in _wheelSetups)
        {
            // 1. 측정기(Metric 설치
            var metric = new WheelMetric(data.col, data.loc);
            metric.LinkSensor(vehicleContext.SensorHub);
            _metrics.Add(metric);
            var actuator = new WheelActuator(data.col, data.mesh,vehicleContext ,data.loc);
            _runners.Add(actuator);
        }
        
    }

    
    private void FixedUpdate()
    {
        //===Test dummy input====
        _mainSystem.RunInputProvide();
        
        //=== Sensing===
        foreach(var runner in _runners) runner.OnSensing();
        foreach (var metric in _metrics) metric.OnSensing();
        
        //===Calculation===
        _mainSystem.RunCalculateModule();
        
        //===Actuating===
        var context = _mainSystem.GetCurrentContext();
        foreach (var runners in _runners)
        {
            runners.OnActuate(context);
        }
        
        context.Update(); 
   
   
    }

    private void Update()
    {
        foreach (var runner in _runners)
        {
            runner.OnVisualUpdate();
        }
    }
    
    

}
