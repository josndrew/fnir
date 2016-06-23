/*
 * test.c
 *
 *  Created on: Jun 23, 2016
 *      Author: Andrew Joseph
 */

#define F_CPU   16000000
#define BUAD    9600
#define BRC     ((F_CPU/16/BUAD) - 1)

#include <avr/io.h>
#include <avr/interrupt.h>
#include <util/delay.h>



//Define functions
//======================
void ioinit(void);      //Initializes IO
//======================

int main (void)
{
    ioinit(); //Setup IO pins and defaults

    while(1)
    {
		PORTB = 0x01;
		_delay_ms(800);

		PORTB = 0x00;
		_delay_ms(200);
    }

    return(0);
}

void ioinit (void)
{
    //1 = output, 0 = input
    DDRB = 0b00000001; //All outputs
}


