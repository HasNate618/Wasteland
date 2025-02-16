using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;
    [SerializeField] float playerSpeed, rotateSpeed;
    public int doorLayer;
    public GameObject openDoorButton;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Detect door
        if (Physics.Raycast(Camera.main.transform.position, transform.forward, out RaycastHit hitInfo, 2f))
        {
            if (hitInfo.collider.gameObject.layer == 7)
            {
                DoorHandler doorHandler = hitInfo.transform.GetComponentInParent<DoorHandler>();
                if (!doorHandler.isOpen)
                {
                    openDoorButton.SetActive(true);
                    if (Input.GetKeyDown(KeyCode.E)) doorHandler.OpenDoor();
                }
            }
        }
        else openDoorButton.SetActive(false);

        // Get WASD input
        //float moveDirectionX = Input.GetAxis("Horizontal"); // A/D or Left/Right Arrow
        //float moveDirectionZ = Input.GetAxis("Vertical");   // W/S or Up/Down Arrow

        // Create movement vector relative to the camera orientation
        Vector3 move = transform.forward * (InputHandler.joystickX-512f)/512f;

        // Move the player
        controller.Move(move * playerSpeed * Time.deltaTime);

        // Mouse look
        //float mouseX = Input.GetAxis("Mouse X");
        transform.Rotate(Vector3.up * (InputHandler.joystickY-512f)/512f * rotateSpeed * Time.deltaTime);
    }

    /*
        // Start is called before the first frame update
        void Start()
        {
            controller = GetComponent<CharacterController>();
        }

        public static Vector2 GetDirection(int x, int y)
        {
            // Normalize x and y to a range of -1 to 1
            float normX = (x - 512) / 512f;
            float normY = (y - 512) / 512f;

            // Determine the primary direction
            if (Mathf.Abs(normX) > Mathf.Abs(normY))
            {
                return normX > 0 ? Vector2.right : Vector2.left;
            }
            else
            {
                return normY > 0 ? Vector2.up : Vector2.down;
            }
        }

        // Update is called once per frame
        void Update()
        {
            int stickX = 400, stickY = 500;
            Vector2 stickInput = GetDirection(stickX, stickY);

            //Debug.Log(stickInput);

            if ((stickInput.x == 0) && (stickInput.y == 0)) return;
            else if (stickInput.y == 1) controller.Move(transform.forward * Time.deltaTime * playerSpeed);
            else if (stickInput.y == -1) controller.Move(transform.forward * Time.deltaTime * playerSpeed);
            else if (stickInput.y == 0) transform.Rotate(new Vector3(0,(stickX-512) * Time.deltaTime * rotateSpeed, 0));
        }*/
}
