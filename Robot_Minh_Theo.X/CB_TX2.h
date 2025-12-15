/* 
 * File:   CB_TX1.h
 * Author: pixel
 *
 * Created on 14 novembre 2024, 21:09
 */

#ifndef CB_TX2_H
#define	CB_TX2_H

int CB_TX2_GetRemainingSize(void);
int CB_TX2_GetDataSize(void);
void SendOneTX2();
unsigned char CB_TX2_Get(void);
void CB_TX2_Add(unsigned char value);
void SendMessageTX2(unsigned char* message, int length);
unsigned char CB_TX2_IsTransmiting(void);


#endif	/* CB_TX1_H */
