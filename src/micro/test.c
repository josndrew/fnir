/*
 * test.c
 *
 *  Created on: Jun 21, 2016
 *      Author: Andrew Joseph
 */
#undef F_CPU
#define F_CPU 16000000UL

#include <avr/io.h>
#include <util/delay.h>
#include <stdio.h>
#include <micro/serial.h>


void ioinit(void);      //Initializes IO


int main(void){
	USART_init();        //Call the USART initialization code
	ioinit(); //Setup IO pins and defaults

	char String[256];
	int count = 0;

	while(1)
	{
		PORTB = 0x01;
		_delay_ms(100);

		if (count > 100){
			count = 0;
		}

		sprintf(String, "%d\n", count, count%3);
		count++;

		USART_putstring(String);

		PORTB = 0x00;
		_delay_ms(100);
	}

	return 0;
}



void ioinit (void)
{
	//1 = output, 0 = input
	DDRB = 0b00000001; //All outputs
}
