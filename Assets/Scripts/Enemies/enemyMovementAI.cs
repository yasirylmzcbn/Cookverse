using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class EnemyMovementAI : MonoBehaviour
{
    private Transform player;
    private NavMeshAgent navMeshAgent;
    private Enemy attackScript;

    void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        attackScript = GetComponent<Enemy>();

        // Stops a bit closer to the player than the attack range
        navMeshAgent.stoppingDistance = attackScript.AttackRange * 0.9f;
    }

    void Update()
    {
        if (player == null) return;

        navMeshAgent.SetDestination(player.position);

        // If agent has reached stopping distance, face player
        if (!navMeshAgent.pathPending &&
            navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
        {
            Vector3 targetPos = player.position;
            targetPos.y = transform.position.y;
            transform.LookAt(targetPos);
        }
    }
}