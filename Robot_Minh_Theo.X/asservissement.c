#include "asservissement.h"
#include "Toolbox.h"
#include "QEI.h"
#include "PWM.h"


#define DISTROUES 0.2175
#define pidInfoLinear 0x0071
unsigned char payload_Pid_info[104] = {};

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
    //    robotState.vitesseLinearConsigne = (robotState.vitesseDroiteConsigne - robotState.vitesseGaucheConsigne)/DISTROUES);
    robotState.PidX.erreur = (robotState.vitesseLinearConsigne - robotState.vitesseLineaireFromOdometry)*0;
    robotState.PidTheta.erreur = robotState.vitesseAngulaireConsigne - robotState.vitesseAngulaireFromOdometry;
    robotState.CorrectionVitesseLineaire = Correcteur(&robotState.PidX, robotState.PidX.erreur);
    robotState.CorrectionVitesseAngulaire = Correcteur(&robotState.PidTheta, robotState.PidTheta.erreur);
    SendPidInfo();
    PWMSetSpeedConsignePolaire(robotState.CorrectionVitesseLineaire, robotState.CorrectionVitesseAngulaire);
    //PWMSetSpeedConsignePolaire(0.5, 0.0);
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
