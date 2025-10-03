using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;

namespace RobotInterface_Ly_Bordes
{
    public class Robot
    {
        public string receivedText = "";
        public Queue<byte> byteListReceived = new Queue<byte>();

        public float distanceTelemetrePlusDroit;
        public float distanceTelemetreDroit;
        public float distanceTelemetreCentre;
        public float distanceTelemetreGauche;
        public float distanceTelemetrePlusGauche;

        public float positionXOdo;
        public float positionYOdo;
        public float angleRadianFromOdometry;

        public float vitesseLineaireFromOdometry;
        public float vitesseAngulaireFromOdometry;
        public float vitesseDroitFromOdometry;
        public float vitesseGaucheFromOdometry;


        public float ThetaGhost;
        public float ThetaWaypoint;


        public PidCorrector PidX;
        public PidCorrector PidTheta;

        public Robot()
        {
            PidX = new PidCorrector();
            PidTheta = new PidCorrector();
        }
    }

    public class PidCorrector
    {
        public double Consigne;
        public double Measure;
        public double Error;
        public double Command;

        public double Kp;
        public double CorrecP;
        public double CorrecP_Max;

        public double Ki;
        public double CorrecI;
        public double CorrecI_Max;

        public double Kd;
        public double CorrecD;
        public double CorrecD_Max;

        public PidCorrector()
        {
            Consigne = 0;
            Measure = 0;
            Error = 0;
            Command = 0;

            Kp = 0;
            Ki = 0;
            Kd = 0;




            CorrecP_Max = 0;
            CorrecI_Max = 0;
            CorrecD_Max = 0;
        }
    }
}
