/* 
 * File:   UART_Protocol.h
 * Author: TP-EO-1
 *
 * Created on 27 novembre 2024, 16:50
 */

#ifndef UART_PROTOCOL_H
#define	UART_PROTOCOL_H
#define Waiting 0
#define FunctionMSB 2
#define FunctionLSB 3
#define PayloadLengthMSB 4
#define PayloadLengthLSB 5
#define Payload 6
#define CheckSum 7
#define ControlXbox 0x0090
#define SET_ROBOT_STATE 0x0051
#define SET_ROBOT_MANUAL_CONTROL 0x0052
#define SetLed 0x0020

unsigned char UartCalculateChecksum(int msgFunction,int msgPayloadLength, unsigned char* msgPayload);
void UartEncodeAndSendMessage(int msgFunction,int msgPayloadLength, unsigned char* msgPayload);
void UartDecodeMessage(unsigned char c);
void UartProcessDecodedMessage(int function,int payloadLength, unsigned char* payload);

#endif	/* UART_PROTOCOL_H */

