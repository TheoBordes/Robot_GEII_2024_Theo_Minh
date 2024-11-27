#include <xc.h>
#include "UART_Protocol.h"
#include "CB_TX1.h"

unsigned char UartCalculateChecksum(int msgFunction, int msgPayloadLength, unsigned char *msgPayload)
{
    // Fonction prenant entree la trame et sa longueur pour calculer le checksum
    unsigned char checksum = (unsigned char)(msgFunction ^ 0xFE);
    for (int j = 0; j < msgPayloadLength; j++)
    {
        checksum = (unsigned char)(checksum ^ msgPayload[j]);
    }
    return checksum;
}

void UartEncodeAndSendMessage(int msgFunction, int msgPayloadLength, unsigned char *msgPayload)
{
    // Fonction d?encodage et d?envoi d?un message
    int j = 0;
    unsigned char message[128] = {};
    message[0] = 0XFE;
    message[2] = (unsigned char)(msgFunction & 0xFF);
    message[1] = (unsigned char)((msgFunction >> 8) & 0xFF);
    message[4] = (unsigned char)(msgPayloadLength & 0xFF);
    message[3] = (unsigned char)((msgPayloadLength >> 8) & 0xFF);
    for (int i = 0; i < msgPayloadLength; i++)
    {
        message[5+i] = msgPayload[j];
        j++;
    }
    message[msgPayloadLength + 5] = UartCalculateChecksum(msgFunction, msgPayloadLength, msgPayload);
    SendMessage(message,6+msgPayloadLength);
}

//int msgDecodedFunction = 0;
//int msgDecodedPayloadLength = 0;
//unsigned char msgDecodedPayload[128];
//int msgDecodedPayloadIndex = 0;
//
//void UartDecodeMessage(unsigned char c) {
//    //Fonction prenant en entree un octet et servant a reconstituer les trames
//
//}
//
//void UartProcessDecodedMessage(int function, int payloadLength, unsigned char* payload) {
//    //Fonction appelee apres le decodage pour executer l?action
//    //correspondant au message recu
//
//}


//*************************************************************************/
//Fonctions correspondant aux messages
//*************************************************************************/