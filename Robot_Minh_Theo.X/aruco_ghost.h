#ifndef ARUCO_GHOST_H
#define ARUCO_GHOST_H

#include <stdint.h>

/* ================= PARAMETRES DE SUIVI ================= */
#define ARUCO_CAMERA_WIDTH       1280      // Largeur image caméra JeVois (pixels)
#define ARUCO_CAMERA_HEIGHT      720       // Hauteur image caméra JeVois (pixels)
#define ARUCO_CAMERA_FOV_H       60.0f     // Champ de vision horizontal (degrés)
#define ARUCO_MARKER_REAL_SIZE   0.10f     // Taille réelle du marqueur ArUco (mètres)
#define ARUCO_CAMERA_FOCAL_PX    985.0f    // Focale approximative en pixels (à calibrer)
#define CAMERA_HFOV_RAD       (ARUCO_CAMERA_FOV_H * M_PI / 180.0) // ? 1.047 rad

#define ARUCO_FOLLOW_DISTANCE    0.3f     // Distance cible de suivi (mètres)
#define ARUCO_DISTANCE_TOLERANCE 0.01f     // Tolérance sur la distance (mètres)
#define ARUCO_TIMEOUT_MS         200       // Timeout perte de détection (ms)
#define ARUCO_LOST_TIMEOUT_MS    1000      // Timeout arrêt complet (ms)

/* ================= MODES DE SUIVI ================= */
typedef enum {
    ARUCO_MODE_DISABLED = 0,    // Suivi désactivé
    ARUCO_MODE_ANGLE_ONLY,      // Suivi angulaire uniquement (rotation)
    ARUCO_MODE_FULL_FOLLOW,     // Suivi complet (angle + distance)
    ARUCO_MODE_APPROACH         // Approche jusqu'à distance cible puis arrêt
} ArUcoFollowMode;

/* ================= STRUCTURES ================= */

typedef struct {
    float x;        // Position X estimée (pixels)
    float y;        // Position Y estimée (pixels)  
    float size;     // Taille estimée (pixels)
} ArUcoEstimate;

typedef struct {
    uint16_t id;
    uint8_t valid;
    uint32_t lastSeenTime;
    float rawCenterX;       // Position brute X (pixels)
    float rawCenterY;       // Position brute Y (pixels)
    float rawWidth;         // Largeur brute (pixels)
    float rawHeight;        // Hauteur brute (pixels)
    float rawSize;          // Taille moyenne brute (pixels)
} ArUcoMarker;

typedef struct {
    /* État du marqueur */
    ArUcoMarker marker;
    uint8_t markerVisible;          // Marqueur actuellement visible
    uint8_t hasValidEstimate;       // Estimation valide disponible
    
    /* Estimations filtrées */
    ArUcoEstimate filtered;         // Position/taille filtrées Kalman
    float estimatedDistance;        // Distance estimée (mètres)
    float estimatedAngle;           // Angle vers marqueur (radians)
    
    /* Paramètres de suivi */
    ArUcoFollowMode followMode;
    uint16_t targetId;              // ID du marqueur à suivre
    float targetDistance;           // Distance cible de suivi (mètres)
    float distanceTolerance;        // Tolérance distance (mètres)
    
    
    float rawcenterX;
    /* Consignes de sortie */
    float cmdLinearSpeed;           // Consigne vitesse linéaire (m/s)
    float cmdAngularSpeed;          // Consigne vitesse angulaire (rad/s)
    
    /* Gains de suivi (modifiables) */
    float gainAngle;                // Gain correcteur angulaire
    float gainDistance;             // Gain correcteur distance
    float maxLinearSpeed;           // Vitesse linéaire max (m/s)
    float maxAngularSpeed;          // Vitesse angulaire max (rad/s)
    
    /* Statistiques */
    uint32_t lastUpdateTime;
    uint32_t detectionCount;
    uint32_t lostCount;
} ArUcoState;

/* ================= VARIABLES GLOBALES ================= */
extern ArUcoState arucoState;

/* ================= FONCTIONS ================= */

/**
 * @brief Initialise le système ArUco avec paramètres par défaut
 */
void ArUco_Init(void);

/**
 * @brief Configure les paramètres de suivi
 * @param mode Mode de suivi (DISABLED, ANGLE_ONLY, FULL_FOLLOW, APPROACH)
 * @param targetId ID du marqueur à suivre
 * @param targetDistance Distance cible en mètres
 */
void ArUco_SetFollowParams(ArUcoFollowMode mode, uint16_t targetId, float targetDistance);

/**
 * @brief Configure les gains du correcteur de suivi
 * @param gainAngle Gain pour correction angulaire
 * @param gainDistance Gain pour correction distance
 * @param maxLinear Vitesse linéaire maximale (m/s)
 * @param maxAngular Vitesse angulaire maximale (rad/s)
 */
void ArUco_SetGains(float gainAngle, float gainDistance, float maxLinear, float maxAngular);

/**
 * @brief Traite un message ArUco reçu par UART (caméra JeVois)
 * @param function Code fonction
 * @param payloadLength Longueur du payload (doit être >= 18)
 * @param payload Données brutes du message
 */
void ArUco_ProcessMessage(uint16_t function, uint16_t payloadLength, uint8_t *payload);

/**
 * @brief Mise à jour du filtre Kalman et calcul des consignes
 *        À appeler périodiquement (ex: dans interruption timer)
 * @param currentTimeMs Timestamp actuel en millisecondes
 */
void ArUco_Update(uint32_t currentTimeMs);

/**
 * @brief Récupère les consignes de vitesse pour le suivi
 * @param linearSpeed Pointeur pour vitesse linéaire (m/s)
 * @param angularSpeed Pointeur pour vitesse angulaire (rad/s)
 * @return 1 si consignes valides, 0 sinon
 */
uint8_t ArUco_GetSpeedCommands(float *linearSpeed, float *angularSpeed);

/**
 * @brief Vérifie si le marqueur est actuellement visible
 * @return 1 si visible, 0 sinon
 */
uint8_t ArUco_IsMarkerVisible(void);

/**
 * @brief Récupère la distance estimée au marqueur
 * @return Distance en mètres, ou -1 si pas d'estimation valide
 */
float ArUco_GetDistance(void);

#endif // ARUCO_GHOST_H