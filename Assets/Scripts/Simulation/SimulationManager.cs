using System;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class SimulationManager: MonoBehaviour
{
    private const float DEFAULT_FIXED_TIMESTEP = 0.02f;

    public event Action OnSimulationStart;
    public event Action OnSimulationStopped;
    [SerializeField]
    private Transform[] _vehicleSpawnPoints;
    [SerializeField]
    private SimulationEnvironment[] _simEnvs;
    [SerializeField]
    private VehicleCamera _camera;

    [SerializeField]
    private Transform _ghostHome;
    [SerializeField]

    
    private List<ITunableVehicle> _activeVehicles = new List<ITunableVehicle>();
    private Dictionary<int, ITunableVehicle> _vehicleMap = new Dictionary<int, ITunableVehicle>();
    
    private List<GameObject> _ghostVehicles = new List<GameObject>();

    private Dictionary<ITunableVehicle, GameObject> _ghostVehicleByITunable = new Dictionary<ITunableVehicle, GameObject>();

    private MetricRecorder _metricRecorder;

    private Rigidbody _mainVehicleRb; // 실제 차량
    private Transform _currentGhostVehicle; // 비교용 반투명 차량 (Ghost)

    private SimulationEnvironment _currentSimEnv;

    private Action<string> _onShowWarning;

    public SimulationEnvironment CurrentSimEnv => _currentSimEnv;
 
    public Dictionary<int, ITunableVehicle> VehicleMap => _vehicleMap; 

    public SimulationEnvironment[] SimulationEnvs => _simEnvs;
    // 현재 '주인공' 혹은 '포커스' 된 차량 (튜닝 툴이 바라볼 대상)
    public ITunableVehicle FocusedVehicle { get; private set; }

    private Transform _focusedVehicleTransform;

    public Transform vehicleTransform => FocusedVehicle.CachedGameObject.transform;

    public Transform GhostVehicle => _currentGhostVehicle;

    
    public bool IsRunning { get; private set; }
    public float ElapsedTime { get; private set; }

    public MetricRecorder MetricRecorder => _metricRecorder;
    public VehicleCamera Camera => _camera;

    private void Awake()
    {
       
    }
    public void Initialize(CarPrefabDatabase db, Action<string> onShowWarning)
    {
        
        _onShowWarning = onShowWarning;
        //레코더 우선 생성
        _metricRecorder = new MetricRecorder();

        int i = 0;
        foreach (var vehiclePrefab in db.Datas)
        {
            if (i >= _vehicleSpawnPoints.Length)
            {
                Debug.LogWarning("There are more vehicles than spawn points. Some vehicles will not be spawned.");
                break;
            }

            Transform spawnPoint = _vehicleSpawnPoints[i];
            GameObject vehicleInstance = Instantiate(vehiclePrefab, spawnPoint.position, spawnPoint.rotation, transform);

            if (vehicleInstance.TryGetComponent<ITunableVehicle>(out var tunableVehicle))
            {
                _activeVehicles.Add(tunableVehicle);
                if (tunableVehicle.Profile != null && !_vehicleMap.ContainsKey(tunableVehicle.Profile.ID))
                {
                    _vehicleMap.Add(tunableVehicle.Profile.ID, tunableVehicle);
                }
            }
            //작동하지 않게 차량 초기화.
            var mainsys = vehicleInstance.GetComponent<CarMainSystem>();
            mainsys.isSimulation = true;
            mainsys.Setup();
            SetCarPhysic(tunableVehicle);
            //=== 고스트 차량 등록 === (인덱스 기반으로 관리)
            var ghost = Instantiate(tunableVehicle.GhostVehicle);
            ghost.transform.position = _ghostHome.position;
            ghost.transform.rotation = _ghostHome.rotation;
            _ghostVehicles.Add(ghost);
            _ghostVehicleByITunable.Add(tunableVehicle,ghost);
            ghost.SetActive(false);
            
            i++;
        }
        SelectCar(0);
        


    }
    public void SelectCar(int id)
    {
        FocusedVehicle = _vehicleMap[id];
        _focusedVehicleTransform = FocusedVehicle.CachedGameObject.transform;
        _metricRecorder.ChangeCar(FocusedVehicle);
        
        // 카메라에게 "이 차를 찍어라" 명령
        SetCameraToTarget(FocusedVehicle);

        //대기위치로 이동(simEnv 0 위치)
        SpawnToSimulation(0,false);
    }

    public void SelectGhostVehicleByLoad(VehicleProfile profile)
    {
        var car = _activeVehicles.Find(x => x.Profile.ID == profile.ID);
        _currentGhostVehicle = _ghostVehicleByITunable[car].transform;
        //_currentGhostVehicle.gameObject.SetActive(true);
    }

    public void StartSimulation()
    {
        //1. 차량을 시뮬레이션 위치에 이동시킨다.
        //2. 차량의 잠겨있던 컴포넌트를 해제한다.
        //3.실행?
        if (!_currentSimEnv.IsSimualtion)
        {
            Debug.LogWarning("현재 위치에선 시뮬레이션을 실행 할 수 없습니다");
            _onShowWarning("현재 위치에선 시뮬레이션을 실행 할 수 없습니다");
            return;
        }
        else if (_currentSimEnv == null)
        {
            //시뮬레이션 장소 배정...
            return;
        }
        
        //SetCarPhysic(true);
        _metricRecorder.StartRecording();
        IsRunning = true;
        
        OnSimulationStart?.Invoke();
    }

    public void StopSimulation()
    {
        //차량 잠구기 후 기존 자리로 이동
        //이벤트 호출
        _metricRecorder.StopRecording();
        IsRunning = false;
        OnSimulationStopped?.Invoke();
    }
    
    public void TeleportMainVehicle(Vector3 pos, Quaternion rot)
    {
        
        _focusedVehicleTransform.position = pos;
        //Debug.Log("car pos : "  + vehicleTransform.position+ " , target pos : "+ pos + "Real Position : " + FocusedVehicle.CachedGameObject.transform.position);
        _focusedVehicleTransform.rotation = rot;
    }

    public void TeleportGhostVehicle(Vector3 pos, Quaternion rot)
    {
        _currentGhostVehicle.position = pos;
        //Debug.Log("car pos : "  + vehicleTransform.position+ " , target pos : "+ pos + "Real Position : " + FocusedVehicle.CachedGameObject.transform.position);
        _currentGhostVehicle.rotation = rot;
    }

    //차량을 시뮬레이션 위치에 이동시킨다. 현재 시뮬레이션 값을 변경시킨다.
    public void SpawnToSimulation(int index, bool isGhost)//인덱스 혹은 이름.
    {
        if (IsRunning) return;
        _currentSimEnv = _simEnvs[index];
        if (isGhost && _currentGhostVehicle != null)
        {
            _currentGhostVehicle.position = _currentSimEnv.Spawn.position;
            _currentGhostVehicle.rotation =  _currentSimEnv.Spawn.rotation;
            return;
        }
   
        if( _focusedVehicleTransform == null)
        {
            Debug.LogError("Focused Vehicle is null. Cannot spawn.");
            return;
        }
        _focusedVehicleTransform.position =   _currentSimEnv.Spawn.position;
        _focusedVehicleTransform.rotation =   _currentSimEnv.Spawn.rotation;
    }
    public void SpawnToCurrentSpawn(bool isGhost)
    {
        if (isGhost)
        {
            _currentGhostVehicle.position = _currentSimEnv.Spawn.position;
            _currentGhostVehicle.rotation =  _currentSimEnv.Spawn.rotation;
            return;
        }
        _focusedVehicleTransform.position =   _currentSimEnv.Spawn.position;
        _focusedVehicleTransform.rotation =  _currentSimEnv.Spawn.rotation;
    }

    public void SetCameraToTarget(ITunableVehicle vehicle)
    {
        if (_camera != null && vehicle.CachedGameObject != null)
        {
            Debug.Log("camera test");
            var rb = FocusedVehicle.CachedGameObject.GetComponent<Rigidbody>();
            _camera.SetTarget(FocusedVehicle.CachedGameObject.transform, rb);
        }
    }

    public void SetCameraToCurrentGhost()
    {
        if (_camera != null && _currentGhostVehicle != null)
        {
            var rb = _currentGhostVehicle.GetComponent<Rigidbody>();
            _camera.SetTarget(_currentGhostVehicle, rb);
        }
    }
    
    

    public void SetTimeScale(float scale)
    {
        // 0보다 작아지면 멈춤 혹은 에러가 나므로 방어
        scale = Mathf.Max(0f, scale);

        // 1. 시간 속도 변경
        //Time.timeScale = scale;

        // 2. [핵심] 물리 연산 주기도 같은 비율로 변경해야 슬로우 모션에서도 부드러움
        // (단, 0일 때는 0으로 나누기 오류가 날 수 있으니 체크)
        // if (scale > 0)
        // {
        //     Time.fixedDeltaTime = DEFAULT_FIXED_TIMESTEP * scale;
        // }    
        
    }

    public void SetCarPhysic(bool isPhysics)
    {
        var car = FocusedVehicle.CachedGameObject;
        if (car != null)
        {
            car.GetComponent<CarMainSystem>().enabled = isPhysics;
            car.GetComponent<CarController>().enabled = isPhysics;
            var agent = car.GetComponent<CarAgent>();
            agent.EndEpisode();
            agent.enabled = isPhysics;
            
            var rb = car.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = !isPhysics;
               
            }
            if (isPhysics)
            {
                rb.interpolation = RigidbodyInterpolation.Interpolate;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;  
            }
            else
            {
                rb.interpolation = RigidbodyInterpolation.None;
            }
        }
        var carSys = FocusedVehicle as CarMainSystem;
        //차량 수치 상태 초기화.
        carSys?.ReStart();
    }
    //Setup시 사용하기 위한 임시 함수
    private void SetCarPhysic(ITunableVehicle vehicle)
    {
        FocusedVehicle = vehicle;
        SetCarPhysic(false);
        FocusedVehicle = null;
    }
    

}