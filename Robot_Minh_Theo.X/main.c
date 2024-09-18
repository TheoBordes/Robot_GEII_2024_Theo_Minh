#include <stdio.h>
#include <stdlib.h>
#include <xc.h>
#include "ChipConfig.h"
#include "IO.h"
#include "timer.h"
#include "PWM.h" 
#include "ADC.h"
#include "robot.h"
#include "main.h"
#include <math.h>
#define SPEED 10

unsigned char stateRobot;
int CapVal = 0;

int trigger(float capteur) {
    if (capteur < DIST_MAX) {
        return 1;
    }
    else {
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
            PWMSetSpeedConsigne(STATE_VIT_AVANCE, MOTEUR_DROIT);
            PWMSetSpeedConsigne(STATE_VIT_AVANCE, MOTEUR_GAUCHE);
            stateRobot = STATE_AVANCE_EN_COURS;
            break;
        case STATE_AVANCE_EN_COURS:
            SetNextRobotStateInAutomaticMode();
            break;
        case STATE_TOURNE_GAUCHE:
            PWMSetSpeedConsigne(ARRET, MOTEUR_DROIT);
            PWMSetSpeedConsigne(TOURNER_VIT, MOTEUR_GAUCHE);
            stateRobot = STATE_TOURNE_GAUCHE_EN_COURS;
            break;
        case STATE_TOURNE_GAUCHE_EN_COURS:
            SetNextRobotStateInAutomaticMode();
            break;

        case STATE_TOURNE_GAUCHE_PLUS:
            PWMSetSpeedConsigne(ARRET, MOTEUR_DROIT);
            PWMSetSpeedConsigne(TOURNER_VIT, MOTEUR_GAUCHE);
            stateRobot = STATE_TOURNE_GAUCHE_PLUS_EN_COURS;
            break;
        case STATE_TOURNE_GAUCHE_PLUS_EN_COURS:
            SetNextRobotStateInAutomaticMode();
            break;

        case STATE_TOURNE_DROITE:
            PWMSetSpeedConsigne(TOURNER_VIT, MOTEUR_DROIT);
            PWMSetSpeedConsigne(ARRET, MOTEUR_GAUCHE);
            stateRobot = STATE_TOURNE_DROITE_EN_COURS;
            break;
        case STATE_TOURNE_DROITE_EN_COURS:
            SetNextRobotStateInAutomaticMode();
            break;

        case STATE_TOURNE_DROITE_PLUS:
            PWMSetSpeedConsigne(TOURNER_VIT, MOTEUR_DROIT);
            PWMSetSpeedConsigne(ARRET, MOTEUR_GAUCHE);
            stateRobot = STATE_TOURNE_DROITE_PLUS_EN_COURS;
            break;
        case STATE_TOURNE_DROITE_PLUS_EN_COURS:
            SetNextRobotStateInAutomaticMode();
            break;

        case STATE_TOURNE_SUR_PLACE_GAUCHE:
            PWMSetSpeedConsigne(TOURNER_SUR_PLACE_VIT, MOTEUR_DROIT);
            PWMSetSpeedConsigne(-TOURNER_SUR_PLACE_VIT, MOTEUR_GAUCHE);
            stateRobot = STATE_TOURNE_SUR_PLACE_GAUCHE_EN_COURS;
            break;
        case STATE_TOURNE_SUR_PLACE_GAUCHE_EN_COURS:
            SetNextRobotStateInAutomaticMode();
            break;
        case STATE_TOURNE_SUR_PLACE_DROITE:
            PWMSetSpeedConsigne(-TOURNER_SUR_PLACE_VIT, MOTEUR_DROIT);
            PWMSetSpeedConsigne(TOURNER_SUR_PLACE_VIT, MOTEUR_GAUCHE);
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
    }
}


unsigned char nextStateRobot = 0;

void SetNextRobotStateInAutomaticMode() {
     CapVal = CallCap();
    switch (CapVal) {
    case 0b00000:
        nextStateRobot  = STATE_AVANCE;
        break;
    case 0b00001:
        nextStateRobot  = STATE_TOURNE_GAUCHE;
        break;
    case 0b00010:
        nextStateRobot  = STATE_TOURNE_GAUCHE;
        break;
    case 0b00011:
        nextStateRobot  = STATE_TOURNE_GAUCHE;
        break;
    case 0b00100:
        nextStateRobot  = STATE_TOURNE_SUR_PLACE_GAUCHE;
        break;
    case 0b00101:
        nextStateRobot  = STATE_TOURNE_SUR_PLACE_GAUCHE ;
        break;
    case 0b00110:
        nextStateRobot  = STATE_TOURNE_GAUCHE;
        break;
    case 0b00111:
        nextStateRobot  = STATE_TOURNE_GAUCHE; 
        break;
    case 0b01000:
        nextStateRobot  = STATE_TOURNE_DROITE;
        break;
    case 0b01001:
        nextStateRobot  = STATE_TOURNE_DROITE; //                      attention 
        break;
    case 0b01010:
        nextStateRobot = STATE_TOURNE_SUR_PLACE_DROITE;
        break;
    case 0b01011:
        nextStateRobot = STATE_TOURNE_SUR_PLACE_DROITE;
        break;
    case 0b01100:
        nextStateRobot  = STATE_TOURNE_DROITE;
        break;
    case 0b01101:
      nextStateRobot = STATE_TOURNE_SUR_PLACE_DROITE;
        break;
    case 0b01110:
        nextStateRobot  = STATE_TOURNE_SUR_PLACE_DROITE;
        break;
    case 0b01111:
        nextStateRobot = STATE_TOURNE_SUR_PLACE_GAUCHE; // ATTENTION
        break;  
    case 0b10000:
        nextStateRobot  = STATE_TOURNE_DROITE;
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
        nextStateRobot  = STATE_TOURNE_SUR_PLACE_DROITE;
        break;
    case 0b10101:
        nextStateRobot = STATE_TOURNE_SUR_PLACE_DROITE;
        break;
    case 0b10110:
        nextStateRobot = STATE_TOURNE_SUR_PLACE_DROITE;
        break;
    case 0b10111:
        nextStateRobot = STATE_TOURNE_SUR_PLACE_DROITE;
        break;
    case 0b11000:
        nextStateRobot  = STATE_TOURNE_DROITE;
        break;
    case 0b11001:
        nextStateRobot = STATE_TOURNE_DROITE; // ATTENTION
        break;
    case 0b11010:
        nextStateRobot = STATE_TOURNE_SUR_PLACE_DROITE;
        break;  
    case 0b11011:
        nextStateRobot = STATE_TOURNE_DROITE;
        break;
    case 0b11100:
        nextStateRobot  = STATE_TOURNE_DROITE;
        break;
    case 0b11101:
        nextStateRobot = STATE_TOURNE_SUR_PLACE_DROITE;
        break;
    case 0b11110:
        nextStateRobot  = STATE_TOURNE_SUR_PLACE_DROITE;
        break;
    case 0b11111:
        nextStateRobot  = STATE_TOURNE_SUR_PLACE_DROITE;
        break;
    default:
        break;
}

    if (nextStateRobot != stateRobot - 1)
        stateRobot = nextStateRobot;
}

int main(void) {
    /***********************************************************************************************Initialisation oscillateur*/
    /***********************************************************************************************/
    InitOscillator();
    /*********************************************************************************************** Configuration des input et output (IO)*/
    /***********************************************************************************************/
    InitIO();
    InitTimer23();
    InitTimer1();
    InitTimer4();
    InitPWM();
    InitADC1();
    /*********************************************************************************************** Boucle Principale*/
    /***********************************************************************************************/
    //    int vitesse = 20;



    while (1) {


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
