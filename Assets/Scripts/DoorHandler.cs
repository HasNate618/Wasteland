using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorHandler : MonoBehaviour
{
    private Animator animator;
    public bool isOpen;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    [ContextMenu("Open")]
    public void OpenDoor()
    {
        if ((Inventory.keys > 0) && !isOpen)
        {
            Inventory.keys--;
            Inventory.instance.UpdateUI();
            animator.SetTrigger("OpenDoor");
            isOpen = true;
            Debug.Log("Door opened!");
        }
        else
        {
            Debug.Log("You need a key to open this door!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
