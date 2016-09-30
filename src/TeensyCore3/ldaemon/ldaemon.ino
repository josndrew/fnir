double sensorValue1;
double sensorValue2;
double sensorValue3;
double sensorValue4;

String data1;
String data2;
String data3;
String data4;
      
void setup()   {                
  Serial.begin(38400);
  randomSeed(analogRead(0));
}

double convertToVolts(int value)
{
    return 3.3*value/1024;
    //return value;
}

void ldaemon()
{
    sensorValue1 = convertToVolts(analogRead(A9));
    sensorValue2 = convertToVolts(analogRead(A8));
    sensorValue3 = convertToVolts(analogRead(A7));
    sensorValue4 = convertToVolts(analogRead(A6));
    
    data1 = String(sensorValue1);
    data2 = String(sensorValue2);
    data3 = String(sensorValue3);
    data4 = String(sensorValue3);
    
    Serial.print(data1);
    Serial.print('\x09');
    Serial.print(data2);
    Serial.print('\x09');
    Serial.print(data3);
    Serial.print('\x09');
    Serial.print(data4);
}

void loop()                     
{
  if (Serial.dtr()) {

    /* Turn on 1st Wavelenght Led HERE */
    
    ldaemon();
    Serial.print('\x09');
    
    /* Turn on 2nd Wavelenght Led HERE */

    ldaemon();
    Serial.print('\n');
  }
}
