using Constants;
using EventArgsLibrary;
using MathNet.Numerics.Interpolation;
using MathNet.Spatial.Euclidean;
using System.Collections.Generic;
using System;
using System.Text;
using System.Timers;
using Utilities;
using Timer = System.Timers.Timer;
using System.Diagnostics;
using MathNet.Numerics.Distributions;
using Point3D = MathNet.Spatial.Euclidean.Point3D;
using BaballeStalkerQuiMarcheVraiment;


namespace Position
{
    public class Position
    {
        public double GetDistanceObjet(double tailleApparente, double tailleTheorique)
        {
            double largeurApparenteBallon22cmA2m = 120; //A mesurer
            double tailleTheoriqueballon = 0.22;
            double distanceObjet = largeurApparenteBallon22cmA2m / tailleApparente * 2 * (tailleTheorique / tailleTheoriqueballon);
            return distanceObjet;
        }

        static public double GetWidthObjet(double largeurObjetImage, double distance)
        {
            double largeurApparenteBallon22cmA2m = 120; //A mesurer
            double largeurObjet = 0.22 * largeurObjetImage / largeurApparenteBallon22cmA2m * distance / 2;
            return largeurObjet;
        }

        //static public MathNet.Spatial.Euclidean.Point3D? DeterminePositionAuSol(PointD ptObjetImage, CameraPosition camPos)
        //{
            //double zCamera = 0.389;
            //if (camPos ==  CameraPosition.EurobotLeft || camPos ==  CameraPosition.EurobotFront || camPos ==  CameraPosition.EurobotRight || camPos ==  CameraPosition.EurobotBack)
            //    zCamera = 0.305;
            //double angleCamera = GetCameraAngle(camPos);

            ////Alpha : angle entre l'axe optique et l'axe du ptObjetImage
            //double distancePtObjetAxeOptique = Toolbox.Distance(ptObjetImage, new PointD(0, 0));

            ////La valeur distancePixel45Horizontal est la distance au centre de l'image du point sur la ligne 45°
            ////situé sur la ligne horizontale passant par le centre
            //double distancePixel45Horizontal = 743.0;

            //double alpha = Toolbox.DegToRad(distancePtObjetAxeOptique * 45 / distancePixel45Horizontal);
            ////Theta : angle entre la verticale de l'image et la droite origine-camera vers ptObjetImage
            //var theta = -(Math.Atan2(ptObjetImage.Y, ptObjetImage.X) - Math.PI / 2);

            //// Définition du vecteur directeur de l'axe optique
            //Vector3D axeRobot = new Vector3D(1, 0, 0);

            //// Création des matrices de rotation autour de l'axe y
            //var matRotationPlongeeCamera = Matrix3D.RotationAroundYAxis(MathNet.Spatial.Units.Angle.FromRadians(angleCamera));
            //var matRotationAlpha = Matrix3D.RotationAroundYAxis(MathNet.Spatial.Units.Angle.FromRadians(-alpha));

            //// Appliquer la rotation à la ligne
            //var axeOptique = axeRobot.TransformBy(matRotationPlongeeCamera);
            //var axeAlpha = axeOptique.TransformBy(matRotationAlpha);
            //var matRotationTheta = Matrix3D.RotationAroundArbitraryVector(axeOptique.Normalize(), MathNet.Spatial.Units.Angle.FromRadians(theta));
            //var axeObjet = axeAlpha.TransformBy(matRotationTheta);

              int distanceMax = 100;

        //    double zCamera;
        //    Vector3D axeObjet;
        //    DeterminePositionObjetCameraBase(ptObjetImage, camPos, out zCamera, out axeObjet);

        //    Line3D ligneObjet = new Line3D(new MathNet.Spatial.Euclidean.Point3D(0, 0, zCamera),
        //        new MathNet.Spatial.Euclidean.Point3D(0 + distanceMax * axeObjet.X, 0 + distanceMax * axeObjet.Y, zCamera + distanceMax * axeObjet.Z));
        //    var ptOnFloor = ligneObjet.IntersectionWith(new MathNet.Spatial.Euclidean.Plane(0, 0, 1, 0)); //calcul intersection avec le sol
        //    if (ptOnFloor == null)
        //        ;
        //    return ptOnFloor;
        //}

        static public MathNet.Spatial.Euclidean.Point3D? DeterminePositionDansEspace(PointD ptObjetImage, double distance)
        {
            /// L'objectif est de déterminer l'axe du référentiel robot dans lequel se situe l'objet observé sur l'écran.
            /// Ainsi on pourra déterminer le pt d'intersection de cet axe avec le sol pour trouver le position d'un objet au sol ou
            /// déterminer l'emplacement de l'objet en fonction de sa distance si celui ci est en l'air

            double zCamera;
            Vector3D axeObjet;
            DeterminePositionObjetCameraBase(ptObjetImage, out zCamera, out axeObjet);

            var ptObjet = new MathNet.Spatial.Euclidean.Point3D(0 + distance * axeObjet.X, 0 + distance * axeObjet.Y, zCamera + distance * axeObjet.Z); // trouve le point objet

            return ptObjet;
        }

        private static void DeterminePositionObjetCameraBase(PointD ptObjetImage, out double zCamera, out Vector3D axeObjet)
        {
            /// Pour construire la direction de l'objet :
            /// On part de l'axe X du robot (en face de la face avant, plan horizontal), Y est sur le côté Gauche et Z en haut : repère direct
            /// L'axe optique est celui du robot avec une rotation d'angle angleCamera par rapport à l'axe Y
            /// Alpha est l'angle entre l'axe optique et l'axe de l'objet, il est lié à la distance apparente entre l'objet dans l'image et le centre de l'image.
            /// Theta est l'angle entre la plan vertical passant par l'axe optique et le plan (axe optique - pt objet). Dans l'image, theta est l'angle entre la verticale 
            /// passant le centre optique et la droite centre optique-pt objet.
            /// 
            /// Pour trouver l'axe de l'objet, on part de l'axe optique calculé précédemment, puis on effectue une rotation d'axe Y et d'angle Alpha 
            /// permettant de se déplacer sur le cercle de centre axe optique et passant par l'objet.
            /// Ensuite, on effectue une rotation d'axe l'axe optique et d'angle theta, le nouvel axe est l'axe de l'objet.


            //zCamera = 0.389;
            zCamera = 1.5;
            //if (camPos == CameraPosition.EurobotLeft || camPos == CameraPosition.EurobotFront || camPos == CameraPosition.EurobotRight || camPos == CameraPosition.EurobotBack)
                //zCamera = 0.305;
            //double angleCamera = GetCameraAngle(camPos); // angle de la caméra

            //Alpha : angle entre l'axe optique et l'axe du ptObjetImage
            double distancePtObjetAxeOptique = Toolbox.Distance(ptObjetImage, new PointD(0, 0)); // calcul de la distance apparente

            //La valeur distancePixel45Horizontal est la distance au centre de l'image du point sur la ligne 45°
            //situé sur la ligne horizontale passant par le centre
            double distancePixel45Horizontal = 700; //Validé sur des tests RoboCup

            double alpha = Math.Atan2(distancePtObjetAxeOptique, distancePixel45Horizontal);// Toolbox.DegToRad(distancePtObjetAxeOptique * 45 / distancePixel45Horizontal);
            //Theta : angle entre la verticale de l'image et la droite origine-camera vers ptObjetImage
            //var theta = -(Math.Atan2(ptObjetImage.X, ptObjetImage.Y) - Math.PI / 2);
            var theta = -(Math.Atan2(ptObjetImage.X, ptObjetImage.Y));


            // Définition du vecteur directeur de l'axe optique
            Vector3D axeRobot = new Vector3D(1, 0, 0);

            // Création des matrices de rotation autour de l'axe y
            var matRotationPlongeeCamera = Matrix3D.RotationAroundYAxis(MathNet.Spatial.Units.Angle.FromRadians(0)); // Caméra horizontale
            var matRotationAlpha = Matrix3D.RotationAroundYAxis(MathNet.Spatial.Units.Angle.FromRadians(-alpha));

            // Appliquer la rotation à la ligne
            var axeOptique = axeRobot.TransformBy(matRotationPlongeeCamera);
            var axeAlpha = axeOptique.TransformBy(matRotationAlpha);
            var matRotationTheta = Matrix3D.RotationAroundArbitraryVector(axeOptique.Normalize(), MathNet.Spatial.Units.Angle.FromRadians(theta));
            axeObjet = axeAlpha.TransformBy(matRotationTheta);

            //Debug.WriteLine("a: " + (alpha/ Math.PI)*180);
            //Debug.WriteLine("t: " + (theta / Math.PI) * 180);
            //Debug.WriteLine("");
            //Debug.WriteLine("V: " + axeObjet);
            //Debug.WriteLine("");
        }

        //public static double GetCameraAngle(CameraPosition camPos)
        //{
        //double angleCamera = 0;
        //if (camPos == CameraPosition.RoboCupLeft || camPos == CameraPosition.RoboCupRight || camPos == CameraPosition.RoboCupBack || camPos == CameraPosition.RoboCupFront)
        //    angleCamera = Toolbox.DegToRad(8);
        //else if (camPos == CameraPosition.EurobotFront || camPos == CameraPosition.EurobotLeft || camPos == CameraPosition.EurobotRight || camPos == CameraPosition.EurobotBack)
        //    angleCamera = Toolbox.DegToRad(30);
        //else
        //    Console.WriteLine("CameraJeVoisProAdapter : camera de type inconnu");
        //return angleCamera;
        //return 0;
        //}
        public void ProcessJeVoisData(int x, int y, int rayon)
        {
            double _xCameraRefRobot, _yCameraRefRobot, _thetaCameraRefRobot;
            /*
            Pour robocupfront
            xcameraRefRobot = 0.161;
            ycameraRefRobot = -0.011;
            thetaCameraRefRobot = 0;
            */
            int largeurImagePixel = 1920;
            int hauteurImagePixel = 1080;

            _xCameraRefRobot = 0;
            _yCameraRefRobot = 0;
            _thetaCameraRefRobot = 0;

            double? distanceObjet = null;
            distanceObjet = GetDistanceObjet(rayon*2, 0.22);

            //Debug.WriteLine("Dist: " + distanceObjet);
            //Debug.WriteLine("");

            MathNet.Spatial.Euclidean.Point3D? posObjet = null;
            posObjet = DeterminePositionDansEspace(new PointD(x, y), (double)distanceObjet);

            Debug.WriteLine("Objet: " + posObjet);
            Debug.WriteLine("");

            if (posObjet != null)
            {
                distanceObjet = Math.Sqrt(Math.Pow(posObjet.Value.X, 2) + Math.Pow(posObjet.Value.Y, 2) + Math.Pow(posObjet.Value.Z, 2));
                double widthObject = GetWidthObjet(largeurImagePixel, (double)distanceObjet);
                double heightObject = GetWidthObjet(hauteurImagePixel, (double)distanceObjet);

                ///On fait bouger les éléments détectés en fonction de la position de la caméra
                var ptRotated = Toolbox.Rotate(new PointD(posObjet.Value.X, posObjet.Value.Y), new PointD(0, 0), _thetaCameraRefRobot);
                var ptRefRobot = new PointD(ptRotated.X + _xCameraRefRobot, ptRotated.Y + _yCameraRefRobot);

                double distanceRefRobot = Math.Sqrt(Math.Pow(ptRefRobot.X, 2) + Math.Pow(ptRefRobot.Y, 2));
                double angleRefRobot = Math.Atan2(ptRefRobot.Y, ptRefRobot.X);

                double xRefRobot = ptRefRobot.X;
                double yRefRobot = ptRefRobot.Y;
                lababalle.posObj = posObjet;
            }

        }      
    }

}
