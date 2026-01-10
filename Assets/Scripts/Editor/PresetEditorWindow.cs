using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Sirenix.OdinInspector.Editor; // Odin 필수

public class PresetEditorWindow : EditorWindow
{
    [SerializeField] private VisualTreeAsset m_VisualTreeAsset = default;

    private TuningRepository _repository;
    private PropertyTree _odinTree;
    private IMGUIContainer _odinContainer;

    private ListView _listView;
    
    // 현재 선택된 타입과 SO
    private Type _currentType;
    private BaseModule _currentSO;

    [MenuItem("Tools/Preset Editor")]
    public static void ShowWindow()
    {
        GetWindow<PresetEditorWindow>("Preset Editor");
    }

    private void OnEnable()
    {
        // Repository만 있으면 됨 (SimManager 연결 불필요)
        _repository = new TuningRepository(new Type[] { 
            typeof(EngineModule),typeof(TransmissionModule),typeof(DrivetrainModule),typeof(ChassisModule) ,
            typeof(SteeringModule) , typeof(BrakeModule), typeof(SuspensionModule),typeof(RimModule),  typeof(TireModule)
        });
    }

    private void OnDisable()
    {
        _odinTree?.Dispose();
    }

    public void CreateGUI()
    {
        if (m_VisualTreeAsset != null) m_VisualTreeAsset.CloneTree(rootVisualElement);

        // UI 찾기
        var typeDropdown = rootVisualElement.Q<DropdownField>("type-selector");
        _listView = rootVisualElement.Q<ListView>("so-list");
        var createBtn = rootVisualElement.Q<Button>("btn-create");
        _odinContainer = rootVisualElement.Q<IMGUIContainer>("odin-inspector");

        // 1. 드롭다운 초기화 (모듈 타입들)
        var allTypes = _repository.GetAllModuleList();
        typeDropdown.choices = allTypes.Select(t => t.Name).ToList();
        typeDropdown.index = 0; // 첫 번째 선택
        _currentType = allTypes[0];

        // 드롭다운 변경 이벤트
        typeDropdown.RegisterValueChangedCallback(evt => 
        {
            // 이름으로 타입 찾기
            _currentType = allTypes.Find(t => t.Name == evt.newValue);
            RefreshList(_listView);
        });

        // 2. 리스트뷰 설정
        _listView.makeItem = () => new Label();
        _listView.bindItem = (e, i) => 
        {
            var datas = _listView.itemsSource as List<BaseModule>;
            (e as Label).text = datas[i].name;
            (e as Label).style.paddingLeft = 5;
        };

        // 리스트 선택 이벤트 -> 우측 오딘 인스펙터 갱신
        _listView.selectionChanged += (selectedItems) =>
        {
            if (selectedItems.Any())
            {
                var selectedSO = selectedItems.First() as BaseModule; // BaseModuleSO라면 캐스팅 수정
                _currentSO = selectedSO;
                RefreshInspector(selectedSO);
            }
         
        };

        //3. 생성 버튼
        createBtn.clicked += () => 
        {
            // 팝업 띄워서 이름 받고 -> Repo.CreateNewModule 호출
            SaveModulePopup.Show("New_Module", (name) => 
            {
                // 임시 데이터 생성
                var tempInstance = (BaseModule)Activator.CreateInstance(_currentType);
                _repository.CreateNewModule(tempInstance, name);
                
                RefreshList(_listView); // 리스트 갱신
            });
        };

        // 초기 리스트 로드
        RefreshList(_listView);
    }

    private void RefreshList(ListView listView)
    {
        listView.ClearSelection();
        var db = _repository.GetMouModuleDatabase(_currentType);
        if (db != null)
        {
            listView.itemsSource = db.Datas.ToList(); // SO 리스트
            listView.Rebuild();
        }
    }

    private void RefreshInspector(BaseModule so)
    {
        _currentSO = so;
        _odinTree?.Dispose();
        
        if (so != null)
        {
            _odinTree = PropertyTree.Create(so);
            _odinContainer.onGUIHandler = () => _odinTree.Draw(false);
        }
        else
        {
            _odinContainer.onGUIHandler = () => GUILayout.Label("Select an item");
        }
    }
}