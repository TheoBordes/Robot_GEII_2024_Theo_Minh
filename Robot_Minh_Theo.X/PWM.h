#define MOTEUR_DROIT 0
#define MOTEUR_GAUCHE 1

void InitPWM(void);
void PWMUpdateSpeed();
void PWMSetSpeedConsignePercent(float vitesseEnPourcents, char moteur);
void PWMSetSpeedConsignePolaire();
//void PWMSetSpeed(float vitesseEnPourcents, int moteur);
