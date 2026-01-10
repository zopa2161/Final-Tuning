using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class CarMainSystem : MonoBehaviour,ITunableVehicle
{
    //===GhostVehicle 세팅===
    [SerializeField]
    private GameObject _ghostVehicle;

    public GameObject GhostVehicle => _ghostVehicle;//접근제한을 어케함.
    //===Test를 위한 세팅===
    public bool isTestSetting =false;
    //===Simulation을 위한 세팅
    public bool isSimulation = false;

    private IInputProvider _currentInputProvider;
    
    [SerializeField]
    private VehicleProfile _profile;
    [Header("Modules")]
    [SerializeField]
    private EngineModule _engine;
    [SerializeField]
    private SuspensionModule _suspension;
    [SerializeField]
    private BrakeModule _brake;
    [SerializeField]
    private TransmissionModule _transmission;
    [SerializeField]
    private RimModule _rim;
    [SerializeField]
    private TireModule _tire;
    [SerializeField]
    private ChassisModule _chassis;
    [SerializeField]
    private DrivetrainModule _drivertain;
    [SerializeField]
    private SteeringModule _steering;
    
    private CarController _carController;
    private InputManager _inputManager;

    private CarAgent _carAgent;
    private AgentManager _agentManager;

    private List<ModuleWrapper> _wrappers = new List<ModuleWrapper>();
    private VehicleContext _currentContext = new VehicleContext();
    private Dictionary<Type, BaseModule> _moduleMap = new Dictionary<Type, BaseModule>();
    private Dictionary<Type, BaseModule> _coppiedModuleMap = new Dictionary<Type, BaseModule>();
    
    //===그 외 ===
    private Rigidbody _rb;
    
    public VehicleContext GetCurrentContext() => _currentContext;
    
    public CarAgent CarAgent => _carAgent;
    public AgentManager AgentManager => _agentManager;
    public VehicleContext Context => _currentContext;

    public GameObject CachedGameObject => gameObject;
    public BaseModule GetModuleData(Type type) => _moduleMap[type];

    public InputManager InputManager => _inputManager;

    public VehicleProfile Profile => _profile;

    public IReadOnlyDictionary<Type, BaseModule> ModuleMap => _coppiedModuleMap;
    
    //===TEST용 외부 디스플레이 프로퍼티===
    [ShowInInspector]
    public float Steering => _currentContext.SteeringAngle;
    [ShowInInspector]
    public float Throttle => _currentContext.Throttle;
    [ShowInInspector]
    public float Brake => _currentContext.Brake;
    

    public void Setup()
    {
        SetupModules();
        _carController = GetComponent<CarController>();
        _carController.Setup(this, _currentContext);
        _inputManager = new InputManager();
        _inputManager.setup(this);
        _agentManager = GetComponent<AgentManager>();
        _agentManager.Setup(_currentContext);
        
    
        _carAgent = GetComponent<CarAgent>();
        _carAgent.Setup(this, _agentManager);
        _carAgent.OnEpisodeReset += () => ReStart();
    }
    public void ReStart()//값 초기화
    {
        _currentContext.ResetEpisode();// 상태값, 관찰값 전부 초기화
    }

    
    private void SetupModules()
    {
        _currentContext.Setup();
        //---순서 중요!---
        CreateWrappers(_engine);
        CreateWrappers(_transmission);
        CreateWrappers(_drivertain);
        CreateWrappers(_brake);
        CreateWrappers(_suspension);
        CreateWrappers(_rim);
        CreateWrappers(_tire);
        CreateWrappers(_chassis);
        CreateWrappers(_steering);

        foreach (var wrapper in _wrappers)
        {
            wrapper.Initialize(_currentContext.StateHub);
        }
      
    }
    
    
    private void CreateWrappers(BaseModule module)
    {
        if (module != null && !_moduleMap.ContainsKey(module.GetType()))
        {
            _moduleMap.Add(module.GetType(), module);
        }

        BaseModule moduleForWrapper;

        if (module != null)
        {
            // If module is not null, create a copy.
            BaseModule copiedModule = Instantiate(module);
            moduleForWrapper = copiedModule;

            // And add it to the _coppiedModuleMap.
            Type moduleType = module.GetType();
            if (!_coppiedModuleMap.ContainsKey(moduleType))
            {
                _coppiedModuleMap.Add(moduleType, copiedModule);
            }
            else
            {
                // Overwrite if called again for the same type.
                _coppiedModuleMap[moduleType] = copiedModule;
            }
        }
        else
        {
            // If module is null, behave as before.
            moduleForWrapper = null;
        }
        
        ModuleWrapper wrapper = new ModuleWrapper();
        wrapper.Initialize(moduleForWrapper); // Use the copy or null.
        _wrappers.Add(wrapper);
        _currentContext.StateHub.RegisterState(wrapper.State);
    }
    public void SetTestMode(IInputProvider testProvider)
    {
        _currentInputProvider = testProvider;
        isTestSetting = true; // Agent 무시
    }

   
    public void RunInputProvide()
    {
        if (isTestSetting)
        {   float[] inputs = _currentInputProvider.GetInput();
            _inputManager.Step(inputs);
        }
    }
    
    public void RunCalculateModule()
    {
        
        foreach (var wrapper in _wrappers)
        {
            wrapper.RunCalculation(_currentContext);
        }
    }
    
    public void TeleportTo(Transform targetTransform)
    {
        if (_rb == null) _rb = GetComponent<Rigidbody>();

        // 1. 보간 끄기 (렌더링이 과거 위치에 남는 원인 제거)
        var originalInterpolation = _rb.interpolation;
        _rb.interpolation = RigidbodyInterpolation.None;

        // 2. 물리 끄기 (충돌 계산 방지)
        _rb.isKinematic = true;

        // 3. 위치 이동 (Transform과 Rigidbody 둘 다)
        transform.position = targetTransform.position;
        transform.rotation = targetTransform.rotation;
        _rb.position = targetTransform.position; // 확실하게 RB 위치도 갱신
        _rb.rotation = targetTransform.rotation;

       
        // 5. 물리 엔진 강제 동기화 (트랜스폼 <-> 물리 좌표 일치시킴)
        Physics.SyncTransforms();

        // 6. 물리 다시 켜기
        _rb.isKinematic = false;
        // 4. 관성 제거
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;

        _rb.interpolation = originalInterpolation; // 원래 설정 복구
    }
    
    
}
