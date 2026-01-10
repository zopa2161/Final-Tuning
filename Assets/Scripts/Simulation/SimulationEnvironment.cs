using UnityEngine;

public class SimulationEnvironment : MonoBehaviour
{
    
    [SerializeField]
    private string name;

    [SerializeField]
    private Transform spawn;

    [SerializeField]
    private bool isSimualtion;
    
    public string Name => name;

    public Transform Spawn => spawn;

    public bool IsSimualtion
    {
        get => isSimualtion;
        private set => isSimualtion = value;
    }
}