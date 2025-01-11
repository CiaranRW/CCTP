using UnityEngine;
using UnityEngine.AI;

public class NavMeshTesting : MonoBehaviour
{
    [SerializeField] private Transform movepositionTransform;

    private NavMeshAgent agent;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        agent.destination = movepositionTransform.position;
    }
}
