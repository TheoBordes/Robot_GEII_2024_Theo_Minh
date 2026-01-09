#ifndef ARUCO_GHOST_H
#define ARUCO_GHOST_H

#include <stdint.h>

#ifndef M_PI
#define M_PI 3.14159265358979323846f
#endif


#define Aruco_Follow_ID 36

#define Aruco_Time_Loss 300

////#define ARUCO_CAMERA_WIDTH       1280.0f   
#define ARUCO_CAMERA_HEIGHT      720.0f   

#define ARUCO_MARKER_REAL_SIZE   0.03f

#define ARUCO_FOLLOW_DISTANCE    0.20f     

#define ARUCO_FILTER_ALPHA       0.3f      

typedef enum {
    ARUCO_MODE_DISABLED = 0,  
    ARUCO_MODE_ANGLE_ONLY,      
    ARUCO_MODE_FULL_FOLLOW,   
    ARUCO_MODE_APPROACH         
} ArUcoFollowMode;




 
void ArUco_Init(void);

void ArUco_SetFollowParams(ArUcoFollowMode mode, uint16_t targetId, float targetDistance);

void ArUco_SetGains(float gainAngle, float gainDistance, float maxLinear, float maxAngular);

void ArUco_ProcessMessage(void);
//float ArUco_GetDistance(void);


#endif