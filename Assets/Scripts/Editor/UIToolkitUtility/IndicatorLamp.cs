using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class IndicatorLamp : VisualElement
{
    private Label _label;     // "ABS" 글자
    private VisualElement _bulb; // 불 들어오는 네모 박스

    // --- 속성 ---
    
    // 1. 라벨 텍스트 ("ABS", "TCS" 등)
    [UxmlAttribute("text")]
    public string Text
    {
        get => _label.text;
        set => _label.text = value;
    }

    // 2. 켜짐/꺼짐 상태
    private bool _isOn = false;
    [UxmlAttribute("is-on")]
    public bool IsOn
    {
        get => _isOn;
        set
        {
            if (_isOn == value) return;
            _isOn = value;
            UpdateState();
        }
    }

    // 3. 램프 색상 (기본: 노란색 경고등)
    [UxmlAttribute("on-color")]
    public Color OnColor { get; set; } = new Color(1f, 0.7f, 0f); // Amber

    // --- 생성자 ---
    public IndicatorLamp()
    {
        // 레이아웃: 가로 배치 (글자 + 램프)
        style.flexDirection = FlexDirection.Row;
        style.alignItems = Align.Center;

        // 1. 글자 생성
        _label = new Label("ABS");
        _label.style.marginRight = 5;
        _label.style.color = new Color(0.6f, 0.6f, 0.6f); // 평소엔 어둡게
        _label.style.fontSize = 12;
        Add(_label);

        // 2. 전구(박스) 생성
        _bulb = new VisualElement();
        _bulb.style.width = 16;
        _bulb.style.height = 10;

// [수정 1] BorderRadius: 4군데 모서리를 각각 지정해야 함
        _bulb.style.borderTopLeftRadius = 2;
        _bulb.style.borderTopRightRadius = 2;
        _bulb.style.borderBottomLeftRadius = 2;
        _bulb.style.borderBottomRightRadius = 2;

// [수정 2] BackgroundColor (이건 원래 잘 됨)
        _bulb.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f); 

// [수정 3] BorderWidth: 4면 두께를 각각 지정
        _bulb.style.borderTopWidth = 1;
        _bulb.style.borderBottomWidth = 1;
        _bulb.style.borderLeftWidth = 1;
        _bulb.style.borderRightWidth = 1;

// [수정 4] BorderColor: 4면 색상을 각각 지정
        _bulb.style.borderTopColor = new Color(0.4f, 0.4f, 0.4f);
        _bulb.style.borderBottomColor = new Color(0.4f, 0.4f, 0.4f);
        _bulb.style.borderLeftColor = new Color(0.4f, 0.4f, 0.4f);
        _bulb.style.borderRightColor = new Color(0.4f, 0.4f, 0.4f);

        Add(_bulb);
    }

    // --- 상태 업데이트 로직 ---
    private void UpdateState()
    {
        if (_isOn)
        {
            // 켜짐: 전구 색상을 밝게 + 글자도 밝게
            _bulb.style.backgroundColor = OnColor;
            _bulb.style.borderTopColor = OnColor;
            _bulb.style.borderBottomColor = OnColor;
            _bulb.style.borderLeftColor = OnColor;
            _bulb.style.borderRightColor = OnColor;
            // (선택) 빛 번짐 효과 (Glow) - 텍스트 쉐도우 활용 꼼수 혹은 그냥 밝게
            _label.style.color = OnColor; 
            _label.style.unityFontStyleAndWeight = FontStyle.Bold;
        }
        else
        {
            // 꺼짐: 원래대로 복구
            _bulb.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
            _bulb.style.borderTopColor = new Color(0.4f, 0.4f, 0.4f);
            _bulb.style.borderBottomColor = new Color(0.4f, 0.4f, 0.4f);
            _bulb.style.borderLeftColor = new Color(0.4f, 0.4f, 0.4f);
            _bulb.style.borderRightColor = new Color(0.4f, 0.4f, 0.4f);
            
            _label.style.color = new Color(0.6f, 0.6f, 0.6f);
            _label.style.unityFontStyleAndWeight = FontStyle.Normal;
        }
    }
}