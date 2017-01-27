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

int ledIndex = 0;

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
    
    //if(uartRxDataNotification->handleValPair.attrHandle == txCharHandle)
    //{
    //    UART_SpiUartPutArray(uartRxDataNotification->handleValPair.value.val, \
    //        (uint32) uartRxDataNotification->handleValPair.value.len);
    //}
    // Change to send data back
    
    uint8   index;
    uint8   uartTxData[mtuSize - 3];
    uint16  uartTxDataLength;
    
    
    
    static uint16 uartIdleCount = UART_IDLE_TIMEOUT;
    
    CYBLE_API_RESULT_T              bleApiResult;
    CYBLE_GATTC_WRITE_CMD_REQ_T     uartTxDataWriteCmd;
    
    

            
        
    unsigned char *x =  uartRxDataNotification->handleValPair.value.val;
    uint8 a[20]={0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 , 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0D, 0x0a};
           
            
            
    int k;
    int count = 0;
    switch (*x)
    {
        case 'a':
        /*
            gettimeofday(&stop, NULL);
            uint8 secs = (double)(stop.tv_usec - start.tv_usec) / 1000 + (double)(stop.tv_sec - start.tv_sec);
            dec_to_str (&a[count], secs, 3);
            a[count + 3] = ',';
            count+=4;
          */  
            
            for (k = 0; k < 4; k++)
            {
                uint16 VAL1=ADC_GetResult16(k);
                uint8 AV1=ADC_CountsTo_mVolts(0,VAL1);
                dec_to_str (&a[count], AV1, 3);
                a[count + 3] = ',';
                count+=4;
            }
            dec_to_str (&a[count], (ledIndex % 4), 1);
            ledIndex = (ledIndex + 1) % 4;
            //a[count + 1] = 0x0D;
            a[count + 1] = 0x0A;
            break;
        case 'b':
            gettimeofday(&start, NULL);
            break;
        default:
            break;
    }
    
            
            
    uartTxDataWriteCmd.attrHandle = rxCharHandle;
    uartTxDataWriteCmd.value.len  = 20; //uartRxDataNotification->handleValPair.value.len;          
    uartTxDataWriteCmd.value.val  = a;  

    do
    {
        bleApiResult = CyBle_GattcWriteWithoutResponse(cyBle_connHandle, &uartTxDataWriteCmd);
        CyBle_ProcessEvents();
    }
    while((CYBLE_ERROR_OK != bleApiResult) && (CYBLE_STATE_CONNECTED == cyBle_state));
            
}

void dec_to_str (uint8* str, uint32_t val, size_t digits)
{
  size_t i=1u;

  for(; i<=digits; i++)
  {
    str[digits-i] = (char)((val % 10u) + '0');
    val/=10u;
  }

  //str[i-1u] = '\0'; // assuming you want null terminated strings?
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
