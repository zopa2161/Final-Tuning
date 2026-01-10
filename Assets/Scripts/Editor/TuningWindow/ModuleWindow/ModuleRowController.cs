using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using Sirenix.OdinInspector.Editor; // Odin 필수

public class ModuleRowController : IDisposable
{
    // UI Root (이걸 스크롤뷰에 Add 함)
    public VisualElement Root { get; private set; }
    
    private VisualElement _body;
    private Label _arrowLabel;
    private bool _isExpanded = true; // 기본값: 펼침
    
    // 데이터
    private Type _moduleType;
    private object _liveData;
    private ScriptableObject _soData;

    // Odin Property Trees (인스펙터 그리는 도구)
    private PropertyTree _liveTree;
    private PropertyTree _soTree;

    // 액션 콜백 (버튼 눌렀을 때 윈도우/컨트롤러에게 알림)
    private Action<Type,Vector2> _onListClicked;
    private Action<Type> _onSetClicked;
    private Action<Type, string> _onSaveClicked;

    public ModuleRowController(
            Type moduleType, 
            VisualTreeAsset template, 
            object liveData, 
            ScriptableObject soData,
            Action<Type, Vector2> onList, Action<Type> onSet, Action<Type, string> onSave)
    {
        _moduleType = moduleType;
        _liveData = liveData;
        _soData = soData;
        _onListClicked = onList;
        _onSetClicked = onSet;
        _onSaveClicked = onSave;

        // 1. 템플릿 복제 (Clone)
        Root = template.CloneTree();

        // 2. 초기화
        InitializeOdin();
        BindButtons();
        UpdateStyles();
        
        _body = Root.Q<VisualElement>("row-body");
        _arrowLabel = Root.Q<Label>("foldout-arrow");
        var header = Root.Q<VisualElement>("row-header");
        var titleLabel = Root.Q<Label>("module-title");

        // 제목 설정
        titleLabel.text = _moduleType.Name; // 혹은 "Engine Module" 등

        // 헤더 클릭 이벤트 (Foldout 토글)
        header.RegisterCallback<ClickEvent>(evt => ToggleFoldout());

        // 초기 상태 적용 (혹시 기본적으로 접어두고 싶다면 여기서 처리)
        // ToggleFoldout();
    }

    private void InitializeOdin()
    {
        // Live Inspector 연결
        var liveContainer = Root.Q<IMGUIContainer>("odin-live");
        if (_liveData != null)
        {
            _liveTree = PropertyTree.Create(_liveData);
            liveContainer.onGUIHandler = () => _liveTree.Draw(false); // false: 마진 없이
        }
        else
        {
            liveContainer.onGUIHandler = () => GUILayout.Label("No Live Data");
        }

        // SO Inspector 연결
        SetupSOTree();

        var nameLabels = Root.Query<Label>("module-lable").ToList();
        foreach (var label in nameLabels)
        {
            label.text = _moduleType.Name;
        }
    }

    // SO가 교체될 때마다 트리 다시 생성해야 함
    private void SetupSOTree()
    {
        var soContainer = Root.Q<IMGUIContainer>("odin-so");
        
        // 기존 트리가 있으면 정리
        _soTree?.Dispose();

        if (_soData != null)
        {
            _soTree = PropertyTree.Create(_soData);
            soContainer.onGUIHandler = () => 
            {
                // SO 쪽은 수정 불가하게 막고 싶다면:
                // EditorGUI.BeginDisabledGroup(true);
                _soTree.Draw(false);
                // EditorGUI.EndDisabledGroup();
            };
        }
        else
        {
            soContainer.onGUIHandler = () => GUILayout.Label("No SO Selected");
        }
    }

    private void BindButtons()
    {
        var listButton = Root.Q<Button>("btn-list");
        listButton.clicked += () => _onListClicked?.Invoke(_moduleType, listButton.worldBound.position);
        Root.Q<Button>("btn-set").clicked += () => 
        {
            _onSetClicked?.Invoke(_moduleType);
            _liveTree?.ApplyChanges(); // 데이터 바뀌었으니 갱신
        };
        Root.Q<Button>("btn-save").clicked += () => 
        {
            // 기본값: 현재 SO 이름이 있으면 그거, 없으면 타입명
            string defaultName = _soData != null ? _soData.name : $"{_moduleType.Name}_New";

            // 1. 팝업 띄우기
            SaveModulePopup.Show(defaultName, (inputName) => //
            {
                // 2. 팝업에서 확인 누르면 -> 컨트롤러에게 이름과 함께 전달
                _onSaveClicked?.Invoke(_moduleType, inputName);
            });
        };
    }
    
    // [핵심 기능] 외부에서 SO가 바뀌었다고 알려주면 화면 갱신
    public void RefreshSO(ScriptableObject newSO)
    {
        _soData = newSO;
        SetupSOTree(); // 오딘 트리 재생성
        
        // 스타일 갱신 (선택됨/안됨 표시 등)
        UpdateStyles();
    }

    private void UpdateStyles()
    {
        // 예: SO가 없으면 배경을 붉게 한다거나 하는 로직 추가 가능
    }
    
    private void ToggleFoldout()
    {
        _isExpanded = !_isExpanded;

        if (_isExpanded)
        {
            // 펼치기
            _body.RemoveFromClassList("body-collapsed");
            _arrowLabel.text = "▼"; // 아래 화살표
            
            // 펴질 때 오딘 트리가 제대로 다시 그려지도록 갱신 요청 (선택사항)
            // _liveTree?.Draw(false); 
        }
        else
        {
            // 접기
            _body.AddToClassList("body-collapsed");
            _arrowLabel.text = "▶"; // 오른쪽 화살표
        }
    }
    

    // 메모리 정리 (윈도우 닫힐 때 필수)
    public void Dispose()
    {
        _liveTree?.Dispose();
        _soTree?.Dispose();
    }
}