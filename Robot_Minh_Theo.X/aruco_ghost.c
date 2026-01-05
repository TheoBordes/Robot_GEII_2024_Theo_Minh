#include "aruco_ghost.h"
#include "robot.h"
#include <math.h>
#include <string.h>

#ifndef M_PI
#define M_PI 3.14159265358979323846f
#endif

/* ================= PARAMETRES DE SUIVI ================= */
#define DEADZONE_ANGLE_DEG      3.0f    // Zone morte angulaire (degrés)
#define DEADZONE_DISTANCE       0.05f   // Zone morte distance (mètres)
#define MAX_ANGLE_FOR_LINEAR    30.0f   // Angle max pour avancer (degrés)

/* ================= ETAT GLOBAL ================= */
ArUcoState arucoState;

/* ================= FONCTIONS UTILITAIRES ================= */
static float getFloatFromBytes(uint8_t *bytes, int offset) {
    union {
        float f;
        uint8_t b[4];
    } u;
    memcpy(u.b, &bytes[offset], 4);
    return u.f;
}

static float clampf(float value, float min, float max) {
    if (value < min) return min;
    if (value > max) return max;
    return value;
}

static float absf(float x) {
    return (x < 0) ? -x : x;
}

/* ================= ESTIMATION DISTANCE ================= */
static float estimateDistanceFromSize(float markerSizePixels) {
    // Distance = (taille_réelle * focale) / taille_pixels
    if (markerSizePixels < 5.0f) return -1.0f;  // Trop petit, invalide
    
    return (ARUCO_MARKER_REAL_SIZE * ARUCO_CAMERA_FOCAL_PX) / markerSizePixels;
}

/* ================= ESTIMATION ANGLE ================= */
static float estimateAngleFromPosition(float centerX) {
    // Convertit la position X en angle (0 = centre de l'image)
    // Positif = marqueur à droite, négatif = marqueur à gauche
    float offsetPixels = centerX - (ARUCO_CAMERA_WIDTH / 2.0f);
    
    // Angle en radians: FOV_H / largeur_image * offset
    float fovRad = ARUCO_CAMERA_FOV_H * (M_PI / 180.0f);
    return offsetPixels * (fovRad / ARUCO_CAMERA_WIDTH);
}

/* ================= INITIALISATION ================= */
void ArUco_Init(void) {
    memset(&arucoState, 0, sizeof(arucoState));
    
    // Paramètres par défaut
    arucoState.followMode = ARUCO_MODE_FULL_FOLLOW;
    arucoState.targetId = 47;
    arucoState.targetDistance = ARUCO_FOLLOW_DISTANCE;
    arucoState.distanceTolerance = ARUCO_DISTANCE_TOLERANCE;
    
    // Gains par défaut (ajustés pour suivi direct sans Kalman)
    arucoState.gainAngle = 2.0f;       // Gain proportionnel angulaire
    arucoState.gainDistance = 1.5f;    // Gain proportionnel distance
    arucoState.maxLinearSpeed = 0.3f;  // m/s max
    arucoState.maxAngularSpeed = 2.0f; // rad/s max
}

/* ================= CONFIGURATION ================= */
void ArUco_SetFollowParams(ArUcoFollowMode mode, uint16_t targetId, float targetDistance) {
    arucoState.followMode = mode;
    arucoState.targetId = targetId;
    arucoState.targetDistance = targetDistance;
}

void ArUco_SetGains(float gainAngle, float gainDistance, float maxLinear, float maxAngular) {
    arucoState.gainAngle = gainAngle;
    arucoState.gainDistance = gainDistance;
    arucoState.maxLinearSpeed = maxLinear;
    arucoState.maxAngularSpeed = maxAngular;
}

/* ================= RECEPTION UART (JEVOIS) ================= */
void ArUco_ProcessMessage(uint16_t function, uint16_t payloadLength, uint8_t *payload) {
    (void)function;
    
    // Format payload: markerId(2) + centerX(4) + centerY(4) + width(4) + height(4) = 18 bytes
    if (payloadLength < 18) return;
    
    uint16_t markerId = ((uint16_t)payload[0] << 8) | payload[1];
    
    // Vérifie si c'est le marqueur qu'on suit
    if (arucoState.targetId != 0 && markerId != arucoState.targetId) {
        return;  // Ignore les autres marqueurs
    }
    
    float centerX = getFloatFromBytes(payload, 2);
    float centerY = getFloatFromBytes(payload, 6);
    float width = getFloatFromBytes(payload, 10);
    float height = getFloatFromBytes(payload, 14);
    
    float avgSize = (width + height) * 0.5f;
    
    // Validation basique
    if (avgSize < 5.0f || avgSize > ARUCO_CAMERA_WIDTH) return;
    if (centerX < 0 || centerX > ARUCO_CAMERA_WIDTH) return;
    if (centerY < 0 || centerY > ARUCO_CAMERA_HEIGHT) return;
    
    // Mise à jour données brutes
    ArUcoMarker *m = &arucoState.marker;
    m->id = markerId;
    m->valid = 1;
    m->lastSeenTime = arucoState.lastUpdateTime;
    m->rawCenterX = centerX;
    m->rawCenterY = centerY;
    m->rawWidth = width;
    m->rawHeight = height;
    m->rawSize = avgSize;
    arucoState.rawcenterX = centerX;
    
    // Calcul direct de la distance et de l'angle (sans Kalman)
    arucoState.estimatedDistance = estimateDistanceFromSize(avgSize);
    arucoState.estimatedAngle = estimateAngleFromPosition(centerX);
    
    // Marquer le marqueur comme visible avec une estimation valide
    arucoState.markerVisible = 1;
    arucoState.hasValidEstimate = 1;
    
    arucoState.detectionCount++;
}

/* ================= MISE A JOUR PERIODIQUE ================= */
void ArUco_Update(uint32_t currentTimeMs) {
    // Vérifier timeout de détection
    uint32_t elapsed = currentTimeMs - arucoState.marker.lastSeenTime;
    
    if (elapsed > ARUCO_TIMEOUT_MS) {
        // Marqueur perdu - arrêter le robot
        arucoState.markerVisible = 0;
        arucoState.cmdLinearSpeed = 0.0f;
        arucoState.cmdAngularSpeed = 0.0f;
        
        if (elapsed > ARUCO_LOST_TIMEOUT_MS) {
            arucoState.hasValidEstimate = 0;
            arucoState.lostCount++;
        }
        return;
    }
    
    // Mode désactivé
    if (arucoState.followMode == ARUCO_MODE_DISABLED) {
        arucoState.cmdLinearSpeed = 0.0f;
        arucoState.cmdAngularSpeed = 0.0f;
        return;
    }
    
    // Calcul des erreurs
    float angleError = arucoState.estimatedAngle;  // Angle vers le marqueur (rad)
    float angleErrorDeg = angleError * (180.0f / M_PI);  // Conversion en degrés
    
    // Erreur de distance = distance actuelle - distance cible
    float distanceError = arucoState.estimatedDistance - arucoState.targetDistance;
    
    // ========== COMMANDE ANGULAIRE ==========
    float angularCmd = 0.0f;
    if (absf(angleErrorDeg) > DEADZONE_ANGLE_DEG) {
        // Correcteur proportionnel sur l'angle
        angularCmd = angleError * arucoState.gainAngle;
        angularCmd = clampf(angularCmd, -arucoState.maxAngularSpeed, arucoState.maxAngularSpeed);
    }
    
    // ========== COMMANDE LINEAIRE ==========
    float linearCmd = 0.0f;
    
    // N'avancer que si l'angle est suffisamment faible (comme dans le Python)
    if (absf(angleErrorDeg) < MAX_ANGLE_FOR_LINEAR) {
        if (arucoState.followMode == ARUCO_MODE_FULL_FOLLOW || 
            arucoState.followMode == ARUCO_MODE_APPROACH) {
            
            // Avancer/reculer selon l'erreur de distance
            if (absf(distanceError) > DEADZONE_DISTANCE) {
                linearCmd = distanceError * arucoState.gainDistance;
                linearCmd = clampf(linearCmd, -arucoState.maxLinearSpeed, arucoState.maxLinearSpeed);
            }
            
            // Mode APPROACH: arrêter si distance cible atteinte
            if (arucoState.followMode == ARUCO_MODE_APPROACH) {
                if (absf(distanceError) < arucoState.distanceTolerance) {
                    linearCmd = 0.0f;
                }
            }
        }
    }
    
    // Mise à jour des consignes
    arucoState.cmdLinearSpeed = linearCmd;
    arucoState.cmdAngularSpeed = angularCmd;
    arucoState.lastUpdateTime = currentTimeMs;
}

/* ================= ACCESSEURS ================= */
uint8_t ArUco_GetSpeedCommands(float *linearSpeed, float *angularSpeed) {
    if (!arucoState.hasValidEstimate) {
        *linearSpeed = 0.0f;
        *angularSpeed = 0.0f;
        return 0;
    }
    
    *linearSpeed = arucoState.cmdLinearSpeed;
    *angularSpeed = arucoState.cmdAngularSpeed;
    return 1;
}

uint8_t ArUco_IsMarkerVisible(void) {
    return arucoState.markerVisible;
}

float ArUco_GetDistance(void) {
    if (!arucoState.hasValidEstimate) return -1.0f;
    return arucoState.estimatedDistance;
}