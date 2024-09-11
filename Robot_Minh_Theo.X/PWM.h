#define MOTEUR_DROIT 0
#define MOTEUR_GAUCHE 1
void InitPWM(void);
//void InitPWM2(void);
void PWMSetSpeed(float vitesseEnPourcents, int moteur);
