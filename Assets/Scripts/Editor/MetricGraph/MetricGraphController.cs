using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class MetricGraphController : ISubController
{
    private VisualElement _graphContainer; // 우측 패널
    private ScrollView _graphScrollView;
    private VisualElement _toggleContainer; // 좌측 패널 (체크박스)

    private Button _saveButton;
    private Button _loadButton;
    private Button _loadButtonB;

    private Label _slotAField;
    private Label _slotBField;
    private Action _onSave, _onLoad, _onLoadB;
    private List<MetricSession> _currentSessions = new List<MetricSession>();
    private List<MetricGraph> _activeGraphs = new List<MetricGraph>();
    
    private List<string> _metricOrder = new List<string>();
    public void BindSessions(List<MetricSession> sessions)
    {
        _currentSessions = sessions;
        _graphScrollView.Clear();
        _toggleContainer.Clear();
        _activeGraphs.Clear();

        if (sessions == null || sessions.Count < 1)
        {
            Debug.LogWarning("컨트롤러의 세션이 준비되지 않았습니다.");
            return;
        }
        
        if (_currentSessions.Count == 1)
        {
            _slotAField.text = _currentSessions[0].sessionName;
        }
        else if (_currentSessions.Count == 2)
        {
            _slotAField.text = _currentSessions[0].sessionName;
            _slotAField.style.color = Color.green;
            _slotBField.text = _currentSessions[1].sessionName;
            _slotBField.style.color = Color.darkGoldenRod;
        }
        
        foreach (var entry in _currentSessions[0].metricEntries)
        {
            string metricName = entry.Name;
            _metricOrder.Add(metricName);
            
            var box = new VisualElement();
            // --- 박스 스타일링 ---
            box.style.backgroundColor = new Color(0.25f, 0.25f, 0.25f); // 기본 배경색
            box.style.borderBottomWidth = 1;
            box.style.borderBottomColor = new Color(0.15f, 0.15f, 0.15f);     // 테두리 색
            box.style.borderTopLeftRadius = 5;
            box.style.borderTopRightRadius = 5;
            box.style.borderBottomLeftRadius = 5;
            box.style.borderBottomRightRadius = 5;
        
            box.style.marginTop = 2;
            box.style.marginBottom = 2; // 박스 간 간격
            box.style.paddingLeft = 5;  // 내부 여백
            box.style.paddingRight = 5;
            box.style.paddingTop = 3;
            box.style.paddingBottom = 3;
            
            var toggle = new Toggle(metricName);
            toggle.RegisterValueChangedCallback(evt => 
            {
                if (evt.newValue)
                {
                    //텅 빈 그래프를 생성한다.
                    var graph = AddGraph(metricName, entry);
                    //그 후 line을 위한 재료를 제공한다.
                    foreach (var session in sessions)
                    {
                        graph.AddLine(session.TimeStampEntry,session.MetricByName(metricName));
                    }
                }
                else RemoveGraph(metricName);
            });
            _toggleContainer.Add(toggle);
            toggle.value = true;
        }
        
        
        
    }

    public MetricGraphController(Action onSave, Action onLoad, Action onLoadB)
    {
        _onSave = onSave;
        _onLoad = onLoad;
        _onLoadB = onLoadB;
    }

    public void Initialize(VisualElement root)
    {
        // UXML에서 영역 찾기
        //_graphContainer = root.Q<VisualElement>("car-graph-view-container");
        _toggleContainer = root.Q<VisualElement>("graph-toggle");
        _graphScrollView = root.Q<ScrollView>("metric-scroll-view");

        _saveButton = root.Q<Button>("session-save-button");
        _loadButton = root.Q<Button>("session-load-button");
        _loadButtonB = root.Q<Button>("b-slot-load");

        _slotAField = root.Q<Label>("slotA-data");
        _slotBField = root.Q<Label>("slotB-data");

        _saveButton.clicked += _onSave;
        _loadButton.clicked += _onLoad;
        if (_onLoadB != null)
        {
            _loadButtonB.clicked += _onLoadB;
        }
        
     
    
    }
    // 레코더가 세션을 만들거나, 차량이 연결되었을 때 호출
    private MetricGraph AddGraph(string name, MetricEntry dataEntry)
    {
        // 그래프 생성 (랜덤 컬러 등 적용)
        var graph = new MetricGraph(name, dataEntry.minValue, dataEntry.maxValue);
        graph.name = name; // 식별자
        graph.style.borderTopWidth = 2;
        graph.style.borderBottomWidth = 2;
        
        // 2. [핵심 3] 끼어들 위치(Index) 찾기
        int myOrderIndex = _metricOrder.IndexOf(name); // 내 번호표 (예: 2번)
        int insertIndex = 0;
        
        // 현재 화면에 나와있는 자식들을 훑어봄
        foreach (var child in _graphScrollView.Children())
        {
            // 자식의 이름으로 번호표 확인
            int childOrderIndex = _metricOrder.IndexOf(child.name);
            
            // "어? 나보다 뒷번호(큰 번호)인 애가 있네?"
            if (childOrderIndex > myOrderIndex)
            {
                // 그럼 걔 앞에 서면 됨. (루프 종료)
                break;
            }
            
            // 나보다 앞번호면 통과, 인덱스 증가
            insertIndex++;
        }

        // 3. 찾은 위치에 삽입 (Insert)
        _graphScrollView.Insert(insertIndex, graph);
        _activeGraphs.Add(graph);

        return graph;
    }
   



    private void RemoveGraph(string name)
    {
        var graph = _activeGraphs.FirstOrDefault(g => g.name == name);
        if (graph != null)
        {
            _graphScrollView.Remove(graph);
            _activeGraphs.Remove(graph);
        }
    }

    public void SetFocusTime(float time)
    {
        _activeGraphs.ForEach(x => x.SetFocusTime(time));
    }

    public void SetLockState(bool isBusy)
    {
        _saveButton.SetEnabled(!isBusy);
        _loadButton.SetEnabled(!isBusy);
        _loadButtonB.SetEnabled(!isBusy);//여기서 추가적인 처리 필요.
    
    }
    
    public void UpdateUIState(bool isBusy, TuningMode currentMode)
    {
        // 1. 공통 버튼들 (모드 상관없이 바쁘면 잠김)
        _saveButton.SetEnabled(!isBusy);
        _loadButton.SetEnabled(!isBusy); // Load A (기본)

        // 2. Load B 버튼 (비교 모드이면서 + 안 바쁠 때만 활성)
        bool isCompareMode = (currentMode == TuningMode.Compare);
        _loadButtonB.SetEnabled(!isBusy && isCompareMode);

        // [옵션] 아예 비교 모드가 아니면 버튼을 숨기고 싶다면?
        // _loadButtonB.style.display = isCompareMode ? DisplayStyle.Flex : DisplayStyle.None;
    }

    // TuningWindow Update에서 호출
    public void Tick()
    {
        // 활성화된 모든 그래프 다시 그리기
        foreach (var graph in _activeGraphs)
        {
            graph.Refresh();
        }
    }

    
}