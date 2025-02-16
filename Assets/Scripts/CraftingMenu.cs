using UnityEngine;
using UnityEngine.UI;

public class CraftingMenu : MonoBehaviour
{
    public GameObject parentObject, bearTrapPrefab; // Parent that holds the UI elements
    public Image[] images; // The three UI Image objects
    public Color highlightColor = Color.white;
    public Color defaultColor = Color.gray;
    public int rotation;
    private bool isTimePaused = false; // Track if time is paused or not

    void Update()
    {
        // Map range (0-300) and highlight the correct image
        float value = Mathf.Clamp(rotation, 0, 300);
        int selectedIndex = Mathf.FloorToInt(value / 100);

        for (int i = 0; i < images.Length; i++)
        {
            images[i].color = (i == selectedIndex) ? highlightColor : defaultColor;
        }

        // Toggle the time and parent object's active state on T press
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (isTimePaused)
            {
                // Resume time and deactivate the parent object
                Time.timeScale = 1f;
                parentObject.SetActive(false);
                isTimePaused = false;

                switch (value)
                {
                    case 0:
                        Debug.Log("Crafting: Bear trap");
                        if (Inventory.scraps >=2)
                        {
                            Inventory.scraps -=2;
                            //Inventory.bearTraps++;
                            Inventory.instance.UpdateUI();

                            Instantiate(bearTrapPrefab, transform.position - transform.up, transform.rotation);
                        }
                        break;
                    case 1:
                        Debug.Log("Crafting: Key");
                        if (Inventory.scraps >= 4)
                        {
                            Inventory.scraps -=4;
                            Inventory.keys++;
                            Inventory.instance.UpdateUI();
                        }
                        break;
                    case 2:
                        Debug.Log("Crafting: Molotov");
                        if (Inventory.scraps >= 2)
                        {
                            Inventory.scraps -=2;
                            Inventory.molotovs++;
                            Inventory.instance.UpdateUI();
                        }
                        break;
                }
            }
            else
            {
                // Pause time and activate the parent object
                Time.timeScale = 0.1f;
                parentObject.SetActive(true);
                isTimePaused = true;
            }
        }

    }
}
