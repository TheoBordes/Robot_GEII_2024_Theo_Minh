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

#define SPEED 10
#define DIST_MAX 100.0
#define DIST_MUR 20.0
#define DIST_M 10
float cap_M;
float cap_G;
float cap_D;
void OperatingSystemLoop2(){
    if (timestamp% 1000 == 0){
        
    }

}

unsigned char stateRobot;

void OperatingSystemLoop(void) {
    switch (stateRobot) {
        case STATE_ATTENTE:
            timestamp = 0;
            PWMSetSpeedConsigne(0, MOTEUR_DROIT);
            PWMSetSpeedConsigne(0, MOTEUR_GAUCHE);
            stateRobot = STATE_ATTENTE_EN_COURS;
        case STATE_ATTENTE_EN_COURS:
            if (timestamp > 1000)
                stateRobot = STATE_AVANCE;
            break;
        case STATE_AVANCE:
            PWMSetSpeedConsigne(15, MOTEUR_DROIT);
            PWMSetSpeedConsigne(15, MOTEUR_GAUCHE);
            stateRobot = STATE_AVANCE_EN_COURS;
            break;
        case STATE_AVANCE_EN_COURS:
            SetNextRobotStateInAutomaticMode();
            break;
        case STATE_TOURNE_GAUCHE:
            PWMSetSpeedConsigne(0, MOTEUR_DROIT);
            PWMSetSpeedConsigne(15, MOTEUR_GAUCHE);
            stateRobot = STATE_TOURNE_GAUCHE_EN_COURS;
            break;
        case STATE_TOURNE_GAUCHE_EN_COURS:
            SetNextRobotStateInAutomaticMode();
            break;
            
        case STATE_TOURNE_GAUCHE_PLUS:
            PWMSetSpeedConsigne(0, MOTEUR_DROIT);
            PWMSetSpeedConsigne(12, MOTEUR_GAUCHE);
            stateRobot = STATE_TOURNE_GAUCHE_PLUS_EN_COURS;
            break;
        case STATE_TOURNE_GAUCHE_PLUS_EN_COURS:
            SetNextRobotStateInAutomaticMode();
            break;
            
        case STATE_TOURNE_DROITE:
            PWMSetSpeedConsigne(15, MOTEUR_DROIT);
            PWMSetSpeedConsigne(0, MOTEUR_GAUCHE);
            stateRobot = STATE_TOURNE_DROITE_EN_COURS;
            break;
        case STATE_TOURNE_DROITE_EN_COURS:
            SetNextRobotStateInAutomaticMode();
            break;
            
         case STATE_TOURNE_DROITE_PLUS:
            PWMSetSpeedConsigne(12, MOTEUR_DROIT);
            PWMSetSpeedConsigne(0, MOTEUR_GAUCHE);
            stateRobot = STATE_TOURNE_DROITE_PLUS_EN_COURS;
            break;
        case STATE_TOURNE_DROITE_PLUS_EN_COURS:
            SetNextRobotStateInAutomaticMode();
            break;
            
        case STATE_TOURNE_SUR_PLACE_GAUCHE:
            PWMSetSpeedConsigne(15, MOTEUR_DROIT);
            PWMSetSpeedConsigne(-15, MOTEUR_GAUCHE);
            stateRobot = STATE_TOURNE_SUR_PLACE_GAUCHE_EN_COURS;
            break;
        case STATE_TOURNE_SUR_PLACE_GAUCHE_EN_COURS:
            SetNextRobotStateInAutomaticMode();
            break;
        case STATE_TOURNE_SUR_PLACE_DROITE:
            PWMSetSpeedConsigne(-15, MOTEUR_DROIT);
            PWMSetSpeedConsigne(15, MOTEUR_GAUCHE);
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


unsigned char nextStateRobot = 0;

void SetNextRobotStateInAutomaticMode() {
    unsigned char positionObstacle = PAS_D_OBSTACLE;
    //ÈDtermination de la position des obstacles en fonction des ÈÈËtlmtres
    if (robotState.distanceTelemetreDroit < 30 &&
            robotState.distanceTelemetreCentre > 20 &&
            robotState.distanceTelemetreGauche > 30) //Obstacle ‡droite
        positionObstacle = OBSTACLE_A_DROITE;
    
    
    if (robotState.distanceTelemetreDroit < 30 &&
            robotState.distanceTelemetreCentre > 20 &&
            robotState.distanceTelemetreGauche > 30) //Obstacle ‡droite
        positionObstacle = OBSTACLE_A_DROITE;
    
    
    else if (robotState.distanceTelemetreDroit > 30 &&
            robotState.distanceTelemetreCentre > 20 &&
            robotState.distanceTelemetreGauche < 30 
            ) //Obstacle ‡gauche
        positionObstacle = OBSTACLE_A_GAUCHE;
    
    
    else if (robotState.distanceTelemetreCentre < 30) //Obstacle en face
        positionObstacle = OBSTACLE_EN_FACE;
    
    else if (robotState.distanceTelemetreDroit > 30 &&
            robotState.distanceTelemetreCentre > 20 &&
            robotState.distanceTelemetreGauche > 30
            ) //pas d?obstacle
        positionObstacle = PAS_D_OBSTACLE;
    
    else if (robotState.distanceTelemetreDroit < 30 &&
            robotState.distanceTelemetrePlusDroit < 30)
        positionObstacle = OBSTACLE_A_DROITE_PLUS;
    
    else if (robotState.distanceTelemetreGauche < 30 &&
            robotState.distanceTelemetrePlusGauche < 30)
        positionObstacle = OBSTACLE_A_GAUCHE_PLUS;
 
    
    
    //ÈDtermination de lÈ?tat ‡venir du robot
    if (positionObstacle == PAS_D_OBSTACLE)
        nextStateRobot = STATE_AVANCE;
    else if (positionObstacle == OBSTACLE_A_DROITE)
        nextStateRobot = STATE_TOURNE_GAUCHE;
    else if (positionObstacle == OBSTACLE_A_GAUCHE)
        nextStateRobot = STATE_TOURNE_DROITE;
    else if (positionObstacle == OBSTACLE_EN_FACE)
        nextStateRobot = STATE_TOURNE_SUR_PLACE_GAUCHE;
    else if (positionObstacle == OBSTACLE_A_DROITE_PLUS )
        nextStateRobot = STATE_TOURNE_DROITE_PLUS;
    else if (positionObstacle == OBSTACLE_A_GAUCHE_PLUS )
        nextStateRobot = STATE_TOURNE_GAUCHE_PLUS;
    //Si l?on n?est pas dans la transition de lÈ?tape en cours
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
        if (ADCIsConversionFinished()) {
            ADCClearConversionFinishedFlag();
            unsigned int * result = ADCGetResult();
            float volts = ((float) result [0])* 3.3 / 4096;
            if (volts<0.325)volts = 0.325; // permet de bloquer une dÈtection ‡ 100 centimËtre
            robotState.distanceTelemetrePlusGauche = 34 / volts - 5;
            volts = ((float) result [1])* 3.3 / 4096;
            if (volts<0.325)volts = 0.325;
            robotState.distanceTelemetreGauche = 34 / volts - 5;
            volts = ((float) result [2])* 3.3 / 4096;
            if (volts<0.325)volts = 0.325;
            robotState.distanceTelemetreCentre = 34 / volts - 5; 
            volts = ((float) result [3])* 3.3 / 4096;
            if (volts<0.325)volts = 0.325;
            robotState.distanceTelemetreDroit = 34 / volts - 5;
            volts = ((float) result [4])* 3.3 / 4096;
            if (volts<0.325)volts = 0.325;
            robotState.distanceTelemetrePlusDroit = 34 / volts - 5;
        }
           
           
           
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
