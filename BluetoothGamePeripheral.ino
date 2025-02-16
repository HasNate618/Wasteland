#include <ArduinoBLE.h>

#include <DFRobot_Heartrate.h>

#include <Arduino_LSM6DSOX.h>
#include <Arduino.h>
#include <ezButton.h>
// #include "DFRobot_Heartrate.h"

// Bluetooth set up
BLEService service("2f216107-06d1-4764-8cde-a91a755f0e7c");  // User defined service
BLEStringCharacteristic inputCharacteristic("d66f7f33-24ae-4dc0-bf22-b9c54f92cdd0", BLERead | BLENotify, 64);  // remote clients will only be able to read this

#define button_PIN 3 // Arduino pin connected to touch sensor
#define Heart_PIN A1 // Arduino pin connected to heart rate sensor
#define FX_PIN   A3 // Arduino pin connected to flex/bend
// Joystick data:
#define VRX_PIN  A0 // Arduino pin connected to VRX pin
#define VRY_PIN  A2 // Arduino pin connected to VRY pin
#define SW_PIN   10  // Arduino pin connected to SW pin


//Constants:
const int flexPin = A3; //pin A3 to read analog input

//Variables:
int flexValue; //save FLEX analog value 
String inputValue;
float aX, aY, aZ;
int vrX = 0;
int vrY = 0;
int swState = 0; // joystick
int buttonState = 0;
uint8_t rateValue;

//Set Buttons & Mode for heart rate sensor
DFRobot_Heartrate heartrate(ANALOG_MODE); 
ezButton swbutton(SW_PIN);
ezButton button(3);  // create ezButton object that attach to pin 7

void setup() {
  Serial.begin(9600) ; // for joystick & flexbend
  swbutton.setDebounceTime(50); // set debounce time to 50 milliseconds
  button.setDebounceTime(50); // set debounce time to 50 milliseconds
  
  pinMode(LED_BUILTIN, OUTPUT); // initialize the built-in LED pin

  if (!BLE.begin()) {  // initialize BLE
    Serial.println("Starting BLE failed!");
    while (1);
  }
  if (!IMU.begin()) {
    Serial.println("Failed to initialize IMU!");
    while (1);
  }

  BLE.setLocalName("Hand Peripheral Device");      // Set name for connection
  BLE.setAdvertisedService(service);               // Advertise service
  service.addCharacteristic(inputCharacteristic);  // Add characteristic to service
  BLE.addService(service);                         // Add service

  BLE.advertise();  // Start advertising
  Serial.print("Peripheral device MAC: ");
  Serial.println(BLE.address());
  Serial.println("Waiting for connections...");
}

// Accelerometer
void AccelerationUpdate() {
  if (IMU.accelerationAvailable()) {
    IMU.readAcceleration(aX, aY, aZ);
    inputValue.concat(String(aX) + "," + String(aY) + "," + String(aZ) + ",");
  }
  else {
    inputValue.concat("0.00,0.00,1.00,");
  }
}

// Button
void ButtonUpdate() {
  buttonState = button.getState();
  // buttonState = !digitalRead(button_PIN); // reverse 1 to 0? but we don't need to if default changed to 0
  inputValue.concat(String(buttonState));
}

// Joystick
void JoystickUpdate() {
  vrX = analogRead(VRX_PIN);
  vrY = analogRead(VRY_PIN);
  swState = swbutton.getState();
  inputValue.concat(String(vrX) + "," + String(vrY) + "," + String(swState) + ",");
}

// Flex
void FlexUpdate() {
  flexValue = analogRead(flexPin); 
  flexValue = map(flexValue, 700, 900, 0, 255);
  inputValue.concat(String(flexValue));
}

// Heartrate
void HeartRateUpdate() {
  heartrate.getValue(Heart_PIN); ///< A1 foot sampled values 
  rateValue = heartrate.getRate(); ///< Get heart rate value  
  inputValue.concat(String(rateValue) + ",");
}



void loop() {
  BLEDevice central = BLE.central();  // Wait for a BLE central to connect

  // if a central is connected to the peripheral:
  if (central) {
    Serial.print("Connected to central MAC: ");
    // print the central's BT address:
    Serial.println(central.address());
    // turn on the LED to indicate the connection:
    digitalWrite(LED_BUILTIN, HIGH);

    swbutton.loop();
    button.loop();
  }

   while (central.connected()) {
      // keep looping while connected
      HeartRateUpdate();
      JoystickUpdate();
      ButtonUpdate();
      FlexUpdate();
      AccelerationUpdate();
      inputCharacteristic.setValue(inputValue);
      inputValue = "";
      delay(50);
    }

    // when the central disconnects, turn off the LED:
    digitalWrite(LED_BUILTIN, LOW);
    Serial.print("Disconnected from central MAC: ");
    Serial.println(central.address());
}

