#ifndef ARUCO_GHOST_H
#define ARUCO_GHOST_H

#include <stdint.h>

/* ================= STRUCTURES ================= */

typedef struct {
    float x;
    float y;
    float z;
} Vector3;

typedef struct {
    uint16_t id;
    uint8_t valid;
    uint32_t lastSeenTime;
    Vector3 position;  // x=centerX (pixels), y=unused, z=avgSize
} ArUcoMarker;

typedef struct {
    ArUcoMarker markers[1];      // Un seul marqueur pour l'instant
    ArUcoMarker *activeMarker;   // Pointeur vers le marqueur actif
   
    uint8_t followEnabled;
    uint16_t targetId;
   
    uint8_t hasEstimate;
    float estimatedCenterX;
    float estimatedSize;
} ArUcoState;

/* ================= VARIABLES GLOBALES ================= */
extern ArUcoState arucoState;

/* ================= FONCTIONS ================= */

/**
 * @brief Initialise le système ArUco
 */
void ArUco_Init(void);

/**
 * @brief Traite un message ArUco reçu par UART
 * @param function Code fonction (non utilisé actuellement)
 * @param payloadLength Longueur du payload (doit être >= 18)
 * @param payload Données brutes du message
 */
void ArUco_ProcessMessage(uint16_t function, uint16_t payloadLength, uint8_t *payload);

/**
 * @brief Mise à jour du filtre ArUco (à appeler périodiquement)
 * @param currentTimeMs Timestamp actuel en millisecondes
 */
void ArUco_Update(uint32_t currentTimeMs);

#endif // ARUCO_GHOST_H