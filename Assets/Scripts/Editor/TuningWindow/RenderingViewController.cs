using CodiceApp.EventTracking.Plastic;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;

public class RenderingViewController : ISubController
{
    private VehicleCamera _camera;
    private Camera _targetCamera;
    private RenderTexture _renderTexture;
    private VisualElement _simulationRenderer;
    private DropdownField _dropdown;
    private DropdownField _envDropdown;
    private SimulationManager _simulationManager;

    private Button _startButton;
    private Button _stopButton;

    private Action _currentStartBtnAction;
    private Action _currentStopBtnAction;

    private Action<float> _onSpeedChanged;
    private Action<float> _onTimelineChanged;

    private Slider _speedSlider;
    private Slider _timelineSlider;

    private Label _speedLabel;
    
    public float CurrentSpeed => _speedSlider.value;
    public bool IsControlTimelineSlider;
    public Slider TimelineSlider => _timelineSlider;

    public RenderingViewController(SimulationManager simulationManager,
        Action onStart,
        Action onStop,
        Action<float> onSpeedChanged,
        Action<float> onTimelineChanged)
    {
        _simulationManager = simulationManager;
        _camera = simulationManager.Camera;
        //SetModeLayout(TuningMode.Live,onStart,onStop);// 간단화.
        _currentStartBtnAction = onStart;
        _currentStopBtnAction = onStop;
        _onSpeedChanged = onSpeedChanged;
        _onTimelineChanged = onTimelineChanged;
    }

    public void Initialize(VisualElement root)
    {
        _targetCamera = _camera.gameObject.GetComponent<Camera>();
        _dropdown = root.Q<DropdownField>("camera-dropdown");
        _simulationRenderer = root.Q<VisualElement>("simulation-renderer");
        
        _speedSlider = root.Q<Slider>("simulation-speed");
        _startButton = root.Q<Button>("simulation-start");
        _stopButton = root.Q<Button>("simulation-stop");
        
        var simImage = new Image();
        simImage.style.flexGrow = 1; // 부모 크기에 꽉 차게
    
        // 3. 렌더 텍스처 연결
        _renderTexture = new RenderTexture(320, 180, 24);
        simImage.image = _renderTexture;
        simImage.scaleMode = ScaleMode.ScaleToFit; // 비율 유지하며 꽉 채우기
        
        // 4. 시뮬레이션 카메라 찾아서 연결
        // (SimulationManager 등을 통해 가져오세요)
        if (_targetCamera != null)
        {
            _targetCamera.targetTexture = _renderTexture;
        }
        
        _simulationRenderer.Add(simImage);
        
        //5 카메라 뷰 드롭다운 연결
        
        var viewList = _camera.presets;
        var nameList = new List<string>();
        foreach (var view in viewList)
        {
            nameList.Add(view.viewName);
            Debug.Log(view.viewName);
            
        }
        
        _dropdown.choices = nameList;
        _dropdown.value = nameList[0]; 
        _dropdown.RegisterValueChangedCallback(evt =>
        {
            _camera.ChangeView(_dropdown.index);
        });
        
        //시뮬레이션 위치 드랍다운
        _envDropdown = root.Q<DropdownField>("sim-env-dropdown");
        var simList = _simulationManager.SimulationEnvs;//이 과정에서 SimEnvs가 null이 되버리는듯? 그럴리가?
        var simNameList = new List<string>();
        foreach (var sim in simList)
        {
            simNameList.Add(sim.Name);
        }

        _envDropdown.choices = simNameList;
        _envDropdown.value = simNameList[0];
        _envDropdown.RegisterValueChangedCallback(evt =>
        {
            _simulationManager.SpawnToSimulation(_envDropdown.index,false);
        });
        // 버튼 연결.
      
        
       
        _startButton.clicked += () => _currentStartBtnAction?.Invoke();
        _stopButton.clicked += ()=> _currentStopBtnAction?.Invoke();
        SetModeLayout(TuningMode.Live, _currentStartBtnAction, _currentStopBtnAction);//초기화 시에는 그냥 첫 상황인 live모드 적용
        
        _speedSlider = root.Q<Slider>("simulation-speed");
        _speedSlider.lowValue = 0.1f;
        _speedSlider.highValue = 5.0f;
        _speedSlider.value = 1f;
        
        _speedSlider.RegisterValueChangedCallback(evt => 
        {
            float newSpeed = evt.newValue;
            
            // 1. 라벨 갱신
            if (_speedLabel != null) _speedLabel.text = $"{newSpeed:F1}x";

            // 2. 컨트롤러에게 전달 -> 매니저 호출
            _onSpeedChanged?.Invoke(newSpeed);
        });

        _timelineSlider = root.Q<Slider>("timeline-slider");
        _timelineSlider.lowValue = 0f;
        _timelineSlider.highValue = 1f;
        _timelineSlider.value = 0f;
        _timelineSlider.RegisterValueChangedCallback(evt =>
        {
            //IsControlTimelineSlider = true;
            _onTimelineChanged?.Invoke(evt.newValue);
            //IsControlTimelineSlider = false;
        });


    }
    

    public void SetModeLayout(TuningMode mode, Action start, Action stop)
    {
        _currentStartBtnAction = start;
        _currentStopBtnAction = stop;
        
        if (mode == TuningMode.Live)
        {
            // [Live 모드 디자인]
            _startButton.text = "Start Sim";
            _startButton.style.backgroundColor = new StyleColor(new Color(0.2f, 0.6f, 0.2f)); // Green
            
            _stopButton.text = "Stop & Reset";
            _stopButton.style.backgroundColor = new StyleColor(new Color(0.8f, 0.2f, 0.2f)); // Red
            
            // 라이브 때는 타임라인 조작 금지 (혹은 숨김)
           // _timelineSlider.SetEnabled(false); 
        }
        else // Replay or Compare
        {
            // [Replay 모드 디자인]
            _startButton.text = "Play / Pause";
            _startButton.style.backgroundColor = new StyleColor(new Color(0.2f, 0.4f, 0.8f)); // Blue
            
            _stopButton.text = "Rewind";
            _stopButton.style.backgroundColor = new StyleColor(Color.gray);
            
            // 리플레이 때는 타임라인 조작 가능
            //_timelineSlider.SetEnabled(true);
        }
    }public void UpdateUIState(bool isBusy, TuningMode currentMode)
    {
        _startButton.SetEnabled(!isBusy);
        _stopButton.SetEnabled(isBusy);
        
        _envDropdown.SetEnabled(!isBusy);
    }
    public void SetLoadedSpawn(string simName)
    {
        _envDropdown.value = simName;
    }
    public void UpdatePlayButtonState(bool isPlaying)
    {
        _startButton.text = isPlaying ? "Pause" : "Play";
    }

  
}