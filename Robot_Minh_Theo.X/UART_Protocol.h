/* 
 * File:   UART_Protocol.h
 * Author: TP-EO-1
 *
 * Created on 27 novembre 2024, 16:50
 */

#ifndef UART_PROTOCOL_H
#define	UART_PROTOCOL_H

unsigned char UartCalculateChecksum(int msgFunction,int msgPayloadLength, unsigned char* msgPayload);
void UartEncodeAndSendMessage(int msgFunction,int msgPayloadLength, unsigned char* msgPayload);
//void UartDecodeMessage(unsigned char c);
//void UartProcessDecodedMessage(int function,int payloadLength, unsigned char* payload);

#endif	/* UART_PROTOCOL_H */
