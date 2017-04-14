/*******************************************************************************
* File Name: app_UART.c
*
* Description:
*  Common BLE application code for client devices.
*
*******************************************************************************
* Copyright 2015, Cypress Semiconductor Corporation.  All rights reserved.
* You may use this file only in accordance with the license, terms, conditions,
* disclaimers, and limitations in the end user license agreement accompanying
* the software package with which this file was provided.
*******************************************************************************/

#include "app_UART.h"
//#define BRIGHTNESS         (39000u)
int BRIGHTNESS = 39000u;      

/*******************************************************************************
* Function Name: HandleUartRxTraffic
********************************************************************************
*
* Summary:
*  This function takes data from received notfications and pushes it to the 
*  UART TX buffer. 
*
* Parameters:
*  CYBLE_GATTC_HANDLE_VALUE_NTF_PARAM_T * - the notification parameter as  
*                                           recieved by the BLE stack
*
* Return:
*   None.
*
*******************************************************************************/
void HandleUartRxTraffic(CYBLE_GATTC_HANDLE_VALUE_NTF_PARAM_T *uartRxDataNotification)
{    
    unsigned char *x =  uartRxDataNotification->handleValPair.value.val;
              
        
    int l; //Variable for Loop Through Light Source           
    
    
    switch (*x)
    {
        case 'r':
            for (l = 0; l < 5; l++) //Loop Through Light Source
            {                                
                switchLED(l);
                CyDelay(10);
                readSensor(l);                
            }
            switchLED(0);
            break;
        case 's':
            millis_Reset();
            break;
        case 'm':
            CySoftwareReset();
            break;
        case 'c':
            for (l = 0; l < 4; l++) //Loop Through Each Sensor
            {
                ADC_IsEndConversion(ADC_WAIT_FOR_RESULT);
                uint16 VAL1 = ADC_GetResult16(l);
                ADC_SetOffset(l,VAL1);
            }
            break;
        case 'v':
            VersionSW();
            break;
        case '0':
            switchLED(0);
            readSensorBitVal();
            break;
        case '1':
            switchLED(1);
            readSensorBitVal();
            break;
        case '2':
            switchLED(2);
            readSensorBitVal();
            break;
        case '3':
            switchLED(3);
            readSensorBitVal();
            break;
        case '4':
            switchLED(4);
            readSensorBitVal();
            break;
        case 't':
            BRIGHTNESS +=1000;
            break;
        default:
            break;
    }
    
}
void VersionSW()
{
    
    uint8 writeBuffer[40]={ 'S', 'W', ' ', 'V', 'e', 'r', 's', 'i', 'o', 'n', 
         ':', ' ', '0', '.', '1', 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
         0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
         0x00, 0x00, 0x00, 0x0D, 0x0A};
                
    sendCommand(writeBuffer); 
}


void readSensor(int index)
{
    int k; //Variable for Loop Through Sensors
    int count = 0; //Place Holder for writeBuffer
    
    uint8 writeBuffer[40]={0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
         0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
         0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
         0x00, 0x00, 0x00, 0x00, 0x00, 0x0D, 0x0A};
                
    uint32 timestamp = millis();
    
    dec_to_str (&writeBuffer[count], timestamp, 10);
    writeBuffer[count + 11] = ',';
    count+=12;
    
    
    for (k = 0; k < 4; k++) //Loop Through Sensors
    {
        uint16 VAL1 = 0;
        do
        {
            VAL1 = 0;
            ADC_IsEndConversion(ADC_WAIT_FOR_RESULT);
            VAL1 = ADC_GetResult16(k) - ADC_offset[k];       
        }while (VAL1 > 64000);
        
        uint16 AV1 = ADC_CountsTo_mVolts(k, VAL1 + ADC_offset[k]);
        
        dec_to_str (&writeBuffer[count], AV1, 4);
        writeBuffer[count + 5] = ',';
        count+=6;
    }
    
    dec_to_str (&writeBuffer[count], index, 0);
    sendCommand(writeBuffer); 
}


void readSensorBitVal()
{
    int k; //Variable for Loop Through Sensors
    int count = 0; //Place Holder for writeBuffer
    
    uint8 writeBuffer[40]={0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
         0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
         0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
         0x00, 0x00, 0x00, 0x00, 0x00, 0x0D, 0x0A}; 
        

    for (k = 0; k < 4; k++) //Loop Through Sensors
    {
        uint16 VAL1 = 0;
        do
        {
            VAL1 = 0;
            ADC_IsEndConversion(ADC_WAIT_FOR_RESULT);
            VAL1 = ADC_GetResult16(k) - ADC_offset[k];        
        }while (VAL1 > 64000);
        
        bit_to_str (&writeBuffer[count], VAL1, 10);

        sendCommand(writeBuffer); 
    }
}


void switchLED(int index)
{
    switch (index)
    {
        case 0:
            PWM_1_WriteCompare(0u);
            PWM_2_WriteCompare(0u);
            PWM_3_WriteCompare(0u);
            PWM_4_WriteCompare(0u);
            break;
        case 1: 
            PWM_1_WriteCompare(BRIGHTNESS);
            PWM_2_WriteCompare(0u);
            PWM_3_WriteCompare(0u);
            PWM_4_WriteCompare(0u);
            break;
        case 2: 
            PWM_1_WriteCompare(0u);
            PWM_2_WriteCompare(BRIGHTNESS);
            PWM_3_WriteCompare(0u);
            PWM_4_WriteCompare(0u);
            break;
        case 3: 
            PWM_1_WriteCompare(0u);
            PWM_2_WriteCompare(0u);
            PWM_3_WriteCompare(BRIGHTNESS);
            PWM_4_WriteCompare(0u);
            break;
        case 4:
            PWM_1_WriteCompare(0u);
            PWM_2_WriteCompare(0u);
            PWM_3_WriteCompare(0u);
            PWM_4_WriteCompare(BRIGHTNESS);
            break;
        default:
            break;
    }
}

void sendCommand(uint8 a[])
{    
    int j;
    for (j = 0; j < 40; j+=20)
    {
        CYBLE_API_RESULT_T              bleApiResult = 0xCDF7;
        CYBLE_GATTC_WRITE_CMD_REQ_T     uartTxDataWriteCmd;
    
        uartTxDataWriteCmd.attrHandle = rxCharHandle;
        uartTxDataWriteCmd.value.len  = 20; //uartRxDataNotification->handleValPair.value.len;
        
        uartTxDataWriteCmd.value.val  = &a[j];  
        
        do
        {
            bleApiResult = CyBle_GattcWriteWithoutResponse(cyBle_connHandle, &uartTxDataWriteCmd);
            CyBle_ProcessEvents();
        }
        while((CYBLE_ERROR_OK != bleApiResult) && (CYBLE_STATE_CONNECTED == cyBle_state));
        
        CyBle_ProcessEvents();
    }
}


void dec_to_str (uint8* str, uint32_t val, size_t digits)
{
  size_t i=1u;
  digits++;
    
  for(; i<=digits; i++)
  {
    
    if ((digits == 5) && (i == (digits - 1)))
    {
        str[digits-i] = '.';
    }
    else if ((digits == 11) && (i == (digits - 7)))
    {
        str[digits-i] = '.';
    }
    else
    {
        str[digits-i] = (char)((val % 10u) + '0');
        val/=10u;
    }
  }
}

void bit_to_str (uint8* str, uint32_t val, size_t digits)
{
  size_t i=1u;
  digits++;
    
  for(; i<=digits; i++)
  {
    str[digits-i] = (char)((val % 10u) + '0');
    val/=10u;
  }
}


/*******************************************************************************
* Function Name: HandleUartTxTraffic
********************************************************************************
*
* Summary:
*  This function takes data from UART RX buffer and pushes it to the server 
*  as Write Without Response command.
*
* Parameters:
*  None.
*
* Return:
*   None.
*
*******************************************************************************/
void HandleUartTxTraffic(void)
{
    /*uint8   index;
    uint8   uartTxData[mtuSize - 3];
    uint16  uartTxDataLength;
    uint8   a[20]={0x31, 0x7A, 0x0D, 0x0a, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 ,0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,0x00};
    
    uartTxDataLength = UART_SpiUartGetRxBufferSize();
    
    static uint16 uartIdleCount = UART_IDLE_TIMEOUT;
    
    CYBLE_API_RESULT_T              bleApiResult;
    CYBLE_GATTC_WRITE_CMD_REQ_T     uartTxDataWriteCmd;
    
    
    
    #ifdef FLOW_CONTROL
        if(uartTxDataLength >= (UART_UART_RX_BUFFER_SIZE - (UART_UART_RX_BUFFER_SIZE/2)))
        {
            DisableUartRxInt();
        }
        else
        {
            EnableUartRxInt();
        }
    #endif
    
    if((uartTxDataLength != 0))
    {
        if(uartTxDataLength >= (mtuSize - 3))
        {
            uartIdleCount       = UART_IDLE_TIMEOUT;
            uartTxDataLength    = mtuSize - 3;
        }
        else
        {
            if(--uartIdleCount == 0)
            {
                /*uartTxDataLength remains unchanged ;
            }
            else
            {
                uartTxDataLength = 0;
            }
        }
        
        if(0 != uartTxDataLength)
        {
            uartIdleCount       = UART_IDLE_TIMEOUT;
            
            for(index = 0; index < uartTxDataLength; index++)
            {
                uartTxData[index] = (uint8) UART_UartGetByte();
            }
            
            //uartTxDataWriteCmd.attrHandle = rxCharHandle;
            //uartTxDataWriteCmd.value.len  = uartTxDataLength;
            //uartTxDataWriteCmd.value.val  = uartTxData;           
  
            
            uartTxDataWriteCmd.attrHandle = rxCharHandle;
            uartTxDataWriteCmd.value.len  = 0x0004;
            uartTxDataWriteCmd.value.val  = a; 
            
            #ifdef FLOW_CONTROL
                DisableUartRxInt();
            #endif
            
            do
            {
                bleApiResult = CyBle_GattcWriteWithoutResponse(cyBle_connHandle, &uartTxDataWriteCmd);
                CyBle_ProcessEvents();
            }
           while((CYBLE_ERROR_OK != bleApiResult) && (CYBLE_STATE_CONNECTED == cyBle_state));
            
        }
    }
*/
}

/*****************************************************************************************
* Function Name: DisableUartRxInt
******************************************************************************************
*
* Summary:
*  This function disables the UART RX interrupt.
*
* Parameters:
*   None.
*
* Return:
*   None.
*
*****************************************************************************************/
void DisableUartRxInt(void)
{
    UART_INTR_RX_MASK_REG &= ~UART_RX_INTR_MASK;  
}

/*****************************************************************************************
* Function Name: EnableUartRxInt
******************************************************************************************
*
* Summary:
*  This function enables the UART RX interrupt.
*
* Parameters:
*   None.
*
* Return:
*   None.
*
*****************************************************************************************/
void EnableUartRxInt(void)
{
    UART_INTR_RX_MASK_REG |= UART_RX_INTR_MASK;  
}

/* [] END OF FILE */
