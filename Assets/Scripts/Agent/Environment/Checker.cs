using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Checker : MonoBehaviour
{
    private int _checkerNumber;

    public void Setup(int number)
    {
        _checkerNumber = number;
    }

    public int GetCheckerNumber()
    {
        return _checkerNumber;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // Draw the checker number above the checker's position
        string label = $"{_checkerNumber}";
        Handles.Label(transform.position + Vector3.up, label);
    }
#endif
}