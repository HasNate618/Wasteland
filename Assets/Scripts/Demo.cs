using System;
using System.Collections;
using UnityEngine;

public class BLEManager : MonoBehaviour
{
    private string targetDeviceID = "BluetoothLE#BluetoothLE6c:2f:80:bb:b0:d8-30:c6:f7:01:c2:fe"; // Target device ID
    private string targetServiceID = "{2f216107-06d1-4764-8cde-a91a755f0e7c}"; // Target service ID
    private string targetCharacteristicID = "{d66f7f33-24ae-4dc0-bf22-b9c54f92cdd0}"; // Target characteristic ID

    private bool isSubscribed = false;
    private bool deviceFound = false;

    void Start()
    {
        Debug.Log("Starting BLE device scan...");
        StartCoroutine(ScanForDevices());
    }

    private IEnumerator ScanForDevices()
    {
        BleApi.StartDeviceScan();
        yield return new WaitForSeconds(2f); // Allow time to discover devices

        BleApi.DeviceUpdate device = new BleApi.DeviceUpdate();
        while (BleApi.PollDevice(ref device, false) == BleApi.ScanStatus.AVAILABLE)
        {
            Debug.Log($"Found Device - Name: {device.name}, ID: {device.id}");

            // Check if the device matches the target ID
            if (device.id == targetDeviceID)
            {
                targetDeviceID = device.id;
                deviceFound = true;
                Debug.Log($"Target device found: {targetDeviceID}");
                break;
            }
        }

        BleApi.StopDeviceScan();

        if (deviceFound)
        {
            Debug.Log("Device found. Scanning for services...");
            StartCoroutine(ScanForServices());
        }
        else
        {
            Debug.LogError("Target device not found.");
        }
    }

    private IEnumerator ScanForServices()
    {
        if (string.IsNullOrEmpty(targetDeviceID))
        {
            Debug.LogError("No device selected. Cannot scan for services.");
            yield break;
        }

        BleApi.ScanServices(targetDeviceID);
        BleApi.Service service;
        float timeout = Time.time + 5f;

        while (Time.time < timeout)
        {
            if (BleApi.PollService(out service, false) == BleApi.ScanStatus.AVAILABLE)
            {
                Debug.Log($"Found service: {service.uuid}");

                if (service.uuid == targetServiceID)
                {
                    Debug.Log("Target service found. Scanning for characteristics...");
                    yield return StartCoroutine(ScanForCharacteristics());
                    yield break;
                }
            }
            yield return new WaitForSeconds(0.1f);
        }

        Debug.LogError("Service scan timed out.");
    }

    private IEnumerator ScanForCharacteristics()
    {
        BleApi.ScanCharacteristics(targetDeviceID, targetServiceID);
        BleApi.Characteristic characteristic;
        float timeout = Time.time + 5f;

        while (Time.time < timeout)
        {
            if (BleApi.PollCharacteristic(out characteristic, false) == BleApi.ScanStatus.AVAILABLE)
            {
                Debug.Log($"Found characteristic: {characteristic.uuid}");

                if (characteristic.uuid == targetCharacteristicID)
                {
                    Debug.Log("Target characteristic found. Subscribing...");
                    BleApi.SubscribeCharacteristic(targetDeviceID, targetServiceID, targetCharacteristicID, false);
                    isSubscribed = true;
                    Debug.Log("Successfully subscribed to characteristic!");
                    yield return StartCoroutine(ReadBLEData());
                    yield break;
                }
            }
            yield return new WaitForSeconds(0.1f);
        }

        Debug.LogError("Characteristic scan timed out.");
    }

    private IEnumerator ReadBLEData()
    {
        Debug.Log("Waiting for BLE data...");
        while (isSubscribed)
        {
            BleApi.BLEData res = new BleApi.BLEData();
            while (BleApi.PollData(out res, false))
            {
                string receivedData = BitConverter.ToString(res.buf, 0, res.size);
                Debug.Log($"Received Data: {receivedData}");
                InputHandler.ParseHexData(receivedData);
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void Update()
    {
        CheckForErrors();
    }

    private void CheckForErrors()
    {
        BleApi.ErrorMessage error;
        BleApi.GetError(out error);
        if (!string.IsNullOrEmpty(error.msg))
        {
            Debug.LogError($"BLE Error: {error.msg}");
        }
    }

    private void OnApplicationQuit()
    {
        BleApi.Quit();
        Debug.Log("BLE connection closed.");
    }
}
