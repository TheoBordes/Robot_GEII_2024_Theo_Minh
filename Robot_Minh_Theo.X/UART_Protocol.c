#include <xc.h>
#include "UART_Protocol.h"
#include "CB_TX1.h"
#include "CB_RX1.h"
#include "PWM.h"
#include "IO.h"
#include "robot.h"

unsigned char UartCalculateChecksum(int msgFunction, int msgPayloadLength, unsigned char *msgPayload) {
    unsigned char checksum = 0;

    checksum ^= 0xFE;
    checksum ^= (unsigned char) (msgFunction >> 8);
    checksum ^= (unsigned char) (msgFunction >> 0);
    checksum ^= (unsigned char) (msgPayloadLength >> 8);
    checksum ^= (unsigned char) (msgPayloadLength >> 0);
    for (int i = 0; i < msgPayloadLength; i++) {
        checksum ^= msgPayload[i];
    }

    return checksum;
}

void UartEncodeAndSendMessage(int msgFunction, int msgPayloadLength, unsigned char *msgPayload) {
    // Fonction d?encodage et d?envoi d?un message
    unsigned char message[msgPayloadLength + 6];

    int pos = 0;
    message[pos++] = 0xFE;
    message[pos++] = (unsigned char) (msgFunction >> 8);
    message[pos++] = (unsigned char) (msgFunction >> 0);
    message[pos++] = (unsigned char) (msgPayloadLength >> 8);
    message[pos++] = (unsigned char) (msgPayloadLength >> 0);
    for (int i = 0; i < msgPayloadLength; i++) {
        message[pos++] = msgPayload[i];
    }
    message[pos++] = UartCalculateChecksum(msgFunction, msgPayloadLength, msgPayload);
    SendMessage(message, pos);
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
                rcvState = FunctionMSB;
                //RichTextBox.Text += "IN";
            }
            break;
        case FunctionMSB:
            msgDecodedFunction = c << 8;
            rcvState = FunctionLSB;
            //RichTextBox.Text += "functionMSB";
            break;
        case FunctionLSB:
            msgDecodedFunction |= c << 0;
            rcvState = PayloadLengthMSB;
            //RichTextBox.Text += "functionLSB";
            break;
        case PayloadLengthMSB:
            msgDecodedPayloadLength = c << 8;
            rcvState = PayloadLengthLSB;
            break;
        case PayloadLengthLSB:
            msgDecodedPayloadLength = c << 0;
            if (msgDecodedPayloadLength == 0)
                rcvState = CheckSum;
            else if (msgDecodedPayloadLength < 1024) {
                rcvState = Payload;
                msgDecodedPayloadIndex = 0;
            } else
                rcvState = Waiting;
            break;
        case Payload:
            msgDecodedPayload[msgDecodedPayloadIndex++] = c;
            if (msgDecodedPayloadIndex >= msgDecodedPayloadLength) {
                rcvState = CheckSum;
            }
            break;
        case CheckSum:
            calculatedChecksum = UartCalculateChecksum(msgDecodedFunction, msgDecodedPayloadLength, msgDecodedPayload);
            //RichTextBox.Text += calculatedChecksum;
            receivedChecksum = c;
            if (calculatedChecksum == receivedChecksum) {
                UartProcessDecodedMessage(msgDecodedFunction, msgDecodedPayloadLength, msgDecodedPayload);
            }
            rcvState = Waiting;
            break;
        default:
            rcvState = Waiting;
            break;
    }
}

int L1 = 0;
int L2 = 0;
int L3 = 0;
int L4 = 0;
int L5 = 0;

void UartProcessDecodedMessage(int function, int payloadLength, unsigned char* payload) {
    int Signe = 1;
    switch (function) {
        case ControlXbox:
            if (payload[0] == 1) {
                Signe = -1;
            }


            PWMSetSpeedConsigne(Signe * payload[1], MOTEUR_DROIT);
            PWMSetSpeedConsigne(Signe * payload[2], MOTEUR_GAUCHE);

            break;
        case SET_ROBOT_STATE:
            // SetRobotState(payload[0]);
            break;
        case SET_ROBOT_MANUAL_CONTROL:
            SetRobotAutoControlState(payload[0]);
//        case SetPID : 
//            SetupPidAsservissement( )
        case SetLed:
            switch (payload[0]) {
                case 1:
                    L1 = payload[1];
                    break;
                case 2:
                    L2 = payload[1];
                    break;
                case 3:
                    L3 = payload[1];
                    break;
                case 4:
                    L4 = payload[1];
                    break;
                case 5:
                    L5 = payload[1];
                    break;
            }
            if (L1 == 1) {

                LED_BLANCHE_2 = L1;
            }
            LED_BLANCHE_2 = L1;
            LED_ORANGE_2 = L2;
            LED_ROUGE_2 = L3;
            LED_VERTE_2 = L4;
            LED_BLEUE_2 = L5;
            break;
        default:
            break;
    }
}

