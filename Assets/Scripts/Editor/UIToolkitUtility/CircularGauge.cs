using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class CircularGauge : VisualElement
{
    private Label _centerLabel;
    //===원형 게이지 속성===
    [UxmlAttribute("line-width")] public float LineWidth { get; set; } = 10f;
    [UxmlAttribute("progress-color")] public Color ProgressColor { get; set; } = new Color(0.2f, 0.8f, 0.2f);
    [UxmlAttribute("track-color")] public Color TrackColor { get; set; } = new Color(0.3f, 0.3f, 0.3f);
    
    private float _highValue = 1.0f;
    [UxmlAttribute("high-value")]
    public float HighValue
    {
        get => _highValue;
        set
        {
            // 0으로 나누기 방지
            _highValue = Mathf.Max(0.001f, value);
            MarkDirtyRepaint();
        }
    }
    
    //=== text 자동 업데이트 ===
    private bool _autoUpdateText = false;
    [UxmlAttribute("auto-update-text")]
    public bool AutoUpdateText
    {
        get => _autoUpdateText;
        set => _autoUpdateText = value;
    }
    
    private string _textFormat = "F1";
    [UxmlAttribute("text-format")]
    public string TextFormat
    {
        get => _textFormat;
        set => _textFormat = value;
    }
    
    // 내부 상태
    private float _value = 0f; // 0.0 ~ 1.0
    [UxmlAttribute("value")]
    public float Value
    {
        get => _value;
        set
        {
            // 값이 바뀌었을 때만 다시 그림 (성능 최적화)
            if (!Mathf.Approximately(_value, value))
            {
                _value = value;

                // ★ 여기가 핵심입니다! ★
                // 스위치가 켜져 있으면 텍스트도 자동으로 갱신
                if (_autoUpdateText)
                {
                    _centerLabel.text = _value.ToString(_textFormat);
                }

                MarkDirtyRepaint();
            }
        }
    }
    
    
    [UxmlAttribute("show-text")]
    public bool ShowText
    {
        get => _centerLabel.style.display == DisplayStyle.Flex;
        set => _centerLabel.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
    }

    [UxmlAttribute("font-size")]
    public int FontSize
    {
        get => (int)_centerLabel.style.fontSize.value.value;
        set => _centerLabel.style.fontSize = value;
    }

    [UxmlAttribute("text-color")]
    public Color TextColor
    {
        get => _centerLabel.style.color.value;
        set => _centerLabel.style.color = value;
    }
    [UxmlAttribute("text")]
    public string Text
    {
        get => _centerLabel.text;
        set
        {
            // 수동으로 텍스트를 넣으면 자동 업데이트를 잠시 끌 수도 있고,
            // 그냥 덮어씌우게 둬도 됩니다. 여기선 그냥 덮어씌웁니다.
            _centerLabel.text = value;
        }
        
    }

    public CircularGauge()
    {
        generateVisualContent += OnGenerateVisualContent;

        _centerLabel = new Label();
        Add(_centerLabel);

        // 스타일링 (중앙 정렬)
        this.style.justifyContent = Justify.Center;
        this.style.alignItems = Align.Center;
        
        _centerLabel.style.position = Position.Absolute;
        _centerLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        _centerLabel.style.fontSize = 14;
        _centerLabel.style.color = Color.white;
        
        style.width = 100;
        style.height = 100;
    }
    

    // 실제 그리기 로직 (매 프레임 호출되는 게 아니라, Repaint 요청 시에만 호출됨)
    private void OnGenerateVisualContent(MeshGenerationContext ctx)
    {
        var painter = ctx.painter2D;
        
        float width = contentRect.width;
        float height = contentRect.height;
        float radius = (Mathf.Min(width, height) / 2f) - (LineWidth / 2f);
        Vector2 center = new Vector2(width / 2f, height / 2f);
        float ratio = Mathf.Clamp01(_value / _highValue);
        // 게이지 시작/끝 각도 (12시 방향 = -90도)
        // 여기서는 -225도(7시) ~ 45도(5시) 까지 270도만 사용한다고 가정
        float startAngle = -225f;
        float sweepAngle = 270f;

        // 1. 트랙(배경) 그리기
        painter.strokeColor = TrackColor;
        painter.lineWidth = LineWidth;
        painter.lineCap = LineCap.Round; // 끝부분 둥글게
        painter.BeginPath();
        painter.Arc(center, radius, startAngle, startAngle + sweepAngle);
        painter.Stroke();

        // 2. 값(Fill) 그리기
        if (ratio > 0)
        {
            painter.strokeColor = ProgressColor;
            painter.BeginPath();
            // 현재 값(%)만큼만 호를 그림
            float currentSweep = sweepAngle * ratio;
            painter.Arc(center, radius, startAngle, startAngle + currentSweep);
            painter.Stroke();
        }
        
    }
}