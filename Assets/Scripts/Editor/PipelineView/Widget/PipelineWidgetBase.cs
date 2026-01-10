// 뷰 내부의 작은 부품을 담당하는 기본 클래스

using UnityEngine.UIElements;

public abstract class PipelineWidgetBase
{
    protected VisualElement _root;

    public PipelineWidgetBase(VisualElement root)
    {
        _root = root;
    }

    public abstract void Update(VehicleContext context);
}