using UnityEngine;
using UnityEngine.AI;

public class EnemyBehaviour : MonoBehaviour
{
    public Transform target;

    private EnemyReferences enemyReferences;

    private float pathUpdateDeadline;

    private float shootingDistance;

    private void Awake()
    {
        enemyReferences = GetComponent<EnemyReferences>();
    }

    private void Start()
    {
        shootingDistance = enemyReferences.navMeshAgent.stoppingDistance;
    }
    void Update()
    {
        if (target != null)
        {
            bool inRange = Vector3.Distance(transform.position, target.position) <= shootingDistance;

            if (inRange)
            {
                LookAtTarget();
            }
            else
            {
                UpdatePath();
            }
        }
    }
    private void LookAtTarget()
    {
        Vector3 lookpos = target.position - transform.position;
        lookpos.y = 0f;
        Quaternion rotation = Quaternion.LookRotation(lookpos);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 0.2f);
    }
    private void UpdatePath()
    {
        if (Time.time >= pathUpdateDeadline)
        {
            Debug.Log("updating Path");
            pathUpdateDeadline = Time.time + enemyReferences.pathUpdateDelay;
            enemyReferences.navMeshAgent.SetDestination(target.position);
        }
    }
}
