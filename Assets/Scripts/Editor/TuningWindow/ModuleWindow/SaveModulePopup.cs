using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class SaveModulePopup : EditorWindow
{
    private Action<string> _onConfirm;
    private TextField _inputField;
    private Label _errorLabel;

    // [핵심] 정적 메서드로 호출 (사용하기 편하게)
    public static void Show(string defaultName, Action<string> onConfirm)
    {
        // 윈도우 생성 (Utility 타입)
        SaveModulePopup wnd = GetWindow<SaveModulePopup>(true, "Save Module", true);
        
        wnd._onConfirm = onConfirm;
        
        // 크기 고정 (작은 팝업)
        wnd.minSize = new Vector2(300, 150);
        wnd.maxSize = new Vector2(300, 150);
        
        // 모달로 띄우기 (이 창을 닫기 전엔 뒤에 창 조작 불가)
        wnd.ShowModalUtility();
        
        // 초기값 세팅 (선택 사항)
        if (wnd._inputField != null) wnd._inputField.value = defaultName;
    }

    public void CreateGUI()
    {
        // UXML 로드
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Scripts/Editor/TuningWindow/ModuleWindow/SaveModulePopup.uxml");
        if (visualTree != null) visualTree.CloneTree(rootVisualElement);

        // UI 찾기
        _inputField = rootVisualElement.Q<TextField>("input-name");
        _errorLabel = rootVisualElement.Q<Label>("error-msg");
        var btnCancel = rootVisualElement.Q<Button>("btn-cancel");
        if (btnCancel == null) { Debug.LogError("btn-cancel not found!"); return; }
        var btnConfirm = rootVisualElement.Q<Button>("btn-confirm");

        // 이벤트 연결
        btnCancel.clicked += Close;
        btnConfirm.clicked += OnConfirmClicked;

        // 키보드 엔터/ESC 처리
        _inputField.RegisterCallback<KeyDownEvent>(evt =>
        {
            if (evt.keyCode == KeyCode.Return) OnConfirmClicked();
            if (evt.keyCode == KeyCode.Escape) Close();
        });

        // 팝업이 뜨면 바로 입력창에 포커스
        rootVisualElement.schedule.Execute(() => _inputField.Focus()).ExecuteLater(50);
  
    }

    private void OnConfirmClicked()
    {
        string newName = _inputField.value.Trim();

        // 유효성 검사 (빈 이름 방지)
        if (string.IsNullOrEmpty(newName))
        {
            _errorLabel.style.visibility = Visibility.Visible;
            return;
        }

        // 콜백 호출 및 닫기
        _onConfirm?.Invoke(newName);
        Close();
    }
}