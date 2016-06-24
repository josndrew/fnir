/*
 * test.c
 *
 *  Created on: Jun 21, 2016
 *      Author: Andrew Joseph
 */
#undef F_CPU
#define F_CPU 16000000UL
#define BAUDRATE 9600
#define BAUD_PRESCALLER (((F_CPU / (BAUDRATE * 16UL))) - 1)

#include <avr/io.h>
#include <util/delay.h>
#include <stdio.h>
#include <micro/serial.h>

int is_running_flag = 1;

void ioinit(void);      //Initializes IO

void USART_init(void){

	UBRR0H = (uint8_t)(BAUD_PRESCALLER>>8);
	UBRR0L = (uint8_t)(BAUD_PRESCALLER);
	UCSR0B = (1<<RXEN0)|(1<<TXEN0);
	UCSR0C = (3<<UCSZ00);
}

int main(void){

	ioinit(); //Setup IO pins and defaults
	USART_init();        //Call the USART initialization code


	char String[256];
	int count = 0;
	is_running_flag = 1;

	while(is_running_flag)
	{

		PORTB = 0x01;
		_delay_ms(100);

		if (count > 100){
			count = 0;
			is_running_flag = 0;
		}

		sprintf(String, "Hello\n", count);
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
