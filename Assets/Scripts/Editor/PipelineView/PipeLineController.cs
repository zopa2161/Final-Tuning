using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class PipelineController : ISubController
{
    private VisualTreeAsset _viewTemplate;
    private PipelineView _view;
    
    private ITunableVehicle _targetVehicle;
    public PipelineController()
    {
       //=== 뷰 생성 ===
        _viewTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
            "Assets/Scripts/Editor/PipelineView/PipelineView.uxml");
        _view = new PipelineView(_viewTemplate);
      
        
    }
    public void Initialize(VisualElement root)
    {
        var container = root.Q<VisualElement>("pipeline-view-container");
        container.Add(_view.Root);
    }

    public void SetTarget(ITunableVehicle vehicle)
    {
        _targetVehicle = vehicle;
    }
    public void Update()
    {
        _view.EngineWidget.Update(_targetVehicle.Context);
        _view.TransmissionWidget.Update(_targetVehicle.Context);
        for(int i = 0; i<4;i++)
        {
            _view.WheelWidget.Units[i].Update(_targetVehicle.Context);
        }
        _view.InputWidget.Update(_targetVehicle.Context);
        _view.BrakeWidget.Update(_targetVehicle.Context);
    }


   
}