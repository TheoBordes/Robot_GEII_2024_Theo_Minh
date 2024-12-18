#include <xc.h>
#include "UART_Protocol.h"
#include "CB_TX1.h"
#include "CB_RX1.h"
#include "PWM.h"
#include "IO.h"
#include "robot.h"

unsigned char UartCalculateChecksum(int msgFunction, int msgPayloadLength, unsigned char *msgPayload) {
    // Fonction prenant entree la trame et sa longueur pour calculer le checksum
    unsigned char checksum = (unsigned char) (msgFunction ^ 0xFE);
    for (int j = 0; j < msgPayloadLength; j++) {
        checksum = (unsigned char) (checksum ^ msgPayload[j]);
    }
    return checksum;
}

void UartEncodeAndSendMessage(int msgFunction, int msgPayloadLength, unsigned char *msgPayload) {
    // Fonction d?encodage et d?envoi d?un message
    int j = 0;
    unsigned char message[128] = {};
    message[0] = 0XFE;
    message[2] = (unsigned char) (msgFunction & 0xFF);
    message[1] = (unsigned char) ((msgFunction >> 8) & 0xFF);
    message[4] = (unsigned char) (msgPayloadLength & 0xFF);
    message[3] = (unsigned char) ((msgPayloadLength >> 8) & 0xFF);
    for (int i = 0; i < msgPayloadLength; i++) {
        message[5 + i] = msgPayload[j];
        j++;
    }
    message[msgPayloadLength + 5] = UartCalculateChecksum(msgFunction, msgPayloadLength, msgPayload);
    SendMessage(message, 6 + msgPayloadLength);
}
int rcvState = Waiting;
int msgDecodedFunction = 0;
int msgDecodedPayloadLength = 0;
unsigned char msgDecodedPayload[128];
int msgDecodedPayloadIndex = 0;
unsigned char calculatedChecksum;
unsigned char receivedChecksum;

void UartDecodeMessage(unsigned char c) {

    switch (rcvState) {
        case Waiting:
            if (c == 0xFE) {
                rcvState = FunctionLSB;
                //RichTextBox.Text += "IN";
            }
            break;
        case FunctionMSB:
            msgDecodedFunction += c;
            rcvState = PayloadLengthLSB;
            //RichTextBox.Text += "functionMSB";
            break;
        case FunctionLSB:
            msgDecodedFunction += c;
            rcvState = FunctionMSB;
            //RichTextBox.Text += "functionLSB";
            break;
        case PayloadLengthMSB:
            msgDecodedPayloadLength += c;
            rcvState = Payload;
            break;
        case PayloadLengthLSB:
            msgDecodedPayloadLength += c;
            rcvState = PayloadLengthMSB;
            break;
        case Payload:
            //RichTextBox.Text += msgDecodedPayloadIndex;
            msgDecodedPayloadIndex += 1;
            if (msgDecodedPayloadIndex == msgDecodedPayloadLength) {
                rcvState = CheckSum;
            }
            msgDecodedPayload[msgDecodedPayloadIndex - 1] = c;

            break;
        case CheckSum:
            calculatedChecksum = UartCalculateChecksum(msgDecodedFunction, msgDecodedPayloadLength, msgDecodedPayload);
            //RichTextBox.Text += calculatedChecksum;
            receivedChecksum = c;
            if (calculatedChecksum == receivedChecksum) {
                //RichTextBox.Text += "ça marche";
                rcvState = Waiting;
                UartProcessDecodedMessage(msgDecodedFunction, msgDecodedPayloadLength, msgDecodedPayload);

            } else {
                rcvState = Waiting;
            }
            msgDecodedFunction = 0;
            msgDecodedPayloadLength = 0;
            msgDecodedPayloadIndex = 0;
            for (int i = 0; i < msgDecodedPayloadLength; i++) {
                msgDecodedPayload[i] = 0;
            }
            break;
        default:
            rcvState = Waiting;
            break;
    }
}

//

void UartProcessDecodedMessage(int function, int payloadLength, unsigned char* payload) {
    int SigneD = 1;
    int SigneG = 1;
    
    switch (function) {
        case ControlXbox:
//            if(payload[1] == 1){
//                SigneD = -1;
//            }
//            if(payload[3] == 1){
//                SigneG = -1;
//            }
                
            PWMSetSpeedConsigne(payload[0], MOTEUR_DROIT);
                                                                                                                                                                                                                                                                                                   
            
            PWMSetSpeedConsigne(payload[1], MOTEUR_GAUCHE);

            break;
        case SET_ROBOT_STATE:
           // SetRobotState(payload[0]);
            break;
        case SET_ROBOT_MANUAL_CONTROL:
            SetRobotAutoControlState(payload[0]);
            break;
        default:
            break;
    }
}

//*************************************************************************/
//Fonctions correspondant aux messages
//*************************************************************************/