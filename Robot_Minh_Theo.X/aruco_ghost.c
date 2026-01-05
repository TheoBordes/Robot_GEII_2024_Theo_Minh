#include "aruco_ghost.h"
#include "robot.h"
#include <math.h>
#include <string.h>

#ifndef M_PI
#define M_PI 3.14159265358979323846f
#endif

/* ================= PARAMETRES DE SUIVI ================= */
#define DEADZONE_ANGLE_DEG      2.0f    // Zone morte angulaire (degrés)
#define DEADZONE_DISTANCE       0.02f   // Zone morte distance (mètres)
#define MAX_ANGLE_FOR_LINEAR    25.0f   // Angle max pour avancer (degrés)

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

/* ================= FILTRE PASSE-BAS ================= */
static float lowPassFilter(float newValue, float oldValue, float alpha, uint8_t isFirst) {
    if (isFirst) {
        return newValue;  // Pas de filtrage pour la première mesure
    }
    return oldValue + alpha * (newValue - oldValue);
}

/* ================= ESTIMATION DISTANCE ================= */
static float estimateDistanceFromSize(float markerSizePixels) {
    // Distance = (taille_réelle * focale) / taille_pixels
    // Formule de projection perspective
    if (markerSizePixels < 10.0f) return -1.0f;  // Trop petit, invalide
    
    return (ARUCO_MARKER_REAL_SIZE * ARUCO_CAMERA_FOCAL_PX) / markerSizePixels;
}

/* ================= ESTIMATION ANGLE ================= */
static float estimateAngleFromPosition(float centerX) {
    // Convertit la position X en pixels vers un angle en radians
    // Centre de l'image = angle 0
    // Positif = marqueur à droite, négatif = marqueur à gauche
    
    float offsetPixels = centerX - (ARUCO_CAMERA_WIDTH / 2.0f);
    
    // Angle = atan(offset / focale) - formule exacte de projection
    return atanf(offsetPixels / ARUCO_CAMERA_FOCAL_PX);
}

/* ================= CALCUL POSITION CARTESIENNE ================= */
static void polarToCartesian(float distance, float angle, float *outX, float *outY) {
    // Conversion coordonnées polaires (distance, angle) vers cartésiennes (X, Y)
    // Dans le repère caméra:
    //   X = distance * sin(angle)  -> positif = droite
    //   Y = distance * cos(angle)  -> positif = devant
    *outX = distance * sinf(angle);
    *outY = distance * cosf(angle);
}

/* ================= CALCUL POSITION CIBLE (GHOST) ================= */
static void calculateTargetPosition(float markerX, float markerY, float targetDist, 
                                    float *outTargetX, float *outTargetY) {
    // Calcule la position où le robot doit se placer pour être à targetDist du marqueur
    // C'est un point sur la ligne robot-marqueur, à targetDist du marqueur
    
    float distToMarker = sqrtf(markerX * markerX + markerY * markerY);
    
    if (distToMarker < 0.01f) {
        // Marqueur trop proche, cible = position actuelle
        *outTargetX = 0.0f;
        *outTargetY = 0.0f;
        return;
    }
    
    // Direction normalisée vers le marqueur
    float dirX = markerX / distToMarker;
    float dirY = markerY / distToMarker;
    
    // Position cible = marqueur - direction * distance_cible
    // C'est le point à targetDist du marqueur, sur la ligne vers le robot
    *outTargetX = markerX - dirX * targetDist;
    *outTargetY = markerY - dirY * targetDist;
}

/* ================= INITIALISATION ================= */
void ArUco_Init(void) {
    memset(&arucoState, 0, sizeof(arucoState));
    
    // Paramètres par défaut
    arucoState.followMode = ARUCO_MODE_FULL_FOLLOW;
    arucoState.targetId = 47;
    arucoState.targetDistance = ARUCO_FOLLOW_DISTANCE;
    arucoState.distanceTolerance = ARUCO_DISTANCE_TOLERANCE;
    arucoState.isFirstMeasurement = 1;
    
    // Gains par défaut
    arucoState.gainAngle = 1.5f;       // Gain proportionnel angulaire (rad/s par rad)
    arucoState.gainDistance = 0.8f;    // Gain proportionnel distance (m/s par m)
    arucoState.maxLinearSpeed = 0.25f; // m/s max
    arucoState.maxAngularSpeed = 1.5f; // rad/s max
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
    
    // Calcul direct de la distance et de l'angle
    float rawDistance = estimateDistanceFromSize(avgSize);
    float rawAngle = estimateAngleFromPosition(centerX);
    
    // Vérification distance valide
    if (rawDistance < 0.0f) return;
    
    arucoState.estimatedDistance = rawDistance;
    arucoState.estimatedAngle = rawAngle;
    
    // Calcul de la position relative (X, Y) dans le repère caméra
    float camX, camY;
    polarToCartesian(rawDistance, rawAngle, &camX, &camY);
    
    // Conversion repère caméra -> repère robot (ajout offset caméra)
    float robotX = camX + CAMERA_OFFSET_X;
    float robotY = camY + CAMERA_OFFSET_Y;
    
    // Filtrage passe-bas pour lisser les mesures
    arucoState.filteredX = lowPassFilter(robotX, arucoState.filteredX, 
                                          ARUCO_FILTER_ALPHA, arucoState.isFirstMeasurement);
    arucoState.filteredY = lowPassFilter(robotY, arucoState.filteredY, 
                                          ARUCO_FILTER_ALPHA, arucoState.isFirstMeasurement);
    
    // Position brute (non filtrée)
    arucoState.relativeX = robotX;
    arucoState.relativeY = robotY;
    
    // Calcul distance et angle filtrés
    arucoState.filteredDistance = sqrtf(arucoState.filteredX * arucoState.filteredX + 
                                        arucoState.filteredY * arucoState.filteredY);
    arucoState.filteredAngle = atan2f(arucoState.filteredX, arucoState.filteredY);
    
    // Calcul position cible (ghost) - où le robot doit être pour maintenir la distance
    calculateTargetPosition(arucoState.filteredX, arucoState.filteredY,
                           arucoState.targetDistance,
                           &arucoState.targetX, &arucoState.targetY);
    
    // Première mesure terminée
    arucoState.isFirstMeasurement = 0;
    
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
            arucoState.isFirstMeasurement = 1;  // Reset filtre
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
    
    // Utiliser les valeurs filtrées pour le suivi
    float angleToMarker = arucoState.filteredAngle;  // Angle vers le marqueur (rad)
    float angleToMarkerDeg = angleToMarker * (180.0f / M_PI);  // Conversion en degrés
    
    // Erreur de distance = distance filtrée - distance cible
    float distanceError = arucoState.filteredDistance - arucoState.targetDistance;
    
    // ========== COMMANDE ANGULAIRE ==========
    // Tourner pour faire face au marqueur
    float angularCmd = 0.0f;
    if (absf(angleToMarkerDeg) > DEADZONE_ANGLE_DEG) {
        angularCmd = angleToMarker * arucoState.gainAngle;
        angularCmd = clampf(angularCmd, -arucoState.maxAngularSpeed, arucoState.maxAngularSpeed);
    }
    
    // ========== COMMANDE LINEAIRE ==========
    float linearCmd = 0.0f;
    
    // N'avancer que si l'angle est suffisamment faible
    if (absf(angleToMarkerDeg) < MAX_ANGLE_FOR_LINEAR) {
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
    return arucoState.filteredDistance;  // Retourne la distance filtrée
}

uint8_t ArUco_GetRelativePosition(float *x, float *y) {
    if (!arucoState.hasValidEstimate) {
        *x = 0.0f;
        *y = 0.0f;
        return 0;
    }
    
    // Retourne la position filtrée (plus stable)
    *x = arucoState.filteredX;
    *y = arucoState.filteredY;
    return 1;
}

uint8_t ArUco_GetRawPosition(float *x, float *y) {
    if (!arucoState.hasValidEstimate) {
        *x = 0.0f;
        *y = 0.0f;
        return 0;
    }
    
    // Retourne la position brute (non filtrée)
    *x = arucoState.relativeX;
    *y = arucoState.relativeY;
    return 1;
}

uint8_t ArUco_GetTargetPosition(float *x, float *y) {
    if (!arucoState.hasValidEstimate) {
        *x = 0.0f;
        *y = 0.0f;
        return 0;
    }
    
    // Retourne la position cible (où le robot doit aller)
    *x = arucoState.targetX;
    *y = arucoState.targetY;
    return 1;
}