/*
 * Semi-equivalent of millis() from the Arduino API.
 * Uses SysTick to generate interrupts to be counted every one millisecond.
 *
 * Written by Eric B. Wertz (eric@edushields.com)
 * Version 1.0
 * Last modified 28-Aug-2015
 */

#include <CyLib.h>
#include "millis.h"

#define INITMILLIS_OK   0
#define INITMILLIS_FAIL 1

volatile static uint32 msCount;

CY_ISR(millisSysTickISR)
{
    msCount++;
}

/*
 * Install ISR in first available slot, if it's not there already.
 * Reset millis() counter to 0.
 */
int millis_Start(void)
{
    unsigned int i;
    int retval=INITMILLIS_FAIL;

    msCount = 0;

    CySysTickStart();

    for (i=0; i < CY_SYS_SYST_NUM_OF_CALLBACKS; ++i) {
        if (!CySysTickGetCallback(i)) {
            CySysTickSetCallback(i, millisSysTickISR);
            retval = INITMILLIS_OK;
            break;
        }
        else if (CySysTickGetCallback(i) == millisSysTickISR) {
            retval = INITMILLIS_OK;
            break;
        }
    }

    return retval;
}

uint32 millis(void)
{
    return msCount;
}

void millis_Reset(void)
{
    msCount = 0;
}
