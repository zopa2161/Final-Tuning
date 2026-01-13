using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.UIElements;

public enum TuningMode
{
    Live,
    Replay,
    Compare
}

public class TuningController
{
    [Header("Controllers")]
    private TuningRepository _repository;
    private SimulationManager _simulationManager;
    private ModuleListController _listController;
    //private GraphViewController _graphController;
    private MetricGraphController _metricController;
    private RenderingViewController _renderingController;
    private ReplayController _replayController;
    private PipelineController _pipelineController;

    private List<ISubController> _subControllers;
    [Header("Side Controllers")]
    private MetricRecorder _recorder;
    private ModuleSearchPicker _cachedPicker;
    
    //카메라 세팅
    [Header("Camera")]
    private VehicleCamera _camera;
    private Camera _targetCamera;
    private RenderTexture _renderTexture;
    private VisualElement _simulationRenderer; // UI Toolkit의 Image 요소

    //모드 관련
    [Header("Mod")]
    private DropdownField _modDropdown;

    private VisualElement _root;
    private TuningMode _currentMode;
    
    // Window가 UI를 그릴 때 데이터를 가져가야 하므로 공개
    public TuningContext Context { get; private set; }
    //모드별 소유하고 있는 세션 표시
    private Label _currentSessionLabel;
    // 데이터 소스들 (여기에 데이터가 있든 없든 상관없음)
    private MetricSession _liveSession; // 레코더 참조
    private MetricSession _slotA;       // 파일 로드 A

    

    // 생성자 대신 Initialize 메서드 사용 권장 (명시적 호출)
    public void Initialize(VisualElement root, Action<string> onShowWarning)
    {
        
        _onShowWarning = onShowWarning;
        _root = root;

        CreateSubController();
        BindInternalEvent();
        InitializeSubController();
        
        //===모드 드랍다운===
        _modDropdown = root.Q<DropdownField>("tuningmode-dropdown");
        var nameList = new List<string>();
        List<string> modNames = Enum.GetNames(typeof(TuningMode)).ToList();

        _currentSessionLabel = root.Q<Label>("current-session-label");
        
        _modDropdown.choices = modNames;
        _modDropdown.value = modNames[0];
        _modDropdown.RegisterValueChangedCallback(evt =>
        {
            SetMode((TuningMode)Enum.Parse(typeof(TuningMode), evt.newValue));
        });
        SetMode(TuningMode.Live);
        RefreshVisualization();
        
        
    }
    //생성자를 통한 우선 생성.
    private void CreateSubController()
    {
        _repository = new TuningRepository(new Type[] { 
            typeof(EngineModule),typeof(TransmissionModule),typeof(DrivetrainModule),typeof(ChassisModule) ,
            typeof(SteeringModule) , typeof(BrakeModule), typeof(SuspensionModule),typeof(RimModule),  typeof(TireModule)
        });
        
        Context = new TuningContext(_repository);
        
        _listController = new ModuleListController(Context);
        
        _pipelineController = new PipelineController();
        
        _simulationManager = UnityEngine.Object.FindFirstObjectByType<SimulationManager>();
        
        _renderingController = new RenderingViewController(_simulationManager, 
            onStart: ()=> _simulationManager.StartSimulation(),
            onStop: ()=> _simulationManager.StopSimulation(),
            onSpeedChanged: (f)=>_simulationManager.SetTimeScale(f),
            onTimelineChanged:(f) =>
            {
                var realTime = f * _replayController.TotalDuration;
                _replayController.ScrubTo(realTime);
                _metricController.SetFocusTime(realTime);
            });
        
        _replayController = new ReplayController(_simulationManager);
        _metricController = new MetricGraphController(onSave: () =>//save 버튼을 누르면요, 
            {
                _liveSession.VehicleProfile = Context.TargetCar.Profile;
                // 1. [UI] 먼저 저장할 경로와 파일명을 입력받습니다.
                // (데이터를 뽑기 전에 이름을 먼저 정해야 내부에 기록할 수 있으니까요)
                string path = UnityEditor.EditorUtility.SaveFilePanel(
                    "Save Recording Session",
                    Application.dataPath,
                    $"Session_{System.DateTime.Now:yyyyMMdd_HHmm}", // 기본 파일명
                    "json"
                );

                // 취소 눌렀으면 중단
                if (string.IsNullOrEmpty(path)) return;

                // 2. [핵심] 파일 경로에서 '확장자를 뺀 파일명'만 추출합니다.
                // 예: "C:/.../MyBestRun.json" -> "MyBestRun"
                string sessionName = System.IO.Path.GetFileNameWithoutExtension(path);

                // 3. Recorder에 있는 현재 세션 객체에 이름을 주입합니다.
                // (MetricRecorder에 CurrentSession 프로퍼티가 public으로 열려있다고 가정)
                _recorder.CurrentSession.sessionName = sessionName; 
        
                // 혹은 Recorder에 전용 메소드가 있다면:
                // _recorder.SetSessionName(sessionName);

                // 4. 이름이 업데이트된 상태의 세션을 JSON으로 변환
                var json = _recorder.GetSessionJson();

                // 5. 저장 수행
                _repository.SaveSession(path, json);
        
                Debug.Log($"세션 '{sessionName}' 저장 완료!");
                _onShowWarning($"세션 '{sessionName}' 저장 완료!");
            },
            onLoad: () =>
            {
                // 1. 파일 열기 패널 띄우기
                // 인자: (창 제목, 시작 폴더, 확장자 필터)
                string path = UnityEditor.EditorUtility.OpenFilePanel(
                    "Load Recording Session",
                    Application.dataPath,
                    "json"
                );
                // 취소 눌렀거나 경로가 비었으면 중단
                if (string.IsNullOrEmpty(path)) return;
                
                LoadFile(path, true);
            },
            onLoadB: () =>
            {
                string path = UnityEditor.EditorUtility.OpenFilePanel(
                    "Load Recording Session",
                    Application.dataPath,
                    "json"
                );
                // 취소 눌렀거나 경로가 비었으면 중단
                if (string.IsNullOrEmpty(path)) return;
                
                LoadFile(path, false);
            },
            onSlotClick: () =>
            {
                //시뮬레이션 내에서 카메라를 본체 한테 옮겨야함.
                //1. 조건 확인을 해야한다.
                if(!_replayController.IsSessionEnd(true)) _simulationManager.SetCameraToTarget(Context.TargetCar);
                else _onShowWarning("A 슬롯의 리플레이가 종료되어 카메라를 차량으로 이동할 수 없습니다.");
            },
            onSlotBClick: () =>
            {
                if(!_replayController.IsSessionEnd(false)) _simulationManager.SetCameraToCurrentGhost();
                else _onShowWarning("B 슬롯의 리플레이가 종료되어 카메라를 차량으로 이동할 수 없습니다.");
            });

        _subControllers = new List<ISubController>();
        //모든 서브 컨트롤러 리스트업
        _subControllers.Add(Context);
        _subControllers.Add(_repository);
        _subControllers.Add(_listController);
        _subControllers.Add(_pipelineController);
        //_subControllers.Add(_simulationManager);
        _subControllers.Add(_renderingController);
        _subControllers.Add(_replayController);
        _subControllers.Add(_metricController);
        
        foreach(var subController in _subControllers)
        {
            subController.OnFloatingWarning+= _onShowWarning;
        }
    }
    //이벤트 구독과정.
    private void BindInternalEvent()
    {
        Context.OnTargetCarChanged += OnTargetCarChanged; //-> 상위 객체(controller가 하위를 구독해서 행동 뿌려주기)
        Context.OnComparisonChanged += _listController.RebuildFullList;
        
        _listController.OnSaved += SaveToSO;
        _listController.OpenListPopup += OpenModuleListPopup;
        
        _simulationManager.OnSimulationStart += HandleSimulationStart;
        _simulationManager.OnSimulationStopped += HandleSimulationStop;
        
        _replayController.OnReplayStart += HandleReplayStart;
        _replayController.OnReplayEnd += HandleReplayEnd;
    }

    private void InitializeSubController()
    {
        
        // --- 비교 모듈들 생성 ---
        _listController.Initialize(_root);
        //listCon-simManager간 순서 의존성 존재!
        if (_simulationManager != null)
        {
            _simulationManager.Initialize(_repository.CarPrefabDatabase, _onShowWarning);
            _recorder = _simulationManager.MetricRecorder;
            // 2. 차량 선택 (여기서 Context.OnTargetCarChanged 이벤트 실행.
            if (_simulationManager.VehicleMap.Count > 0)
            {
                var firstCar = _simulationManager.VehicleMap[0];
                Context.SetTargetCar(firstCar); // <---  이벤트 발생 지점
                //이 메소드는 원래 차량 목록에서 차량 선택시 사용되어야 하지만 차량 바꾸는 기능이 없어서 초기화에서만 사용중이다.
            }
        }
        _pipelineController.Initialize(_root);
        _metricController.Initialize(_root);
        _renderingController.Initialize(_root);
    }
    private MetricSession _slotB; // 파일 로드 B
 
    private Func<bool> _checkIsBusy;
    private Action<string> _onShowWarning;

    // 최종적으로 그래프에 보여줄 리스트
    private List<MetricSession> _viewingSessions = new List<MetricSession>();
    private bool _isRealtime = false;

    public void OpenModuleListPopup(Type type, Vector2 screenPos)
    {
        // 1. DB에서 리스트 가져오기
        var db = _repository.GetMouModuleDatabase(type);
        if (db == null || db.Datas.Count == 0)
        {
            Debug.LogWarning($"[{type.Name}] 표시할 모듈 리스트가 없습니다.");
            return;
        }

        if (_cachedPicker == null)
        {
            _cachedPicker = ScriptableObject.CreateInstance<ModuleSearchPicker>();
        }

        // 2. 내용물만 갈아끼우기 (Re-Initialize)
        _cachedPicker.Init(
            title: $"Select {type.Name}",
            items: db.Datas.ToList(),
            onSelect: (selectedData) => 
            {
                Context.SetComparisonSO(type, selectedData);
            }
        );

        // 3. 띄우기
        SearchWindow.Open(new SearchWindowContext(screenPos), _cachedPicker);
        
    }
    
    
    public void StartAutoTest()
    {
        var car = Context.TargetCar as CarMainSystem;
        if (car == null) return;

        // 1. 기존 Agent 끄기 (충돌 방지)
        var agent = car.GetComponent<Unity.MLAgents.Agent>();
        if (agent != null) agent.enabled = false;

        // 2. 오토 파일럿 주입!
        car.SetTestMode(new AutoInputProvider());
    }
    public void SetMode(TuningMode mode)
    {
        //if (_currentMode == mode) return;
        
        _currentMode = mode;
        // [전략 패턴] 모드에 따라 "누구한테 물어볼지" 함수를 갈아끼움
        switch (mode)
        {
            case TuningMode.Live:
                // 라이브 모드일 땐 SimManager의 상태를 확인하도록 연결
                _checkIsBusy = () => _simulationManager.IsRunning;
                break;

            case TuningMode.Replay:
                _checkIsBusy = () => _replayController.IsReplaying;
                break;
            case TuningMode.Compare:
                // 리플레이 모드일 땐 ReplayManager의 상태를 확인하도록 연결
                // (일시정지가 아니면 바쁜 것)
                _checkIsBusy = () => _replayController.IsReplaying;
                //B slot을 load 오픈 해야함.
                break;
        }

        // 모드 바뀌었으니 UI 갱신
        RefreshUI();

        RefreshVisualization(); // "상황이 바뀌었으니 다시 계산해"
    }
    
    
// [B] 파일 로드: 데이터만 채우고 갱신 요청
    public void LoadFile(string path, bool isSlotA)
    {
        var session = _repository.LoadSession(path);
        if (session == null)
        {
            _onShowWarning("불러오려는 세션이 손상되었습니다.");
            return;
        }

        if (isSlotA)
        {
            _slotA = session;
            _renderingController.SetLoadedSpawn(session.SimName);//-> 본체를 보낸다. 그런데 로직상 조금 엉킴
        }
        else
        {
            _slotB = session;
            _simulationManager.SelectGhostVehicleByLoad(session.VehicleProfile);

            var nameList = _simulationManager.SimulationEnvs.Select(x => x.Name).ToList();
            var index = nameList.IndexOf(session.SimName);
            _simulationManager.SpawnToSimulation(index, true);

        }

        // 로드 직후 사용자 편의를 위해 모드를 자동으로 바꿔줄 수도 있음
        if (isSlotA && _currentMode == TuningMode.Live)
        {
            _currentMode = TuningMode.Replay;
            _modDropdown.value = "Replay";
        }
        //세션의 트랙 이름을 확인해서 차량을 이동시켜야함.
        Debug.Log("simName : "+ session.SimName);
       

        RefreshVisualization(); // "데이터가 들어왔으니 다시 계산해"
        RefreshUI();
    }
    
    // [C] 데이터 지우기 (슬롯 비우기)
    public void ClearSlot(bool isSlotA)
    {
        if (isSlotA) _slotA = null;
        else _slotB = null;
        
        RefreshVisualization();
    }

    // --- 3. [핵심] 중앙 갱신 로직 (The Reconciler) ---
    // 모드와 데이터 상태를 보고 최종 리스트를 결정하는 유일한 곳
    private void RefreshVisualization()
    {
        _viewingSessions.Clear();
        _isRealtime = false;

        _liveSession = _recorder.CurrentSession;//이게 null?

        switch (_currentMode)
        {
            case TuningMode.Live:
                // 라이브 세션이 있으면 추가
                if (_liveSession != null) _viewingSessions.Add(_liveSession);
                Debug.Log("Live Mod Current Session " + _viewingSessions.Count);
               
                _isRealtime = true;
                
                _renderingController.SetModeLayout(
                    _currentMode,
                    start: () => _simulationManager.StartSimulation(), // Start
                    stop: () => _simulationManager.StopSimulation()   // Stop
                );
                break;

            case TuningMode.Replay:
                // 슬롯 A에 데이터가 있으면 보여줌 (없으면 빈 화면이 됨)
                if (_slotA != null) _viewingSessions.Add(_slotA);
                _renderingController.SetModeLayout(
                    _currentMode,
                    start: () =>
                    {   
                        Debug.Log("ReplayStart");
                        _replayController.EnterReplayMode(_slotA, _slotB);
                   
                    },
                    stop : () =>
                    {   
                        Debug.Log("replay stop");
                        _replayController.ExitReplayMode();
                 
                    });
                
                break;

            case TuningMode.Compare:
                // 슬롯 A, B에 있는 대로 다 보여줌
                // (하나만 있어도 하나만 보여주고 에러 안 남)
                if (_slotA != null) _viewingSessions.Add(_slotA);
                if (_slotB != null) _viewingSessions.Add(_slotB);
                _renderingController.SetModeLayout(
                    _currentMode,
                    start: () =>
                    {   
                        Debug.Log("ReplayStart");
                        _replayController.EnterReplayMode(_slotA, _slotB);
                   
                    },
                    stop : () =>
                    {   
                        Debug.Log("replay stop");
                        _replayController.ExitReplayMode();
                 
                    });
                break;
        }

        // [최종 전달] 그래프 컨트롤러는 모드를 모름. 그냥 리스트만 받음.
        _metricController.BindSessions(_viewingSessions);
        // (선택사항) UI 상태 갱신 (버튼 활성화 등)
        // UpdateUIState();
    }

 

    public void Update(float dt)
    {
        if (_currentMode == TuningMode.Live)
        {
            //_graphController.Tick();
            _recorder.Tick(dt* _renderingController.CurrentSpeed);
            _metricController.SetFocusTime(_recorder.CurrentTime); 
            _pipelineController.Update();
        }
        else
        {
            var currentDt = 1f;
            // [리플레이] 재생 중이라면 시간 흐름 처리

            if (_replayController.IsReplaying && !_replayController.IsPaused)
            {
                currentDt = dt * _renderingController.CurrentSpeed;
                // [동기화 핵심] 
                // 1. 차량 위치 이동
               
                //카메라 위치 컨트롤.
                var trackAFinished = _replayController.IsSessionEnd(true);
                var trackBFinished = _replayController.IsSessionEnd(false);
                
                if (!_replayController.IsTrackAFinished && trackAFinished)
                {
                    
                    _simulationManager.SetCameraToCurrentGhost();
                    _replayController.IsTrackAFinished = true;
                    _onShowWarning("A 슬롯의 리플레이가 종료되어 카메라를 유령 차량으로 이동합니다.");
                }
                else if (!_replayController.IsTrackBFinished && trackBFinished)
                {
                    _simulationManager.SetCameraToTarget(Context.TargetCar);
                    _replayController.IsTrackBFinished = true;
                    _onShowWarning("B 슬롯의 리플레이가 종료되어 카메라를 본체 차량으로 이동합니다.");
                }
                if (trackAFinished) _replayController.IsTrackAFinished= false;
                if (trackBFinished) _replayController.IsTrackBFinished = false;
       
                if(_replayController.IsTrackAFinished )
                // 2. 그래프 커서 이동 (시간 동기화)
                _metricController.SetFocusTime(_replayController.CurrentTime);

                //무한루프를 방지하기 위한 로직.
                //1. 슬라이더를 조작하고 있는 상황에서는 값을 갱신 시켜주면 안된다.
                //1-1. 슬라이더 조작 상태를 어떻게 인식할 것인가.
                _renderingController.TimelineSlider.value = _replayController.CurrentTime / _replayController.TotalDuration;
                
                _replayController.Tick(currentDt);
              
            }
        }
    }

    
    
    public void Dispose()
    {
        if (_targetCamera != null) _targetCamera.targetTexture = null;
        if (_renderTexture != null) _renderTexture.Release();
        _listController.OnDispose();
    }

   
    private void RefreshUI()
    {
        // 컨트롤러는 현재 모드가 뭔지 알 필요 없음.
        // 그냥 등록된 함수(_checkIsBusy)를 실행해서 결과를 받아옴.
        bool isBusy = _checkIsBusy != null ? _checkIsBusy() : false;
        
        UpdateUIState(isBusy, _currentMode);
       _renderingController.UpdateUIState(isBusy, _currentMode);
       _metricController.UpdateUIState(isBusy, _currentMode);
       
       //세션 UI 변경해주기.
       if (_currentMode == TuningMode.Live)
       {
           if (_liveSession != null && !_liveSession.IsEmpty()) _currentSessionLabel.text = "current session : " + "live Session" + "<Recorded>";
           else _currentSessionLabel.text = "current session : " + "live Session" + "<Empty>";
       }
       else
       {
           if (_slotA != null) _currentSessionLabel.text = "current session : "+"loaded Session <" + _slotA.sessionName + ">";
           else _currentSessionLabel.text = "current session : "+"non loaded data";
       }
    }

    private void UpdateUIState(bool isBusy, TuningMode currentMod)
    {
        _modDropdown.SetEnabled(!isBusy);
    }

    #region Callback
    //시작 함수는 매니저에 정의후 버튼에 직접 전달.
    private void HandleSimulationStart()//단순히 함수 이어주기. -> sim Manager와 연결
    {
        _simulationManager.SetCarPhysic(true);
   
        _liveSession = _recorder.CurrentSession;//이때 이미 레코더의 새로운 세션이 생성된 상태,.
        
        _liveSession.SimName = _simulationManager.CurrentSimEnv.Name;
        _viewingSessions.Clear();
        _viewingSessions.Add(_liveSession);
        _metricController.BindSessions(_viewingSessions);
        RefreshUI();
        //_metricController.BindSessions(_viewingSessions);//현재 세션과 그래프 묶어주기.
    }
    private void HandleSimulationStop()//-> sim Manager와 연결
    {
        //저장을 위한 기능을 여기로 분리
        _simulationManager.SetCarPhysic(false);// 여전히 움직일 필요는 전혀 없음. 시뮬 시작시만 on
        _simulationManager.SpawnToCurrentSpawn(false);
        
        RefreshUI();
    }

    private void HandleReplayStart()
    {
        _simulationManager.SetCarPhysic(false);
        RefreshUI();
    }

    
    private void HandleReplayEnd()
    {
        
        Debug.Log("Context.TargetCar : "+ Context.TargetCar.CachedGameObject.transform.position);
        _simulationManager.SetCarPhysic(false);// 여전히 움직일 필요는 전혀 없음. 시뮬 시작시만 on
        _simulationManager.SpawnToCurrentSpawn(false);
        Debug.Log("Context.TargetCar : "+ Context.TargetCar.CachedGameObject.transform.position);
        if(_currentMode == TuningMode.Compare) _simulationManager.SpawnToCurrentSpawn(true);
        _simulationManager.SetCameraToTarget(Context.TargetCar );
        
        
        
        RefreshUI();
    }
    
    //저장을 위한 콜백함수
    public void SaveToSO(Type t, string name)//save 버튼
    {
        var liveModule = Context.GetLiveData(t); // 저장 대상.
        _repository.CreateNewModule(liveModule, name);
        RefreshUI();
    }
    
    //타겟 차량이 바뀌었을 때 콜백 함수
    public void OnTargetCarChanged()
    {
        var car = Context.TargetCar;
        //_graphController.OnSetTargetCar(car);
        _listController.RebuildFullList();
        _pipelineController.SetTarget(car);
    }
    
    

    #endregion
    
}