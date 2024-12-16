using MathNet.Numerics.LinearAlgebra;
using System.Drawing;
using System.Numerics;

namespace Utilities
{
    /// <summary>
    /// Contient plusieurs fonctions mathématiques utiles.
    /// </summary>
    public static class Toolbox
    {

        public static EventWaitHandle waitHandle = new EventWaitHandle(true, EventResetMode.AutoReset, "SHARED_BY_ALL_PROCESSES");

        /// <summary>
        /// Renvoie la valeur max d'une liste de valeurs
        /// </summary>
        public static double Max(params double[] values)
            => values.Max();

        /// <summary>Converti un angle en degrés en un angle en radians.</summary>
        public static float DegToRad(float angleDeg)
            => angleDeg * (float)Math.PI / 180f;

        /// <summary>Converti un angle en degrés en un angle en radians.</summary>
        public static double DegToRad(double angleDeg)
            => angleDeg * Math.PI / 180;

        /// <summary>Converti un angle en degrés en un angle en radians.</summary>
        public static float RadToDeg(float angleRad)
            => angleRad / (float)Math.PI * 180f;

        /// <summary>Converti un angle en radians en un angle en degrés.</summary>
        public static double RadToDeg(double angleRad)
            => angleRad / Math.PI * 180;

        /// <summary>Renvoie l'angle modulo 2*pi entre -pi et pi.</summary>
        public static double Modulo2Pi(double angleRad)
        {
            double angleTemp = (angleRad - Math.PI) % (2 * Math.PI) + Math.PI;
            return (angleTemp + Math.PI) % (2 * Math.PI) - Math.PI;
        }

        /// <summary>Renvoie l'angle modulo pi entre -pi/2 et pi/2.</summary>
        public static double ModuloPi(double angleRad)
        {
            double angleTemp = (angleRad - Math.PI / 2.0) % Math.PI + Math.PI / 2.0;
            return (angleTemp + Math.PI / 2.0) % Math.PI - Math.PI / 2.0;
        }


        /// <summary>Renvoie l'angle modulo pi entre -pi et pi.</summary>
        public static double ModuloPiDivTwo(double angleRad)
        {
            double angleTemp = (angleRad - Math.PI / 4.0) % (Math.PI / 2) + Math.PI / 4.0;
            return (angleTemp + Math.PI / 4.0) % (Math.PI / 2) - Math.PI / 4.0;
        }

        /// <summary>Borne la valeur entre les deux valeurs limites données.</summary>
        public static double LimitToInterval(double value, double lowLimit, double highLimit)
        {
            if (value > highLimit)
                return highLimit;
            else if (value < lowLimit)
                return lowLimit;
            else
                return value;
        }
        public static double LinearInInterval(double value, double borneBasse, double borneHaute, double valeurBorneBasse, double valeurBorneHaute)
        {
            if (valeurBorneHaute >= valeurBorneBasse)
                return Toolbox.LimitToInterval((value - borneBasse) / (borneHaute - borneBasse) * (valeurBorneHaute - valeurBorneBasse) + valeurBorneBasse, valeurBorneBasse, valeurBorneHaute);
        
        else
                return Toolbox.LimitToInterval((value - borneBasse) / (borneHaute - borneBasse) * (valeurBorneHaute - valeurBorneBasse) + valeurBorneBasse, valeurBorneHaute, valeurBorneBasse);
        }

        /// <summary>Décale un angle dans un intervale de [-PI, PI] autour d'un autre.</summary>
        public static double Modulo2PiAroundAngle(double angleToCenterAround, double angleToCorrect)
        {
            // On corrige l'angle obtenu pour le moduloter autour de l'angle Kalman
            int decalageNbTours = (int)Math.Round((angleToCorrect - angleToCenterAround) / (2 * Math.PI));
            double thetaDest = angleToCorrect - decalageNbTours * 2 * Math.PI;

            return thetaDest;
        }


        /// <summary>Décale un angle dans un intervale de [-PI/4, PI/4] autour d'un autre.</summary>
        public static double ModuloPiDiv2AroundAngle(double angleToCenterAround, double angleToCorrect)
        {
            // On corrige l'angle obtenu pour le moduloter autour de l'angle Kalman
            int decalageNbTours = (int)Math.Round((angleToCorrect - angleToCenterAround) / (Math.PI / 2));
            double thetaDest = angleToCorrect - decalageNbTours * Math.PI / 2;

            return thetaDest;
        }


        /// <summary>Décale un angle dans un intervale de [-PI/2, PI/2] autour d'un autre.</summary>
        public static double ModuloPiAroundAngle(double angleToCenterAround, double angleToCorrect)
        {
            // On corrige l'angle obtenu pour le moduloter autour de l'angle Kalman
            int decalageNbTours = (int)Math.Round((angleToCorrect - angleToCenterAround) / (Math.PI));
            double thetaDest = angleToCorrect - decalageNbTours * Math.PI;

            return thetaDest;
        }

        public static double Distance(PointD pt1, PointD pt2)
        {
            if (pt1 != null && pt2 != null)
                return Math.Sqrt((pt2.X - pt1.X) * (pt2.X - pt1.X) + (pt2.Y - pt1.Y) * (pt2.Y - pt1.Y));
            else
                return double.PositiveInfinity;
            //return Math.Sqrt(Math.Pow(pt2.X - pt1.X, 2) + Math.Pow(pt2.Y - pt1.Y, 2));
        }

        public static double Distance(Point3D pt1, Point3D pt2)
        {
            if (pt1 != null && pt2 != null)
                return Math.Sqrt((pt2.X - pt1.X) * (pt2.X - pt1.X) + (pt2.Y - pt1.Y) * (pt2.Y - pt1.Y) + (pt2.Z - pt1.Z) * (pt2.Z - pt1.Z));
            else
                return double.PositiveInfinity;
        }

        public static double Distance(PointF pt1, PointF pt2)
        {
            if (pt1 != null && pt2 != null)
                return Math.Sqrt((pt2.X - pt1.X) * (pt2.X - pt1.X) + (pt2.Y - pt1.Y) * (pt2.Y - pt1.Y));
            else
                return double.PositiveInfinity;
        }

        public static int SquareDistance(Point pt1, Point pt2)
        {
            if (pt1 != null && pt2 != null)
                return (pt2.X - pt1.X) * (pt2.X - pt1.X) + (pt2.Y - pt1.Y) * (pt2.Y - pt1.Y);
            else
                return int.MaxValue;
        }

        public static double Distance(Location pt1, Location pt2)
        {
            if (pt1 != null && pt2 != null)
                return Math.Sqrt((pt2.X - pt1.X) * (pt2.X - pt1.X) + (pt2.Y - pt1.Y) * (pt2.Y - pt1.Y));
            else
                return double.PositiveInfinity;
            //return Math.Sqrt(Math.Pow(pt2.X - pt1.X, 2) + Math.Pow(pt2.Y - pt1.Y, 2));
        }

        public static double Distance(PolarPointRssi pt1, PolarPointRssi pt2)
        {
            return Math.Sqrt(pt1.Distance * pt1.Distance + pt2.Distance * pt2.Distance - 2 * pt1.Distance * pt2.Distance * Math.Cos(pt1.Angle - pt2.Angle));
        }

        public static double DistanceL1(PointD pt1, PointD pt2)
        {
            return Math.Abs(pt2.X - pt1.X) + Math.Abs(pt2.Y - pt1.Y);
            //return Math.Sqrt(Math.Pow(pt2.X - pt1.X, 2) + Math.Pow(pt2.Y - pt1.Y, 2));
        }

        public static double Distance(double xPt1, double yPt1, double xPt2, double yPt2)
        {
            return Math.Sqrt(Math.Pow(xPt2 - xPt1, 2) + Math.Pow(yPt2 - yPt1, 2));
        }

        public static double DistancePointToLine(PointD pt, PointD LinePt, double LineAngle)
        {
            var xLineVect = Math.Cos(LineAngle);
            var yLineVect = Math.Sin(LineAngle);
            var dot = (pt.X - LinePt.X) * (yLineVect) - (pt.Y - LinePt.Y) * (xLineVect);
            return Math.Abs(dot);
        }


        public static double DistancePointToLine(PointD pt, PointD ptSeg1, PointD ptSeg2)
        {
            var A = pt.X - ptSeg1.X;
            var B = pt.Y - ptSeg1.Y;
            var C = ptSeg2.X - ptSeg1.X;
            var D = ptSeg2.Y - ptSeg1.Y;

            double dot = A * C + B * D;
            double len_sq = C * C + D * D;
            double param = -1;
            if (len_sq != 0) //in case of 0 length line
                param = dot / len_sq;

            double xx, yy;

            //if (param < 0)
            //{
            //    xx = ptSeg1.X;
            //    yy = ptSeg1.Y;
            //}
            //else if (param > 1)
            //{
            //    xx = ptSeg2.X;
            //    yy = ptSeg2.Y;
            //}
            //else
            //{
                xx = ptSeg1.X + param * C;
                yy = ptSeg1.Y + param * D;
            //}

            var dx = pt.X - xx;
            var dy = pt.Y - yy;

            double distance = Math.Sqrt(dx * dx + dy * dy);
            return distance;
        }

        public static double DistancePointToSegment(PointD pt, PointD ptSeg1, PointD ptSeg2)
        {
            var A = pt.X - ptSeg1.X;
            var B = pt.Y - ptSeg1.Y;
            var C = ptSeg2.X - ptSeg1.X;
            var D = ptSeg2.Y - ptSeg1.Y;

            double dot = A * C + B * D;
            double len_sq = C * C + D * D;
            double param = -1;
            if (len_sq != 0) //in case of 0 length line
                param = dot / len_sq;

            double xx, yy;

            if (param < 0)
            {
                xx = ptSeg1.X;
                yy = ptSeg1.Y;
            }
            else if (param > 1)
            {
                xx = ptSeg2.X;
                yy = ptSeg2.Y;
            }
            else
            {
                xx = ptSeg1.X + param * C;
                yy = ptSeg1.Y + param * D;
            }

            var dx = pt.X - xx;
            var dy = pt.Y - yy;

            double distance = Math.Sqrt(dx * dx + dy * dy);
            return distance;            
        }


        public static PointD FindClosestPointOnSegment(PointD pt, PointD ptSeg1, PointD ptSeg2)
        {
            var A = pt.X - ptSeg1.X;
            var B = pt.Y - ptSeg1.Y;
            var C = ptSeg2.X - ptSeg1.X;
            var D = ptSeg2.Y - ptSeg1.Y;

            double dot = A * C + B * D;
            double len_sq = C * C + D * D;
            double param = -1;
            if (len_sq != 0) //in case of 0 length line
                param = dot / len_sq;

            double xx, yy;

            if (param < 0)
            {
                xx = ptSeg1.X;
                yy = ptSeg1.Y;
            }
            else if (param > 1)
            {
                xx = ptSeg2.X;
                yy = ptSeg2.Y;
            }
            else
            {
                xx = ptSeg1.X + param * C;
                yy = ptSeg1.Y + param * D;
            }

            var dx = pt.X - xx;
            var dy = pt.Y - yy;

            return new PointD(xx, yy);
        }


        public static (PointD, bool) ProjectionPointOnSegment(PointD pt, PointD segmentPt1, PointD segmentPt2)
        {
            var A = pt.X - segmentPt1.X;
            var B = pt.Y - segmentPt1.Y;
            var C = segmentPt2.X - segmentPt1.X;
            var D = segmentPt2.Y - segmentPt1.Y;

            double dot = A * C + B * D;
            double len_sq = C * C + D * D;
            double param = -1;
            if (len_sq != 0) //in case of 0 length line
                param = dot / len_sq;

            double xx, yy;

            bool isInsideSegment = false;

            xx = segmentPt1.X + param * C;
            yy = segmentPt1.Y + param * D;
            
            if(param<=1 && param >=0)
            {
                isInsideSegment = true;
            }

            return (new PointD(xx, yy), isInsideSegment); 

        }

        public static (PointD, bool) ProjectionPointOnSegment(PointD pt, SegmentD segment)
        {
            return ProjectionPointOnSegment(pt, segment.PtDebut, segment.PtFin);
        }

            public static double DistancePointToSegment(PointD pt, SegmentD segment)
        {
            var ptSeg1 = segment.PtDebut;
            var ptSeg2 = segment.PtFin;

            return DistancePointToSegment(pt, ptSeg1, ptSeg2);
        }

        public static PointD GetInterceptionLocation(LocationExtended target, Location hunter, double huntingSpeed)
        {
            //D'après Al-Kashi, si d est la distance entre le pt target et le pt chasseur, que les vitesses sont constantes 
            //et égales à Vtarget et Vhunter
            //Rappel Al Kashi : A² = B²+C²-2BCcos(alpha) , alpha angle opposé au segment A
            //On a au moment de l'interception à l'instant Tinter: 
            //A = Vh * Tinter
            //B = VT * Tinter
            //C = initialDistance;
            //alpha = Pi - capCible - angleCible

            double targetSpeed = Math.Sqrt(Math.Pow(target.Vx, 2) + Math.Pow(target.Vy, 2));
            double initialDistance = Toolbox.Distance(new PointD(hunter.X, hunter.Y), new PointD(target.X, target.Y));
            double capCible = Math.Atan2(target.Vy, target.Vx);
            double angleCible = Math.Atan2(target.Y - hunter.Y, target.X - hunter.X);
            double angleCapCibleDirectionCibleChasseur = Math.PI - capCible + angleCible;

            //Résolution de ax²+bx+c=0 pour trouver Tinter
            double a = Math.Pow(huntingSpeed, 2) - Math.Pow(targetSpeed, 2);
            double b = 2 * initialDistance * targetSpeed * Math.Cos(angleCapCibleDirectionCibleChasseur);
            double c = -Math.Pow(initialDistance, 2);

            double delta = b * b - 4 * a * c;
            double t1 = (-b - Math.Sqrt(delta)) / (2 * a);
            double t2 = (-b + Math.Sqrt(delta)) / (2 * a);

            
            if (delta > 0)
            {
                PointD ptInterT1 = new PointD(target.X + targetSpeed * Math.Cos(capCible) * t1, target.Y + targetSpeed * Math.Sin(capCible) * t1);
                PointD ptInterT2 = new PointD(target.X + targetSpeed * Math.Cos(capCible) * t2, target.Y + targetSpeed * Math.Sin(capCible) * t2);

                var segHunterTarget = new SegmentD(hunter.ToPointD(), target.ToPointD());
                var segHunterPtInter1 = new SegmentD(hunter.ToPointD(), ptInterT1);
                var segHunterPtInter2 = new SegmentD(hunter.ToPointD(), ptInterT2);

                double angleDeplacementPtInter1 = Toolbox.Angle(segHunterTarget, segHunterPtInter1);
                double angleDeplacementPtInter2 = Toolbox.Angle(segHunterTarget, segHunterPtInter2);

                //if (Math.Abs(angleDeplacementPtInter1) < Math.Abs(angleDeplacementPtInter2))
                //{
                //    return ptInterT1;
                //}
                //else
                {
                    return ptInterT2;
                }
            }
            else
                return null;
        }


        //public static (PointD, double) GetInterceptionLocationOrthogonale(LocationExtended target, Location hunter, double huntingSpeed, double huntingDelay)
        //{

        //    //target = new LocationExtended(0, 0, 0, 1, 0, 0, Constants.ObjectType.Balle);
        //    //hunter = new Location(3, 1.8, 0);
        //    //huntingSpeed = 0.50;
        //    /// On va calculer les positions de la balle au cours du temps considérant que la vitesse balle est constante
        //    /// ce qui permet de calculer les distances balle - pos prévue de la balle
        //    /// et hunter - pos prevue du hunter
        //    /// quand ces deux positions sont dans un ratio K = VH/(VB*COeff Marge)
        //    /// On a le pt de chasse
        //    double coeffSecurite = 1.2;
        //    var balleSpeed = Math.Sqrt(target.Vx * target.Vx + target.Vy * target.Vy);
        //    //double ratioSouhaite = huntingSpeed / balleSpeed / coeffSecurite;

        //    double ratio_T1 = 0;

        //    PointD posTargetPrevue = target.ToPointD();

        //    var distanceTargetHunter = Toolbox.Distance(target.ToPointD(), hunter.ToPointD());

        //    double t = 0;
        //    bool interceptionPointFound = false;
        //    for (t=0.05; t<10; t+=0.05)
        //    {
        //        posTargetPrevue = new PointD(target.X + t * target.Vx, target.Y + t*target.Vy);
        //        var distanceTargetPosTargetPrevue = Toolbox.Distance(target.ToPointD(), posTargetPrevue);
        //        var distanceHunterPosTargetPrevue = Toolbox.Distance(hunter.ToPointD(), posTargetPrevue);
        //        var distanceHunterParcourue = t * huntingSpeed; 
        //            //Toolbox.Distance(hunter.ToPointD(), posTargetPrevue);
        //        //var tempsTargetPosTargetPrevue = distanceTargetPosTargetPrevue / balleSpeed;
        //        //var tempsHunterPosTargetPrevue = distanceHunterPosTargetPrevue / huntingSpeed;

        //        double ratio = distanceTargetPosTargetPrevue / distanceHunterPosTargetPrevue;  // le ratio doit augmenter... sinon on a dépassé le point limite

        //        if(ratio > coeffSecurite)
        //        {
        //            /// On arrête, on a dépassé la position optimale
        //            interceptionPointFound = true;
        //            break;
        //        }
        //        ratio_T1 = ratio;                
        //    }

        //    if (interceptionPointFound)
        //        return (posTargetPrevue, t);
        //    else if (Toolbox.Distance(posTargetPrevue, target.ToPointD()) > huntingSpeed * t)
        //    {
        //        ///Cible impossible
        //        return (null, double.PositiveInfinity);
        //    }
        //    else
        //    {
        //        return (posTargetPrevue, Toolbox.Distance(posTargetPrevue, hunter.ToPointD()) / huntingSpeed);
        //    }
        //        ;
        //    //    return (target, Double.PositiveInfinity);

        //}

        public static (PointD, double) GetInterceptionLocationOrthogonale(LocationExtended target, Location hunter, double huntingSpeed, double huntingDelay)
        {

            /// On va calculer les positions de la balle au cours du temps considérant que la vitesse balle est constante
            /// ce qui permet de calculer les distances balle - pos prévue de la balle
            /// et hunter - pos prevue du hunter
            /// quand ces deux positions sont dans un ratio K = VH/(VB*COeff Marge)
            /// On a le pt de chasse
            double coeffSecurite = 3;
            var balleSpeed = Math.Sqrt(target.Vx * target.Vx + target.Vy * target.Vy);
            double ratioSouhaite = huntingSpeed / balleSpeed / coeffSecurite;

            double ratio_T1 = 0;
            bool isInit = false;

            PointD posTargetPrevue = target.ToPointD();

            double t = 0;
            for (t = 0; t < 3; t += 0.02)
            {
                posTargetPrevue = new PointD(target.X + t * target.Vx, target.Y + t * target.Vy);
                var distanceTargetPosTargetPrevue = Toolbox.Distance(target.ToPointD(), posTargetPrevue);
                var distanceHunterPosTargetPrevue = Toolbox.Distance(hunter.ToPointD(), posTargetPrevue);
                double ratio = distanceTargetPosTargetPrevue / distanceHunterPosTargetPrevue;  // le ratio doit augmenter... sinon on a dépassé le point limite
                if (ratio < ratio_T1)
                {
                    /// On arrête, on a dépassé la position optimale
                    break;
                }
                ratio_T1 = ratio;
            }
            return (posTargetPrevue, t);
        }

        public static PointD OffsetLocation(PointD P, Location PtRef, bool invertOffset = false)
        {
            try
            {
                if (invertOffset == false)
                {
                    var VectRef0x = P.X - PtRef.X;
                    var VectRef0y = P.Y - PtRef.Y;
                    var VectRefPtx = Math.Cos(-PtRef.Theta) * VectRef0x - Math.Sin(-PtRef.Theta) * VectRef0y;
                    var VectRefPty = Math.Sin(-PtRef.Theta) * VectRef0x + Math.Cos(-PtRef.Theta) * VectRef0y;
                    return new PointD(VectRefPtx, VectRefPty);
                }
                else
                {
                    var VectRefPtx = P.X;
                    var VectRefPty = P.Y;
                    var VectRef0x = Math.Cos(+PtRef.Theta) * VectRefPtx - Math.Sin(+PtRef.Theta) * VectRefPty;
                    var VectRef0y = Math.Sin(+PtRef.Theta) * VectRefPtx + Math.Cos(+PtRef.Theta) * VectRefPty;
                    return new PointD(VectRef0x + PtRef.X, VectRef0y + PtRef.Y);
                }
            }
            catch
            {
                Console.WriteLine("Exception Toolbox : OffsetLocation");
                return P;
            }
        }

        //public static SegmentD OffsetSegment(SegmentD S, Location PtRef, bool invertOffset = false)
        //{
        //    if (invertOffset == false)
        //    {
        //        var PtDebutRotated = new PointD(
        //            Math.Cos(+PtRef.Theta) * (S.PtDebut.X - PtRef.X) - Math.Sin(+PtRef.Theta) * (S.PtDebut.Y - PtRef.Y),
        //            Math.Sin(+PtRef.Theta) * (S.PtDebut.X - PtRef.X) + Math.Cos(+PtRef.Theta) * (S.PtDebut.Y - PtRef.Y));
        //        var PtFinRotated = new PointD(
        //            Math.Cos(+PtRef.Theta) * (S.PtFin.X - PtRef.X) - Math.Sin(+PtRef.Theta) * (S.PtFin.Y - PtRef.X),
        //            Math.Sin(+PtRef.Theta) * (S.PtFin.X - PtRef.X) + Math.Cos(+PtRef.Theta) * (S.PtFin.Y - PtRef.Y));
        //        return new SegmentD(PtDebutRotated, PtFinRotated);
        //    }
        //    else
        //    {
        //        var PtDebutRotated = new PointD(
        //            Math.Cos(+PtRef.Theta) * S.PtDebut.X - Math.Sin(+PtRef.Theta) * S.PtDebut.Y + PtRef.X,
        //            Math.Sin(+PtRef.Theta) * S.PtDebut.X + Math.Cos(+PtRef.Theta) * S.PtDebut.Y + PtRef.Y);
        //        var PtFinRotated = new PointD(
        //            Math.Cos(+PtRef.Theta) * S.PtFin.X - Math.Sin(+PtRef.Theta) * S.PtFin.Y + PtRef.X,
        //            Math.Sin(+PtRef.Theta) * S.PtFin.X + Math.Cos(+PtRef.Theta) * S.PtFin.Y + PtRef.Y);                
        //        return new SegmentD(PtDebutRotated, PtFinRotated);
        //    }
        //}
        public static SlamSegment OffsetSegment(SlamSegment S, Location PtRef, bool invertOffset = false)
        {
            if (invertOffset == false)
            {
                var PtDebutRotated = new PointD(
                    Math.Cos(-PtRef.Theta) * (S.PtDebut.X - PtRef.X) - Math.Sin(-PtRef.Theta) * (S.PtDebut.Y - PtRef.Y),
                    Math.Sin(-PtRef.Theta) * (S.PtDebut.X - PtRef.X) + Math.Cos(-PtRef.Theta) * (S.PtDebut.Y - PtRef.Y));
                var PtFinRotated = new PointD(
                    Math.Cos(-PtRef.Theta) * (S.PtFin.X - PtRef.X) - Math.Sin(-PtRef.Theta) * (S.PtFin.Y - PtRef.X),
                    Math.Sin(-PtRef.Theta) * (S.PtFin.X - PtRef.X) + Math.Cos(-PtRef.Theta) * (S.PtFin.Y - PtRef.Y));
                return new SlamSegment(PtDebutRotated, PtFinRotated);
            }
            else
            {
                var PtDebutRotated = new PointD(
                    Math.Cos(+PtRef.Theta) * S.PtDebut.X - Math.Sin(+PtRef.Theta) * S.PtDebut.Y + PtRef.X,
                    Math.Sin(+PtRef.Theta) * S.PtDebut.X + Math.Cos(+PtRef.Theta) * S.PtDebut.Y + PtRef.Y);
                var PtFinRotated = new PointD(
                    Math.Cos(+PtRef.Theta) * S.PtFin.X - Math.Sin(+PtRef.Theta) * S.PtFin.Y + PtRef.X,
                    Math.Sin(+PtRef.Theta) * S.PtFin.X + Math.Cos(+PtRef.Theta) * S.PtFin.Y + PtRef.Y);
                return new SlamSegment(PtDebutRotated, PtFinRotated);
            }
        }


        public static SlamSegment Rotate(SlamSegment S, PointD Centre, double Angle)
        {
            var PtDebutRotated = new PointD(
                Math.Cos(Angle) * (S.PtDebut.X - Centre.X) - Math.Sin(Angle) * (S.PtDebut.Y - Centre.Y) + Centre.X,
                Math.Sin(Angle) * (S.PtDebut.X - Centre.X) + Math.Cos(Angle) * (S.PtDebut.Y - Centre.Y) + Centre.Y);

            var PtFinRotated = new PointD(
                Math.Cos(Angle) * (S.PtFin.X - Centre.X) - Math.Sin(Angle) * (S.PtFin.Y - Centre.X) + Centre.X,
                Math.Sin(Angle) * (S.PtFin.X - Centre.X) + Math.Cos(Angle) * (S.PtFin.Y - Centre.Y) + Centre.Y);
            return new SlamSegment(PtDebutRotated, PtFinRotated);
        }


        public static PointD Rotate(PointD pt, PointD Centre, double Angle)
        {
           return new PointD(
                Math.Cos(Angle) * (pt.X - Centre.X) - Math.Sin(Angle) * (pt.Y - Centre.Y) + Centre.X,
                Math.Sin(Angle) * (pt.X - Centre.X) + Math.Cos(Angle) * (pt.Y - Centre.Y) + Centre.Y);
        }




        public static SlamSegment TranslateSegment(SlamSegment S, Vector2 v)
        {
            var PtDebutRotated = new PointD(
                S.PtDebut.X + v.X,
                S.PtDebut.Y + v.Y);

            var PtFinRotated = new PointD(
                S.PtFin.X + v.X,
                S.PtFin.Y + v.Y);
            return new SlamSegment(PtDebutRotated, PtFinRotated);
        }

        public static Location OffsetLocation(Location l, Location offset)
        {
            var angle = l.Theta - offset.Theta;
            var vangle = l.Vtheta - offset.Vtheta;
            var xTranslate = l.X - offset.X;
            var yTranslate = l.Y - offset.Y;
            var xOffset = Math.Cos(-offset.Theta) * xTranslate - Math.Sin(-offset.Theta) * yTranslate;
            var yOffset = Math.Sin(-offset.Theta) * xTranslate + Math.Cos(-offset.Theta) * yTranslate;
            var vxTranslate = l.Vx - offset.Vx;
            var vyTranslate = l.Vy - offset.Vy;
            var vxOffset = Math.Cos(-offset.Theta) * vxTranslate - Math.Sin(-offset.Theta) * vyTranslate;
            var vyOffset = Math.Sin(-offset.Theta) * vxTranslate + Math.Cos(-offset.Theta) * vyTranslate;
            return new Location(xOffset, yOffset, angle, vxOffset, vyOffset, 0);
        }

        public static LocationExtended OffsetLocation(LocationExtended l, Location offset)
        {
            try
            {
                var angle = l.Theta - offset.Theta;
                var vangle = l.Vtheta - offset.Vtheta;
                var xTranslate = l.X - offset.X;
                var yTranslate = l.Y - offset.Y;
                var xOffset = Math.Cos(-offset.Theta) * xTranslate - Math.Sin(-offset.Theta) * yTranslate;
                var yOffset = Math.Sin(-offset.Theta) * xTranslate + Math.Cos(-offset.Theta) * yTranslate;
                var vxTranslate = l.Vx - offset.Vx;
                var vyTranslate = l.Vy - offset.Vy;
                var vxOffset = Math.Cos(offset.Theta) * vxTranslate - Math.Sin(offset.Theta) * vyTranslate;
                var vyOffset = Math.Sin(offset.Theta) * vxTranslate + Math.Cos(offset.Theta) * vyTranslate;
                return new LocationExtended(xOffset, yOffset, l.Z, angle, vxOffset, vyOffset, 0, l.Type, l.Width, l.Height);
            }
            catch 
            {
                Console.WriteLine("Exception Utilities : OffsetLocation"); 
                return null; 
            }
        }


        public static bool Intersect(SegmentD line1, SegmentD line2, out PointD intersectPoint)
        {
            intersectPoint = null;

            PointD x = new PointD(line2.PtDebut.X - line1.PtDebut.X, line2.PtDebut.Y - line1.PtDebut.Y);
            PointD d1 = new PointD(line1.PtFin.X - line1.PtDebut.X, line1.PtFin.Y - line1.PtDebut.Y);
            PointD d2 = new PointD(line2.PtFin.X - line2.PtDebut.X, line2.PtFin.Y - line2.PtDebut.Y);

            double det = d1.X * d2.Y - d1.Y * d2.X;
            if (det == 0)
                return false;

            double t1 = (x.X * d2.Y - x.Y * d2.X) / det;
            intersectPoint = new PointD(line1.PtDebut.X + d1.X * t1, line1.PtDebut.Y + d1.Y * t1);

            if (intersectPoint.X >= Math.Min(line1.PtDebut.X, line1.PtFin.X) && intersectPoint.X <= Math.Max(line1.PtDebut.X, line1.PtFin.X) &&
               intersectPoint.X >= Math.Min(line2.PtDebut.X, line2.PtFin.X) && intersectPoint.X <= Math.Max(line2.PtDebut.X, line2.PtFin.X) &&
               intersectPoint.Y >= Math.Min(line1.PtDebut.Y, line1.PtFin.Y) && intersectPoint.Y <= Math.Max(line1.PtDebut.Y, line1.PtFin.Y) &&
               intersectPoint.Y >= Math.Min(line2.PtDebut.Y, line2.PtFin.Y) && intersectPoint.Y <= Math.Max(line2.PtDebut.Y, line2.PtFin.Y))
                return true;

            return false;
        }


        public static bool IntersectLine(SegmentD line1, SegmentD line2, out PointD intersectPoint)
        {
            intersectPoint = null;
            double intersectMax = 10;

            PointD x = new PointD(line2.PtDebut.X - line1.PtDebut.X, line2.PtDebut.Y - line1.PtDebut.Y);
            PointD d1 = new PointD(line1.PtFin.X - line1.PtDebut.X, line1.PtFin.Y - line1.PtDebut.Y);
            PointD d2 = new PointD(line2.PtFin.X - line2.PtDebut.X, line2.PtFin.Y - line2.PtDebut.Y);

            double det = d1.X * d2.Y - d1.Y * d2.X;
            if (det == 0)
                return false;

            double t1 = (x.X * d2.Y - x.Y * d2.X) / det;
            intersectPoint = new PointD(line1.PtDebut.X + d1.X * t1, line1.PtDebut.Y + d1.Y * t1);

            PointD averagePoint = new PointD(
                (line1.PtDebut.X + line1.PtFin.X + line2.PtDebut.X + line2.PtFin.X) / 4,
                (line1.PtDebut.Y + line1.PtFin.Y + line2.PtDebut.Y + line2.PtFin.Y) / 4);
            if (Math.Abs(intersectPoint.X - averagePoint.X) <= intersectMax &&
               Math.Abs(intersectPoint.Y - averagePoint.Y) <= intersectMax)
                return true;

            return false;
        }

        public static double ScalarProduct(SegmentExtended s1, SegmentExtended s2)
        {
            return (s1.Segment.X2 - s1.Segment.X1) * (s2.Segment.X2 - s2.Segment.X1) + (s1.Segment.Y2 - s1.Segment.Y1) * (s2.Segment.Y2 - s2.Segment.Y1);
        }
        public static double ScalarProduct(SegmentD s1, SegmentD s2)
        {
            return (s1.X2 - s1.X1) * (s2.X2 - s2.X1) + (s1.Y2 - s1.Y1) * (s2.Y2 - s2.Y1);
        }
        public static double Angle(SegmentD s1, SegmentD s2)
        {
            return Toolbox.Modulo2Pi(s2.Angle- s1.Angle);
        }
        public static double Angle(SegmentExtended s1, SegmentExtended s2)
        {
            return Toolbox.Modulo2Pi(s2.Segment.Angle - s1.Segment.Angle);
        }
        public static double VectorProduct(SegmentD s1, SegmentD s2)
        {
            return (s1.X2 - s1.X1) * (s2.Y2 - s2.Y1) - (s1.Y2 - s1.Y1) * (s2.X2 - s2.X1);
        }

        public static double? Cross(MathNet.Numerics.LinearAlgebra.Vector<double> left, MathNet.Numerics.LinearAlgebra.Vector<double> right)
        {
            double? result = null;

            if (left.Count == 2 && right.Count == 2)
            {
                result = left[0] * right[1] - left[1] * right[0];
            }
            return result;
        }
    }
}

