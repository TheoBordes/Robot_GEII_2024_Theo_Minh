/* 
 * File:   asservissement.h
 * Author: E306_PC1
 *
 * Created on 3 septembre 2025, 10:34
 */

#ifndef ASSERVISSEMENT_H
#define	ASSERVISSEMENT_H


typedef struct _PidCorrector
{
    double Kp;
    double Ki;
    double Kd;
    double erreurProportionelleMax;
    double erreurIntegraleMax;
    double erreurDeriveeMax;
    double erreurIntegrale;
    double epsilon_1;
    double erreur;
    //For Debug only
    double corrP;
    double corrI;
    double corrD;
}PidCorrector;

#define DISTROUES 0.2175

void UpdateConsGhost();
void UpdateArucoFollow();

void SetupPidAsservissement(volatile PidCorrector* PidCorr, double Kp, double Ki, double Kd, double proportionelleMax, double integralMax , double deriveeMax);
double Correcteur(volatile PidCorrector* PidCorr, double erreur);
void UpdateAsservissement();
void SendPidInfo();

#endif	/* ASSERVISSEMENT_H */

