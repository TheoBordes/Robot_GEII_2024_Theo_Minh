#include <stdio.h>
#include <stdlib.h>
#include <xc.h>
#include "UART.h"
#include "ChipConfig.h"
#include "QEI.h"
#include "IO.h"
#include "timer.h"
#include "PWM.h" 
#include "ADC.h"
#include "robot.h"
#include "CB_TX1.h"
#include "CB_RX1.h"
#include "UART_Protocol.h"
#include "main.h"
#include <math.h>

#define SPEED 10
#define NUM_INPUTS 5
#define NUM_OUTPUTS 2
#define NUM_HIDDEN 3
#define NUM_TRAINING_SETS 4

int speeds[2];
int vitL, vitR;
unsigned char stateRobot;
int CapVal = 0;
unsigned char payload_Telemetre[5] = {};
unsigned char payload_motors[2] = {};


//void determine_speeds2() {
//    int left_1 = robotState.distanceTelemetreGauche;
//    int left_2 = robotState.distanceTelemetrePlusGauche;
//    int center = robotState.distanceTelemetreCentre;
//    int right_1 = robotState.distanceTelemetreDroit;
//    int right_2 = robotState.distanceTelemetrePlusDroit;
//    
//    
//    float avg_left = (left_1 + left_2) / 2.0;
//    float avg_right = (right_1 + right_2) / 2.0;
//    
//    float weighted_avg_left = ( 2*avg_left + center) / 3.0;
//    float weighted_avg_right = (2* avg_right + center) / 3.0;
//    
//
//    float max_speed = 30.0;
//    float min_speed = 5.0;
//    
// 
//    float speedsL = min_speed + ((weighted_avg_left / 100.0) * (max_speed - min_speed));
//    float speedsR = min_speed + ((weighted_avg_right / 100.0) * (max_speed - min_speed));
//    
//    PWMSetSpeedConsigne(speedsL, MOTEUR_DROIT);
//    PWMSetSpeedConsigne(speedsR, MOTEUR_GAUCHE);
//}

void determine_speeds() {
    int left_1 = robotState.distanceTelemetreGauche;
    int left_2 = robotState.distanceTelemetrePlusGauche;
    int center = robotState.distanceTelemetreCentre;
    int right_1 = robotState.distanceTelemetreDroit;
    int right_2 = robotState.distanceTelemetrePlusDroit;


    float avg_left = (left_1 + left_2) / 2.0;
    float avg_right = (right_1 + right_2) / 2.0;

    float weighted_avg_left = (2.0 * avg_left + center) / 3.0;
    float weighted_avg_right = (2.0 * avg_right + center) / 3.0;

    float max_speed = 23.0;
    float min_speed = 12.0;

    speeds[1] = min_speed + ((weighted_avg_left / 100.0) * (max_speed - min_speed));
    speeds[0] = min_speed + ((weighted_avg_right / 100.0) * (max_speed - min_speed));
    if (speeds[0] > 23.0) {
        LED_BLANCHE_2 = 1;

    } else {
        LED_BLANCHE_2 = 0;
    }
}

int trigger(float capteur) {
    if (capteur < DIST_MAX) {
        return 1;
    } else {
        return 0;
    }
}

int trigger_mid(float capteur) {
    if (capteur < DIST_MAX_MID) {
        return 1;
    } else {
        return 0;
    }
}

unsigned char CallCap() {
    unsigned char value = 0;
    if (trigger(robotState.distanceTelemetrePlusGauche)) {
        value |= 0b10000;

    }
    if (trigger(robotState.distanceTelemetreGauche)) {
        value |= 0b01000;

    }
    if (trigger(robotState.distanceTelemetreCentre)) {
        value |= 0b00100;

    }
    if (trigger(robotState.distanceTelemetreDroit)) {
        value |= 0b000010;

    }
    if (trigger(robotState.distanceTelemetrePlusDroit)) {
        value |= 0b00001;

    }
    return value;
}

void OperatingSystemLoop(void) {
    //determine_speeds();
    switch (stateRobot) {

        case STATE_ATTENTE:
            timestamp = 0;
            PWMSetSpeedConsigne(ARRET, MOTEUR_DROIT);
            PWMSetSpeedConsigne(ARRET, MOTEUR_GAUCHE);
            stateRobot = STATE_ATTENTE_EN_COURS;
        case STATE_ATTENTE_EN_COURS:
            if (timestamp > 1000)
                stateRobot = STATE_AVANCE;
            break;
        case STATE_AVANCE:
            if (robotState.distanceTelemetreCentre > 80) {
                vitR = 25;
                vitL = 25;
            } else {
                vitR = 15;
                vitL = 15;
            }
            PWMSetSpeedConsigne(speeds[0], MOTEUR_DROIT);
            PWMSetSpeedConsigne(speeds[1], MOTEUR_GAUCHE);
            stateRobot = STATE_AVANCE_EN_COURS;
            break;
        case STATE_AVANCE_EN_COURS:
            SetNextRobotStateInAutomaticMode();
            break;
        case STATE_TOURNE_GAUCHE:
            PWMSetSpeedConsigne(ARRET, MOTEUR_DROIT);
            PWMSetSpeedConsigne(speeds[0], MOTEUR_GAUCHE);
            stateRobot = STATE_TOURNE_GAUCHE_EN_COURS;
            break;
        case STATE_TOURNE_GAUCHE_EN_COURS:
            SetNextRobotStateInAutomaticMode();
            break;

        case STATE_TOURNE_GAUCHE_PLUS:
            PWMSetSpeedConsigne(ARRET, MOTEUR_DROIT);
            PWMSetSpeedConsigne(speeds[0], MOTEUR_GAUCHE);
            stateRobot = STATE_TOURNE_GAUCHE_PLUS_EN_COURS;
            break;
        case STATE_TOURNE_GAUCHE_PLUS_EN_COURS:
            SetNextRobotStateInAutomaticMode();
            break;

        case STATE_TOURNE_DROITE:
            PWMSetSpeedConsigne(speeds[1], MOTEUR_DROIT);
            PWMSetSpeedConsigne(ARRET, MOTEUR_GAUCHE);
            stateRobot = STATE_TOURNE_DROITE_EN_COURS;
            break;
        case STATE_TOURNE_DROITE_EN_COURS:
            SetNextRobotStateInAutomaticMode();
            break;

        case STATE_TOURNE_DROITE_PLUS:
            PWMSetSpeedConsigne(speeds[1], MOTEUR_DROIT);
            PWMSetSpeedConsigne(ARRET, MOTEUR_GAUCHE);
            stateRobot = STATE_TOURNE_DROITE_PLUS_EN_COURS;
            break;
        case STATE_TOURNE_DROITE_PLUS_EN_COURS:
            SetNextRobotStateInAutomaticMode();
            break;

        case STATE_TOURNE_SUR_PLACE_GAUCHE:
            PWMSetSpeedConsigne(15 + speeds[1], MOTEUR_DROIT);
            PWMSetSpeedConsigne(-15 - speeds[0], MOTEUR_GAUCHE);
            stateRobot = STATE_TOURNE_SUR_PLACE_GAUCHE_EN_COURS;
            break;
        case STATE_TOURNE_SUR_PLACE_GAUCHE_EN_COURS:
            SetNextRobotStateInAutomaticMode();
            break;
        case STATE_TOURNE_SUR_PLACE_DROITE:
            PWMSetSpeedConsigne(-15 - speeds[1], MOTEUR_DROIT);
            PWMSetSpeedConsigne(15 + speeds[0], MOTEUR_GAUCHE);
            stateRobot = STATE_TOURNE_SUR_PLACE_DROITE_EN_COURS;
            break;
        case STATE_TOURNE_SUR_PLACE_DROITE_EN_COURS:
            SetNextRobotStateInAutomaticMode();
            break;
        default:
            stateRobot = STATE_ATTENTE;
            break;
    }
}

//void OperatingSystemLoop(void) {
//
//    switch (stateRobot) {
//        case STATE_ATTENTE:
//            timestamp = 0;
//            PWMSetSpeedConsigne(ARRET, MOTEUR_DROIT);
//            PWMSetSpeedConsigne(ARRET, MOTEUR_GAUCHE);
//            stateRobot = STATE_ATTENTE_EN_COURS;
//        case STATE_ATTENTE_EN_COURS:
//            if (timestamp > 1000)
//                stateRobot = STATE_AVANCE;
//            break;
//        case STATE_AVANCE:
//            PWMSetSpeedConsigne(STATE_VIT_AVANCE, MOTEUR_DROIT);
//            PWMSetSpeedConsigne(STATE_VIT_AVANCE, MOTEUR_GAUCHE);
//            stateRobot = STATE_AVANCE_EN_COURS;
//            break;
//        case STATE_AVANCE_EN_COURS:
//            SetNextRobotStateInAutomaticMode();
//            break;
//        case STATE_TOURNE_GAUCHE:
//            PWMSetSpeedConsigne(ARRET, MOTEUR_DROIT);
//            PWMSetSpeedConsigne(TOURNER_VIT, MOTEUR_GAUCHE);
//            stateRobot = STATE_TOURNE_GAUCHE_EN_COURS;
//            break;
//        case STATE_TOURNE_GAUCHE_EN_COURS:
//            SetNextRobotStateInAutomaticMode();
//            break;
//
//        case STATE_TOURNE_GAUCHE_PLUS:
//            PWMSetSpeedConsigne(ARRET, MOTEUR_DROIT);
//            PWMSetSpeedConsigne(TOURNER_VIT, MOTEUR_GAUCHE);
//            stateRobot = STATE_TOURNE_GAUCHE_PLUS_EN_COURS;
//            break;
//        case STATE_TOURNE_GAUCHE_PLUS_EN_COURS:
//            SetNextRobotStateInAutomaticMode();
//            break;
//
//        case STATE_TOURNE_DROITE:
//            PWMSetSpeedConsigne(TOURNER_VIT, MOTEUR_DROIT);
//            PWMSetSpeedConsigne(ARRET, MOTEUR_GAUCHE);
//            stateRobot = STATE_TOURNE_DROITE_EN_COURS;
//            break;
//        case STATE_TOURNE_DROITE_EN_COURS:
//            SetNextRobotStateInAutomaticMode();
//            break;
//
//        case STATE_TOURNE_DROITE_PLUS:
//            PWMSetSpeedConsigne(TOURNER_VIT, MOTEUR_DROIT);
//            PWMSetSpeedConsigne(ARRET, MOTEUR_GAUCHE);
//            stateRobot = STATE_TOURNE_DROITE_PLUS_EN_COURS;
//            break;
//        case STATE_TOURNE_DROITE_PLUS_EN_COURS:
//            SetNextRobotStateInAutomaticMode();
//            break;
//
//        case STATE_TOURNE_SUR_PLACE_GAUCHE:
//            PWMSetSpeedConsigne(TOURNER_SUR_PLACE_VIT, MOTEUR_DROIT);
//            PWMSetSpeedConsigne(-TOURNER_SUR_PLACE_VIT, MOTEUR_GAUCHE);
//            stateRobot = STATE_TOURNE_SUR_PLACE_GAUCHE_EN_COURS;
//            break;
//        case STATE_TOURNE_SUR_PLACE_GAUCHE_EN_COURS:
//            SetNextRobotStateInAutomaticMode();
//            break;
//        case STATE_TOURNE_SUR_PLACE_DROITE:
//            PWMSetSpeedConsigne(-TOURNER_SUR_PLACE_VIT, MOTEUR_DROIT);
//            PWMSetSpeedConsigne(TOURNER_SUR_PLACE_VIT, MOTEUR_GAUCHE);
//            stateRobot = STATE_TOURNE_SUR_PLACE_DROITE_EN_COURS;
//            break;
//        case STATE_TOURNE_SUR_PLACE_DROITE_EN_COURS:
//            SetNextRobotStateInAutomaticMode();
//            break;
//        default:
//            stateRobot = STATE_ATTENTE;
//            break;
//    }
//}

void ADC_value() {
    if (ADCIsConversionFinished()) {
        ADCClearConversionFinishedFlag();
        unsigned int * result = ADCGetResult();
        float volts = ((float) result [0])* 3.3 / 4096;
        if (volts < 0.325)volts = 0.325; // permet de bloquer une détection à 100 centimètre
        robotState.distanceTelemetrePlusGauche = 34 / volts - 5;
        volts = ((float) result [1])* 3.3 / 4096;
        if (volts < 0.325)volts = 0.325;
        robotState.distanceTelemetreGauche = 34 / volts - 5;
        volts = ((float) result [2])* 3.3 / 4096;
        if (volts < 0.325)volts = 0.325;
        robotState.distanceTelemetreCentre = 34 / volts - 5;
        volts = ((float) result [3])* 3.3 / 4096;
        if (volts < 0.325)volts = 0.325;
        robotState.distanceTelemetreDroit = 34 / volts - 5;
        volts = ((float) result [4])* 3.3 / 4096;
        if (volts < 0.325)volts = 0.325;
        robotState.distanceTelemetrePlusDroit = 34 / volts - 5;
        payload_Telemetre[0] = (unsigned char) robotState.distanceTelemetrePlusGauche;
        payload_Telemetre[1] = (unsigned char) robotState.distanceTelemetreGauche;
        payload_Telemetre[2] = (unsigned char) robotState.distanceTelemetreCentre;
        payload_Telemetre[3] = (unsigned char) robotState.distanceTelemetreDroit;
        payload_Telemetre[4] = (unsigned char) robotState.distanceTelemetrePlusDroit;
        UartEncodeAndSendMessage(0x0030, 5, payload_Telemetre);        
    }
}


unsigned char nextStateRobot = 0;

void SetNextRobotStateInAutomaticMode() {
    CapVal = CallCap();
    switch (CapVal) {
        case 0b00000:
            nextStateRobot = STATE_AVANCE;
            break;
        case 0b00001:
            nextStateRobot = STATE_TOURNE_GAUCHE;
            break;
        case 0b00010:
            nextStateRobot = STATE_TOURNE_GAUCHE;
            break;
        case 0b00011:
            nextStateRobot = STATE_TOURNE_GAUCHE;
            break;
        case 0b00100:
            nextStateRobot = STATE_TOURNE_SUR_PLACE_GAUCHE;
            break;
        case 0b00101:
            break;
        case 0b00110:
            nextStateRobot = STATE_TOURNE_GAUCHE;
            break;
        case 0b00111:
            nextStateRobot = STATE_TOURNE_GAUCHE;
            break;
        case 0b01000:
            nextStateRobot = STATE_TOURNE_DROITE;
            break;
        case 0b01001:
            nextStateRobot = STATE_TOURNE_DROITE; //                      attention 
            break;
        case 0b01010:
            nextStateRobot = STATE_TOURNE_DROITE;
            break;
        case 0b01011:
            nextStateRobot = STATE_TOURNE_DROITE;
            break;
        case 0b01100:
            nextStateRobot = STATE_TOURNE_DROITE;
            break;
        case 0b01101:
            nextStateRobot = STATE_TOURNE_SUR_PLACE_DROITE;
            break;
        case 0b01110:
            nextStateRobot = STATE_TOURNE_SUR_PLACE_DROITE;
            break;
        case 0b01111:
            nextStateRobot = STATE_TOURNE_SUR_PLACE_GAUCHE; // ATTENTION
            break;
        case 0b10000:
            nextStateRobot = STATE_TOURNE_DROITE;
            break;
        case 0b10001:
            nextStateRobot = STATE_AVANCE;
            break;
        case 0b10010:
            nextStateRobot = STATE_TOURNE_GAUCHE;
            break;
        case 0b10011:
            nextStateRobot = STATE_TOURNE_SUR_PLACE_DROITE;
            break;
        case 0b10100:
            nextStateRobot = STATE_TOURNE_SUR_PLACE_DROITE;
            break;
        case 0b10101:
            nextStateRobot = STATE_TOURNE_SUR_PLACE_DROITE;
            break;
        case 0b10110:
            nextStateRobot = STATE_TOURNE_DROITE;
            break;
        case 0b10111:
            nextStateRobot = STATE_TOURNE_SUR_PLACE_DROITE;
            break;
        case 0b11000:
            nextStateRobot = STATE_TOURNE_DROITE;
            break;
        case 0b11001:
            nextStateRobot = STATE_TOURNE_DROITE; // ATTENTION
            break;
        case 0b11010:
            nextStateRobot = STATE_TOURNE_DROITE;
            break;
        case 0b11011:
            nextStateRobot = STATE_TOURNE_DROITE;
            break;
        case 0b11100:
            nextStateRobot = STATE_TOURNE_DROITE;
            break;
        case 0b11101:
            nextStateRobot = STATE_TOURNE_SUR_PLACE_DROITE;
            break;
        case 0b11110:
            nextStateRobot = STATE_TOURNE_SUR_PLACE_DROITE;
            break;
        case 0b11111:
            nextStateRobot = STATE_TOURNE_SUR_PLACE_DROITE;
            break;
        default:
            break;
    }

    if (nextStateRobot != stateRobot - 1) {
        stateRobot = nextStateRobot;
        robotState.taskEnCours = stateRobot;
    }
}

int main(void) {
    /***********************************************************************************************Initialisation oscillateur*/
    /***********************************************************************************************/
    InitOscillator();
    /*********************************************************************************************** Configuration des input et output (IO)*/
    /***********************************************************************************************/
    InitIO();
    LED_BLEUE_1 = 1;
    LED_BLANCHE_1 = 1;
    LED_ORANGE_1 = 1;
    LED_VERTE_1 = 1;
    LED_ROUGE_1 = 1;
    
    InitQEI1();
    InitQEI2();
    InitTimer23();
    InitTimer1();
    InitTimer4();
    InitPWM();
    InitADC1();
    InitUART();
    /*********************************************************************************************** Boucle Principale*/
    /***********************************************************************************************/
    //int vitesse = 20;


    if (BOUTON1 == 0) {


        while (1) {

            while (CB_RX1_IsDataAvailable()) {
                UartDecodeMessage(CB_RX1_Get());
            }
//            if (flagMessageMotor) {
//                flagMessageMotor = 0;
//                payload_motors[0] = (unsigned char) robotState.vitesseGaucheConsigne;
//                payload_motors[1] = (unsigned char) robotState.vitesseDroiteConsigne;
//                UartEncodeAndSendMessage(0x0040, 2, payload_motors);
//            }         
//            else {
//            }
            if (robotState.distanceTelemetreGauche > 20) {
                LED_BLEUE_1 = 1;
            } else {
                LED_BLEUE_1 = 0;

            }
            if (robotState.distanceTelemetrePlusGauche > 20) {
                LED_BLANCHE_1 = 1;

            } else {
                LED_BLANCHE_1 = 0;

            }
            if (robotState.distanceTelemetreCentre > 20) {
                LED_ORANGE_1 = 1;

            } else {
                LED_ORANGE_1 = 0;

            }
            if (robotState.distanceTelemetrePlusDroit > 20) {
                LED_VERTE_1 = 1;

            } else {
                LED_VERTE_1 = 0;

            }
            if (robotState.distanceTelemetreDroit > 20) {
                LED_ROUGE_1 = 1;
            } else {
                LED_ROUGE_1 = 0;
            }

        }
    }
}
