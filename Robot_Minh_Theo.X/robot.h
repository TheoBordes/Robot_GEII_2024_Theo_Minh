#ifndef ROBOT_H
#define ROBOT_H

typedef struct robotStateBITS {
    union {
        struct {
            unsigned char taskEnCours;
            float vitesseGaucheConsigne;
            float vitesseGaucheCommandeCourante;
            float vitesseDroiteCommandeCourante;
            float vitesseDroiteConsigne;
            float distanceTelemetrePlusGauche;
            float distanceTelemetreGauche;
            float distanceTelemetreCentre;
            float distanceTelemetreDroit;
            float distanceTelemetrePlusDroit;
            unsigned int mode;
        };
    };
} ROBOT_STATE_BITS;
extern volatile ROBOT_STATE_BITS robotState;
void SetRobotAutoControlState(int mode);
#endif /* ROBOT_H */
