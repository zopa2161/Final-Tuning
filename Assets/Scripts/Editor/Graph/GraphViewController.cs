using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class GraphViewController
{
    private const float NODE_WIDTH = 400, NODE_HEIGHT = 300;
  
    private const float CX = 400f; // 캔버스 시작 X (중앙점)
    private const float CY = 300f; // 캔버스 시작 Y (중앙점)
    private const float GAP_X = 300f; // 좌우 간격 (차폭)
    private const float GAP_Y = 200f; // 상하 간격 (길이)
    
    private CarDebugGraph _graphView;

    private VisualElement _graphContainer;

    private Dictionary<Type, Type> _nodeTypeMap = new Dictionary<Type, Type>()
    {
        {typeof(EngineModule), typeof(EngineNode)},
        {typeof(TransmissionModule), typeof(TransmissionNode)},
        {typeof(DrivetrainModule), typeof(DrivetrainNode)},
        {typeof(BrakeModule), typeof(BrakeNode)},
        {typeof(SteeringModule), typeof(SteeringNode)}
    };
    private List<BaseCarModuleNode> _activeNodes = new List<BaseCarModuleNode>();
    private Dictionary<Type,BaseCarModuleNode> _activeNodeMap = new Dictionary<Type,BaseCarModuleNode>();
    private List<WheelNode> _wheelNodes = new List<WheelNode>();
    
    public GraphViewController()
    {
        _graphView = new CarDebugGraph();
    }
    public void Initialize(VisualElement root)
    {
        _graphContainer = root.Q<VisualElement>("bottom-metrics");
        _graphContainer.Add(_graphView);
    }
    //===Test 버튼===
    public void SetTestButton(Action provider)
    {
        _graphView.SetTestButton(provider);
    }

  
    //여기서는 차량이 바뀐 경우를 처리.(초기 처리도 가능)
    public void OnSetTargetCar(ITunableVehicle targetCar)
    {
        _graphView.DeleteElements(_graphView.nodes);
        _activeNodes.Clear();
        _wheelNodes.Clear();

        foreach (var kvp in targetCar.ModuleMap)
        {
            var moduleType = kvp.Key;
            if (_nodeTypeMap.TryGetValue(moduleType, out Type nodeClassType))
            {
                var node = (BaseCarModuleNode)Activator.CreateInstance(nodeClassType);
                node.Bind(targetCar);
                var position = GetFixedPosition(node); 
                node.SetPosition(position);
                _graphView.AddElement(node);
                _activeNodes.Add(node);
                _activeNodeMap.Add(moduleType, node);
            }
        }

        for (int i = 0; i < 4; i++)
        {
            CreateWheelNode(i,targetCar);
            
        }
        //엣지 강제 배정.
        BindVehicle(targetCar);
    }
    
    

    // [핵심] 두 노드의 포트를 강제로 연결하는 함수
    private void ConnectPorts(BaseCarModuleNode outputNode, string outputPortName, 
        BaseCarModuleNode inputNode, string inputPortName)
    {
        // 1. 포트 찾기
        Port outPort = outputNode.GetPort(outputPortName, Direction.Output);
        Port inPort = inputNode.GetPort(inputPortName, Direction.Input);
        if(outPort ==null) Debug.Log("No Port");
        if (outPort == null || inPort == null) return;

        // 2. 엣지 생성 (Unity API 사용)
        Edge edge = outPort.ConnectTo(inPort);

        // 3. 그래프에 추가 (이걸 해야 눈에 보임)
        _graphView.AddElement(edge);
        
        // 3. [핵심] Source Node에게 "이 엣지는 네 데이터(RPM 등)를 표현해야 해"라고 등록
        // 보통 데이터 흐름은 Out -> In 이므로, SourceNode가 엣지 색상을 제어하는 것이 자연스럽습니다.
        outputNode.OnEdgeConnected(outPort, edge);

        // (필요하다면 TargetNode에도 등록 가능하지만, 보통 색상은 보내는 쪽 데이터 기준입니다)
    }
    
    public void BindVehicle(ITunableVehicle car)
    {
        // [자동 배선 작업]
        // 딕셔너리(_activeNodeMap)에 저장된 노드들을 꺼내서 연결

        if (_activeNodeMap.TryGetValue(typeof(EngineModule), out var engineNode) &&
            _activeNodeMap.TryGetValue(typeof(TransmissionModule), out var transNode))
        {
            ConnectPorts(engineNode,"Torque Out", transNode, "Torque In");
            ConnectPorts(engineNode,"Rpm Out", transNode, "Rpm In");

            
        }
        
        if (_activeNodeMap.TryGetValue(typeof(DrivetrainModule), out var drivetrainNode) &&
            _activeNodeMap.TryGetValue(typeof(TransmissionModule), out var transNode2))
        {
            ConnectPorts(transNode2, "Final Torque out", drivetrainNode, "Final Torque In");

            ConnectPorts(drivetrainNode, "Torque FL Out", _wheelNodes[0], "Torque In");
            ConnectPorts(drivetrainNode, "Torque FR Out", _wheelNodes[1], "Torque In");
            ConnectPorts(drivetrainNode, "Torque RL Out", _wheelNodes[2], "Torque In");
            ConnectPorts(drivetrainNode, "Torque RR Out", _wheelNodes[3], "Torque In");
        }
        if (_activeNodeMap.TryGetValue(typeof(BrakeModule), out var brake))
        {
            ConnectPorts(brake, "Brake Torque FL Out", _wheelNodes[0], "Brake Torque In");
            ConnectPorts(brake, "Brake Torque FR Out", _wheelNodes[1], "Brake Torque In");
            ConnectPorts(brake, "Brake Torque RL Out", _wheelNodes[2], "Brake Torque In");
            ConnectPorts(brake, "Brake Torque RR Out", _wheelNodes[3], "Brake Torque In");

        }

    }
    private void CreateWheelNode(int index, ITunableVehicle vehicle)
    {
        var node = new WheelNode(index);
        node.Bind(vehicle);

        // [핵심] 탑뷰 배치 좌표 계산 (하드코딩)
        // 기준점 (Engine/Trans 뒤쪽)
        float baseX = 1000f; 
        float baseY = 200f;
    
        float wheelGapX = 300f; // 축거 (Wheelbase) - 화면상 거리
        float wheelGapY = 400f; // 윤거 (Track Width) - 화면상 거리

        float x = baseX;
        float y = baseY;
        
        // X축 결정 (짝수=왼쪽, 홀수=오른쪽)
        // 0, 2 -> Left (-Gap)
        // 1, 3 -> Right (+Gap)
        float xDir = (index % 2 == 0) ? -1f : 1f;
        x = CX + (GAP_X * xDir);

        // Y축 결정 (0~1=앞, 2~3=뒤)
        // 0, 1 -> Front (-Gap)
        // 2, 3 -> Rear (+Gap)
        float yDir = (index < 2) ? -1f : 1f;
        y = CY + (GAP_Y * yDir);

        

        node.SetPosition(new Rect(x, y, 0, 0));
        _graphView.AddElement(node);
        
        _wheelNodes.Add(node);
        _activeNodes.Add(node);
    
        // (선택사항) 여기서 서스펜션/액슬 노드와 엣지를 연결해주면 더 완벽함
    }
    
    
    private Rect GetFixedPosition(BaseCarModuleNode node)
    {
        float x = CX;
        float y = CY;

        // C# 7.0 패턴 매칭 (Type 체크 + 변수 할당을 동시에)
        switch (node)
        {
            // 1. 타이어 (0:FL, 1:FR, 2:RL, 3:RR)
            case WheelNode tireNode:
                int index = tireNode.WheelIndex;

                // X축 결정 (짝수=왼쪽, 홀수=오른쪽)
                // 0, 2 -> Left (-Gap)
                // 1, 3 -> Right (+Gap)
                float xDir = (index % 2 == 0) ? -1f : 1f;
                x = CX + (GAP_X * xDir);

                // Y축 결정 (0~1=앞, 2~3=뒤)
                // 0, 1 -> Front (-Gap)
                // 2, 3 -> Rear (+Gap)
                float yDir = (index < 2) ? -1f : 1f;
                y = CY + (GAP_Y * yDir);
                break;

            // 2. 엔진 (앞바퀴 라인 중앙)
            case EngineNode _:
                x = CX;
                y = CY - GAP_Y; 
                break;

            // 3. 변속기 (정중앙)
            case TransmissionNode _:
                x = CX;
                y = CY;
                break;

            // 4. 구동계/차축 (뒷바퀴 라인 중앙)
            case DrivetrainNode _: // 혹은 AxleNode
                x = CX;
                y = CY + GAP_Y;
                break;

            // 5. 기타 (ECU, 브레이크 등은 오른쪽 구석으로)
            default:
                x = CX + GAP_X + 250f; // 오른쪽으로 쭉 뺌
                y = CY;
                break;
        }

        return new Rect(x, y, NODE_WIDTH, NODE_HEIGHT);
    }
    
    public void Tick()
    {
        foreach (var node in _activeNodes)
        {
            node.Render();
        }
    }


}