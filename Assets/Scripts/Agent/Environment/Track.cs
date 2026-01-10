using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;

public class Track : MonoBehaviour
{
    [SerializeField] private TrackModule[] _trackModules;
    private List<Checker> _checkers = new List<Checker>();

#if UNITY_EDITOR
    // OnValidate is called when the script is loaded or a value is changed in the inspector.
    // With the manual setup button, this might become redundant or cause too frequent updates.
    // private void OnValidate()
    // {
    //     Setup();
    // }
#endif
    

    private void OnDisable()
    {
        // Unsubscribe from module changes only in the editor
#if UNITY_EDITOR
        if(_trackModules != null)
        {
            foreach (var module in _trackModules)
            {
                if(module != null) module.OnReverseChanged -= Setup;
            }
        }
#endif
    }

    [Button("Setup Track", ButtonSizes.Large)]
    public void Setup()
    {
        if (_trackModules == null) return;

        _checkers.Clear();
        foreach (var module in _trackModules)
        {
            if (module == null) continue;
            
            var moduleCheckers = module.Checkers;
            if (moduleCheckers == null) continue;

            if (module._isReverse)
            {
                _checkers.AddRange(moduleCheckers.Reverse().Where(c => c != null));
            }
            else
            {
                _checkers.AddRange(moduleCheckers.Where(c => c != null));
            }
        }
        if(_trackModules != null)
        {
            foreach (var module in _trackModules)
            {
                if(module != null) module.OnReverseChanged += Setup;
            }
        }

        for (int i = 0; i < _checkers.Count; i++)
        {
            _checkers[i].Setup(i);
        }
        
        // In editor, force scene view to update to show new checker numbers
#if UNITY_EDITOR
        UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
        UnityEditor.SceneView.RepaintAll();
#endif
    }
}
