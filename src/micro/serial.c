/*
 * serial.c
 *
 *  Created on: Jun 23, 2016
 *      Author: Andrew Joseph
 */
#include <avr/io.h>

#ifndef F_CPU
	#define F_CPU 16000000UL
#endif
#define BAUDRATE 9600
#define BAUD_PRESCALLER (((F_CPU / (BAUDRATE * 16UL))) - 1)



unsigned char USART_receive(void){

	while(!(UCSR0A & (1<<RXC0)));
	return UDR0;

}

void USART_send( unsigned char data){

	while(!(UCSR0A & (1<<UDRE0)));
	UDR0 = data;

}

void USART_putstring(char* StringPtr){

	while(*StringPtr != 0x00){
		USART_send(*StringPtr);
		StringPtr++;}
}

