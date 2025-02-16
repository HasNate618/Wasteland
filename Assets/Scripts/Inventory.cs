using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static int scraps=0, keys=0, molotovs=0, bearTraps=0;
    [SerializeField] private TextMeshProUGUI scrapsCount, keysCount, molotovsCount, bearTrapsCount;

    public static Inventory instance;

    // Start is called before the first frame update
    void Start()
    {
        //UpdateUI();
        if (instance == null) instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateUI()
    {
        scrapsCount.text = scraps.ToString();
        keysCount.text = keys.ToString();
        molotovsCount.text = molotovs.ToString();
        bearTrapsCount.text = bearTraps.ToString();
    }
}
