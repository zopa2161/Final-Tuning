using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class BaseCarModuleNode : Node
{
    public string GUID; // 저장/로드용 식별자
    public BaseModule ModuleData; // 연결된 SO 데이터

    protected VehicleContext _context;
    
    private Dictionary<Port, List<Edge>> _portEdgeMap = new Dictionary<Port, List<Edge>>();
    // 생성자
    public BaseCarModuleNode()
    {
        
        
        GUID = System.Guid.NewGuid().ToString();
        this.AddToClassList("car-node");
    }

    // [Helper] 포트 생성 함수 (매번 쓰기 귀찮으니까)
    protected Port CreatePort(string portName, Direction direction, Port.Capacity capacity, System.Type type)
    {
        // 마지막 인자로 타입을 넘겨줍니다.
        Port port = InstantiatePort(Orientation.Horizontal, direction, capacity, type);
    
        port.portName = portName;
        return port;
    }

    public virtual void Bind(ITunableVehicle vehicle)
    {
        _context = vehicle.Context;
    }
    //context에서 값을 찾아서 매핑.
    public abstract void Render();
    
    protected virtual FloatField CreateValueField()
    {
        var field = new FloatField();
        
        // 1. 값 수정 못하게 막음 (디버깅용이니까)
        field.isReadOnly = true; 
        
        // 2. 스타일 다듬기
        field.style.width = 60; // 너비 고정
        field.style.marginLeft = 5;
        
        // (선택) 테두리를 없애서 숫자만 깔끔하게 보이게 하려면:
        // field.AddToClassList("unity-base-field__input--no-border"); 
        
        return field;
    }
    
    public Port GetPort(string portName, Direction direction)
    {
        // Input 포트들은 inputContainer에, Output은 outputContainer에 있습니다.
        var container = direction == Direction.Input ? inputContainer : outputContainer;

        // container 자식들 중에서 Port 타입이면서 portName이 일치하는 것 찾기
        foreach (var element in container.Children())
        {
            if (element is Port port && port.portName == portName)
            {
                return port;
            }
        }
        return null;
    }
    
    //=== 엣지 커스텀 코드 ===
    // 1. Controller가 호출할 함수: 엣지 등록
    public void OnEdgeConnected(Port port, Edge edge)
    {
        if (!_portEdgeMap.ContainsKey(port))
        {
            _portEdgeMap[port] = new List<Edge>();
        }
        
        if (!_portEdgeMap[port].Contains(edge))
        {
            _portEdgeMap[port].Add(edge);
        }
    }

    // 2. Controller가 호출할 함수: 엣지 연결 해제 (정리용)
    public void OnEdgeDisconnected(Port port, Edge edge)
    {
        if (_portEdgeMap.ContainsKey(port))
        {
            _portEdgeMap[port].Remove(edge);
        }
    }

    // 3. [Render 내부에서 사용] 특정 포트의 엣지 색상/두께 변경
    protected void UpdatePortEdges(Port port, float value, float min, float max)
    {
        if (!_portEdgeMap.ContainsKey(port)) return;
        Debug.Log("UpdatePortEdges" + "Value : "+ value );
        // 값에 따른 색상 계산 (예: 파랑 -> 빨강)
        float t = Mathf.Clamp01((value - min) / (max - min));
        Color targetColor = Color.Lerp(Color.cyan, Color.red, t);
        
        // 두께 계산 (값이 클수록 두껍게, 2px ~ 10px)
        float thickness = Mathf.Lerp(2f, 10f, t);

        foreach (var edge in _portEdgeMap[port])
        {
            // [수정 포인트] edge.outputColor -> edge.edgeControl.outputColor
            // EdgeControl이 실제 렌더링을 담당하는 내부 객체입니다.
            if (edge.edgeControl != null)
            {
                edge.edgeControl.inputColor = targetColor;
                edge.edgeControl.outputColor = targetColor;
            
                // [옵션] 두께 변경 (Draw 시점의 선 두께)
                // Unity 버전에 따라 edgeControl.fromCapColor 등으로 캡 색상도 맞춰줘야 할 수 있음
            
                // *참고: edgeControl에는 public width 속성이 없을 수 있습니다.
                // 그럴 땐 style을 건드려야 하는데, 실시간 업데이트로는 색상 변경만으로도 충분히 효과적입니다.
            }

            // 3. 다시 그리기 요청 (이걸 해야 화면이 갱신됨)
            edge.MarkDirtyRepaint();
        }
    }
}