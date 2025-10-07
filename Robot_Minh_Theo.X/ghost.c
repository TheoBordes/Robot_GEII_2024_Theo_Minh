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

const float AccTheta = 180.0f;
const float VThetaMax = 60.0f;
const float Tsampling = 0.05f;

float vLin = 0.0;
const float accLin = 50.0;
const float vLinMax = 100.0;
double positionWaypoint;
double distanceRestante;


float vTheta=0;
unsigned char payload_Ghost[6] = {};
double cercle =360;





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
     
    getBytesFromFloat(payload_Ghost, 0, (float) robotState.thetaGhost);
    UartEncodeAndSendMessage(Ghost_angle, 6, payload_Ghost);
    
}


void UpdateDeplacementGhost()
        {
        UartEncodeAndSendMessage(Ghost_angle, 6, payload_Ghost);
        
            double dx = robotState.positionWaypoint.x - robotState.positionGhost.x;
            double dy = robotState.positionWaypoint.y - robotState.positionGhost.y;

            distanceRestante = sqrt(dx * dx + dy * dy);


            double distanceArret = (vLin * vLin) / (2 * accLin);
            if (vLin < 0)
                distanceArret = -distanceArret;

            double incrementD = vLin * Tsampling;

            double dirX = dx / distanceRestante;
            double dirY = dy / distanceRestante;

            if (((distanceArret >= 0 && distanceRestante >= 0) || (distanceArret <= 0 && distanceRestante <= 0)) &&
                Abs(distanceRestante) >= Abs(distanceArret))
            {
                vLin = Min(vLin + accLin * Tsampling, vLinMax);
            }
            else
            {
                vLin = Max(vLin - accLin * Tsampling, 0);
            }

            if (Abs(distanceRestante) < Abs(incrementD))
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
