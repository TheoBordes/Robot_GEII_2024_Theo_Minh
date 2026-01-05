#include "aruco_ghost.h"
#include "robot.h"
#include <math.h>
#include <string.h>

/* ================= PARAMETRES FILTRE KALMAN ================= */
#define KALMAN_Q_POS    0.5f    // Bruit processus position
#define KALMAN_Q_SIZE   0.3f    // Bruit processus taille
#define KALMAN_R_POS    8.0f    // Bruit mesure position
#define KALMAN_R_SIZE   5.0f    // Bruit mesure taille
#define KALMAN_P_INIT   20.0f   // Incertitude initiale

/* ================= ETAT GLOBAL ================= */
ArUcoState arucoState;

/* ================= FILTRE KALMAN 1D AVEC VITESSE ================= */
typedef struct {
    float x;        // État estimé (position ou taille)
    float v;        // Vitesse estimée
    float P_x;      // Covariance position
    float P_v;      // Covariance vitesse
    float Q;        // Bruit processus
    float R;        // Bruit mesure
    uint8_t initialized;
} Kalman1D;

static Kalman1D kalmanX = {0};      // Filtre pour position X
static Kalman1D kalmanSize = {0};   // Filtre pour taille (distance)

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

/* ================= KALMAN 1D OPERATIONS ================= */
static void Kalman_Init(Kalman1D *k, float Q, float R) {
    k->x = 0.0f;
    k->v = 0.0f;
    k->P_x = KALMAN_P_INIT;
    k->P_v = KALMAN_P_INIT;
    k->Q = Q;
    k->R = R;
    k->initialized = 0;
}

static void Kalman_Predict(Kalman1D *k, float dt) {
    // Prédiction: x = x + v*dt
    k->x = k->x + k->v * dt;
    
    // Mise à jour covariance avec bruit de processus
    k->P_x += k->P_v * dt * dt + k->Q;
    k->P_v += k->Q * 0.5f;
}

static void Kalman_Update(Kalman1D *k, float measurement, float dt) {
    // Gain de Kalman
    float K = k->P_x / (k->P_x + k->R);
    
    // Innovation (différence mesure - prédiction)
    float innovation = measurement - k->x;
    
    // Mise à jour état
    float oldX = k->x;
    k->x = k->x + K * innovation;
    
    // Mise à jour vitesse (estimation par différence)
    if (dt > 0.001f) {
        float newVelocity = (k->x - oldX) / dt;
        k->v = k->v * 0.7f + newVelocity * 0.3f;  // Filtre passe-bas sur vitesse
    }
    
    // Mise à jour covariance
    k->P_x = (1.0f - K) * k->P_x;
}

static void Kalman_SetMeasurement(Kalman1D *k, float measurement) {
    k->x = measurement;
    k->v = 0.0f;
    k->P_x = KALMAN_P_INIT;
    k->P_v = KALMAN_P_INIT;
    k->initialized = 1;
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
    float offsetPixels = centerX - (ARUCO_CAMERA_WIDTH / 2.0f);
    
    // Angle = atan(offset / focale)
    // Approximation linéaire pour petits angles: angle ≈ offset * (FOV/width)
    float fovRad = ARUCO_CAMERA_FOV_H * (3.14159265f / 180.0f);
    return -offsetPixels * (fovRad / ARUCO_CAMERA_WIDTH);
}

/* ================= INITIALISATION ================= */
void ArUco_Init(void) {
    memset(&arucoState, 0, sizeof(arucoState));
    
    // Paramètres par défaut
    arucoState.followMode = ARUCO_MODE_FULL_FOLLOW;
    arucoState.targetId = 47;
    arucoState.targetDistance = ARUCO_FOLLOW_DISTANCE;
    arucoState.distanceTolerance = ARUCO_DISTANCE_TOLERANCE;
    
    // Gains par défaut
    arucoState.gainAngle = 2.0f;
    arucoState.gainDistance = 1.5f;
    arucoState.maxLinearSpeed = 0.5f;    // m/s
    arucoState.maxAngularSpeed = 2.0f;   // rad/s
    
    // Init filtres Kalman
    Kalman_Init(&kalmanX, KALMAN_Q_POS, KALMAN_R_POS);
    Kalman_Init(&kalmanSize, KALMAN_Q_SIZE, KALMAN_R_SIZE);
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
    
    arucoState.detectionCount++;
}

/* ================= MISE A JOUR PERIODIQUE ================= */
void ArUco_Update(uint32_t currentTimeMs) {
    float dt = (currentTimeMs - arucoState.lastUpdateTime) * 0.001f;
    if (dt <= 0.0f || dt > 1.0f) dt = 0.01f;  // Sécurité
    arucoState.lastUpdateTime = currentTimeMs;
    
    ArUcoMarker *m = &arucoState.marker;
    
    // Vérifie si marqueur visible (pas de timeout)
    uint8_t wasVisible = arucoState.markerVisible;
    arucoState.markerVisible = (m->valid && 
                                (currentTimeMs - m->lastSeenTime) <= ARUCO_TIMEOUT_MS);
    
    // Détection de perte
    if (wasVisible && !arucoState.markerVisible) {
        arucoState.lostCount++;
    }
    
    /* ========== FILTRE KALMAN ========== */
    if (!kalmanX.initialized && arucoState.markerVisible) {
        // Première détection: initialiser les filtres
        Kalman_SetMeasurement(&kalmanX, m->rawCenterX);
        Kalman_SetMeasurement(&kalmanSize, m->rawSize);
        arucoState.hasValidEstimate = 1;
        
    } else if (kalmanX.initialized) {
        // Prédiction (toujours)
        Kalman_Predict(&kalmanX, dt);
        Kalman_Predict(&kalmanSize, dt);
        
        // Mise à jour si mesure disponible
        if (arucoState.markerVisible) {
            Kalman_Update(&kalmanX, m->rawCenterX, dt);
            Kalman_Update(&kalmanSize, m->rawSize, dt);
        }
        // Sinon on garde la prédiction
    }
    
    /* ========== CALCUL ESTIMATIONS ========== */
    if (arucoState.hasValidEstimate) {
        arucoState.filtered.x = kalmanX.x;
        arucoState.filtered.size = kalmanSize.x;
        arucoState.estimatedDistance = estimateDistanceFromSize(kalmanSize.x);
        arucoState.estimatedAngle = estimateAngleFromPosition(kalmanX.x);
    }
    
    /* ========== CALCUL CONSIGNES DE SUIVI ========== */
    if (arucoState.followMode == ARUCO_MODE_DISABLED || !arucoState.hasValidEstimate) {
        arucoState.cmdLinearSpeed = 0.0f;
        arucoState.cmdAngularSpeed = 0.0f;
        
    } else {
        // Temps depuis dernière détection réelle
        uint32_t timeSinceSeen = currentTimeMs - m->lastSeenTime;
        
        if (timeSinceSeen > ARUCO_LOST_TIMEOUT_MS) {
            // Perdu depuis trop longtemps: arrêt complet
            arucoState.cmdLinearSpeed = 0.0f;
            arucoState.cmdAngularSpeed = 0.0f;
            arucoState.hasValidEstimate = 0;
            kalmanX.initialized = 0;
            kalmanSize.initialized = 0;
            
        } else {
            // Facteur d'atténuation si prédiction seule (pas de mesure récente)
            float confidence = 1.0f;
            if (timeSinceSeen > 100) {
                confidence = 1.0f - ((float)(timeSinceSeen - 100) / (float)(ARUCO_LOST_TIMEOUT_MS - 100));
                if (confidence < 0.2f) confidence = 0.2f;
            }
            
            /* Consigne angulaire: corriger l'angle pour centrer le marqueur */
            float angleError = arucoState.estimatedAngle;
            arucoState.cmdAngularSpeed = arucoState.gainAngle * angleError * confidence;
            arucoState.cmdAngularSpeed = clampf(arucoState.cmdAngularSpeed, 
                                                -arucoState.maxAngularSpeed, 
                                                arucoState.maxAngularSpeed);
            
            /* Consigne linéaire: maintenir la distance cible */
            if (arucoState.followMode == ARUCO_MODE_ANGLE_ONLY) {
                arucoState.cmdLinearSpeed = 0.0f;
                
            } else {
                float distanceError = arucoState.estimatedDistance - arucoState.targetDistance;
                
                // Zone morte autour de la distance cible
                if (absf(distanceError) < arucoState.distanceTolerance) {
                    arucoState.cmdLinearSpeed = 0.0f;
                } else {
                    arucoState.cmdLinearSpeed = arucoState.gainDistance * distanceError * confidence;
                    arucoState.cmdLinearSpeed = clampf(arucoState.cmdLinearSpeed,
                                                       -arucoState.maxLinearSpeed,
                                                       arucoState.maxLinearSpeed);
                }
                
                // Mode APPROACH: ne pas reculer
                if (arucoState.followMode == ARUCO_MODE_APPROACH && arucoState.cmdLinearSpeed < 0) {
                    arucoState.cmdLinearSpeed = 0.0f;
                }
                
                // Réduire vitesse linéaire si angle trop grand (tourner d'abord)
                if (absf(angleError) > 0.3f) {  // ~17 degrés
                    arucoState.cmdLinearSpeed *= 0.3f;
                } else if (absf(angleError) > 0.15f) {  // ~8 degrés
                    arucoState.cmdLinearSpeed *= 0.7f;
                }
            }
        }
    }
    
    /* ========== MISE A JOUR ROBOT STATE ========== */
    robotState.centerX_Aruco = arucoState.filtered.x;
    robotState.markerAvgSize = arucoState.filtered.size;
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