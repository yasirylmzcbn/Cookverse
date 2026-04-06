using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class EnemyMovementAI : MonoBehaviour
{
    private Transform player;
    private NavMeshAgent navMeshAgent;
    private Animator anim;
    private Enemy attackScript;
    bool moving = true;
    float lastSpeed = -1f;

    void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        attackScript = GetComponent<Enemy>();
        anim = GetComponentInChildren<Animator>();

        // Stops a bit closer to the player than the attack range
        navMeshAgent.stoppingDistance = attackScript.AttackRange * 0.9f;
    }

    void Update()
    {
        if (player == null) return;

        navMeshAgent.SetDestination(player.position);
        moving = true;

        // If agent has reached stopping distance, face player
        if (
            //!navMeshAgent.pathPending &&
            navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
        {
            Vector3 targetPos = player.position;
            targetPos.y = transform.position.y;
            transform.LookAt(targetPos);
            moving = false;
        }

        float speedValue = moving ? 1f : 0f;
        if (anim != null && speedValue != lastSpeed)
        {
            Debug.Log(this + " speed changed to " + speedValue);
            anim.SetFloat("Speed", speedValue);
            lastSpeed = speedValue;
        }
    }
}