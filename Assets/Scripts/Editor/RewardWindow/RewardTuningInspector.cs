using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using System.Runtime.InteropServices;
using Sirenix.OdinInspector.Editor;

public class RewardTuningInspector : ISubController
{
    
    private RewardProfile _targetProfile; //=> 타겟 프로파일이 변하면 이벤트 울려야함.
    
    private IMGUIContainer _imguiContainer; // Odin(IMGUI)을 담을 그릇
    private VisualElement _odinContainer;

    private PropertyTree _viewTree;

    private Button _listButton;
    //live : live 객체를 클론해서 저장 
    //nonLIve : 비어있는 객체 생성. 현재 객체 클론해 생성
    // Create : nl전용, clone는 양쪽 사용
    private Button _createButton;
    private Button _saveButton;
    
    private Action<Vector2> _onListClicked;
    private Action _onCreateClicked;
    private Action<RewardProfile, string> _onSaveClicked;

    public RewardTuningInspector(RewardTuningMode mode, Action<Vector2> onListClicked, Action onCreateClicked, Action<RewardProfile,string> onSaveClicked)
    {
        _onListClicked = onListClicked;
        _onCreateClicked = onCreateClicked;
        _onSaveClicked = onSaveClicked;
    }
    public void Initialize(VisualElement root)
    {
        
        _odinContainer =root.Q<VisualElement>("odin-container");
        _imguiContainer = new IMGUIContainer();
        _odinContainer.Add( _imguiContainer);
        
        _listButton = root.Q<Button>("profile-list");
        _listButton.clicked += () => _onListClicked(_listButton.worldBound.position);

        _createButton = root.Q<Button>("create-profile");
        _saveButton = root.Q<Button>("save-profile");
        
        _createButton.clicked += () => _onCreateClicked?.Invoke();
        _saveButton.clicked += () =>
        {
            //1. 팝업
            SaveRewardProfilePopup.Show("defaultName", (inputName) =>
            {
                _onSaveClicked?.Invoke(_targetProfile, inputName);
            });
            
        };


    }

    public event Action<string> OnFloatingWarning;

    public void SetTarget(RewardProfile profile)
    {
        _targetProfile = profile;
        
        _viewTree?.Dispose();
 
        
        if (_targetProfile != null)
        {
            _viewTree = PropertyTree.Create(_targetProfile);
            _imguiContainer.onGUIHandler = () => _viewTree.Draw(false);
        }
        else _imguiContainer.onGUIHandler = () => GUILayout.Label("No item");
        
    }
    
  

    
}