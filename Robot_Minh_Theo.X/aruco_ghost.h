#ifndef ARUCO_GHOST_H
#define ARUCO_GHOST_H

#include <stdint.h>

#ifndef M_PI
#define M_PI 3.14159265358979323846f
#endif

/* ================= PARAMETRES CAMERA JEVOIS ================= */
#define ARUCO_CAMERA_WIDTH       1280.0f   // Largeur image caméra JeVois (pixels)
#define ARUCO_CAMERA_HEIGHT      720.0f    // Hauteur image caméra JeVois (pixels)
#define ARUCO_CAMERA_FOV_H       60.0f     // Champ de vision horizontal (degrés)

// Focale calculée: f = (largeur/2) / tan(FOV_H/2)
// Pour FOV=60°: f = 640 / tan(30°) = 640 / 0.577 ≈ 1109 pixels
#define ARUCO_CAMERA_FOCAL_PX    1109.0f   // Focale en pixels (calculée depuis FOV)

/* ================= PARAMETRES MARQUEUR ================= */
#define ARUCO_MARKER_REAL_SIZE   0.05f     // Taille réelle du marqueur ArUco (mètres) - 5cm

/* ================= OFFSET CAMERA PAR RAPPORT AU CENTRE ROBOT ================= */
// Position de la caméra dans le repère robot (à mesurer sur ton robot)
#define CAMERA_OFFSET_X          0.0f      // Décalage latéral caméra (m) - positif = droite
#define CAMERA_OFFSET_Y          0.0f      // Décalage vers l'avant caméra (m) - positif = devant

/* ================= PARAMETRES DE SUIVI ================= */
#define ARUCO_FOLLOW_DISTANCE    0.30f     // Distance cible de suivi (mètres)
#define ARUCO_DISTANCE_TOLERANCE 0.03f     // Tolérance sur la distance (mètres)
#define ARUCO_TIMEOUT_MS         300       // Timeout perte de détection (ms)
#define ARUCO_LOST_TIMEOUT_MS    1000      // Timeout arrêt complet (ms)

/* ================= FILTRE DE LISSAGE ================= */
#define ARUCO_FILTER_ALPHA       0.3f      // Coefficient filtre (0.1=lent, 0.9=rapide)

/* ================= MODES DE SUIVI ================= */
typedef enum {
    ARUCO_MODE_DISABLED = 0,    // Suivi désactivé
    ARUCO_MODE_ANGLE_ONLY,      // Suivi angulaire uniquement (rotation)
    ARUCO_MODE_FULL_FOLLOW,     // Suivi complet (angle + distance)
    ARUCO_MODE_APPROACH         // Approche jusqu'à distance cible puis arrêt
} ArUcoFollowMode;

/* ================= STRUCTURES ================= */

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
    uint8_t isFirstMeasurement;     // Première mesure (pas de filtrage)
    
    /* Position relative du marqueur dans repère ROBOT (après offset caméra) */
    float relativeX;                // Position X (mètres) - positif = droite
    float relativeY;                // Position Y (mètres) - positif = devant
    
    /* Position filtrée (lissée) */
    float filteredX;                // Position X filtrée
    float filteredY;                // Position Y filtrée
    
    /* Position cible (ghost) = où le robot doit aller */
    float targetX;                  // Cible X (pour maintenir distance)
    float targetY;                  // Cible Y (pour maintenir distance)
    
    /* Estimations */
    float estimatedDistance;        // Distance brute au marqueur (mètres)
    float estimatedAngle;           // Angle brut vers marqueur (radians)
    float filteredDistance;         // Distance filtrée
    float filteredAngle;            // Angle filtré
    
    /* Paramètres de suivi */
    ArUcoFollowMode followMode;
    uint16_t targetId;              // ID du marqueur à suivre (0 = tous)
    float targetDistance;           // Distance cible de suivi (mètres)
    float distanceTolerance;        // Tolérance distance (mètres)
    
    float rawcenterX;               // Position X brute pixels (pour debug)
    
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
 * @brief Mise à jour du suivi et calcul des consignes
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
 * @brief Récupère la distance estimée au marqueur (filtrée)
 * @return Distance en mètres, ou -1 si pas d'estimation valide
 */
float ArUco_GetDistance(void);

/**
 * @brief Récupère la position filtrée du marqueur dans le repère robot
 * @param x Pointeur pour position X (mètres) - positif = droite
 * @param y Pointeur pour position Y (mètres) - positif = devant
 * @return 1 si position valide, 0 sinon
 */
uint8_t ArUco_GetRelativePosition(float *x, float *y);

/**
 * @brief Récupère la position brute (non filtrée) du marqueur
 * @param x Pointeur pour position X (mètres) - positif = droite
 * @param y Pointeur pour position Y (mètres) - positif = devant
 * @return 1 si position valide, 0 sinon
 */
uint8_t ArUco_GetRawPosition(float *x, float *y);

/**
 * @brief Récupère la position cible (ghost) où le robot doit aller
 * @param x Pointeur pour cible X (mètres)
 * @param y Pointeur pour cible Y (mètres)
 * @return 1 si position valide, 0 sinon
 */
uint8_t ArUco_GetTargetPosition(float *x, float *y);

#endif // ARUCO_GHOST_H