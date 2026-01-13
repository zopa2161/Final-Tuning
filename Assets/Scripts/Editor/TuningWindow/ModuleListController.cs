
//모듈 비교 뷰의 생성,리프레시 관리.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

public class ModuleListController : ISubController
{
    
    public event Action<Type, string> OnSaved;
    public event Action<Type, Vector2> OpenListPopup;
    
    private VisualTreeAsset _rowTemplate;
    
    private VisualElement _listContainer;
    private TuningContext _context;
    private Dictionary<Type, ModuleRowController> _rowMap = new Dictionary<Type, ModuleRowController>();

    public ModuleListController(TuningContext context)
    {
        _context = context;
    }
    public void Initialize(VisualElement root)
    {
        _listContainer = root.Q<VisualElement>("module-list-container");
        _rowTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Scripts/Editor/TuningWindow/ModuleWindow/ModuleComparisonRow.uxml");
    }

    public event Action<string> OnFloatingWarning;

    public void RebuildFullList()//이벤트에 반응할거에요
    {
        // 기존 정리
        _listContainer.Clear();//이니셜라이즈가 안된경우 바동되면 안ㄴ덴다,
        foreach (var row in _rowMap.Values) row.Dispose();
        _rowMap.Clear();

        if (_context.TargetCar == null) return;

        // Repository에 있는 모든 모듈 타입에 대해 Row 생성
        // (_controller를 통해 repo에 접근하거나 Context가 타입을 알고 있어야 함.
        // 여기서는 Context가 초기화 때 만들어둔 ComparisonMap의 Key를 사용한다고 가정)
        
        foreach (var type in _context.ComparisonMap.Keys)
        {
            CreateAndAddRow(type);
        }
    }
    
    private void CreateAndAddRow(Type type)
    {
        // 데이터 준비
        var liveData = _context.GetLiveData(type);
        var soData = _context.ComparisonMap[type] as ScriptableObject;

        // Row Controller 생성 (버튼 클릭 시 Controller에게 명령 전달)
        var rowCtrl = new ModuleRowController(
            type,
            _rowTemplate,
            liveData,
            soData,
            onList: (t,v) => OpenListPopup.Invoke(t,v), // 나중에 팝업 연결
            onSet:  (t) => ApplyToLive(t),                 // Controller 명령
            onSave: (t,n) => OnSaved.Invoke(t,n)                     // Controller 명령
        );

        // 딕셔너리에 등록 & UI 추가
        _rowMap.Add(type, rowCtrl);
        _listContainer.Add(rowCtrl.Root);
    }
    
    
    public void ApplyToLive(Type t)//setup버튼
    {
        var liveModule = _context.GetLiveData(t);       // Target (받을 놈)
        var soData = _context.ComparisonMap[t]; // Source (줄 놈)

        // 한 줄로 끝!
        liveModule.CopyFrom(soData);
    }

    public void OnDispose()
    {
        _listContainer.Clear();
        foreach (var row in _rowMap.Values) row.Dispose();
        _rowMap.Clear();
    }

    
}