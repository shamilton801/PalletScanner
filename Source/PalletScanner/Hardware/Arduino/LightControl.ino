// Adjust this value to add delay or advancement
// Positive = delay, Negative = advance
const int cameraPhaseDelay_us = 12600;  // Try 050µs delay to start

volatile uint8_t overflowCount = 0;
static bool running = false;

void light_and_camera_setup() {
  pinMode(pwmPin1, OUTPUT);
  pinMode(triggerPin1, OUTPUT);
  pinMode(triggerPin2, OUTPUT);
  digitalWrite(triggerPin1, LOW);
  digitalWrite(triggerPin2, LOW);

  // Timer1 - 72 Hz PWM on Pin 9
  TCCR1A = (1 << COM1A1) | (1 << WGM11);
  TCCR1B = (1 << WGM13) | (1 << WGM12) | (1 << CS11) | (1 << CS10);

  ICR1 = 3471;  // 72 Hz
  OCR1A = (ICR1 + 1) * 2 / 100 - 1; // 2% duty

  TIMSK1 |= (1 << TOIE1); // Enable Timer1 overflow interrupt

  // Timer3 for pulse width
  TCCR3A = 0;
  TCCR3B = 0;
}

void set_running_status(bool enable) {
  if (enable == running) return;

  noInterrupts();
  digitalWrite(triggerPin1, LOW);
  digitalWrite(triggerPin2, LOW);
  running = enable;
  noInterrupts();
}

ISR(TIMER1_OVF_vect) {
  if (!running) {
    return;
  }

  overflowCount++;

  if (overflowCount >= 6) { // 72Hz / 12Hz = every 6th cycle
    overflowCount = 0;

    // Optional phase delay
    if (cameraPhaseDelay_us > 0) {
      delayMicroseconds(cameraPhaseDelay_us); // Delay camera trigger
    }

    // Start camera pulse
    digitalWrite(triggerPin1, HIGH);
    digitalWrite(triggerPin2, HIGH);

    // 714 µs = 1428 ticks at 2MHz (prescaler = 8)
    OCR3A = 1428;
    TCNT3 = 0;
    TCCR3A = 0;
    TCCR3B = (1 << WGM32) | (1 << CS31);
    TIMSK3 |= (1 << OCIE3A);
  }
}

ISR(TIMER3_COMPA_vect) {
  if (!running) return;
  
  digitalWrite(triggerPin1, LOW);
  digitalWrite(triggerPin2, LOW);
  TCCR3B = 0;
  TIMSK3 &= ~(1 << OCIE3A);
}
