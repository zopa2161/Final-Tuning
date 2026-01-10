using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class RewardTuningWindow : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    private RewardTuningController _controller;
    
    [MenuItem("Tools/RewardTuningWindow")]
    public static void ShowExample()
    {
        
        RewardTuningWindow wnd = GetWindow<RewardTuningWindow>();
        wnd.titleContent = new GUIContent("RewardTuningWindow");
       
    }
    private void OnEnable()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private void OnDisable()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        
    }
    // 상태가 변할 때(플레이 -> 정지, 정지 -> 플레이) UI를 다시 그림
    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        // UI를 싹 지우고 CreateGUI를 다시 호출하여 상태에 맞는 화면을 그림
        rootVisualElement.Clear();
        CreateGUI();
    }
    public void CreateGUI()
    {
        m_VisualTreeAsset.CloneTree(rootVisualElement);
        
        VisualElement root = rootVisualElement;
        _controller = new RewardTuningController();
        _controller.Initialize(root);
        
        
        
    }
}
