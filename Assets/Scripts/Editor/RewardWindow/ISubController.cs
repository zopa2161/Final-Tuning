using UnityEngine.UIElements;

// 모든 서브 컨트롤러의 규격
public interface ISubController
{
    // root: 이 컨트롤러가 그림을 그릴 UI 부모 요소
    void Initialize(VisualElement root);

}