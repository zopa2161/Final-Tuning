using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ReplayController : ISubController
{
    public event Action OnReplayStart;
    public event Action OnReplayEnd;
    
    private SimulationManager _simManager;
    
    // 재생할 데이터
    private List<VehiclePose> _trackA;
    private List<VehiclePose> _trackB; // 비교용 (Ghost)

    public float CurrentTime { get; private set; } // 그래프 등 외부에서 읽기용
    public float TotalDuration { get; private set; }
    public bool IsReplaying { get; private set; } // 모드 자체가 리플레이인가?
    public bool IsPaused { get; private set; } = false; // 일시정지 상태인가?
    
    public ReplayController(SimulationManager manager)
    {
        _simManager = manager;
    }

    public void Initialize(VisualElement root) { }

    // 세션 로드 및 리플레이 모드 진입
    public void EnterReplayMode(MetricSession sessionA, MetricSession sessionB)
    {
        IsReplaying = true;
        IsPaused = false;
        _trackA = sessionA?.trackData;
        _trackB = sessionB?.trackData;

        // 전체 길이 계산 (둘 중 긴 것 기준)
        float maxA = (_trackA != null && _trackA.Count > 0) ? _trackA[_trackA.Count - 1].time : 0f;
        float maxB = (_trackB != null && _trackB.Count > 0) ? _trackB[_trackB.Count - 1].time : 0f;
        TotalDuration = Mathf.Max(maxA, maxB);

        CurrentTime = 0f;
       
        //고스트 비클 셋업
        if (sessionB != null)
        {
            _simManager.GhostVehicle.gameObject?.SetActive(true);
            
        }

        // 0초 위치로 이동
        Step(0f);
        OnReplayStart?.Invoke();
    }

    public void ExitReplayMode()
    {
        IsReplaying = false;
        IsPaused = false;
        _trackA = null;
        _trackB = null;

        // SimManager에게 물리 켜라고 명령
        //_simManager.SetPhysicsState(true);
        
        OnReplayEnd?.Invoke();
    }

    // 재생/일시정지 토글
    public void TogglePlay()
    {
        IsPaused = !IsPaused;
    }

    // 슬라이더로 시간 강제 이동 (Scrubbing)
    public void ScrubTo(float time)
    {
        CurrentTime = Mathf.Clamp(time, 0, TotalDuration);
        Step(0f); // 시간 이동 후 즉시 반영
    }

    // [핵심] 외부(TuningController)에서 dt만 던져줌
    public void Tick(float dt)
    {
        if (!IsReplaying) return;

        // 일시정지가 아닐 때만 시간 흐름
        if (!IsPaused)
        {
            CurrentTime += dt;
            
            // 끝에 도달하면 멈춤 or 루프
            if (CurrentTime >= TotalDuration)
            {
                CurrentTime = TotalDuration;
                IsReplaying = false;
                OnReplayEnd?.Invoke();
            }
        }

        // 시간 변화에 따른 위치 계산 및 이동
        Step(0f); // 이미 위에서 CurrentTime을 바꿨으므로 dt는 0으로 처리하거나 내부 로직 분리
    }

    // 실제 이동 로직 (Tick 내부에서 호출)
    private void Step(float dt)
    {
        // 1. 계산 (Logic)
        if (_trackA != null)
        {
            ReplaySystem.EvaluatePose(_trackA, CurrentTime, out Vector3 pos, out Quaternion rot);
            // 2. 명령 (Command)
            _simManager.TeleportMainVehicle(pos, rot);
        }

        if (_trackB != null)
        {
            ReplaySystem.EvaluatePose(_trackB, CurrentTime, out Vector3 pos, out Quaternion rot);
            _simManager.TeleportGhostVehicle(pos, rot);
        }
    }
    

    // [핵심] TuningController가 매 프레임 호출
    
}