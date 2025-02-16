using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class BearTrap : MonoBehaviour
{
    [SerializeField] private float trapDuration = 3f; // Time the monster stays trapped

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Monster")) // Ensure the monster has the correct tag
        {
            Debug.Log($"{other.name} triggered the bear trap!");

            NavMeshAgent agent = other.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                StartCoroutine(TrapMonster(agent));
            }

        }
    }

    private IEnumerator TrapMonster(NavMeshAgent agent)
    {
        MonsterController monsterController = agent.gameObject.GetComponentInChildren<MonsterController>();

        //agent.isStopped = true; // Stop movement
        //agent.enabled = false; // Disable NavMeshAgent to prevent movement updates
        monsterController.enabled=false; // Disable MonsterController
        monsterController.Disable(); // Disable MonsterController

        Debug.Log($"{agent.gameObject.name} is trapped for {trapDuration} seconds.");

        yield return new WaitForSeconds(trapDuration); // Wait for trap duration

        //agent.enabled = true; // Re-enable movement
        //agent.isStopped = false; // Resume movement
        monsterController.enabled=true; // Enable MonsterController

        Debug.Log($"{agent.gameObject.name} is free to move again!");
        Destroy(gameObject); // Destroy the bear trap
    }
}
