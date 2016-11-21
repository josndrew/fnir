#include <TimedAction.h>

#define USBSERIAL Serial2      // Arduino Leonardo, Teensy, Fubarino
#define RES  12
#define AVG  32 // change HARDWARE AVG to 1, 2, 4, 8, 16, 32
#define PINS 5

/*************************************************************************/
const int led = 13;
uint8_t adc_pins[] = {A3, A2, A1, A0};
uint8_t led_pins[] = {A9, A8, A7, A6, A5};

int INTERVAL = 1000; // __ 1000 = 1 sec
int NUM_ITER = 100;//INTERVAL/2.5; 
double timestamp = 0;
int t = 0;
int indexNum = -1;
double sumReading = -10000.0;

/*************************************************************************/


/******************************************************/
void ldaemon(void)
{
  if (USBSERIAL.available() == 0)
  {
    analogWrite(led_pins[indexNum], 0);
    if (indexNum < PINS - 1) {
      indexNum += 1;
    }
    else {
      indexNum = 0;
    }
    analogWrite(led_pins[indexNum], 120);
  }
  else
  {
    analogWrite(led_pins[indexNum], 0);
  }
}


/******************************************************/

TimedAction changeLightAction = TimedAction(INTERVAL / 2, ldaemon);


void setup()
{
  // initialize the digital pin as an output.
  pinMode(led, OUTPUT);

  for (int i = 0; i < PINS - 1; i++) {
    pinMode(adc_pins[i], INPUT);
    pinMode(led_pins[i], OUTPUT);
  }

  USBSERIAL.begin(115200);
  USBSERIAL.setTimeout(0);
  analogReadResolution(RES);
  analogReadAveraging(AVG);

}

void loop()
{
  USBSERIAL.flush();
  
  if (USBSERIAL.available() == 0)
  {
    bool recievedCmd = false;
    digitalWrite(led, LOW);    // turn the LED off by making the voltage LOW

    do {
      changeLightAction.check();
      char incomingByte = USBSERIAL.read();
      
      if (incomingByte == 's') // RESTART CMD
      {
        timestamp = 0;
        analogWrite(led_pins[indexNum], 0);
        indexNum = -1;
        t = millis();
        recievedCmd = true;
      }
      else if (incomingByte == 'r') // SEND DATA CMD
      {
        recievedCmd = true;
      }
    } while (!recievedCmd);

    digitalWrite(led, HIGH);   // turn the LED on (HIGH is the voltage level)
    changeLightAction.check();

    timestamp = ((millis() - t) / (1000.0));
    USBSERIAL.print(timestamp, 4);


    for (int i = 0; i < 4; i++)
    {
      USBSERIAL.print(',');
      sumReading = 0;
      for (int j = 0; j < NUM_ITER; j++)
      {
        int value = analogRead(adc_pins[i]); // read a new value, will return ADC_ERROR_VALUE if the comparison is false.
        double newReading = value;
        sumReading += newReading;
      }
      double value = sumReading / (NUM_ITER * 1000);
      USBSERIAL.print(value, 3);
    }

    USBSERIAL.print(',');
    USBSERIAL.print(indexNum);

    USBSERIAL.print('\n');

    /* SAMPLE OUTPUT LINE: "time   ,S1Red,S1Gre,S2Red,S2Gre,S3Red,S3Gre,S4Red,S4Gre,LED" */
    /* SAMPLE OUTPUT LINE: "23.9100,1.696,1.775,1.984,1.750,1.972,1.702,1.780,1.760,0" */

    changeLightAction.check();
  }
}
