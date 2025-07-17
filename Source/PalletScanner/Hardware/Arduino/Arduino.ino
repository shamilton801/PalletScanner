#define BAUD 9600

#define START_SCAN_BYTE 0xD0
#define STOP_SCAN_BYTE  0xD1

const int pwmPin1              = 9;   // Timer1 OC1A — LED dimming
const int triggerPin1          = 5;   // Camera trigger 1
const int triggerPin2          = 4;   // Camera trigger 2 (mirrored)
const int extTriggerPin        = 0;   // External trigger (for external source to start/stop scan)
const unsigned long debounceMs = 100; // How long to wait to make sure a change in state is not a simple blip in power

static int extTriggerPinState;
static unsigned long debounceReadTime;
static bool debouncing = false;

void setup() {
  Serial.begin(BAUD);
  pinMode(extTriggerPin, INPUT);
  extTriggerPinState = digitalRead(extTriggerPin); 
  light_and_camera_setup();
}

void loop() {
  if (Serial.available() > 0) {
    process_byte(Serial.read());
    check_ext_trigger();
  }
}

void process_byte(uint8_t byte) {
  switch (byte) {
    case START_SCAN_BYTE:
      set_running_status(true);
      break;
    case STOP_SCAN_BYTE:
      set_running_status(false);
      break;
  }
}

void check_ext_trigger() {
  int newPinState = digitalRead(extTriggerPin)
  if (extTriggerPinState == newPinState) {
    debouncing = false;
    return;
  } 

  if (debouncing && (millis() - debounceReadTime) > debounceMs) {
    // Right now we only care risig edge conditions to start a scan
    if (newPinState == HIGH) set_running_status(true);
    
    debouncing = false;
    extTriggerPinState = newPinState
  } else if (!debouncing) {
    debouncing = true;
    debounceReadTime = millis();
  }

}

