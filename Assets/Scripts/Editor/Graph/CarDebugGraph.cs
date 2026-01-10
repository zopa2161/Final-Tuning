using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class CarDebugGraph : GraphView
{
    public CarDebugGraph()
    {
        // 1. 기본 설정 (줌, 팬, 드래그 등)
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        // 2. 배경 그리드 추가 (스타일)
        var grid = new GridBackground();
        Insert(0, grid);
        grid.StretchToParentSize();

        // 3. 스타일링 (전체 화면 꽉 채우기)
        this.StretchToParentSize();
        
    }
    
    // 노드 생성 헬퍼
    private Node CreateSimpleNode(string name, Vector2 pos)
    {
        var node = new Node { title = name };
        node.SetPosition(new Rect(pos, new Vector2(150, 100)));
        return node;
    }
    
    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        var compatiblePorts = new List<Port>();

        // 캔버스에 있는 모든 포트를 검사
        foreach (var endPort in ports)
        {
            // 1. 자기 자신과는 연결 불가
            if (startPort == endPort) continue;

            // 2. 같은 노드끼리는 연결 불가 (자기가 자기한테 꽂는 것 방지)
            if (startPort.node == endPort.node) continue;

            // 3. 입출력 방향이 달라야 함 (Input <-> Output)
            if (startPort.direction == endPort.direction) continue;

            // 4. [타입 체크] 포트의 타입이 같아야만 연결 허용!
            // (Engine의 Torque Out은 Trans의 Torque In하고만 연결됨)
            if (startPort.portType != endPort.portType) continue;
            
            

            // 모든 조건을 통과했으면 연결 가능한 포트임
            compatiblePorts.Add(endPort);
        }

        return compatiblePorts;
    }

    public void SetTestButton(Action provider)
    {
        var button = new Button(() => provider())
        {
            text = "Auto Input Test"
        };
        Add(button);
    }
}