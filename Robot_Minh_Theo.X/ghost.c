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

const float AccTheta = 60.0f;
const float VThetaMax = 17.0f;
const float Tsampling = 0.01f;

float vLin = 0.0;
const float accLin = 20.0;
const float vLinMax = 8.0;
double positionWaypoint;
double distanceRestante= 0;


float vTheta=0;
unsigned char payload_Ghost[6] = {};
unsigned char payload_GhostPos[8] = {};
double cercle = 360;





void UpdateRotation()
{
    double thetaRestant = ModuloByAngle(robotState.thetaWaypoint- robotState.thetaGhost);
    thetaRestant = NormalizeAngle(thetaRestant); 

    double thetaArret = (vTheta * vTheta) / (2 * AccTheta);
    if (vTheta < 0)
        thetaArret = -thetaArret;

    double incrementTheta = vTheta * Tsampling;

    if (((thetaArret >= 0 && thetaRestant >= 0) || (thetaArret <= 0 && thetaRestant <= 0)) &&
        Abs(thetaRestant) >= Abs(thetaArret))
    {
        if (thetaRestant > 0)
            vTheta = Min(vTheta + (AccTheta * Tsampling), VThetaMax);
        else if (thetaRestant < 0)
            vTheta = Max(vTheta - (AccTheta * Tsampling), -VThetaMax);
    }
    else 
    {
        if (vTheta > 0)
            vTheta = Max(vTheta - (AccTheta * Tsampling), 0);
        else if (vTheta < 0)
            vTheta = Min(vTheta + (AccTheta * Tsampling), 0);
    }

    if (Abs(thetaRestant) < Abs(incrementTheta))
        incrementTheta = thetaRestant;

     robotState.thetaGhost  += incrementTheta;
     robotState.thetaGhost  = NormalizeAngle( robotState.thetaGhost );

    if (vTheta == 0 && Abs(thetaRestant) < 0.01)
         robotState.thetaGhost  = robotState.thetaWaypoint;
     
  
    
}


void UpdateDeplacementGhost()
{
    double dx = robotState.positionWaypoint.x - robotState.positionGhost.x;
    double dy = robotState.positionWaypoint.y - robotState.positionGhost.y;
    double dirX =0;
    double dirY =0;
    
    
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
        fabs(distanceRestante) >= fabs(distanceArret))
    {
        vLin = Min(vLin + accLin * Tsampling, vLinMax);
    }
    else
    {
        vLin = Max(vLin - accLin * Tsampling, 0);
    }

    if (fabs(distanceRestante) < fabs(incrementD))
    {
        incrementD = distanceRestante;
    }

    robotState.positionGhost.x += (float)(dirX * incrementD);
    robotState.positionGhost.y += (float)(dirY * incrementD);

    if (vLin == 0 && distanceRestante < 0.01)
    {
        robotState.positionGhost = robotState.positionWaypoint;
    }

}




double AngleVersCible(Point robot, Point target)
{
    double dx = target.x - robot.x;
    double dy = -(target.y - robot.y); 

    double angleDeg = atan2f(dy, dx) * 180.0 / PI;
    return NormalizeAngle(angleDeg+90);
}

 double NormalizeAngle(double angle)
  {
      angle = ModuloByAngle(angle);
      if (angle > 180)
          angle -= 360;
      return angle;
  }

double ModuloByAngle(double angle)
{
    return fmod(angle + cercle, cercle);
}

int GhostState = 0;

extern Point posRobot;
extern Point posTarget;
double angle = 0;


void UpdateGhost()
{
    switch (GhostState)
    {
        case Idle:
            if (GhostFlag)
            { 
                GhostState = Rotation;
            }
            break;
        case Rotation:
            angle = AngleVersCible(robotState.positionGhost, robotState.positionWaypoint);
            robotState.thetaWaypoint = angle  ;   
            UpdateRotation();

       
            if (  robotState.thetaGhost == angle)
            {
                GhostState = DeplacementLineaire;
                
            }
            break;
        case DeplacementLineaire:
            //double distance = DistancePointToSegment();
        UpdateDeplacementGhost();
        if (distanceRestante < 1e-6)
        {
            vLin = 0;   
            GhostFlag = 0;
            GhostState = Idle;
        }


            break;
    }
}



void sendInfoGhost(){
    getBytesFromFloat(payload_GhostPos, 0, (float)robotState.positionGhost.x);
    getBytesFromFloat(payload_GhostPos, 4, (float)robotState.positionGhost.y);
    UartEncodeAndSendMessage(Ghost_position, 8, payload_GhostPos);
    
    
    
    getBytesFromFloat(payload_Ghost, 0, (float) robotState.thetaGhost);
    UartEncodeAndSendMessage(Ghost_angle, 6, payload_Ghost);
    
}


