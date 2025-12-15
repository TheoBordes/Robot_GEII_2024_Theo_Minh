#include "aruco_ghost.h"
#include "UART_Protocol.h"
#include "ghost.h"
#include "robot.h"
#include <math.h>
#include <string.h>

/* ================= PARAMÈTRES ================= */
#define JEVOIS_IMAGE_WIDTH   640.0f
#define JEVOIS_FOCAL_LENGTH  300.0f
#define JEVOIS_MARKER_SIZE  0.05f

#define ARUCO_TIMEOUT_MS    300

/* ================= ÉTAT ================= */
ArUcoState arucoState;
static uint32_t lastUpdateTimeMs = 0;

/* ================= UTILS ================= */
static float getFloatFromBytes(uint8_t *bytes, int offset)
{
    union { float f; uint8_t b[4]; } u;
    memcpy(u.b, &bytes[offset], 4);
    return u.f;
}

/* ================= INIT ================= */
void ArUco_Init(void)
{
    memset(&arucoState, 0, sizeof(arucoState));
    arucoState.followEnabled = 1;
    arucoState.targetId = 47;
}

/* ================= RÉCEPTION UART ================= */
void ArUco_ProcessMessage(uint16_t function,
                          uint16_t payloadLength,
                          uint8_t *payload)
{

    uint16_t markerId =
        ((uint16_t)payload[0] << 8) | payload[1];

    float centerX = getFloatFromBytes(payload, 2);
    float width   = getFloatFromBytes(payload, 10);
    float height  = getFloatFromBytes(payload, 14);

    float avgSize = (width + height) * 0.5f;
    if (avgSize < 2.0f)
        return;

    ArUcoMarker *marker = &arucoState.markers[0];

    marker->id = markerId;
    marker->valid = 1;
    marker->lastSeenTime = lastUpdateTimeMs;

    /* === Z (distance) === */
    marker->position.z =
        (JEVOIS_MARKER_SIZE * JEVOIS_FOCAL_LENGTH) / avgSize;

    /* === X (latéral) === */
    float dx = centerX - (JEVOIS_IMAGE_WIDTH * 0.5f);
    marker->position.x =
        (dx * marker->position.z) / JEVOIS_FOCAL_LENGTH;

    marker->position.y = 0.0f;

    arucoState.activeMarker = marker;
}

/* ================= CALCUL CIBLE GHOST ================= */
static void calculateTargetPosition(ArUcoMarker *marker)
{
    float robotX = robotState.positionRobot.x;
    float robotY = robotState.positionRobot.y;
    float robotA = robotState.angleRadianFromOdometry;

    float cosA = cosf(robotA);
    float sinA = sinf(robotA);

    /* Position marqueur monde */
    float markerWorldX =
        robotX + marker->position.z * cosA
               - marker->position.x * sinA;

    float markerWorldY =
        robotY + marker->position.z * sinA
               + marker->position.x * cosA;

    arucoState.markerWorldPosition.x = markerWorldX;
    arucoState.markerWorldPosition.y = markerWorldY;

    /* Angle robot ? marqueur */
    float angleToMarker =
        atan2f(markerWorldY - robotY,
               markerWorldX - robotX);

    arucoState.targetPosition.x =
        markerWorldX - ARUCO_FOLLOW_DISTANCE * cosf(angleToMarker);

    arucoState.targetPosition.y =
        markerWorldY - ARUCO_FOLLOW_DISTANCE * sinf(angleToMarker);
}

/* ================= UPDATE ================= */
void ArUco_Update(uint32_t currentTimeMs)
{
    lastUpdateTimeMs = currentTimeMs;

    if (!arucoState.followEnabled)
        return;

    ArUcoMarker *marker = arucoState.activeMarker;

    if (marker == NULL || !marker->valid)
        return;

    if ((currentTimeMs - marker->lastSeenTime) > ARUCO_TIMEOUT_MS) {
        marker->valid = 0;
        arucoState.activeMarker = NULL;
        return;
    }

    calculateTargetPosition(marker);
    Point target;
    target.x = arucoState.targetPosition.x;
    target.y = arucoState.targetPosition.y;
    SetGhostTarget(target);
}
