#include "aruco_ghost.h"
#include "robot.h"
#include <math.h>
#include <string.h>

/* ================= PARAMÈTRES CAMÉRA ================= */
#define ARUCO_TIMEOUT_MS 500 /* ms */

/* ================= FILTRE KALMAN SIMPLIFIÉ ================= */
#define KALMAN_Q 0.5f      // Bruit du processus (petite valeur = modèle fiable)
#define KALMAN_R 8.0f      
#define KALMAN_P_INIT 20.0f // Incertitude initiale

/* ================= ÉTAT ================= */
ArUcoState arucoState;
static uint32_t lastUpdateTimeMs = 0;

/* État Kalman pour centerX */
typedef struct {
    float x;      // État estimé (position)
    float v;      // Vitesse estimée
    float P_x;    // Covariance position
    float P_v;    // Covariance vitesse
    uint32_t lastMeasurementTime;
    uint8_t initialized;
} KalmanState;

static KalmanState kalmanX = {0};

/* ================= UTILS ================= */
static float getFloatFromBytes(uint8_t *bytes, int offset) {
    union {
        float f;
        uint8_t b[4];
    } u;
    memcpy(u.b, &bytes[offset], 4);
    return u.f;
}

/* ================= KALMAN 1D AVEC VITESSE ================= */
static void Kalman_Predict(KalmanState *k, float dt) {
    // Prédiction: x = x + v*dt
    k->x = k->x + k->v * dt;
   
    // Mise à jour covariance avec bruit de processus
    k->P_x += k->P_v * dt * dt + KALMAN_Q;
    k->P_v += KALMAN_Q * 0.5f;
}

static void Kalman_Update(KalmanState *k, float measurement) {
    // Gain de Kalman
    float K = k->P_x / (k->P_x + KALMAN_R);
   
    // Innovation (différence mesure - prédiction)
    float innovation = measurement - k->x;
   
    // Mise à jour état
    k->x = k->x + K * innovation;
   
    // Mise à jour vitesse (dérivée de l'innovation)
    k->v = k->v * 0.8f + innovation * 0.2f;
   
    // Mise à jour covariance
    k->P_x = (1.0f - K) * k->P_x;
}

/* ================= INIT ================= */
void ArUco_Init(void) {
    memset(&arucoState, 0, sizeof(arucoState));
    arucoState.followEnabled = 1;
    arucoState.targetId = 47;
    arucoState.hasEstimate = 0;
   
    // Init Kalman
    kalmanX.x = 0.0f;
    kalmanX.v = 0.0f;
    kalmanX.P_x = KALMAN_P_INIT;
    kalmanX.P_v = KALMAN_P_INIT;
    kalmanX.initialized = 0;
}

/* ================= RECEPTION UART ================= */
void ArUco_ProcessMessage(uint16_t function, uint16_t payloadLength, uint8_t *payload) {
    (void)function;
    if (payloadLength < 18) return;
   
    uint16_t markerId = ((uint16_t)payload[0] << 8) | payload[1];
    float centerX = getFloatFromBytes(payload, 2);
    float width = getFloatFromBytes(payload, 10);
    float height = getFloatFromBytes(payload, 14);
   
    float avgSize = (width + height) * 0.5f;
    if (avgSize < 2.0f) return;
   
    ArUcoMarker *marker = &arucoState.markers[0];
    marker->id = markerId;
    marker->valid = 1;
    marker->lastSeenTime = lastUpdateTimeMs;
   
    marker->position.x = centerX;
    marker->position.y = 0.0f;
    marker->position.z = avgSize;
   
    arucoState.activeMarker = marker;
}

/* ================= UPDATE ================= */
void ArUco_Update(uint32_t currentTimeMs) {
    float dt = (currentTimeMs - lastUpdateTimeMs) * 0.001f; // secondes
    lastUpdateTimeMs = currentTimeMs;
   
    ArUcoMarker *marker = arucoState.activeMarker;
    uint8_t markerVisible = (marker != NULL && marker->valid &&
                             (currentTimeMs - marker->lastSeenTime) <= ARUCO_TIMEOUT_MS);
   
    if (!kalmanX.initialized && markerVisible) {
        // Initialisation avec première mesure
        kalmanX.x = marker->position.x;
        kalmanX.v = 0.0f;
        kalmanX.P_x = KALMAN_P_INIT;
        kalmanX.P_v = KALMAN_P_INIT;
        kalmanX.lastMeasurementTime = currentTimeMs;
        kalmanX.initialized = 1;
       
        arucoState.estimatedCenterX = kalmanX.x;
        arucoState.estimatedSize = marker->position.z;
        arucoState.hasEstimate = 1;
       
    } else if (kalmanX.initialized) {
        // Prédiction (toujours effectuée)
        Kalman_Predict(&kalmanX, dt);
       
        // Mise à jour si mesure disponible
        if (markerVisible) {
            Kalman_Update(&kalmanX, marker->position.x);
            kalmanX.lastMeasurementTime = currentTimeMs;
           
            // Mise à jour taille (filtre simple)
            if (!arucoState.hasEstimate) {
                arucoState.estimatedSize = marker->position.z;
            } else {
                arucoState.estimatedSize = 0.3f * marker->position.z + 0.7f * arucoState.estimatedSize;
            }
        }
        // Si pas de mesure, on garde juste la prédiction
       
        arucoState.estimatedCenterX = kalmanX.x;
        arucoState.hasEstimate = 1;
    }
   
    robotState.centerX_Aruco = arucoState.estimatedCenterX;
    robotState.markerAvgSize = arucoState.estimatedSize;
}