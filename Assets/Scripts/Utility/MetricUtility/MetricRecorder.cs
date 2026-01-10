using Newtonsoft.Json;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public class MetricRecorder
{
    private ITunableVehicle _vehicle;
    private IMetricHub _metricHub;

    private Transform _vehicleTransform;
    
    private float _samplingInterval = 0.1f;
    
    private float _sampleTimer;

    private MetricSession _currentSession;
    private bool _isRecording;
    private float _totalTime = 0f;

    public float CurrentTime => _totalTime;
    
    public MetricRecorder(float interval = 0.1f)
    {
        _samplingInterval = interval;
    }
    
    public MetricSession CurrentSession => _currentSession;
    
    
    public void ChangeCar(ITunableVehicle vehicle)
    {
        _vehicle = vehicle;
        _vehicleTransform = vehicle.CachedGameObject.transform;
        // 차량의 Context 등을 통해 Hub 연결
        // (구조에 따라 vehicle.Context.MetricHub 등으로 접근)
        _metricHub = vehicle.Context.MetricHub; 
    }
    //새로운 세션을 생성해 메트릭 허브에게 전달
    private void PrepareSession()
    {
        if (_vehicle == null || _metricHub == null) return;
        _currentSession = new MetricSession();
        
        // 메타 데이터 저장 (선택 사항)
        
        _currentSession.StartTime = System.DateTime.Now.ToString();

        // ★ 허브야, 이 세션에 빨대(Func) 좀 꽂아줘
        _metricHub.RegisterSession(_currentSession);
    }
    public void StartRecording()
    {
        if (_vehicle == null) return;
   
        PrepareSession();
        _sampleTimer = 0f;
        _totalTime = 0f; // 시간 초기화
        _isRecording = true;
        
    }
    
    // [핵심] 외부 Update에서 호출될 함수
   
    public void Tick(float dt)
    {
        if (!_isRecording || _currentSession == null) return;

        _sampleTimer += dt;
        _totalTime += dt; // 전체 시간 누적

        // 샘플링 주기 도달 시
        if (_sampleTimer >= _samplingInterval)
        {
            // [핵심] 현재 누적 시간을 넘겨줌
            _currentSession.CaptureAll(_totalTime);
            
            //트랜스폼 데이터는 수집 메트릭과 다르게 공통값이므로 다른 방법으로 측정한다.
            var pose = new VehiclePose(
                _totalTime, 
                _vehicleTransform.position, 
                _vehicleTransform.rotation
            );
            CurrentSession.trackData.Add(pose);

            _sampleTimer = 0f; 
        }
    }
    public void StopRecording()
    {
        if (!_isRecording) return;

        _isRecording = false;
        Debug.Log($"[Recorder] Recording Stopped. Total Frames: {_currentSession.metricEntries[0].Values.Count}"); // 예시

        // ★ 여기서 파일 저장 로직 호출
        //SaveToFile();
    }

    private void SaveToFile()
    {
        if (_currentSession == null) return;

        // Newtonsoft.Json을 이용해 저장 (Func는 [JsonIgnore] 처리 되어있어야 함)
        string json = JsonConvert.SerializeObject(_currentSession, Formatting.Indented);
        
        // 경로 설정 (Assets 폴더 밑이나 PersistentDataPath 등)
        string fileName = $"Session_{System.DateTime.Now:yyyyMMdd_HHmmss}.json";
        string path = Path.Combine(Application.dataPath, "Recordings", fileName);

        // 폴더 없으면 생성
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        
        File.WriteAllText(path, json);
        Debug.Log($"[Recorder] Saved to: {path}");
        
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh(); // 에디터에서 파일 바로 보이게
#endif
    }

    public string GetSessionJson()
    {
        if (_currentSession == null) return null;

        // Newtonsoft.Json을 이용해 저장 (Func는 [JsonIgnore] 처리 되어있어야 함)
        // 직렬화 구조는 계층적일 것임.
        string json = JsonConvert.SerializeObject(_currentSession, Formatting.Indented);
        return json;
    }

}