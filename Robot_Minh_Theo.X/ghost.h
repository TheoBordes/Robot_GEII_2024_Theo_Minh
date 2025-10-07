/* 
 * File:   ghost.h
 * Author: E306_PC1
 *
 * Created on 24 septembre 2025, 10:34
 */

#ifndef GHOST_H
#define	GHOST_H

#define Idle 0
#define Rotation 1
#define DeplacementLineaire 2

typedef struct {
    double x;
    double y;
} Point;


void UpdateRotation();
double ModuloByAngle(double angle);
double NormalizeAngle(double angle);
double AngleVersCible(Point robot, Point target);
void UpdateDeplacementGhost();




#endif	/* GHOST_H */

