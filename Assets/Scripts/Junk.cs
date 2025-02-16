using UnityEngine;

public class Junk : MonoBehaviour
{
    public float interactionRange = 5f; // Distance within which the player can interact
    public float fieldOfVisionAngle = 60f; // Angle of the object's field of vision (FOV)
    public Transform player; // The player's transform

    public Mesh[] meshes;
    public MeshFilter meshFilter, handMesh;
    private Mesh activeMesh;

    public GameObject interactButton;

    void Start()
    {
        if (player == null)
        {
            player = GameObject.Find("Player").transform; // Assign the player if not manually set
            if (player == null)
            {
                Debug.LogError("Player object not found.");
                return;
            }
        }

        // Get the MeshFilter component attached to the GameObject
        meshFilter = GetComponent<MeshFilter>();

        if (meshFilter != null && meshes.Length > 0)
        {
            // Select a random mesh from the array
            Mesh selectedMesh = meshes[Random.Range(0, meshes.Length)];

            // Apply the selected mesh to the MeshFilter
            meshFilter.mesh = selectedMesh;
            activeMesh = selectedMesh;
            Debug.Log($"Selected mesh: {selectedMesh.name}");
        }
        else
        {
            Debug.LogWarning("MeshFilter not found or no meshes assigned.");
        }
    }

    void Update()
    {
        bool interact = Input.GetKey(KeyCode.E);
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, transform.position - Camera.main.transform.position, out hit, 2f))
        {
            if (hit.collider.gameObject == gameObject)
            {
                // Listen for the interaction button press
                interactButton.SetActive(true);
                if (interact) Interact();
                return;
            }
        }
        interactButton.SetActive(false);
    }

    void Interact()
    {
        // Interaction logic (e.g., open a door, pickup an item, etc.)
        Debug.Log("Interacting with junk...");
        interactButton.SetActive(false);
        Inventory.scraps++;
        Inventory.instance.UpdateUI();
        handMesh.mesh = activeMesh;
        Destroy(gameObject);
    }
}
