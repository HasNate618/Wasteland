using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;

public class InputHandler : MonoBehaviour
{
    public static InputHandler instance;
    public static int heartRate, joystickX, joystickY, button, flex, accelX, accelY, accelZ;
    public TextMeshProUGUI heartRateText;

    // Start is called before the first frame update
    void Start()
    {
        if (instance == null) instance = this;
    }

    // Method to parse the received hex data
    public static void ParseHexData(string hexData)
    {
        // Split the hex string into individual hex values
        string[] hexValues = hexData.Split('-');
        byte[] bytes = new byte[hexValues.Length];

        // Convert hex string to byte array
        for (int i = 0; i < hexValues.Length; i++)
        {
            bytes[i] = Convert.ToByte(hexValues[i], 16);
        }

        // Convert the byte array to a string (ASCII interpretation)
        string dataString = Encoding.ASCII.GetString(bytes);

        string[] data = dataString.Split(',');
        Debug.Log($"Data: {dataString}");

        heartRate = int.Parse(data[0]);
        instance.heartRateText.text = "BMP: " + heartRate.ToString();

        joystickX = int.Parse(data[1]);
        joystickY = int.Parse(data[2]);
    }
}
