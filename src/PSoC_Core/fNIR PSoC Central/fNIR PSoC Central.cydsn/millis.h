#include <cytypes.h>

/*
 * millis_Start()
 *
 * Starts the SysTick timer and sets the callback function using the first available
 * ISR slots for the SysTick time, which will be called every one millisecond (per
 * CySysTickStart()'s default behavior).
 *
 * Takes:   no parameters
 * Returns: 0 if OK, otherwise 1 if no available SysTick ISR slots exist
 */
int millis_Start(void);

/*
 * millis_Reset()
 *
 * Resets the counter maintained by millis() to zero.
 */
void millis_Reset(void);

/*
 * millis()
 *
 * Returns the number of milliseconds since millis_Start() was called.
 */
uint32 millis();

    