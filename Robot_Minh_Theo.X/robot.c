#include "robot.h"
#include "asservissement.h" 


volatile ROBOT_STATE_BITS robotState;

void SetRobotAutoControlState( int mode){
    robotState.mode = mode;
}

_PidCorrector PidX;
_PidCorrector PidTheta;

        