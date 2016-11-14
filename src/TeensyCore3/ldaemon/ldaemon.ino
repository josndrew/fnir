#include <TimedAction.h>

#define USBSERIAL Serial      // Arduino Leonardo, Teensy, Fubarino
#define RES  12
#define AVG  32 // change HARDWARE AVG to 1, 2, 4, 8, 16, 32
#define PINS 4
#define INTERVAL 400 // __ 1000 = 1 sec
#define NUM_ITER INTERVAL/2.5


/*************************************************************************/
const int led = 13;
uint8_t adc_pins[] = {A3, A3, A2, A2, A1, A1, A0, A0};
uint8_t led_pins[] = {A9, A8, A9, A8, A7, A6, A7, A6};
bool led_on = false;
double timestamp = 0;
int t = 0;
int indexNum = -1;
volatile double values[] = {0, 0, 0, 0, 0, 0, 0, 0};
double sumReading = -10000.0;

/*************************************************************************/


/******************************************************/
void ldaemon(void)
{
  if (USBSERIAL.dtr())
  {
    analogWrite(led_pins[indexNum], 0);
    if (indexNum < (PINS * 2) - 1) {
      indexNum += 1;
    }
    else {
      indexNum = 0;
    }
    analogWrite(led_pins[indexNum], 120);
    sumReading = 0;

    for (int j = 0; j < NUM_ITER; j++)
    {
      int value = analogRead(adc_pins[indexNum]); // read a new value, will return ADC_ERROR_VALUE if the comparison is false.
      //double newReading = map(value, 0, 1023, 0, 255); //value * 3.3 / 1024;
      double newReading = value;

      sumReading += newReading;
    }

    values[indexNum] = sumReading / (NUM_ITER * 1000);
  }
  else
  {
    analogWrite(led_pins[indexNum], 0);
  }

}

/******************************************************/

TimedAction readSensorAction = TimedAction(INTERVAL / 2, ldaemon);


void setup()
{
  // initialize the digital pin as an output.
  pinMode(led, OUTPUT);

  for (int i = 0; i < (PINS * 2) - 1; i++) {
    pinMode(adc_pins[i], INPUT);
  }

  USBSERIAL.begin(115200);
  USBSERIAL.setTimeout(0);
  analogReadResolution(RES);
  analogReadAveraging(AVG);

}

void loop()
{
  bool recievedCmd = false;
  bool debug = false;

  if (led_on)
  {
    digitalWrite(led, LOW);    // turn the LED off by making the voltage LOW
    led_on = false;
  }

  do {
    readSensorAction.check();
    char incomingByte = Serial.read();
    if (incomingByte == 's')
    {
      timestamp = 0;
      t = millis();
      USBSERIAL.flush();
      digitalWrite(led, HIGH);   // turn the LED on (HIGH is the voltage level)
      led_on = true;
      recievedCmd = true;
    }
    else if (incomingByte == 'r')
    {
      digitalWrite(led, HIGH);   // turn the LED on (HIGH is the voltage level)
      led_on = true;
      recievedCmd = true;
    }
    else if (incomingByte == 'd')
    {
      timestamp = 0;
      USBSERIAL.flush();
      digitalWrite(led, HIGH);   // turn the LED on (HIGH is the voltage level)
      led_on = true;
      recievedCmd = true;
      debug = true;
    }
  } while (!recievedCmd);

  readSensorAction.check();

  timestamp = ((millis() - t) / (1000.0));
  USBSERIAL.print(timestamp, 4);

  for (int i = 0; i < PINS * 2; i++)
  {
    USBSERIAL.print(',');
    USBSERIAL.print(values[i], 3);
  }

  USBSERIAL.print(',');
  USBSERIAL.print(indexNum);

  USBSERIAL.print('\n');

  /* SAMPLE OUTPUT LINE: "time   ,S1Red,S1Gre,S2Red,S2Gre,S3Red,S3Gre,S4Red,S4Gre,LED" */
  /* SAMPLE OUTPUT LINE: "23.9100,1.696,1.775,1.984,1.750,1.972,1.702,1.780,1.760,0" */

  readSensorAction.check();
  /* FOR DEBUG ONLY */
  if (debug)
  {
    Serial.print("ADC sampling rate (t "); Serial.print(.05); Serial.print(") = ");
    Serial.print(PINS * 2 * AVG * NUM_ITER * 1 / (.05)); Serial.println("SPS");
  }

}
