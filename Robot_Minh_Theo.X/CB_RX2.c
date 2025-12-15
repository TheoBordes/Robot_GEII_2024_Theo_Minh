#include <xc.h>
#include <stdio.h>
#include <stdlib.h>
#include "UART_Protocol.h"
#include "CB_RX2.h"

#define CBRX2_BUFFER_SIZE 1024

static unsigned int cbRx2Head = 0;
static unsigned int cbRx2Tail = 0;


unsigned char cbRx2Buffer[CBRX2_BUFFER_SIZE];

void CB_RX2_Add(unsigned char value) {
    int next = cbRx2Head + 1;
    if (next >= CBRX2_BUFFER_SIZE)
        next = 0;

    if (next != cbRx2Tail) {
        cbRx2Buffer[cbRx2Head] = value;
        cbRx2Head = next;
    }
}

unsigned char CB_RX2_Get(void) {
    unsigned char value = cbRx2Buffer[cbRx2Tail];
    cbRx2Tail++;
    if (cbRx2Tail >= CBRX2_BUFFER_SIZE)
        cbRx2Tail = 0;
    return value;
}

unsigned char CB_RX2_IsDataAvailable(void) {
    if (cbRx2Head != cbRx2Tail)
        return 1;
    else
        return 0;
}

void __attribute__((interrupt, no_auto_psv)) _U2RXInterrupt(void) {
    IFS1bits.U2RXIF = 0; // clear RX interrupt flag
    /* check for receive errors */
    if (U2STAbits.FERR == 1) {
        U2STAbits.FERR = 0;
    }
    /* must clear the overrun error to keep uart receiving */
    if (U2STAbits.OERR == 1) {
        U2STAbits.OERR = 0;
    }
    /* get the data */
    while (U2STAbits.URXDA == 1) {
        CB_RX2_Add(U2RXREG);
    }
}


int CB_RX2_GetDataSize(void) {
    if (cbRx2Head >= cbRx2Tail)
        return cbRx2Head - cbRx2Tail;
    else
        return CBRX2_BUFFER_SIZE - cbRx2Tail + cbRx2Head;
}

int CB_RX2_GetRemainingSize(void) {
    return (CBRX2_BUFFER_SIZE - 1) -CB_RX2_GetDataSize();
}

