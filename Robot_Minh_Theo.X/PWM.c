#include <xc.h>
#include "IO.h"
#include "PWM.h"
#include "timer.h"
#include "Toolbox.h"
#include "robot.h"
#include "UART_Protocol.h"   
#include "UART.h"
#define PWMPER 24.0

float acceleration = 3;
float talon = 20;

void InitPWM(void) {
    PTCON2bits.PCLKDIV = 0b000; //Divide by 1
    PTPER = 100 * PWMPER; //éPriode en pourcentage
    //éRglage PWM moteur 1 sur hacSEXheur 1
    IOCON1bits.PMOD = 0b11; //PWM I/O pin pair is in the True Independent Output mode
    IOCON1bits.PENL = 1;
    IOCON1bits.PENH = 1;
    FCLCON1 = 0x0003; //éDsactive la gestion des faults
    IOCON2bits.PMOD = 0b11; //PWM I/O pin pair is in the True Independent Output mode
    IOCON2bits.PENL = 1;
    IOCON2bits.PENH = 1;
    FCLCON2 = 0x0003; //éDsactive la gestion des faults
    /* Enable PWM Module */
    PTCONbits.PTEN = 1;
}

//void PWMSetSpeed(float vitesseEnPourcents, int moteur) { premier test des moteurs
//    if (Abs(vitesseEnPourcents) > 100) {
//        return;
//    }
//    switch (moteur) {
//        case MOTEUR_GAUCHE:
//            if (vitesseEnPourcents < 0) {
//                SDC1 = Abs(vitesseEnPourcents) * PWMPER + talon;
//                PDC1 = talon;
//            } else {
//                PDSEXC1 = vitesseEnPourcents * PWMPER + talon;
//                SDC1 = talon;
//            }
//            LED_ROUGE_2 = 1;
//            break;
//        case MOTEUR_DROIT:
//            if (vitesseEnPourcents < 0) {
//                PDC2 = Abs(vitesseEnPourcents) * PWMPER + talon;
//                SDC2 = talon;
//            } else {
//                SDC2 = vitesseEnPourcents * PWMPER + talon;
//                PDC2 = talon;
//            }
//            LED_ROUGE_1 = 1;
//            break;
//    }
//}

void PWMUpdateSpeed() {
    // Cette fonction est appelee sur timer et permet de suivre des rampes d acceleration
    if (robotState.vitesseDroiteCommandeCourante < robotState.vitesseDroiteConsigne)
        robotState.vitesseDroiteCommandeCourante = Min(robotState.vitesseDroiteCommandeCourante + acceleration, robotState.vitesseDroiteConsigne);

    if (robotState.vitesseDroiteCommandeCourante > robotState.vitesseDroiteConsigne)
        robotState.vitesseDroiteCommandeCourante = Max(robotState.vitesseDroiteCommandeCourante - acceleration, robotState.vitesseDroiteConsigne);

    if (robotState.vitesseDroiteCommandeCourante >= 0) {
        SDC1 = robotState.vitesseDroiteCommandeCourante * PWMPER + talon;
        PDC1 = talon;
    } else {
        SDC1 = talon;
        PDC1 = -robotState.vitesseDroiteCommandeCourante * PWMPER + talon;
    }

    if (robotState.vitesseGaucheCommandeCourante < robotState.vitesseGaucheConsigne)
        robotState.vitesseGaucheCommandeCourante = Min(robotState.vitesseGaucheCommandeCourante + acceleration, robotState.vitesseGaucheConsigne);

    if (robotState.vitesseGaucheCommandeCourante > robotState.vitesseGaucheConsigne)
        robotState.vitesseGaucheCommandeCourante = Max(robotState.vitesseGaucheCommandeCourante - acceleration, robotState.vitesseGaucheConsigne);

    if (robotState.vitesseGaucheCommandeCourante > 0) {
        SDC2 = robotState.vitesseGaucheCommandeCourante * PWMPER + talon;
        PDC2 = talon;
    } else {
        SDC2 = talon;
        PDC2 = -robotState.vitesseGaucheCommandeCourante * PWMPER + talon;
    }

}

void PWMSetSpeedConsigne(float vitesseEnPourcents, char moteur) {
    unsigned char payload_state[5] = {};
    payload_state[0] = robotState.taskEnCours;
    payload_state[1] = (unsigned char) (timestamp & 0xFF);
    payload_state[2] = (unsigned char) ((timestamp >> 8) & 0xFF);
    payload_state[3] = (unsigned char) ((timestamp >> 16) & 0xFF);
    payload_state[4] = (unsigned char) ((timestamp >> 24) & 0xFF);
//    UartEncodeAndSendMessage(0x0050, 5, payload_state);

    if (Abs(vitesseEnPourcents) > 100) {
        return;
    }
    switch (moteur) {
        case MOTEUR_GAUCHE:
            robotState.vitesseGaucheConsigne = vitesseEnPourcents;
            break;
        case MOTEUR_DROIT:
            robotState.vitesseDroiteConsigne = -vitesseEnPourcents;
            break;
    }
}