#include "robot.h"

#include <math.h>

#include "asservissement.h"
#include "Toolbox.h"
#include "ghost.h"
#include "QEI.h"
#include "PWM.h"
#include "Utilities.h"
#include "UART_Protocol.h"
#include "IO.h"
#include <xc.h>
#include "main.h"
#include "aruco_ghost.h"
#include "timer.h"

#define DISTROUES 0.2175
#define pidInfoLinear 0x0071
unsigned char payload_Pid_info[104] = {};
extern unsigned long timestamp;
extern unsigned long aruco_time;

void SetupPidAsservissement(volatile PidCorrector* PidCorr, double Kp, double Ki, double Kd, double proportionelleMax, double integralMax, double deriveeMax) {
    PidCorr->Kp = Kp;
    PidCorr->erreurProportionelleMax = proportionelleMax; //On limite la correction due au Kp
    PidCorr->Ki = Ki;
    PidCorr->erreurIntegraleMax = integralMax; //On limite la correction due au Ki
    PidCorr->Kd = Kd;
    PidCorr->erreurDeriveeMax = deriveeMax;
}

double Correcteur(volatile PidCorrector* PidCorr, double erreur) {
    PidCorr->erreur = erreur;

    double erreurProportionnelle = LimitToInterval(erreur, -PidCorr->erreurProportionelleMax / PidCorr->Kp, PidCorr->erreurProportionelleMax / PidCorr->Kp);
    PidCorr->corrP = erreurProportionnelle * PidCorr->Kp;

    PidCorr->erreurIntegrale += erreur / FREQ_ECH_QEI;
    PidCorr->erreurIntegrale = LimitToInterval(PidCorr->erreurIntegrale + erreur / FREQ_ECH_QEI, -PidCorr->erreurIntegraleMax / PidCorr->Ki, PidCorr->erreurIntegraleMax / PidCorr->Ki);
    PidCorr->corrI = PidCorr->erreurIntegrale * PidCorr->Ki;

    double erreurDerivee = (erreur - PidCorr->epsilon_1) * FREQ_ECH_QEI;
    double deriveeBornee = LimitToInterval(erreurDerivee, -PidCorr->erreurDeriveeMax / PidCorr->Kd, PidCorr->erreurDeriveeMax / PidCorr->Kd);
    PidCorr->epsilon_1 = erreur;
    PidCorr->corrD = deriveeBornee * PidCorr->Kd;

    return PidCorr->corrP + PidCorr->corrI + PidCorr->corrD;

}

void UpdateAsservissement() {
    LED_BLEUE_1 = !LED_BLEUE_1;

    robotState.PidSpeedGauche.erreur = robotState.vitesseGaucheConsigne - robotState.vitesseGaucheFromOdometry;
    robotState.PidSpeedDroite.erreur = robotState.vitesseDroiteConsigne - robotState.vitesseDroitFromOdometry;
    robotState.CorrectionVitesseDroite = Correcteur(&robotState.PidSpeedDroite, robotState.PidSpeedDroite.erreur);
    robotState.CorrectionVitesseGauche = Correcteur(&robotState.PidSpeedGauche, robotState.PidSpeedGauche.erreur);


    PWMSetSpeedConsigneIndependant(robotState.CorrectionVitesseDroite, MOTEUR_DROIT);
    PWMSetSpeedConsigneIndependant(robotState.CorrectionVitesseGauche, MOTEUR_GAUCHE);

    //    robotState.PidX.erreur = robotState.vitesseLinearConsigne - robotState.vitesseLineaireFromOdometry;
    //    robotState.PidTheta.erreur = robotState.vitesseAngulaireConsigne - robotState.vitesseAngulaireFromOdometry;
    //    robotState.CorrectionVitesseLineaire = Correcteur(&robotState.PidX, robotState.PidX.erreur);
    //    robotState.CorrectionVitesseAngulaire = Correcteur(&robotState.PidTheta, robotState.PidTheta.erreur);
    //
    //    PWMSetSpeedConsignePolaire(robotState.CorrectionVitesseLineaire, robotState.CorrectionVitesseAngulaire);
}

void UpdateConsGhost() {
    double erreurTheta = NormalizeAngle(robotState.thetaGhost - robotState.angleRadianFromOdometry);

    double dx = robotState.positionGhost.x - robotState.xPosFromOdometry;
    double dy = robotState.positionGhost.y - robotState.yPosFromOdometry;

    double distance = sqrt(dx * dx + dy * dy);

    double angleToGhost = atan2(dy, dx);
    double angleRobot = robotState.angleRadianFromOdometry;
    double diffAngle = NormalizeAngle(angleToGhost - angleRobot);

    double erreurLineaire = distance * cos(diffAngle);


    //robotState.vitesseLinearConsigne = Correcteur(&robotState.PD_Position_Lineaire, erreurLineaire);
    robotState.vitesseAngulaireConsigne = Correcteur(&robotState.PD_Position_Angulaire, erreurTheta);

    robotState.vitesseDroiteConsigne = robotState.vitesseLinearConsigne + robotState.vitesseAngulaireConsigne * DISTROUES / 2;
    robotState.vitesseGaucheConsigne = robotState.vitesseLinearConsigne - robotState.vitesseAngulaireConsigne * DISTROUES / 2;




    LED_ROUGE_1 = !LED_ROUGE_1;

    //    
    //    unsigned char testEnvoi[16];
    //    getBytesFromFloat(testEnvoi, 0, (float)(robotState.positionGhost.x));
    //    getBytesFromFloat(testEnvoi, 4, (float)(robotState.positionGhost.y));
    //    getBytesFromFloat(testEnvoi, 8, (float)(angleRobot));
    //    getBytesFromFloat(testEnvoi, 12, (float)(erreurLineaire));
    //    
    //    UartEncodeAndSendMessage(0x00FF, 16, testEnvoi);
}

extern unsigned long  aruco_time;
void UpdateArucoFollow() {

    if ( (timestamp - aruco_time) > Aruco_Time_Loss ){
        robotState.ArucoSpeedLin = 0;
        robotState.ArucoSpeedAngle = 0;
        robotState.vitesseLineaireGhost=  0;
         
    }
                                                                                                                                                                                  
    robotState.vitesseDroiteConsigne =
            robotState.ArucoSpeedLin + robotState.ArucoSpeedAngle * DISTROUES / 2;

    robotState.vitesseGaucheConsigne =
            robotState.ArucoSpeedLin - robotState.ArucoSpeedAngle * DISTROUES / 2;



}

void SendPidInfo() {

    getBytesFromFloat(payload_Pid_info, 0, (float) robotState.vitesseLinearConsigne);
    getBytesFromFloat(payload_Pid_info, 4, (float) robotState.vitesseLineaireFromOdometry);
    getBytesFromFloat(payload_Pid_info, 8, (float) robotState.PidX.erreur);
    getBytesFromFloat(payload_Pid_info, 12, (float) robotState.CorrectionVitesseLineaire);
    getBytesFromFloat(payload_Pid_info, 16, (float) robotState.PidX.Kp);
    getBytesFromFloat(payload_Pid_info, 20, (float) robotState.PidX.corrP);
    getBytesFromFloat(payload_Pid_info, 24, (float) robotState.PidX.erreurProportionelleMax);
    getBytesFromFloat(payload_Pid_info, 28, (float) robotState.PidX.Ki);
    getBytesFromFloat(payload_Pid_info, 32, (float) robotState.PidX.corrI);
    getBytesFromFloat(payload_Pid_info, 36, (float) robotState.PidX.erreurIntegraleMax);
    getBytesFromFloat(payload_Pid_info, 40, (float) robotState.PidX.Kd);
    getBytesFromFloat(payload_Pid_info, 44, (float) robotState.PidX.corrD);
    getBytesFromFloat(payload_Pid_info, 48, (float) robotState.PidX.erreurDeriveeMax);


    getBytesFromFloat(payload_Pid_info, 52, (float) robotState.vitesseAngulaireConsigne);
    getBytesFromFloat(payload_Pid_info, 56, (float) robotState.vitesseAngulaireFromOdometry);
    getBytesFromFloat(payload_Pid_info, 60, (float) robotState.PidTheta.erreur);
    getBytesFromFloat(payload_Pid_info, 64, (float) robotState.CorrectionVitesseAngulaire);
    getBytesFromFloat(payload_Pid_info, 68, (float) robotState.PidTheta.Kp);
    getBytesFromFloat(payload_Pid_info, 72, (float) robotState.PidTheta.corrP);
    getBytesFromFloat(payload_Pid_info, 76, (float) robotState.PidTheta.erreurProportionelleMax);
    getBytesFromFloat(payload_Pid_info, 80, (float) robotState.PidTheta.Ki);
    getBytesFromFloat(payload_Pid_info, 84, (float) robotState.PidTheta.corrI);
    getBytesFromFloat(payload_Pid_info, 88, (float) robotState.PidTheta.erreurIntegraleMax);
    getBytesFromFloat(payload_Pid_info, 92, (float) robotState.PidTheta.Kd);
    getBytesFromFloat(payload_Pid_info, 96, (float) robotState.PidTheta.corrD);
    getBytesFromFloat(payload_Pid_info, 100, (float) robotState.PidTheta.erreurDeriveeMax);
    UartEncodeAndSendMessage(pidInfoLinear, 104, payload_Pid_info);

}
