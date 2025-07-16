#define CAMERA_LINE_1_TRIGGER 4
#define CAMERA_LINE_2_TRIGGER 5
#define BAUD 9600

#define START_SCAN 0xD0
#define STOP_SCAN 0xD1

void process_byte();
void start_scan();
void stop_scan();

void setup() {
  Serial.begin(BAUD);
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
  // Start jeremy's camera/lights code
}

void stop_scan() {
  // Stop jeremy's camera/lights code
}