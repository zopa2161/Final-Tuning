using System;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TrackModule : MonoBehaviour
{
    [SerializeField] private Checker[] _checkers;
    public Checker[] Checkers => _checkers;

    [SerializeField] private GameObject[] _leftWalls;
    [SerializeField] private GameObject[] _rightWalls;
    
    [SerializeField] private bool _isWallTagSwapped;
    [SerializeField] public bool _isReverse;

#if UNITY_EDITOR
    public event Action OnReverseChanged;
    private bool _previousReverseState;

    private void OnValidate()
    {
        if (Application.isPlaying) return;
        if (_isReverse != _previousReverseState)
        {
            OnReverseChanged?.Invoke();
            _previousReverseState = _isReverse;
        }
        UnityEditor.EditorApplication.delayCall += UpdateWallTags;
    }

    private void OnDrawGizmos()
    {
        DrawWallColliders();
    }

    private void DrawWallColliders()
    {
        Color leftColor = _isWallTagSwapped ? Color.blue : Color.red;
        Color rightColor = _isWallTagSwapped ? Color.red : Color.blue;

        if (_leftWalls != null)
        {
            Gizmos.color = leftColor;
            DrawCollidersFor(_leftWalls);
        }

        if (_rightWalls != null)
        {
            Gizmos.color = rightColor;
            DrawCollidersFor(_rightWalls);
        }
    }

    private void DrawCollidersFor(GameObject[] walls)
    { 
        // Draw the MeshCollider wireframe for each wall object
        foreach (var wallObject in walls)
        {
            if (wallObject == null) continue;

            MeshCollider meshCollider = wallObject.GetComponent<MeshCollider>();
            if (meshCollider != null && meshCollider.enabled && meshCollider.sharedMesh != null)
            {
                Gizmos.matrix = wallObject.transform.localToWorldMatrix;
                Gizmos.DrawWireMesh(meshCollider.sharedMesh);
            }
        }
        Gizmos.matrix = Matrix4x4.identity;
    }


    private void UpdateWallTags()
    {
        
        string leftTag = _isWallTagSwapped ? "RightWall" : "LeftWall";
        string rightTag = _isWallTagSwapped ? "LeftWall" : "RightWall";

        if (_leftWalls != null)
        {
            foreach (var wall in _leftWalls)
            {
                if(wall) wall.tag = leftTag;
            }
        }

        if (_rightWalls != null)
        {
            
            foreach (var wall in _rightWalls)
            {
                if(wall) wall.tag = rightTag;
            } 
        }
    }
#endif
}
