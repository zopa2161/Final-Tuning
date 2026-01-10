using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class VerticalGauge : VisualElement
{
    // --- 1. 속성 ---
    private float _value = 0f;
    [UxmlAttribute("value")]
    public float Value
    {
        get => _value;
        set
        {
            _value = Mathf.Clamp01(value);
            MarkDirtyRepaint();
        }
    }
    

    [UxmlAttribute("fill-color")]
    public Color FillColor { get; set; } = new Color(0.2f, 0.6f, 1f); 

    [UxmlAttribute("track-color")]
    public Color TrackColor { get; set; } = new Color(0.15f, 0.15f, 0.15f); // 더 어두운 회색

    private float _gaugeWidth = 20f;
    [UxmlAttribute("Gauge-width")]
    public float GaugeWidth
    {
        get => _gaugeWidth;
        set
        {
            _gaugeWidth = value;
            AdaptingSize();
            MarkDirtyRepaint();
        } 
    }
    private float _gaugeHeight = 100f;
    [UxmlAttribute("Gauge-height")]
    public float GaugeHeight
    {
        get => _gaugeHeight;
        set
        {
            _gaugeHeight = value;
            AdaptingSize();
            MarkDirtyRepaint();
        } 
    }
    
    // --- 2. 생성자 ---
    public VerticalGauge()
    {
        generateVisualContent += OnGenerateVisualContent;
        
        style.width = 20; // 각진 모양은 조금 더 얇아도 예쁩니다
        style.height = 100;
        style.marginBottom = 5;
    }

    // --- 3. 그리기 로직 (심플!) ---
    private void OnGenerateVisualContent(MeshGenerationContext mgc)
    {
        var painter = mgc.painter2D;

        float width = GaugeWidth;
        float height = GaugeHeight;
        
        // A. 배경 트랙 (단순 사각형)
        painter.fillColor = TrackColor;
        painter.BeginPath();
        painter.MoveTo(new Vector2(0, 0));
        painter.LineTo(new Vector2(width, 0));
        painter.LineTo(new Vector2(width, height));
        painter.LineTo(new Vector2(0, height));
        painter.ClosePath();
        painter.Fill();

        // B. 채우기 (Fill) - 아래에서 위로
        if (_value > 0)
        {
            float fillHeight = height * _value;
            float yPos = height - fillHeight; // 채워질 윗부분의 Y 좌표

            painter.fillColor = FillColor;
            painter.BeginPath();
            painter.MoveTo(new Vector2(0, yPos));
            painter.LineTo(new Vector2(width, yPos));
            painter.LineTo(new Vector2(width, height));
            painter.LineTo(new Vector2(0, height));
            painter.ClosePath();
            painter.Fill();
        }
    }

    private void AdaptingSize()
    {
        style.width = GaugeWidth;
        style.height = GaugeHeight;
    }
}