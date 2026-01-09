#include "aruco_ghost.h"
#include "robot.h"

void ArUco_Init(void) {

}

void ArUco_SetFollowParams(ArUcoFollowMode mode, uint16_t targetId, float targetDistance) {


}

void ArUco_SetGains(float gainAngle, float gainDistance, float maxLinear, float maxAngular) {

}

void ArUco_ProcessMessage(void) {

    if ( (robotState.Aruco_ID == Aruco_Follow_ID)) {


        robotState.positionGhost.x = (robotState.Z_Aruco / 1000.0) / 1.88;
        robotState.positionGhost.y = robotState.X_Aruco / 1000.0;

        int inHysteresis = 0;

        double dx = robotState.X_Aruco;
        double dy = robotState.Y_Aruco;
        double dz = robotState.Z_Aruco;

        double distance = sqrt(dx * dx + dy * dy + (dz / 1.88) * (dz / 1.88)) / 1000.0;

        const double distanceConsigne = 0.30;
        const double hysteresis = 0.1;


        double vitesseConsigne = 0.0;

        if (inHysteresis) {
            if (distance < (distanceConsigne - hysteresis) || distance > (distanceConsigne + hysteresis)) {
                inHysteresis = 0;
            } else {
                vitesseConsigne = 0.0;
            }
        }

        if (!inHysteresis) {
            if (distance > (distanceConsigne - hysteresis) && distance < (distanceConsigne + hysteresis)) {
                inHysteresis = 1;
                vitesseConsigne = 0.0;
            } else {
                double erreurDist = distance - distanceConsigne;
                vitesseConsigne = 1.5 * erreurDist;
            }
        }

        if (vitesseConsigne > 0.4) vitesseConsigne = 0.4;
        if (vitesseConsigne < 0) vitesseConsigne = 0;



        /////////////
        int inHysteresisAngle = 0;
        double angle = -atan2(robotState.X_Aruco, robotState.Z_Aruco);
        const double angleConsigne = 0;
        const double angleHysteresis = 0.2;


        double vitesseAngleConsigne = 0.0;

        if (inHysteresisAngle) {
            if (angle < (angleConsigne - angleHysteresis) || angle > (angleConsigne + angleHysteresis)) {
                inHysteresisAngle = 0;
            } else {
                vitesseAngleConsigne = 0.0;
            }
        }

        if (!inHysteresisAngle) {
            if (angle > (angleConsigne - angleHysteresis) && angle < (angleConsigne + angleHysteresis)) {
                inHysteresisAngle = 1;
                vitesseAngleConsigne = 0.0;
            } else {
                double erreurAngle = angle - angleConsigne;
                vitesseAngleConsigne = 30 * erreurAngle;
            }
        }

        if (vitesseAngleConsigne > 2) vitesseAngleConsigne =2;
        if (vitesseAngleConsigne <  -2) vitesseAngleConsigne = -2;

        robotState.ArucoSpeedLin = vitesseConsigne;
        robotState.ArucoSpeedAngle = vitesseAngleConsigne;
        
    }

}


