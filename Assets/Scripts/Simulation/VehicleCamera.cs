using System.Collections.Generic;
using UnityEngine;

public class VehicleCamera : MonoBehaviour
{
    [System.Serializable]
    public class CameraViewPreset
    {
        public string viewName;
        public Vector3 offset;       // 차량 기준 카메라 위치 (예: 뒤쪽 위 0, 2, -5)
        public Vector3 lookAtOffset; // 차량의 어디를 볼 것인가 (예: 차체 중심 0, 0.5, 0)
        public float smoothTime = 0.1f; // 따라가는 지연 시간 (낮을수록 빠릿함)
        public float fieldOfView = 60f; // 기본 FOV
    }

    [Header("Target Settings")]
    [SerializeField] private Transform _target;
    [SerializeField] private Rigidbody _targetRb;

    [Header("View Settings")]
    public List<CameraViewPreset> presets = new List<CameraViewPreset>();
    public int currentViewIndex = 0;

    [Header("Dynamic Effects")]
    public bool enableDynamicFOV = true;
    public float minFOV = 60f;
    public float maxFOV = 80f;
    public float speedForMaxFOV = 200f; // 이 속도(km/h 등)일 때 최대 FOV

    // 내부 연산용 변수
    private Camera _cam;
    private Vector3 _currentVelocity;
    private Vector3 _desiredPosition;
    private float _defaultSmoothTime = 0.1f;

    private void Awake()
    {
        _cam = GetComponent<Camera>();
        
        // 프리셋이 없으면 기본값 하나 추가 (에러 방지)
        if (presets.Count == 0)
        {
            presets.Add(new CameraViewPreset 
            { 
                viewName = "Default", 
                offset = new Vector3(0, 3, -6), 
                lookAtOffset = Vector3.zero 
            });
        }
    }

    // 1. 타겟 설정 (외부에서 호출)
    public void SetTarget(Transform targetTransform, Rigidbody targetRigidbody)
    {
        _target = targetTransform;
        _targetRb = targetRigidbody;

        // 타겟이 바뀌면 카메라를 즉시 해당 위치로 이동 (텔레포트)하여 튀는 현상 방지
        if (_target != null)
        {
            ApplyViewImmediate();
        }
    }

    // 2. 뷰 변경 명령 (외부에서 호출)
    public void ChangeView(int index)
    {
        if (index >= 0 && index < presets.Count)
        {
            currentViewIndex = index;
        }
    }

    // 3. 다음 뷰로 순환 (편의 기능)
    public void NextView()
    {
        currentViewIndex = (currentViewIndex + 1) % presets.Count;
    }

    // 물리 연산이 끝난 후 카메라가 따라가야 덜덜거리지 않음
    private void LateUpdate()
    {
        if (_target == null) return;

        var preset = presets[currentViewIndex];

        // --- 위치 이동 (Follow) ---
        // 타겟의 회전을 반영한 월드 좌표 계산
        // TransformPoint: 로컬 좌표(Offset)를 월드 좌표로 변환
        Vector3 targetWorldPos = _target.TransformPoint(preset.offset);

        // SmoothDamp로 부드럽게 이동
        transform.position = Vector3.SmoothDamp(
            transform.position, 
            targetWorldPos, 
            ref _currentVelocity, 
            preset.smoothTime
        );

        // --- 회전 (Look At) ---
        // 차량의 특정 지점을 바라봄
        Vector3 lookTarget = _target.TransformPoint(preset.lookAtOffset);
        transform.LookAt(lookTarget);

        // --- 다이나믹 FOV (속도감) ---
        if (enableDynamicFOV && _targetRb != null)
        {
            // 속도(m/s) -> km/h 변환 (약 3.6 곱함)
            float speed = _targetRb.linearVelocity.magnitude * 3.6f;
            float targetFOV = Mathf.Lerp(preset.fieldOfView, maxFOV, speed / speedForMaxFOV);
            
            // FOV도 부드럽게 변경
            _cam.fieldOfView = Mathf.Lerp(_cam.fieldOfView, targetFOV, Time.deltaTime * 2f);
        }
        else
        {
            _cam.fieldOfView = Mathf.Lerp(_cam.fieldOfView, preset.fieldOfView, Time.deltaTime * 5f);
        }
    }

    // 타겟 변경 시 즉시 이동하는 헬퍼
    private void ApplyViewImmediate()
    {
        var preset = presets[currentViewIndex];
        transform.position = _target.TransformPoint(preset.offset);
        transform.LookAt(_target.TransformPoint(preset.lookAtOffset));
        _currentVelocity = Vector3.zero;
    }

    // 에디터에서 뷰 위치 미리보기 (선택사항)
    private void OnDrawGizmosSelected()
    {
        if (_target == null) return;

        Gizmos.color = Color.cyan;
        foreach (var preset in presets)
        {
            Vector3 pos = _target.TransformPoint(preset.offset);
            Vector3 look = _target.TransformPoint(preset.lookAtOffset);
            
            Gizmos.DrawWireSphere(pos, 0.5f);
            Gizmos.DrawLine(pos, look);
        }
    }
}