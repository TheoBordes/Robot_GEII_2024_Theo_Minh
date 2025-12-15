#ifndef ARUCO_GHOST_H
#define ARUCO_GHOST_H

#include <stdint.h>

/* ================= CONFIG ================= */
#define ARUCO_MAX_MARKERS      1  // version simple : un seul marqueur

#define ARUCO_FOLLOW_DISTANCE  0.10f

/* ================= STRUCTURES ================= */

typedef struct {
    float x;
    float y;
    float z;
} ArUcoPosition;

typedef struct {
    uint16_t id;
    uint8_t  valid;
    uint32_t lastSeenTime;

    ArUcoPosition position;   // repère caméra (x = latéral, z = distance)

} ArUcoMarker;

typedef struct {
    ArUcoMarker markers[ARUCO_MAX_MARKERS];

    ArUcoMarker *activeMarker;

    int16_t targetId;
    uint8_t followEnabled;

    /* Position absolue (monde) */
    ArUcoPosition markerWorldPosition;

    /* Cible envoyée au Ghost */
    ArUcoPosition targetPosition;

} ArUcoState;

/* ================= VARIABLES GLOBALES ================= */

extern ArUcoState arucoState;

/* ================= API PUBLIQUE ================= */

void ArUco_Init(void);

void ArUco_ProcessMessage(uint16_t function,
                          uint16_t payloadLength,
                          uint8_t *payload);

void ArUco_Update(uint32_t currentTimeMs);

#endif /* ARUCO_GHOST_H */
