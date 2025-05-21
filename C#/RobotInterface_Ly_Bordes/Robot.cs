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
        public float distanceTelemetreplusDroit;
        public float distanceTelemetreDroit;
        public float distanceTelemetreCentre;
        public float distanceTelemetreGauche;
        public float distanceTelemetreplusGauche;
        public float positionYOdo;
        public float positionXOdo ;
        public float angleRadianFromOdometry ;
        public float vitesseLineaireFromOdometry;
        public float vitesseAngulaireFromOdometry;
        public float vitesseDroitFromOdometry;
        public float vitesseGaucheFromOdometry;

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


        public Robot()
        {
            
        }
    
    }
}
