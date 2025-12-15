#include <xc.h>
#include <stdio.h>
#include <stdlib.h>
#include "CB_TX2.h"




#define CBTX2_BUFFER_SIZE 1024
int cbTx2Head;
int cbTx2Tail;
unsigned char cbTx2Buffer[CBTX2_BUFFER_SIZE];
unsigned char isTransmittingTX2 = 0;

void SendMessageTX2(unsigned char* message, int length) {
    unsigned char i = 0;
    if (CB_TX2_GetRemainingSize() >= length) {
        for (i = 0; i < length; i++)
            CB_TX2_Add(message[i]);
        if (!CB_TX2_IsTransmiting())
            SendOneTX2();
    }
    else{
     i=0;
    }
}

void CB_TX2_Add(unsigned char value) {
    cbTx2Buffer[cbTx2Head++] = value;
    if (cbTx2Head >= CBTX2_BUFFER_SIZE)
        cbTx2Head = 0;
}

unsigned char CB_TX2_Get(void) {
    unsigned char value = cbTx2Buffer[cbTx2Tail++];
    if (cbTx2Tail >= CBTX2_BUFFER_SIZE)
        cbTx2Tail = 0;
    return value;

}

void __attribute__((interrupt, no_auto_psv)) _U2TXInterrupt(void) {
    IFS1bits.U2TXIF = 0; // clear TX interrupt flag
    if (cbTx2Tail != cbTx2Head) {
        SendOneTX2();
    } else
        isTransmittingTX2 = 0;
}

void SendOneTX2() {
    isTransmittingTX2 = 1;
    unsigned char value = CB_TX2_Get();
    U2TXREG = value; // Transmit one character
}

unsigned char CB_TX2_IsTransmiting(void) {
    return isTransmittingTX2;
}

int CB_TX2_GetDataSize(void) {
    if (cbTx2Head >= cbTx2Tail)
        return cbTx2Head - cbTx2Tail;
    else
        return CBTX2_BUFFER_SIZE - cbTx2Tail + cbTx2Head;
}

int CB_TX2_GetRemainingSize(void) {
    return CBTX2_BUFFER_SIZE - CB_TX2_GetDataSize();
}

