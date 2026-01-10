using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;

public class MetricGraph : VisualElement
{
    private class GraphLine
    {
        public MetricEntry DataEntry; // Y축 데이터
        public MetricEntry TimeEntry; // X축 데이터 (보통 세션마다 시간축이 다를 수 있음)
        public Color LineColor;
    }
    
    
    private float _timeWindow = 10.0f; // 최근 10초만 표시
    private float _minValue,_maxValue;

    public MetricGraph(string name, float min, float max)
    {
        _minValue = min;
        _maxValue = max;

        style.flexGrow = 1;
        style.flexShrink = 0;
        // 1. [Main Container] 설정
        // 전체 높이를 조금 늘림 (제목 공간 포함)
        style.marginBottom = 5;
        style.flexDirection = FlexDirection.Column; // 위아래로 쌓기
    
        // 2. [Title] 그래프 상단 이름 추가
        var titleLabel = new Label(name)
        {
            style = 
            {
                fontSize = 12,
                color = Color.white,
                unityFontStyleAndWeight = FontStyle.Bold,
                marginBottom = 2, // 그래프와의 간격
                marginLeft = 2,
                flexGrow = 0
            }
        };
        Add(titleLabel);
        
        style.height = 150; // 그래프 높이 고정
        style.backgroundColor = new Color(0.1f, 0.1f, 0.1f);
        style.borderTopColor = Color.gray;
        style.borderTopWidth = 1;
        style.borderBottomWidth = 1;
        style.borderBottomColor = Color.gray;
        style.overflow = Overflow.Hidden; // 그래프 튀어나감 방지
        style.flexShrink = 0;
        style.flexGrow = 1;


        // 4. Min/Max 레이블은  안에 넣어야 함
        var nameLabel = new Label(name) {
            style = { position = Position.Absolute, top = 2, left = 2, color = Color.gray, fontSize = 10 }
        };
        var maxLabel = new Label($"{_maxValue:F0}") {
            style = { position = Position.Absolute, top = 2, right = 2, color = Color.gray, fontSize = 10 }
        };
        var minLabel = new Label($"{_minValue:F0}") {
            style = { position = Position.Absolute, bottom = 2, right = 2, color = Color.gray, fontSize = 10 }
        };
        Add(nameLabel);
        Add(maxLabel);
        Add(minLabel);
        
        // 5. [중요] 그리기 이벤트를 에 연결
        generateVisualContent += OnGenerateVisualContent;
        
    }
    
    //===상태값===
    private float _focusTime = 0f;
    
    // [핵심] 선 추가 메서드
    public void AddLine(MetricEntry time, MetricEntry data, Color color)
    {
        _lines.Add(new GraphLine { TimeEntry = time, DataEntry = data, LineColor = color });
        MarkDirtyRepaint(); // 다시 그려!
    }

    private void OnGenerateVisualContent(MeshGenerationContext ctx)
    {
        
        Rect rect = ctx.visualElement.contentRect; // 혹은 .contentRect
        
        if (float.IsNaN(rect.width) || float.IsNaN(rect.height) || rect.width < 1f || rect.height < 1f)
        {
           
            return; 
        }
        
        var painter = ctx.painter2D;
        float width = contentRect.width;
        float height = contentRect.height;
        //Rect rect = new Rect(0, 0, width, height);
        
        painter.fillColor = Color.black;  
        painter.BeginPath();  
        painter.MoveTo(new Vector2(0, 0));  
        painter.LineTo(new Vector2(rect.width, 0));  
        painter.LineTo(new Vector2(rect.width, rect.height));  
        painter.LineTo(new Vector2(0, rect.height));  
        painter.ClosePath();  
        painter.Fill();  
        // 2. [추가] 회색 그리드 그리기 (데이터보다 뒤에 있어야 함)
        DrawGrid(painter, width, height);

        foreach (var line in _lines)
        {
            DrawSingleLine(painter,line, width, height);
        }
        painter.strokeColor = new Color(1, 1, 1, 0.5f); // 반투명 흰색
        painter.lineWidth = 2.0f;
        painter.BeginPath();
        painter.MoveTo(new Vector2(width / 2f, 0)); // 상단 중앙
        painter.LineTo(new Vector2(width / 2f, height)); // 하단 중앙
        painter.Stroke();
        
    }
    
    private List<GraphLine> _lines = new List<GraphLine>();

    public void AddLine(MetricEntry time, MetricEntry data)
    {
        var line = new GraphLine()
        {
            DataEntry = data,
            TimeEntry = time,
            LineColor = _lines.Count >0? Color.darkGoldenRod : Color.limeGreen
        };
        
        _lines.Add(line);
    }

    // [추가된 그리드 그리기 함수]
    private void DrawGrid(Painter2D painter, float width, float height)
    {
        painter.strokeColor = new Color(0.3f, 0.3f, 0.3f); // 어두운 회색
        painter.lineWidth = 1.0f;
        painter.BeginPath();

        // 가로선 (Horizontal) - 4등분 (25%, 50%, 75%)
        int hDivs = 4;
        for (int i = 1; i < hDivs; i++)
        {
            float y = (height / hDivs) * i;
            painter.MoveTo(new Vector2(0, y));
            painter.LineTo(new Vector2(width, y));
        }

        // 세로선 (Vertical) - 10초 윈도우니까 1초마다 (10등분)
        // 오실로스코프처럼 화면에 고정된 그리드를 그립니다.
        int vDivs = 10;
        for (int i = 1; i < vDivs; i++)
        {
            float x = (width / vDivs) * i;
            painter.MoveTo(new Vector2(x, 0));
            painter.LineTo(new Vector2(x, height));
        }

        painter.Stroke();
    }

    public void Refresh() => MarkDirtyRepaint();
    
    public void SetFocusTime(float time)
    {
        _focusTime = time;
        //Debug.Log("focusTime : " + time);
        MarkDirtyRepaint(); // 다시 그리기 요청
    }
    
    
    private void DrawSingleLine(Painter2D painter, GraphLine line, float w, float h)
    {
        if (line.TimeEntry == null || line.DataEntry == null || line.TimeEntry.Values.Count < 2) return;

        painter.strokeColor = line.LineColor;
        painter.lineWidth = 2.0f;
        painter.BeginPath();

        // 4. 범위 계산
        float lastTime = _focusTime + (_timeWindow/ 2f);
        float startTime = _focusTime - (_timeWindow/ 2f);
        var timeRange = _timeWindow;
        float rangeY = _maxValue - _minValue;
        if (rangeY <= 0.0001f) rangeY = 1f;

        bool isStarted = false;
        int count = line.TimeEntry.Values.Count;

        // 5. [중요] 정방향 루프 (0 -> Count)
        // Painter2D는 펜을 움직이는 방식이라 순서대로 그려야 합니다.
        // 역순으로 그리면 선이 꼬이거나 면이 채워지는 버그가 생깁니다.

        for (int i = 0; i < count; i++)
        {
            float t = line.TimeEntry.Values[i];
            
            if (t < startTime - 1.0f || t > lastTime + 1.0f) continue;
            // 아직 화면 왼쪽 밖인 데이터는 건너뜀

            float val = line.DataEntry.Values[i];

            // 좌표 변환
            float x  = ((t - startTime) / timeRange) * w;
            float normalizedY = (val - _minValue) / rangeY;
            float y = h * (1.0f - Mathf.Clamp01(normalizedY)); // Clamp로 튀는거 방지

            if (!isStarted)
            {
                painter.MoveTo(new Vector2(x, y));
                isStarted = true;
            }
            else
            {
                painter.LineTo(new Vector2(x, y));
            }
        }

        painter.Stroke();
    }
}