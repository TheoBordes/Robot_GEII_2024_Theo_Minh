#include <stdio.h>
#include <stdlib.h>
#include <xc.h>
#include "ChipConfig.h"
#include "IO.h"
#include "timer.h"
#include "PWM.h" 
#include "ADC.h"
#include "robot.h"

int main(void) {

    /***********************************************************************************************Initialisation oscillateur*/
    /***********************************************************************************************/
    InitOscillator();
    /*********************************************************************************************** Configuration des input et output (IO)*/
    /***********************************************************************************************/
    InitIO();
    InitTimer23();
    InitTimer1();
    InitPWM();
    InitADC1();
    //PWMSetSpeed(100,0);
    //PWMSetSpeed(20,1);
    //    LED_BLANCHE_1 = 1;
    //    LED_BLEUE_1 = 1;
    //    LED_ORANGE_1 = 1;
    //    LED_ROUGE_1 = 1;
    //    LED_VERTE_1 = 1;
    //
    //    LED_BLANCHE_2 = 1;
    //    LED_BLEUE_2 = 1;
    //    LED_ROUGE_2 = 1;
    //    LED_VERTE_2 = 1;
    /*********************************************************************************************** Boucle Principale*/
    /***********************************************************************************************/
    int vitesse = 20;
     
    while (1) {
        if (ADCIsConversionFinished()) {
            ADCClearConversionFinishedFlag();
            unsigned int * result = ADCGetResult();
            float volts = ((float) result [0])* 3.3 / 4096;
            robotState.distanceTelemetrePlusGauche = 34 / volts - 5;
            volts = ((float) result [1])* 3.3 / 4096;
            robotState.distanceTelemetreGauche = 34 / volts - 5;
            volts = ((float) result [2])* 3.3 / 4096;
            robotState.distanceTelemetreCentre = 34 / volts - 5;
            volts = ((float) result [3])* 3.3 / 4096;
            robotState.distanceTelemetrePlusDroit = 34 / volts - 5;
            volts = ((float) result [4])* 3.3 / 4096;
            robotState.distanceTelemetreDroit = 34 / volts - 5;
        }
        if ( robotState.distanceTelemetreGauche > 20){
            LED_BLEUE_1 = 1;
        }   
        else {
            LED_BLEUE_1 = 0;
   
        }
        if ( robotState.distanceTelemetrePlusGauche > 20){
            LED_BLANCHE_1 = 1;
 
        }
        else {
            LED_BLANCHE_1 = 0;

        }
        if ( robotState.distanceTelemetreCentre > 20){
            LED_ORANGE_1 = 1;
  
        }
        else {
            LED_ORANGE_1 = 0;

        }
        if ( robotState.distanceTelemetreDroit > 20){
            LED_ROUGE_1 = 1;
 
        }
        else {
            LED_ROUGE_1 = 0;
  
        }
        if ( robotState.distanceTelemetrePlusDroit > 20){
            LED_VERTE_1 = 1;
        }
        else {
            LED_VERTE_1 = 0;
        }
        if ( LED_VERTE_1 && LED_ROUGE_1 && LED_ORANGE_1 && LED_BLANCHE_1 && LED_BLEUE_1 ){
            vitesse = 10;
        }
        else {
            vitesse = 1;
        }
        PWMSetSpeedConsigne(-vitesse, 1);
        PWMSetSpeedConsigne(-vitesse, 0);
        
    }

}

