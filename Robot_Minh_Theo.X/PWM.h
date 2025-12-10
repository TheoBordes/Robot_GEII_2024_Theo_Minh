#define MOTEUR_DROIT 0
#define MOTEUR_GAUCHE 1

void InitPWM(void);
void PWMUpdateSpeed();
void PWMSetSpeedConsigne(float vitesse, char moteur);
void PWMSetSpeedConsignePolaire();
void PWMSetSpeedConsigneIndependant( double vitesse, unsigned char moteur );
//void PWMSetSpeed(float vitesseEnPourcents, int moteur);
