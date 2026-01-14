#ifndef ROBOT_H
#define ROBOT_H
#include "asservissement.h"
#include "ghost.h"

typedef struct robotStateBITS {

    union {

        struct {
            unsigned char taskEnCours;
            float vitesseGaucheConsigne;
            float vitesseDroiteConsigne;
            float vitesseGaucheMoteurPercent;
            float vitesseDroiteMoteurPercent;
            float vitesseDroiteMoteur;
            float vitesseGaucheMoteur;
            float distanceTelemetrePlusGauche;
            float distanceTelemetreGauche;
            float distanceTelemetreCentre;
            float distanceTelemetreDroit;
            float distanceTelemetrePlusDroit;
            unsigned int mode;

            float vitesseDroitFromOdometry;
            float vitesseGaucheFromOdometry;
            float vitesseLineaireFromOdometry;
            float vitesseAngulaireFromOdometry;
            float xPosFromOdometry_1;
            float xPosFromOdometry;
            float yPosFromOdometry_1;
            float yPosFromOdometry;
            float vitesseBITEAngulaireFromOdometry;
            float angleRadianFromOdometry_1;
            float angleRadianFromOdometry;

            PidCorrector PidX;
            PidCorrector PidTheta;
            PidCorrector PidSpeedGauche;
            PidCorrector PidSpeedDroite;
            PidCorrector PD_Position_Lineaire;
            PidCorrector PD_Position_Angulaire;

            PidCorrector PD_Position_aruco_angle;

            float centerX_Aruco;
            float markerAvgSize;

            int Aruco_Flag;
            int Aruco_ID;
            float X_Aruco;
            float Y_Aruco;
            float Z_Aruco;
            float ArucoSpeedLin;
            float ArucoSpeedAngle;
            

            float CorrectionVitesseLineaire;
            float CorrectionVitesseAngulaire;
            float CorrectionVitesseDroite;
            float CorrectionVitesseGauche;
            float vitesseLinearConsigne;
            float vitesseAngulaireConsigne;

            float vitesseLineaireGhost;
            float vitesseAngulaireGhost;


            float thetaGhost;
            float thetaWaypoint;
            Point positionWaypoint;
            Point positionGhost;
            Point positionRobot;

        };
    };
} ROBOT_STATE_BITS;
extern volatile ROBOT_STATE_BITS robotState;
void SetRobotAutoControlState(int mode);
#endif /* ROBOT_H */
