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
        public Robot()
        {
            
        }
    
    }
}
