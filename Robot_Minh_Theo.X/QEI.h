/* 
 * File:   QEI.h
 * Author: E306_PC1
 *
 * Created on 6 janvier 2025, 14:26
 */
#include "robot.h"





#ifndef QEI_H
#define	QEI_H

typedef struct {
    float vitesseDroitFromOdometry;
    float vitesseGaucheFromOdometry;
    float vitesseLineaireFromOdometry;
    float vitesseAngulaireFromOdometry;

    float xPosFromOdometry_1;
    float xPosFromOdometry;


    float yPosFromOdometry_1;
    float yPosFromOdometry;
    float vitesseBITEAngulaireFromOdometry;
    float angleRadianFromOdometry_1;
    float angleRadianFromOdometry;


} robotState;

void InitQEI1();
void InitQEI2();
void QEIUpdateData();

#endif	/* QEI_H */

