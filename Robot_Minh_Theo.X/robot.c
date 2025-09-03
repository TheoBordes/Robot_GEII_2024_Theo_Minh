#include "robot.h"


volatile ROBOT_STATE_BITS robotState;




void SetRobotAutoControlState( int mode){
    robotState.mode = mode;
}


        