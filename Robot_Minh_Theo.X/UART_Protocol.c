#include <xc.h>
#include "UART_Protocol.h"
#include "CB_TX1.h"
#include "CB_RX1.h"
#include "CB_TX2.h"
#include "CB_RX2.h"
#include "PWM.h"
#include "IO.h"
#include "robot.h"
#include "Utilities.h"
#include "asservissement.h"
#include "ghost.h"
#include "aruco_ghost.h"




unsigned char payload_PidX[12] = {};
unsigned char payload_PidT[12] = {};
unsigned char payload_test[12] = {};
unsigned char aruco_flag = 0;
extern unsigned long timestamp;
unsigned long aruco_time = 0;

unsigned char UartCalculateChecksum(int msgFunction, int msgPayloadLength, unsigned char *msgPayload) {
    unsigned char checksum = 0;

    checksum ^= 0xFE;
    checksum ^= (unsigned char) (msgFunction >> 8);
    checksum ^= (unsigned char) (msgFunction);
    checksum ^= (unsigned char) (msgPayloadLength >> 8);
    checksum ^= (unsigned char) (msgPayloadLength);
    for (int i = 0; i < msgPayloadLength; i++)
        checksum ^= msgPayload[i];

    return checksum;
}

void UartEncodeAndSendMessage(int msgFunction, int msgPayloadLength, unsigned char *msgPayload) {
    unsigned char message[msgPayloadLength + 6];
    int pos = 0;

    message[pos++] = 0xFE;
    message[pos++] = (unsigned char) (msgFunction >> 8);
    message[pos++] = (unsigned char) (msgFunction);
    message[pos++] = (unsigned char) (msgPayloadLength >> 8);
    message[pos++] = (unsigned char) (msgPayloadLength);

    for (int i = 0; i < msgPayloadLength; i++)
        message[pos++] = msgPayload[i];

    message[pos++] = UartCalculateChecksum(msgFunction, msgPayloadLength, msgPayload);
    SendMessageTX1(message, pos);
}

int rcvStateTX1 = Waiting;
int msgDecodedFunctionTX1 = 0;
int msgDecodedPayloadLengthTX1 = 0;
unsigned char msgDecodedPayloadTX1[1024];
int msgDecodedPayloadIndexTX1 = 0;
unsigned char calculatedChecksumTX1;
unsigned char receivedChecksumTX1;

void UartDecodeMessageTX1(unsigned char c) {
    switch (rcvStateTX1) {
        case Waiting:
            if (c == 0xFE) {
                msgDecodedPayloadIndexTX1 = 0;
                rcvStateTX1 = FunctionMSB;
            }
            break;

        case FunctionMSB:
            msgDecodedFunctionTX1 = c << 8;
            rcvStateTX1 = FunctionLSB;
            break;

        case FunctionLSB:
            msgDecodedFunctionTX1 |= c;
            rcvStateTX1 = PayloadLengthMSB;
            break;

        case PayloadLengthMSB:
            msgDecodedPayloadLengthTX1 = c << 8;
            rcvStateTX1 = PayloadLengthLSB;
            break;

        case PayloadLengthLSB:
            msgDecodedPayloadLengthTX1 |= c;
            msgDecodedPayloadIndexTX1 = 0;
            if (msgDecodedPayloadLengthTX1 == 0)
                rcvStateTX1 = CheckSum;
            else if (msgDecodedPayloadLengthTX1 < 1024)
                rcvStateTX1 = Payload;
            else
                rcvStateTX1 = Waiting;
            break;

        case Payload:
            msgDecodedPayloadTX1[msgDecodedPayloadIndexTX1++] = c;
            if (msgDecodedPayloadIndexTX1 >= msgDecodedPayloadLengthTX1)
                rcvStateTX1 = CheckSum;
            break;

        case CheckSum:
            calculatedChecksumTX1 = UartCalculateChecksum(
                    msgDecodedFunctionTX1,
                    msgDecodedPayloadLengthTX1,
                    msgDecodedPayloadTX1
                    );
            receivedChecksumTX1 = c;
            if (calculatedChecksumTX1 == receivedChecksumTX1)
                UartProcessDecodedMessage(
                    msgDecodedFunctionTX1,
                    msgDecodedPayloadLengthTX1,
                    msgDecodedPayloadTX1
                    );
            rcvStateTX1 = Waiting;
            break;

        default:
            rcvStateTX1 = Waiting;
            break;
    }
}

int rcvStateTX2 = Waiting;
int msgDecodedFunctionTX2 = 0;
int msgDecodedPayloadLengthTX2 = 0;
unsigned char msgDecodedPayloadTX2[1024];
int msgDecodedPayloadIndexTX2 = 0;
unsigned char calculatedChecksumTX2;
unsigned char receivedChecksumTX2;

void UartDecodeMessageTX2(unsigned char c) {
    switch (rcvStateTX2) {
        case Waiting:
            if (c == 0xFE) {
                msgDecodedPayloadIndexTX2 = 0;
                rcvStateTX2 = FunctionMSB;
            }
            break;

        case FunctionMSB:
            msgDecodedFunctionTX2 = c << 8;
            rcvStateTX2 = FunctionLSB;
            break;

        case FunctionLSB:
            msgDecodedFunctionTX2 |= c;
            rcvStateTX2 = PayloadLengthMSB;
            break;

        case PayloadLengthMSB:
            msgDecodedPayloadLengthTX2 = c << 8;
            rcvStateTX2 = PayloadLengthLSB;
            break;

        case PayloadLengthLSB:
            msgDecodedPayloadLengthTX2 |= c;
            msgDecodedPayloadIndexTX2 = 0;
            if (msgDecodedPayloadLengthTX2 == 0)
                rcvStateTX2 = CheckSum;
            else if (msgDecodedPayloadLengthTX2 < 1024)
                rcvStateTX2 = Payload;
            else
                rcvStateTX2 = Waiting;
            break;

        case Payload:
            msgDecodedPayloadTX2[msgDecodedPayloadIndexTX2++] = c;
            if (msgDecodedPayloadIndexTX2 >= msgDecodedPayloadLengthTX2)
                rcvStateTX2 = CheckSum;
            break;

        case CheckSum:
            calculatedChecksumTX2 = UartCalculateChecksum(
                    msgDecodedFunctionTX2,
                    msgDecodedPayloadLengthTX2,
                    msgDecodedPayloadTX2
                    );
            receivedChecksumTX2 = c;
            if (calculatedChecksumTX2 == receivedChecksumTX2)
                UartProcessDecodedMessage(
                    msgDecodedFunctionTX2,
                    msgDecodedPayloadLengthTX2,
                    msgDecodedPayloadTX2
                    );
            rcvStateTX2 = Waiting;
            break;

        default:
            rcvStateTX2 = Waiting;
            break;
    }
}


int L1 = 0;
int L2 = 0;
int L3 = 0;
int L4 = 0;
int L5 = 0;
float kp = 0;
float ki = 0;
float kd = 0;

float kp_PD_Lin;
float ki_PD_Lin;
float kd_PD_Lin;

float kp_PD_Ang;
float ki_PD_Ang;
float kd_PD_Ang;
Point posRobot;
Point posTarget;
int GhostFlag = 0;

void UartProcessDecodedMessage(int function, int payloadLength, unsigned char* payload) {
    switch (function) {
            //        case ControlXbox:
            //            if (payload[0] == 1) {
            //                Signe = -1;
            //            }
            //
            //
            //            PWMSetSpeedConsignePercent(Signe * payload[1], MOTEUR_DROIT);
            //            PWMSetSpeedConsignePercent(Signe * payload[2], MOTEUR_GAUCHE);
            //
            //            break;
        case SET_ROBOT_STATE:
            // SetRobotState(payload[0]);
            break;
        case SET_ROBOT_MANUAL_CONTROL:
            SetRobotAutoControlState(payload[0]);
            break;
        case setConsigne:
            robotState.vitesseLinearConsigne = getFloat(payload, 0);
            robotState.vitesseAngulaireConsigne = getFloat(payload, 4);


            break;
        case SetPIDX:

            robotState.PidX.corrP = 0;
            robotState.PidX.corrI = 0;
            robotState.PidX.corrD = 0;
            robotState.PidX.erreurIntegrale = 0;
            robotState.CorrectionVitesseLineaire = 0;
            robotState.vitesseLinearConsigne = 0;


            kp = getFloat(payload, 0);
            ki = getFloat(payload, 4);
            kd = getFloat(payload, 8);

            SetupPidAsservissement(&robotState.PidX, (double) kp, (double) ki, (double) kd, 100, 100, 100);



            UartEncodeAndSendMessage(SetPIDX, 12, payload_PidX);
            break;
        case SetPIDT:

            robotState.PidTheta.corrP = 0;
            robotState.PidTheta.corrI = 0;
            robotState.PidTheta.corrD = 0;
            robotState.PidTheta.erreurIntegrale = 0;
            robotState.CorrectionVitesseAngulaire = 0;
            robotState.vitesseAngulaireConsigne = 0;


            kp = getFloat(payload, 0);
            ki = getFloat(payload, 4);
            kd = getFloat(payload, 8);

            getBytesFromFloat(payload_PidT, 0, (float) (kp));
            getBytesFromFloat(payload_PidT, 4, (float) (ki));
            getBytesFromFloat(payload_PidT, 8, (float) (kd));
            SetupPidAsservissement(&robotState.PidTheta, (double) kp, (double) ki, (double) kd, 100, 100, 100);
            UartEncodeAndSendMessage(SetPIDT, 12, payload_PidT);
            break;
        case resetPid:
            robotState.PidTheta.corrP = 0;
            robotState.PidTheta.corrI = 0;
            robotState.PidTheta.corrD = 0;
            robotState.PidTheta.erreurIntegrale = 0;

            robotState.PidX.corrP = 0;
            robotState.PidX.corrI = 0;
            robotState.PidX.corrD = 0;
            robotState.PidX.erreurIntegrale = 0;
            robotState.CorrectionVitesseAngulaire = 0;
            robotState.CorrectionVitesseLineaire = 0;


            robotState.PD_Position_Lineaire.corrP = 0;
            robotState.PD_Position_Lineaire.corrI = 0;
            robotState.PD_Position_Lineaire.corrD = 0;
            robotState.PD_Position_Lineaire.erreurIntegrale = 0;

            robotState.PD_Position_Angulaire.corrP = 0;
            robotState.PD_Position_Angulaire.corrI = 0;
            robotState.PD_Position_Angulaire.corrD = 0;
            robotState.PD_Position_Angulaire.erreurIntegrale = 0;



            SetupPidAsservissement(&robotState.PidTheta, 0, 0, 0, 100, 100, 100);
            SetupPidAsservissement(&robotState.PidX, 0, 0, 0, 100, 100, 100);
            SetupPidAsservissement(&robotState.PD_Position_Lineaire, 0, 0, 0, 100, 100, 100);
            SetupPidAsservissement(&robotState.PD_Position_Angulaire, 0, 0, 0, 100, 100, 100);

            break;

        case ghostSetPID:
            robotState.PD_Position_Lineaire.corrP = 0;
            robotState.PD_Position_Lineaire.corrI = 0;
            robotState.PD_Position_Lineaire.corrD = 0;
            robotState.PD_Position_Lineaire.erreurIntegrale = 0;

            robotState.PD_Position_Angulaire.corrP = 0;
            robotState.PD_Position_Angulaire.corrI = 0;
            robotState.PD_Position_Angulaire.corrD = 0;
            robotState.PD_Position_Angulaire.erreurIntegrale = 0;


            kp_PD_Lin = getFloat(payload, 0);
            ki_PD_Lin = getFloat(payload, 4);
            kd_PD_Lin = getFloat(payload, 8);

            kp_PD_Ang = getFloat(payload, 12);
            ki_PD_Ang = getFloat(payload, 16);
            kd_PD_Ang = getFloat(payload, 20);

            getBytesFromFloat(payload_PidT, 0, (float) (kp_PD_Lin));
            getBytesFromFloat(payload_PidT, 4, (float) (ki_PD_Lin));
            getBytesFromFloat(payload_PidT, 8, (float) (kd_PD_Lin));

            getBytesFromFloat(payload_PidT, 12, (float) (kp_PD_Ang));
            getBytesFromFloat(payload_PidT, 16, (float) (ki_PD_Ang));
            getBytesFromFloat(payload_PidT, 20, (float) (kd_PD_Ang));


            SetupPidAsservissement(&robotState.PD_Position_Lineaire, (double) kp_PD_Lin, (double) ki_PD_Lin, (double) kd_PD_Lin, 100, 100, 100);
            SetupPidAsservissement(&robotState.PD_Position_Angulaire, (double) kp_PD_Ang, (double) ki_PD_Ang, (double) kd_PD_Ang, 100, 100, 100);

            break;

        case Ghost_angle:
            GhostFlag = 1;
            robotState.positionWaypoint.x = getFloat(payload, 0);
            robotState.positionWaypoint.y = getFloat(payload, 4);



            break;
        case Aruco_Detected:
            LED_BLANCHE_2 = !LED_BLANCHE_2;
            float X = getFloat(payload, 2);
            float Y = getFloat(payload, 6);
            float Z = getFloat(payload, 10);
            robotState.Aruco_ID = getIntFrom2Bytes(payload, 0);
            if (robotState.Aruco_ID == 36) {
                aruco_time = timestamp;

                robotState.X_Aruco = X;
                robotState.Y_Aruco = Y;
                robotState.Z_Aruco = Z;
            }
            //ArUco_ProcessMessage();

            break;
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

