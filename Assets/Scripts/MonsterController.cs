using UnityEngine;
using UnityEngine.AI;

public class MonsterController : MonoBehaviour
{
    ////// PATHFINDING //////

    public NavMeshAgent agent;

    public static Transform player;
    static PlayerHealth playerHealth;

    public LayerMask whatIsGround, whatIsPlayer, combinedLayerMask;

    // Patrolling
    [SerializeField] Vector3 walkPoint;
    private bool walkPointSet;
    [SerializeField] private int walkPointMinTime, stillMinTime;
    private float walkPointSetTime, stillTime;
    [SerializeField] float walkPointRange, stayTime;
    private float timeSinceStay;

    // Movement speeds
    [SerializeField] float walkSpeed = 2f;
    [SerializeField] float runSpeed = 5f;

    // Attacking
    [System.Serializable]
    public class EnemyAttack
    {
        public string animation, sound;
        public float damage;

        public Transform origin;
        public float zOffset, radius, rotationOffset;
    }

    [SerializeField] EnemyAttack[] attacks;
    private EnemyAttack activeAttack;
    [SerializeField] float timeBetweenAttacks;
    public bool alreadyAttacked;

    // States
    [SerializeField] float sightRange, attackRange;
    [SerializeField] bool playerInAttackRange, playerInSight;
    [SerializeField] Vector3 eyeHeight;

    // Chasing
    [SerializeField] float lostSightCooldown = 3f;
    private float lostSightTimer;

    ////// ANIMATING //////

    public Animator animator;
    private static int walkingHash, runningHash, attackingHash;

    private void Start()
    {
        if (player == null)
        {
            player = GameObject.Find("Player").transform;
            Debug.Log(player ? "Player found" : "Player not found");

            playerHealth = player.GetComponent<PlayerHealth>();

            walkingHash = Animator.StringToHash("isWalking");
            runningHash = Animator.StringToHash("isRunning");
            attackingHash = Animator.StringToHash("isAttacking");
        }

        agent.speed = walkSpeed; // Set default to walking speed
    }

    private void Update()
    {
        if (player == null)
        {
            enabled = false;
            return;
        }

        Vector3 toPlayer = (player.position - transform.position).normalized;
        float dotProduct = Vector3.Dot(transform.forward, toPlayer); // Dot product to check direction

        // Check for sight and attack range
        playerInAttackRange = Vector3.Distance(transform.position, player.position) <= attackRange;

        bool playerIsInFront = dotProduct > 0.3f; // Only detect if player is in front (adjust threshold if needed)

        if (playerIsInFront)
        {
            if (Physics.Raycast(transform.position + eyeHeight, toPlayer, out RaycastHit hitInfo, sightRange, combinedLayerMask))
            {
                playerInSight = hitInfo.transform == player;
            }
        }
        else
        {
            playerInSight = false; // Ignore player if they are behind
        }

        // Debug visualization
        Color rayColor = playerInSight ? Color.green : Color.red;
        Debug.DrawRay(transform.position + eyeHeight, toPlayer * sightRange, rayColor);

        // Handle states
        if (!playerInSight && !playerInAttackRange)
        {
            if (lostSightTimer > 0)
            {
                // Continue chasing if the grace period is active
                lostSightTimer -= Time.deltaTime;
                ChasePlayer();
            }
            else
            {
                Patroling(); // Patrol after the cooldown
            }
        }
        else if (playerInSight && !playerInAttackRange)
        {
            ChasePlayer();
            lostSightTimer = lostSightCooldown; // Reset the timer when the player is in sight
            return;
        }
        else if (playerInSight && playerInAttackRange)
        {
            StartAttack();
        }

        animator.SetBool(runningHash, false);
        animator.SetBool(walkingHash, agent.velocity.magnitude > 0);
    }

    void OnDrawGizmos()
    {
        if (activeAttack == null) return;
        Gizmos.color = Color.red;
        //Gizmos.DrawSphere(transform.forward * activeAttack.zOffset + transform.position, activeAttack.radius);
    }

    private void Patroling()
    {
        agent.speed = walkSpeed; // Walk during patrol

        if (!walkPointSet && timeSinceStay <= 0)
        {
            SearchWalkPoint();
        }

        if (walkPointSet)
        {
            agent.SetDestination(walkPoint);
            FaceMovementDirection(agent.velocity);
        }
        else
        {
            agent.velocity = Vector3.zero;
        }

        if (agent.velocity.magnitude > 0)
        {
            if (walkPointSetTime > 0)
            {
                walkPointSetTime -= Time.deltaTime;
            }
            else
            {
                walkPointSetTime = walkPointMinTime;
                walkPointSet = false;
                SearchWalkPoint();
            }
        }
        else
        {
            if (stillTime > 0)
            {
                stillTime -= Time.deltaTime;
            }
            else
            {
                stillTime = stillMinTime;
                walkPointSet = false;
                SearchWalkPoint();
            }
        }

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        //Walk point reached
        if (distanceToWalkPoint.magnitude < 1f)
        {
            walkPointSet = false;
            timeSinceStay -= Time.deltaTime;
        }
    }

    private void SearchWalkPoint()
    {
        //Calculate random point in range
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, out RaycastHit hitInfo, 2f, whatIsGround))
        {
            walkPointSet = true;
            timeSinceStay = stayTime;
            walkPointSetTime = walkPointMinTime;
        }
    }

    private void ChasePlayer()
    {
        agent.speed = runSpeed; // Run when chasing
        agent.SetDestination(player.position);
        FaceMovementDirection(agent.velocity);
        animator.SetBool(walkingHash, false);
        animator.SetBool(runningHash, agent.velocity.magnitude > 0);
    }

    private void FaceMovementDirection(Vector3 movementDirection)
    {
        if (movementDirection.magnitude > 0.1f)
        {
            // Calculate the rotation to face the movement direction
            Quaternion toRotation = Quaternion.LookRotation(movementDirection.normalized);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, agent.angularSpeed * Time.deltaTime);
        }
    }

    private void StartAttack()
    {
        if (playerHealth.dead) return;

        if (!alreadyAttacked)
        {
            // Pick a new attack every time
            activeAttack = attacks[Random.Range(0, attacks.Length)];

            animator.SetBool(activeAttack.animation, true);
            enabled = false;

            alreadyAttacked = true;
        }

        agent.SetDestination(transform.position);

        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
        if (activeAttack != null)
            transform.rotation *= Quaternion.Euler(0, activeAttack.rotationOffset, 0);
    }

    public void Attack()
    {
        if (Physics.CheckSphere(activeAttack.origin.position + activeAttack.zOffset * transform.forward, activeAttack.radius, whatIsPlayer))
        {
            playerHealth.TakeDamage(activeAttack.damage);
        }
    }

    public void EndAttack()
    {
        enabled = true;
        Invoke(nameof(ResetAttack), timeBetweenAttacks);
        animator.SetBool(activeAttack.animation, false);
        //activeAttack = null;
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    public AudioSource audioSource;
    public void PlaySound(string name)
    {
        //Audio.PlaySound(name, audioSource);
    }

    public void Disable()
    {
        agent.SetDestination(transform.position);
        animator.SetBool(walkingHash, false);
        animator.SetBool(runningHash, false);
        animator.SetBool(attackingHash, false);
    }
}
