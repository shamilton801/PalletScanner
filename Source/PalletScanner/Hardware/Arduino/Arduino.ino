#define BAUD 9600

#define START_SCAN 0xD0
#define STOP_SCAN 0xD1

void process_byte();
void start_scan();
void stop_scan();

void setup() {
  Serial.begin(BAUD);
  light_and_camera_setup();
}

void loop() {
  if (Serial.available() > 0) {
    process_byte(Serial.read());
  }
}

void process_byte(uint8_t byte) {
  switch (byte) {
    case START_SCAN:
      start_scan();
      break;
    case STOP_SCAN:
      stop_scan();
      break;
  }
}

void start_scan() {
  set_running_status(true);
}

void stop_scan() {
  set_running_status(false);
}