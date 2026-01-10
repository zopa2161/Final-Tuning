using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class TuningWindow : EditorWindow
{
    [SerializeField] private VisualTreeAsset m_WindowTemplate = default;
    
    private TuningController _controller;
    
    // UI 요소
    private Button _toggleBtn;
    private VisualElement _rightPanel;
    
    [MenuItem("Tools/Vehicle/Tuning Window")]
    public static void ShowWindow()
    {
        TuningWindow wnd = GetWindow<TuningWindow>();
        wnd.titleContent = new GUIContent("Vehicle Tuner");
        wnd.minSize = new Vector2(800, 600);
    }

    private void OnEnable()
    {
        if (_controller == null) _controller = new TuningController();
    }
    
    private void OnDisable()
    {
        _controller?.Dispose();
    }
    
    // 2. 토스트 알림용
    public void ShowToast(string message)
    {
        // 윈도우 중앙에 슥 떴다 사라짐
        this.ShowNotification(new GUIContent(message));
    }
    
    public void CreateGUI()
    {
        if (m_WindowTemplate != null)
        {
            m_WindowTemplate.CloneTree(rootVisualElement);
        }

        if (_controller == null) _controller = new TuningController();
        _controller.Initialize(rootVisualElement, onShowWarning: ShowToast);
        
        // 버튼 이벤트 연결
        BindTogglePanel();
    }

    private void Update()
    {
        if (_controller != null) _controller.Update(Time.deltaTime);
    }
    
    private void BindTogglePanel()
    {
        _toggleBtn = rootVisualElement.Q<Button>("btn-toggle-panel");
        _rightPanel = rootVisualElement.Q<VisualElement>("right-panel");

        if (_toggleBtn != null && _rightPanel != null)
        {
            _toggleBtn.clicked += OnToggleRightPanel;
        }
    }

    private void OnToggleRightPanel()
    {
        // [롤백 상태]
        // 현재는 기능을 잠시 꺼둡니다.
        // 나중에 display: None / Flex 로직을 다시 구현하거나
        // USS 애니메이션 방식을 사용할 때 여기에 코드를 넣으시면 됩니다.
        
        Debug.Log("패널 접기 기능은 잠시 비활성화되었습니다.");
    }
}