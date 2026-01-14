#include "aruco_ghost.h"
#include "robot.h"
#include "math.h"

void ArUco_Init(void) {

}

void ArUco_SetFollowParams(ArUcoFollowMode mode, uint16_t targetId, float targetDistance) {


}

void ArUco_SetGains(float gainAngle, float gainDistance, float maxLinear, float maxAngular) {

}
//
//void ArUco_ProcessMessage(void) {
//    // --- 1. Filtrage des entrées (Anti-bruit)
//    static double fX = 0, fY = 0;
//    const double alpha = 0.25; 
//    fX = (alpha * ((robotState.Z_Aruco / 1000.0) / 1.88)) + (1.0 - alpha) * fX;
//    fY = (alpha * (robotState.X_Aruco / 1000.0)) + (1.0 - alpha) * fY;
//
//    // --- 2. Paramètres de stabilité (Très prudents)
//    const double DIST_STOP      = 0.35;
//    const double vLinMaxAbs     = 2;  // Vitesse max en ligne droite
//    const double vAngMaxAbs     = 2.0;   // Vitesse de rotation max
//    const double accLin         = 0.18;  // Accélération linéaire douce
//    const double accAng         = 1   // Accélération angulaire douce
//    const double Kp_angle       = 2.2;   
//    const double Tsampling      = 0.01;  
//
//    // --- 3. Calculs géométriques
//    double distanceRestante = sqrt(fX*fX + fY*fY);
//    double angleCible = -atan2(fY, fX); 
//    double erreurDistance = distanceRestante - DIST_STOP;
//
//    // --- 4. GESTION DE LA VITESSE ANGULAIRE (Prioritaire)
//    double targetAng = angleCible * Kp_angle;
//    if (targetAng > vAngMaxAbs)  targetAng = vAngMaxAbs;
//    if (targetAng < -vAngMaxAbs) targetAng = -vAngMaxAbs;
//
//    // Rampe d'accélération angulaire (pour ne pas choquer les moteurs)
//    if (robotState.ArucoSpeedAngle < targetAng) 
//        robotState.ArucoSpeedAngle += accAng * Tsampling;
//    else if (robotState.ArucoSpeedAngle > targetAng)
//        robotState.ArucoSpeedAngle -= accAng * Tsampling;
//
//    // --- 5. GESTION DE LA VITESSE LINÉAIRE (Découplée)
//    // STRATÉGIE : On réduit la vitesse linéaire en fonction de la vitesse de rotation actuelle
//    // Si le robot tourne à fond, vLinAutorisee sera proche de 0.
//    double ratioRotation = fabs(robotState.ArucoSpeedAngle) / vAngMaxAbs;
//    double vLinMaxDynamique = vLinMaxAbs * (1.0 - ratioRotation);
//
//    if (erreurDistance <= 0 || fabs(angleCible) > 0.4) { 
//        // Si trop de travers ou trop près, on n'avance pas linéairement
//        robotState.vitesseLineaireGhost = 0;
//    } 
//    else {
//        // Calcul du freinage progressif
//        double vDeFreinage = sqrt(2.0 * accLin * erreurDistance);
//        
//        // Rampe d'accélération linéaire
//        robotState.vitesseLineaireGhost += accLin * Tsampling;
//
//        // Application des 3 limites : Accélération, Freinage, et Couplage avec la rotation
//        if (robotState.vitesseLineaireGhost > vDeFreinage)      robotState.vitesseLineaireGhost = vDeFreinage;
//        if (robotState.vitesseLineaireGhost > vLinMaxDynamique) robotState.vitesseLineaireGhost = vLinMaxDynamique;
//    }
//
//    // --- 6. Mise à jour finale
//    robotState.ArucoSpeedLin = robotState.vitesseLineaireGhost;
//    // robotState.ArucoSpeedAngle est mis à jour par la rampe au point 4
//}
//
//void ArUco_ProcessMessage(void) {
//    // --- 1. Filtrage des entrées (Stabilité des mesures)
//    static double fX = 0, fY = 0;
//    const double alpha = 0.2;
//    fX = (alpha * ((robotState.Z_Aruco / 1000.0) / 1.88)) + (1.0 - alpha) * fX;
//    fY = (alpha * (robotState.X_Aruco / 1000.0)) + (1.0 - alpha) * fY;
//
//    // --- 2. Paramètres de mouvement
//    const double DIST_STOP = 0.30;
//    const double vLinMax = 1; // Vitesse de croisière limitée
//    const double accLin = 0.7; // Accélération TRÈS douce pour éviter les à-coups
//    const double Tsampling = 0.01; // 10ms
//    const double SEUIL_CENTRE = 555; // Tolérance d'alignement (~10°)
//
//    // --- 3. Calculs de distance et d'angle
//    double distanceRestante = sqrt(fX * fX + fY * fY);
//    double angleCible = -atan2(fY, fX);
//    double erreurAngleAbs = fabs(angleCible);
//    double erreurDistance = distanceRestante - DIST_STOP;
//
//    // --- 4. Logique de commande linéaire
//    // On n'avance QUE si :
//    // - On est assez loin (erreurDistance > 0)
//    // - ET l'ArUco est bien au centre (erreurAngleAbs < SEUIL_CENTRE)
//    if (erreurDistance <= 0 || erreurAngleAbs > SEUIL_CENTRE) {
//        robotState.vitesseLineaireGhost = 0;
//    } else {
//        // A. Calcul de la vitesse de freinage (pour un arrêt sans choc)
//        double vDeFreinage = sqrt(2.0 * accLin * erreurDistance);
//
//        // B. Rampe d'accélération progressive
//        robotState.vitesseLineaireGhost += accLin * Tsampling;
//
//        // C. Application des limites (Accélération < Freinage < Max)
//        if (robotState.vitesseLineaireGhost > vDeFreinage) {
//            robotState.vitesseLineaireGhost = vDeFreinage;
//        }
//        if (robotState.vitesseLineaireGhost > vLinMax) {
//            robotState.vitesseLineaireGhost = vLinMax;
//        }
//    }
//    if (distanceRestante  >= DIST_STOP) {
//        robotState.ArucoSpeedLin = robotState.vitesseLineaireGhost;
//    } else {
//        robotState.ArucoSpeedLin = 0.0;
//    }
//
//    // --- 5. Envoi des consignes
//    robotState.ArucoSpeedAngle = 0.0; // Forcé à 0 comme demandé
//}


// Linéaire parfait 
//void ArUco_ProcessMessage(void) {
//    // --- 1. Filtrage des entrées (Stabilité des mesures)
//    static double fX = 0, fY = 0;
//    const double alpha = 0.2;
//    fX = (alpha * ((robotState.Z_Aruco / 1000.0) / 1.88)) + (1.0 - alpha) * fX;
//    fY = (alpha * (robotState.X_Aruco / 1000.0)) + (1.0 - alpha) * fY;
//
//    // --- 2. Paramètres de mouvement
//    const double DIST_STOP = 0.30;
//    const double vLinMax = 1; // Vitesse de croisière limitée
//    const double accLin = 0.7; // Accélération TRÈS douce pour éviter les à-coups
//    const double Tsampling = 0.01; // 10ms
//    const double SEUIL_CENTRE = 555; // Tolérance d'alignement (~10°)
//
//    // --- 3. Calculs de distance et d'angle
//    double distanceRestante = sqrt(fX * fX + fY * fY);
//    double angleCible = -atan2(fY, fX);
//    double erreurAngleAbs = fabs(angleCible);
//    double erreurDistance = distanceRestante - DIST_STOP;
//
//    // --- 4. Logique de commande linéaire
//    // On n'avance QUE si :
//    // - On est assez loin (erreurDistance > 0)
//    // - ET l'ArUco est bien au centre (erreurAngleAbs < SEUIL_CENTRE)
//    if (erreurDistance <= 0 || erreurAngleAbs > SEUIL_CENTRE) {
//        robotState.vitesseLineaireGhost = 0;
//    } else {
//        // A. Calcul de la vitesse de freinage (pour un arrêt sans choc)
//        double vDeFreinage = sqrt(2.0 * accLin * erreurDistance);
//
//        // B. Rampe d'accélération progressive
//        robotState.vitesseLineaireGhost += accLin * Tsampling;
//
//        // C. Application des limites (Accélération < Freinage < Max)
//        if (robotState.vitesseLineaireGhost > vDeFreinage) {
//            robotState.vitesseLineaireGhost = vDeFreinage;
//        }
//        if (robotState.vitesseLineaireGhost > vLinMax) {
//            robotState.vitesseLineaireGhost = vLinMax;
//        }
//    }
//    if (distanceRestante  >= DIST_STOP) {
//        robotState.ArucoSpeedLin = robotState.vitesseLineaireGhost;
//    } else {
//        robotState.ArucoSpeedLin = 0.0;
//    }
//
//    // --- 5. Envoi des consignes
//    robotState.ArucoSpeedAngle = 0.0; // Forcé à 0 comme demandé
//}





void ArUco_ProcessMessage(void) {
    // --- 1. Filtrage des entrées (Anti-bruit)
    static double fX = 0, fY = 0;
    const double alpha = 0.25; 
    fX = (alpha * ((robotState.Z_Aruco / 1000.0) / 1.88)) + (1.0 - alpha) * fX;
    fY = (alpha * (robotState.X_Aruco / 1000.0)) + (1.0 - alpha) * fY;

    // --- 2. Paramètres de stabilité (Très prudents)
    const double DIST_STOP      = 0.35;
    const double vAngMaxAbs     = 2.0;   // Vitesse de rotation max
    const double accLin         = 0.4;  // Accélération linéaire douce
    const double accAng         = 10.0  ;// Accélération angulaire douce
    const double Kp_angle       = 2.4;   
    const double Tsampling      = 0.01;  

    // --- 3. Calculs géométriques
    double distanceRestante = sqrt(fX*fX + fY*fY);
    double angleCible = -atan2(fY, fX); 
    double erreurDistance = distanceRestante - DIST_STOP;

    // --- 4. GESTION DE LA VITESSE ANGULAIRE (Prioritaire)
    double targetAng = angleCible * Kp_angle;
    if (targetAng > vAngMaxAbs)  targetAng = vAngMaxAbs;
    if (targetAng < -vAngMaxAbs) targetAng = -vAngMaxAbs;

    // Rampe d'accélération angulaire (pour ne pas choquer les moteurs)
    if (robotState.ArucoSpeedAngle < targetAng) 
        robotState.ArucoSpeedAngle += accAng * Tsampling;
    else if (robotState.ArucoSpeedAngle > targetAng)
        robotState.ArucoSpeedAngle -= accAng * Tsampling;
}