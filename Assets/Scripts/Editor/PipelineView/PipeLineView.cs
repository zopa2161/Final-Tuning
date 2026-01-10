using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class PipelineView
{
    // 외부(Window)에서 이 뷰를 붙일 수 있게 Root 노출
    public VisualElement Root { get; private set; }
    
    private VisualElement _engineModule;

    private VisualElement _transmissionModule;
    private VisualElement _wheelModule;
    private VisualElement _inputModule; 
    private VisualElement _brakeModule;
    
    private InputWidget _inputWidget;
    private EngineWidget _engineWidget;
    private TransmissionWidget _transmissionWidget;
    private WheelWidget _wheelWidget;
    private BrakeWidget _brakeWidget;
    
    public EngineWidget EngineWidget => _engineWidget;
    public TransmissionWidget TransmissionWidget => _transmissionWidget;
    public WheelWidget WheelWidget => _wheelWidget;
    public BrakeWidget BrakeWidget => _brakeWidget;

    public InputWidget InputWidget => _inputWidget;
    public PipelineView(VisualTreeAsset template)
    {
        Root = template.CloneTree();
        //=== 바퀴 템플릿 세팅===
        //1. 바퀴 템플릿을 가져온다.
        //2. 클론해서 살린뒤 그 값을 넘겨서 wheelUnitWidget를 생성한다.
        //3. add한다.

        var wheelTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
            "Assets/Scripts/Editor/PipelineView/Widget/WheelModule.uxml");
        _wheelModule = wheelTemplate.CloneTree();
        Root.Q<VisualElement>("wheel-module-container").Add(_wheelModule);
    
        //=== 모듈 찾아주기===
        _engineModule = Root.Q<VisualElement>("engine-module");
        _transmissionModule = Root.Q<VisualElement>("transmission-module");
        _inputModule = Root.Q<VisualElement>("input-module");
        _brakeModule = Root.Q<VisualElement>("brake-module");
        //=== 위젯 클래스 생성==
        _engineWidget = new EngineWidget(_engineModule);
        _transmissionWidget = new TransmissionWidget(_transmissionModule);
        _wheelWidget= new WheelWidget(_wheelModule);
        _inputWidget = new InputWidget(_inputModule);
        _brakeWidget = new BrakeWidget(_brakeModule);

    }


    
}