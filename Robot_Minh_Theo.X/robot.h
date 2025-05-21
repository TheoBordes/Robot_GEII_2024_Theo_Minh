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
            
            double vitesseDroitFromOdometry;
            double vitesseGaucheFromOdometry;
            double vitesseLineaireFromOdometry;
            double vitesseAngulaireFromOdometry;
            double xPosFromOdometry_1;
            double xPosFromOdometry;
            double yPosFromOdometry_1;
            double yPosFromOdometry;
            double vitesseBITEAngulaireFromOdometry;
            double angleRadianFromOdometry_1;
            double angleRadianFromOdometry;
            
            double Consigne;
            double Measure;
            double Error;
            double Command;
            double Kp;
            double CorrecP;
            double CorrecP_Max;
            double Ki;
            double CorrecI;
            double CorrecI_Max;
            double Kd;
            double CorrecD;
            double CorrecD_Max;

        };
    };
} ROBOT_STATE_BITS;
extern volatile ROBOT_STATE_BITS robotState;
void SetRobotAutoControlState(int mode);
#endif /* ROBOT_H */
