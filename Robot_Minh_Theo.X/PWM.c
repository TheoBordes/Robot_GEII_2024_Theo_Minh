#include <xc.h>
#include "IO.h"
#include "PWM.h"
#include "Toolbox.h"

#define PWMPER 24.0
void InitPWM(void) {
PTCON2bits.PCLKDIV = 0b000; //Divide by 1
PTPER = 100 * PWMPER; //éPriode en pourcentage
//éRglage PWM moteur 1 sur hacheur 1
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

//void InitPWM2(void) {
//PTCON2bits.PCLKDIV = 0b000; //Divide by 1
//PTPER = 100 * PWMPER; //éPriode en pourcentage
////éRglage PWM moteur 2 sur hacheur 2
//IOCON3bits.PMOD = 0b11; //PWM I/O pin pair is in the True Independent Output mode
//IOCON3bits.PENL = 1;
//IOCON3bits.PENH = 1;
//FCLCON3 = 0x0003; //éDsactive la gestion des faults
//IOCON4bits.PMOD = 0b11; //PWM I/O pin pair is in the True Independent Output mode
//IOCON4bits.PENL = 1;
//IOCON4bits.PENH = 1;
//FCLCON4 = 0x0003; //éDsactive la gestion des faults
///* Enable PWM Module */
//PTCONbits.PTEN = 1;
//}
double talon = 20;
void PWMSetSpeed(float vitesseEnPourcents, int moteur)
{
    if (Abs(vitesseEnPourcents) > 100){
        return;
    }
    switch(moteur){
        case MOTEUR_GAUCHE : 
            if ( vitesseEnPourcents < 0){
                SDC1 = Abs(vitesseEnPourcents) * PWMPER + talon;
                PDC1 = talon;
            }
            else {
                PDC1 = vitesseEnPourcents * PWMPER + talon;
                SDC1 = talon;
            } 
           LED_ROUGE_2 = 1;
           break;
        case MOTEUR_DROIT :
            if ( vitesseEnPourcents < 0){
                PDC2 = Abs(vitesseEnPourcents) * PWMPER + talon;
                SDC2 = talon;
            }
            else {
                SDC2 = vitesseEnPourcents * PWMPER + talon;
                PDC2 = talon;
            } 
            LED_ROUGE_1 = 1;
            break;
    }
}
