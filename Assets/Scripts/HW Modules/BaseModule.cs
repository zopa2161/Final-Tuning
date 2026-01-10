using UnityEngine;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Module/Base Module")]
public abstract class BaseModule : SerializedScriptableObject
{   
    [SerializeField, HideInInspector]
    protected int id;

    [SerializeField, HideInInspector]
    protected string moduleName;
    
    [SerializeField]
    private Sprite icon;

    // Odin으로 읽기 전용 표시
    [BoxGroup( "Module Info")]
    [ShowInInspector, ReadOnly, LabelText("Id"), PropertyOrder(-1)]
    public int Id
    {
        get => id;
        set => id = value;
    }

    // Odin에서 편집 가능, 값 변경 시 에셋 이름을 동기화
    [BoxGroup( "Module Info")]
    [ShowInInspector, LabelText("Name"), OnValueChanged(nameof(SyncAssetName))]
    public string Name
    {
        get => moduleName;
        set
        {
            moduleName = value;
#if UNITY_EDITOR
            // OnValueChanged가 호출되지 않는 상황 안전장치
            SyncAssetName();
#endif
        }
    }
    
    public Sprite Icon => icon;
    public int ID => id;
    public string ModuleName => moduleName;

    // 인스펙터에서 직접 변경했을 때도 이름 동기화
    private void OnValidate()
    {
        if (moduleName != name)
            name = moduleName;
    }

    public void CopyFrom(BaseModule source)
    {
        // 1. 타입 안전성 체크
        if (source.GetType() != this.GetType())
        {
            Debug.LogError($"[CopyFrom Error] 타입이 달라서 복사할 수 없습니다. Target: {this.GetType()}, Source: {source.GetType()}");
            return;
        }

        // 2. JSON 직렬화 -> 역직렬화로 값 덮어쓰기 (Deep Copy)
        // 유니티 내부 기능을 쓰므로 매우 빠르고 안정적입니다.
        string json = JsonUtility.ToJson(source);
        JsonUtility.FromJsonOverwrite(json, this);
    }
#if UNITY_EDITOR
    private void SyncAssetName()
    {
        if (moduleName == name) return;
        name = moduleName;
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
    }
#endif

    public abstract void Calculate(VehicleContext context);
    public abstract void OnCalculate();
    public abstract BaseModuleState CreateState();
    
    public virtual void InitializeState(BaseModuleState state, VehicleStateHub hub) { }
}