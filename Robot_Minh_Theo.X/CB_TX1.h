/* 
 * File:   CB_TX1.h
 * Author: pixel
 *
 * Created on 14 novembre 2024, 21:09
 */

#ifndef CB_TX1_H
#define	CB_TX1_H

int CB_TX1_GetRemainingSize(void);
int CB_TX1_GetDataSize(void);
unsigned char CB_TX1_IsTranmitting(void);
void SendOneTX1();
unsigned char CB_TX1_Get(void);
void CB_TX1_Add(unsigned char value);
void SendMessageTX1(unsigned char* message, int length);

#endif	/* CB_TX1_H */
