#include "ghost.h"
#include "robot.h"
#include "math.h"
#include  "asservissement.h"
#include "UART_Protocol.h"

#include "Utilities.h"
#include "Toolbox.h"
//const float AccTheta = 60.0f;
//const float VThetaMax = 17.0f;
//const float Tsampling = 0.1f;
extern int GhostFlag;

const float AccTheta = 1.8;
const float VThetaMax = 2.0;
const float Tsampling = 0.01f;

float vLin = 0.0;
const float accLin = 0.5;
const float vLinMax = 1;
double positionWaypoint;
double distanceRestante = 0;


float vTheta = 0;
unsigned char payload_Ghost[6] = {};
unsigned char payload_GhostPos[8] = {};
double cercle = 360;

void UpdateRotation() {
    double thetaRestant = NormalizeAngle(robotState.thetaWaypoint - robotState.thetaGhost);

    double thetaArret = (vTheta * vTheta) / (2.0 * AccTheta);
    if (vTheta < 0)
        thetaArret = -thetaArret;

    double incrementTheta = vTheta * Tsampling;

    if (((thetaArret >= 0 && thetaRestant >= 0) || (thetaArret <= 0 && thetaRestant <= 0)) &&
            fabs(thetaRestant) >= fabs(thetaArret)) {
        if (thetaRestant > 0)
            vTheta = fmin(vTheta + (AccTheta * Tsampling), VThetaMax);
        else
            vTheta = fmax(vTheta - (AccTheta * Tsampling), -VThetaMax);
    } else {
        if (vTheta > 0)
            vTheta = fmax(vTheta - (AccTheta * Tsampling), 0);
        else if (vTheta < 0)
            vTheta = fmin(vTheta + (AccTheta * Tsampling), 0);
    }

    if (fabs(thetaRestant) < fabs(incrementTheta))
        incrementTheta = thetaRestant;

    robotState.thetaGhost += incrementTheta;
    robotState.thetaGhost = NormalizeAngle(robotState.thetaGhost);

    if (fabs(thetaRestant) < 0.01 && fabs(vTheta) < 1e-3) {
        robotState.thetaGhost = robotState.thetaWaypoint;
        vTheta = 0;
    }
}

void UpdateDeplacementGhost() {
    double dx = robotState.positionWaypoint.x - robotState.positionGhost.x;
    double dy = robotState.positionWaypoint.y - robotState.positionGhost.y;
    double dirX = 0;
    double dirY = 0;


    double incrementD = vLin * Tsampling;



    double distanceArret = (vLin * vLin) / (2.0 * accLin);
    if (vLin < 0)
        distanceArret = -distanceArret;

    distanceRestante = sqrt(dx * dx + dy * dy);
    if (distanceRestante < 1e-6) {
        dirX = 0;
        dirY = 0;
        incrementD = 0;
    } else {
        dirX = dx / distanceRestante;
        dirY = dy / distanceRestante;
    }


    if (((distanceArret >= 0 && distanceRestante >= 0) || (distanceArret <= 0 && distanceRestante <= 0)) &&
            fabs(distanceRestante) >= fabs(distanceArret)) {
        vLin = Min(vLin + accLin * Tsampling, vLinMax);
    } else {
        vLin = Max(vLin - accLin * Tsampling, 0);
    }

    if (fabs(distanceRestante) < fabs(incrementD)) {
        incrementD = distanceRestante;
    }

    robotState.positionGhost.x += (float) (dirX * incrementD);
    robotState.positionGhost.y += (float) (dirY * incrementD);

    if (vLin == 0 && distanceRestante < 0.01) {
        robotState.positionGhost = robotState.positionWaypoint;
    }


}

double AngleVersCible(Point robot, Point target) {
    double dx = target.x - robot.x;
    double dy = target.y - robot.y;

    double angleRad = atan2(dy, dx);
    return NormalizeAngle(angleRad);
}

double NormalizeAngle(double angle) {
    while (angle > M_PI) angle -= 2.0 * M_PI;
    while (angle < -M_PI) angle += 2.0 * M_PI;
    return angle;
}

void SetGhostTarget(Point cible) {
    robotState.positionWaypoint.x = cible.x;
    robotState.positionWaypoint.y = cible.y;
    GhostFlag = 1;
}




int GhostState = 0;

extern Point posRobot;
extern Point posTarget;
double angle = 0;

void UpdateGhost() {
    switch (GhostState) {
        case Idle:
            if (GhostFlag) {
                GhostState = Rotation;
                angle = AngleVersCible(robotState.positionGhost, robotState.positionWaypoint);
                robotState.thetaWaypoint = angle;
            }
            break;
        case Rotation:
            
            UpdateRotation();

            if (fabs(robotState.thetaGhost - angle) < 0.2 && fabs(robotState.angleRadianFromOdometry - robotState.thetaGhost) < 0.2) {
                GhostState = DeplacementLineaire;
            } // je suis pas sur s'il faut se baser sur la position du robot pour passer au ghost de déplacement linéaire
//            if ( robotState.thetaGhost == robotState.thetaWaypoint){
//                   GhostState = DeplacementLineaire;
//            }
            break;
        case DeplacementLineaire:
            //double distance = DistancePointToSegment();
            UpdateDeplacementGhost();
            if (distanceRestante < 1e-6 && distance(robotState.positionGhost, robotState.positionRobot) < 0.01) {
                vLin = 0;
                GhostFlag = 0;
                GhostState = Idle;
            }


            break;
    }
}

double distance(Point a, Point b) {
    double dx = a.x - b.x;
    double dy = a.y - b.y;
    return sqrt(dx * dx + dy * dy);
}

void sendInfoGhost() {
    getBytesFromFloat(payload_GhostPos, 0, (float) robotState.positionGhost.x);
    getBytesFromFloat(payload_GhostPos, 4, (float) robotState.positionGhost.y);
    UartEncodeAndSendMessage(Ghost_position, 10, payload_GhostPos);



    getBytesFromFloat(payload_Ghost, 0, (float) robotState.thetaGhost);
    UartEncodeAndSendMessage(Ghost_angle, 6, payload_Ghost);

}


