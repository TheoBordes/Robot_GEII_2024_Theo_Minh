 #include "ghost.h"
#include "robot.h"
#include "math.h"
#include  "asservissement.h"
#include "UART_Protocol.h"

#include "Utilities.h"
#include "Toolbox.h"
const float AccTheta = 48.0f;
const float VThetaMax = 17.0f;
const float Tsampling = 0.1f;
float vAng;
unsigned char payload_Ghost[6] = {};

void UpdateRotation() {
    double thetaRestant = ModuloByAngle(robotState.thetaGhost, robotState.thetaWaypoint) - robotState.thetaGhost;
    thetaRestant = NormalizeAngle(thetaRestant); 

    double thetaArret = (vAng * vAng) / (2 * AccTheta);
    if (vAng < 0)
        thetaArret = -thetaArret;

    double incrementTheta = vAng * Tsampling;

    if (((thetaArret >= 0 && thetaRestant >= 0) || (thetaArret <= 0 && thetaRestant <= 0)) &&
            Abs(thetaRestant) >= Abs(thetaArret)) {
        if (thetaRestant > 0)
            vAng = Min(vAng + (AccTheta * Tsampling), VThetaMax);
        else if (thetaRestant < 0)
            vAng = Max(vAng - (AccTheta * Tsampling), -VThetaMax);
    } else {
        if (vAng > 0)
            vAng = Max(vAng - (AccTheta * Tsampling), 0);
        else if (vAng < 0)
            vAng = Min(vAng + (AccTheta * Tsampling), 0);
    }

    if (Abs(thetaRestant) < Abs(incrementTheta))
        incrementTheta = thetaRestant;

    robotState.thetaGhost += incrementTheta;
    robotState.thetaGhost = NormalizeAngle(robotState.thetaGhost);

    if (vAng == 0 && Abs(thetaRestant) < 0.01)
        robotState.thetaGhost = robotState.thetaWaypoint;


    getBytesFromFloat(payload_Ghost, 0, (float) robotState.thetaGhost);
    UartEncodeAndSendMessage(ghost, 6, payload_Ghost);
}

double AngleVersCible(Point robot, Point target) {
    double dx = target.x - robot.x;
    double dy = target.y - robot.y;
    return atan2f(dy, dx) * 180.0 / 3.14;
}

float NormalizeAngle(float angle) {
    angle = fmodf(angle, 360.0f);
    if (angle > 180) angle -= 360;
    if (angle < -180) angle += 360;
    return angle;
}


float ModuloByAngle(float from, float to) {
    float delta = NormalizeAngle(from - to);
    return from + delta;
}
