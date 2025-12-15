#include <xc.h>
#include <stdio.h>
#include <stdlib.h>
#include "CB_TX1.h"
#define CBTX1_BUFFER_SIZE 1024
int cbTx1Head;
int cbTx1Tail;
unsigned char cbTx1Buffer[CBTX1_BUFFER_SIZE];
unsigned char isTransmittingTX1 = 0;

void SendMessageTX1(unsigned char* message, int length) {
    unsigned char i = 0;
    if (CB_TX1_GetRemainingSize() >= length) {
        for (i = 0; i < length; i++)
            CB_TX1_Add(message[i]);
        if (!CB_TX1_IsTranmitting())
            SendOneTX1();
    }
    else{
     i=0;
    }
}

void CB_TX1_Add(unsigned char value) {
    cbTx1Buffer[cbTx1Head++] = value;
    if (cbTx1Head >= CBTX1_BUFFER_SIZE)
        cbTx1Head = 0;
}

unsigned char CB_TX1_Get(void) {
    unsigned char value = cbTx1Buffer[cbTx1Tail++];
    if (cbTx1Tail >= CBTX1_BUFFER_SIZE)
        cbTx1Tail = 0;
    return value;

}

void __attribute__((interrupt, no_auto_psv)) _U1TXInterrupt(void) {
    IFS0bits.U1TXIF = 0; // clear TX interrupt flag
    if (cbTx1Tail != cbTx1Head) {
        SendOneTX1();
    } else
        isTransmittingTX1 = 0;
}

void SendOneTX1() {
    isTransmittingTX1 = 1;
    unsigned char value = CB_TX1_Get();
    U1TXREG = value; // Transmit one character
}

unsigned char CB_TX1_IsTranmitting(void) {
    return isTransmittingTX1;
}

int CB_TX1_GetDataSize(void) {
    if (cbTx1Head >= cbTx1Tail)
        return cbTx1Head - cbTx1Tail;
    else
        return CBTX1_BUFFER_SIZE - cbTx1Tail + cbTx1Head;
}

int CB_TX1_GetRemainingSize(void) {
    return CBTX1_BUFFER_SIZE - CB_TX1_GetDataSize();
}
