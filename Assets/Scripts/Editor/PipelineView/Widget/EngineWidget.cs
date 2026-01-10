using UnityEngine.UIElements;

public class EngineWidget : PipelineWidgetBase
{
   

    private CircularGauge _rpmGauge;
    private CircularGauge _speedGauge;
    private VerticalGauge _torqueGauge;


    // 생성자에서 자기 영역 찾기
    public EngineWidget(VisualElement root) : base(root)
    {
        _rpmGauge = root.Q<CircularGauge>("rpm-gauge");
        _rpmGauge.ShowText = true;
        _rpmGauge.TextFormat = "F0";
        _rpmGauge.AutoUpdateText = true;
        _rpmGauge.FontSize = 14;
        
        _speedGauge = root.Q<CircularGauge>("speed-gauge");
        _speedGauge.ShowText = true;
        _speedGauge.TextFormat = "F0";
        _speedGauge.AutoUpdateText = true;
        _speedGauge.FontSize = 14;
        _speedGauge.HighValue = 200;
        _torqueGauge = root.Q<VerticalGauge>("torque-vertical-gauge");

    }
    
    public override void Update(VehicleContext context)
    {
        _rpmGauge.HighValue = context.StateHub.Engine.maxRPM;
        _rpmGauge.Value = context.StateHub.Engine.currentRPM;
        _speedGauge.Value = context.SensorHub.ChassisSensor.ChassisData.speedKmh;
        
        _torqueGauge.Value = context.StateHub.Engine.generatedTorque / context.StateHub.Engine.maxTorque;
  
    
    }
}