using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ExtendedSerialPort_NS;
using System.IO.Ports;
using System.Windows.Threading;
using System.Windows.Automation.Provider;
using System.Collections;
using System.Data.Common;
using System.Threading;
using static System.Net.Mime.MediaTypeNames;
using System.Reflection.Metadata;
using System.Linq.Expressions;
using SciChart.Charting.Visuals;
using static SciChart.Drawing.Utility.PointUtil;
using WpfOscilloscopeControl;
using SciChart.Data.Model;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using WpfAsservissementDisplay;
using static RobotInterface_Ly_Bordes.MainWindow;
using System.Printing;
using Vector = System.Windows.Vector;
using System.Drawing;
using Point = System.Windows.Point;
using WpfWorldMap_NS;






namespace RobotInterface_Ly_Bordes
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary> 
    /// 
    

    public partial class MainWindow : Window
    {
        ExtendedSerialPort serialPort1;
        Robot robot = new Robot();
        AsservissementRobot2RouesDisplayControl test;
        private Timer TimerAffichage;
        //private Controller gamepad;
        public int flag;
        public float  xPosRobot =50;
        public float yPosRobot = 50;
        private double timestamp;


        //Variable du Ghost
        private double thetaGhost;
        private double thetaWaypoint;
        private double vTheta = 0;
        private const double VThetaMax = 60;
        private const double AccTheta = 180;
        private const double Tsampling = 0.05;
        private Point positionGhost = new Point(50, 50);
        private Point positionWaypoint;
        double vLin = 0.0;
        double accLin = 50.0;
        double vLinMax = 100.0;
        double distanceRestante;


        public MainWindow()
        {
            timestamp = 0;
            SciChartSurface.SetRuntimeLicenseKey("VKOUDZGU6WndydcBQTqx4px2yWsaXqbn+hIKIxA5AE7Vii9ai5FosulEM8j2NYkBkJFZ6Ei2pFlUIV8aoE7bc3FfN3QRUwtvCaGqmrseTOeNsCz9p4t2CBk7TjcTPW7JTOYnIH/UjoRxi8b0BK6MDi8XJUS98gXSybDb/cn070Y5voaiKvusgmvvAOjcwuGcPQuyV7vJlzqh3LqLL3TqJnJMTdGmM00s8VFb7U+sxfbzT/h8SQuY13u/3i5sSz0VEI6YYJeiiX3oMajfHwA/SGyyDFTZmDAAfILtohF7ag+hnEpUDqhudgYjXqVwVtc0oUZNT8Ghtx0ek2bjkQukPtp8/44M1wiOdZORUOCAxeh3oTPZKjEGRjkpbN/UKprgi8/Xvf11BuXzTJLXklmSZLFRsgxcx3nvQVwae9oY5HABtwOk+q/bdsNBKyPmhjNLM1+y5qSlpIQlHzm/EdvN44AX5iR43d4dxfLx9QN7KHvaUbHpqNXVKLUsq0g1g6mEGntw5fXj");

            InitializeComponent();
           
            //Setting SerialPort
            serialPort1 = new ExtendedSerialPort("COM4", 115200, Parity.None, 8, StopBits.One);// Changer le port USB (COM)
            serialPort1.DataReceived += SerialPort1_DataReceived;
            serialPort1.Open();


            //Setting Timer
            DispatcherTimer timerAffichage;
            timerAffichage = new DispatcherTimer();
            timerAffichage.Interval = new TimeSpan(0, 0, 0, 0, 4);

            timerAffichage.Tick += TimerAffichage_Tick;
            timerAffichage.Start();
            //gamepad = new Controller(UserIndex.One);
            // Create a thread
            //Thread backgroundThread = new Thread(new ThreadStart(ProcessQueue));
            //// Start thread
            //backgroundThread.IsBackground = true;
            //backgroundThread.Start();

            //Setting Oscillo 
            oscilloSpeed.isDisplayActivated = true;
            oscilloSpeed.AddOrUpdateLine(1, 200, "Vitesse_Linéaire");
            oscilloSpeed.ChangeLineColor(1, Colors.Blue);
            oscilloSpeed.AddOrUpdateLine(2, 200, "Vitesse_Angulaire");
            oscilloSpeed.ChangeLineColor(2, Colors.Green);
            WpfWorldMap.UpdatePosRobot(xPosRobot, yPosRobot);
            

        }

        bool gamepad_state = true;
        int compteur = 0;

        private void TimerAffichage_Tick(object sender, EventArgs e)
        {
            //if (!gamepad.IsConnected)
            //{
            //    gamepad_state = false ;
            //    byte[] mode = new byte[] {1};
            //    UartEncodeAndSendMessage(0x0052, 0, mode);
            //}
            timestamp++;
            //else if ( gamepad.IsConnected ) {
            //    byte[] mode = new byte[] { 0 };
            //    UartEncodeAndSendMessage(0x0052, 1, mode);
            //    var state = gamepad.GetState();
            //    var gamepadState = state.Gamepad;
            //    byte rightTrigger = gamepadState.RightTrigger; 
            //    byte dividedValueR = (byte)(rightTrigger / 3f);
            //    short rightThumbStickX = gamepadState.RightThumbX;
            //    byte dividedValueSX = (byte)(rightThumbStickX / 650f);
            //    byte leftTrigger = gamepadState.LeftTrigger;
            //    byte dividedValueL = (byte)(leftTrigger / 3f);
            //    //RichTextBox.Dispatcher.BeginInvoke(new Action(() =>
            //    //           RichTextBox.Text += $"Buttons: {dividedValue}\n"));
            //    byte[] vit = new byte[] { dividedValueR,dividedValueL };

            //    UartEncodeAndSendMessage(0X0090, 2, vit);
            //    compteur += 1;
            //    RichTextBox.Dispatcher.BeginInvoke(new Action(() =>
            //             // RichTextBox.Text += $"vitesse: {dividedValue}\n"));
            //             RichTextBox.Text = $"compteur {compteur}\n"
            //             ));

            //    if (gamepadState.Buttons != GamepadButtonFlags.None)
            //    {
            //        RichTextBox.Dispatcher.BeginInvoke(new Action(() => RichTextBox.Text += $"Buttons: {gamepadState.Buttons}\n"));
            //    }

            //}


            RichTextBox.Dispatcher.BeginInvoke(new Action(() => RichTextBox.Text =  $"posX: {robot.positionXOdo}\n"));
            RichTextBox.Dispatcher.BeginInvoke(new Action(() => RichTextBox.Text += $"posY: {robot.positionYOdo}\n"));
            RichTextBox.Dispatcher.BeginInvoke(new Action(() => RichTextBox.Text += $"Angle: {robot.angleRadianFromOdometry * 180.0/Math.PI}\n"));
            RichTextBox.Dispatcher.BeginInvoke(new Action(() => RichTextBox.Text += $"AngleGhost: {robot.ThetaGhost}\n"));
            RichTextBox.Dispatcher.BeginInvoke(new Action(() => RichTextBox.Text += $"VitLin: {robot.vitesseLineaireFromOdometry}\n"));
            RichTextBox.Dispatcher.BeginInvoke(new Action(() => RichTextBox.Text += $"VitAngl: {robot.vitesseAngulaireFromOdometry}\n"));
            RichTextBox.Dispatcher.BeginInvoke(new Action(() => RichTextBox.Text += $"VitD: {robot.vitesseDroitFromOdometry}\n"));
            RichTextBox.Dispatcher.BeginInvoke(new Action(() => RichTextBox.Text += $"VitG: {robot.vitesseGaucheFromOdometry}\n"));



            oscilloSpeed.AddPointToLine(1, timestamp , robot.vitesseLineaireFromOdometry);
            oscilloSpeed.AddPointToLine(2, timestamp , robot.vitesseAngulaireFromOdometry);

            //UpdateGhost();
            



        }

        private void ProcessQueue()
        {
            while (true)
            {
                while (robot.byteListReceived.Count != 0)
                {
                    DecodeMessage(robot.byteListReceived.Dequeue());
                    //RichTextBox.Text += "0x" + robot.byteListReceived.Dequeue().ToString("X2") + " ";
                    //if (robot.byteListReceived.Count == 0)
                    //{
                    //    RichTextBox.Text += "\n";
                    //}

                }
                Thread.Sleep(10);
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        { 
        }

        public void SerialPort1_DataReceived(object sender, DataReceivedArgs e)
        {
            for (int i = 0;i <e.Data.Length;i++) {
                DecodeMessage(e.Data[i]);
                //robot.byteListReceived.Enqueue(e.Data[i]);
            }
            //robot.receivedText += Encoding.ASCII.GetString(e.Data, 0, e.Data.Length);
        }
        
        private void buttonEnvoyer_Click(object sender, RoutedEventArgs e)
        {
            if (TextBoxEmission.Text != "")
            {
                byte[] array = Encoding.ASCII.GetBytes(TextBoxEmission.Text);
                UartEncodeAndSendMessage(0x0080, (Int16)array.Length, array);
                TextBoxEmission.Text = "";
            }
        }
        private void sendMessage()
        {
            serialPort1.Write(TextBoxEmission.Text);
        }
        public byte CalculateChecksum(Int16 msgFunction,int msgPayloadLength, byte[] msgPayload)
        {
            // Todo faire checksum pour command and playload    
            byte checksum = 0;

            checksum ^= 0xFE;
            checksum ^= (byte)(msgFunction >> 8);
            checksum ^= (byte)(msgFunction >> 0);
            checksum ^= (byte)(msgPayloadLength >> 8);
            checksum ^= (byte)(msgPayloadLength >> 0);

            for (int j = 0; j < msgPayloadLength; j++)
            {
                checksum ^= msgPayload[j];                
            }
            return checksum;
        }
        void UartEncodeAndSendMessage(Int16 msgFunction, Int16 msgPayloadLength, byte[] msgPayload)
        {
            byte[] msg = new byte[msgPayloadLength + 6];
            int pos = 0;
            msg[pos++] = 0xFE;
            msg[pos++] = (byte)(msgFunction >> 8);
            msg[pos++] = (byte)(msgFunction >> 0);
            msg[pos++] = (byte)(msgPayloadLength >> 8);
            msg[pos++] = (byte)(msgPayloadLength >> 0);
            for (int i = 0; i < msgPayloadLength; i++)
            {
                msg[pos++] = msgPayload[i];
            }
            msg[pos++] = CalculateChecksum(msgFunction, msgPayloadLength, msgPayload);
            serialPort1.Write(msg, 0, pos);
        }

        //int compteur1 = 0;
        void ProcessDecodedMessage(int msgFunction, int msgPayloadLength, byte[] msgPayload)
        {
            switch (msgFunction)
            {
                case (byte)IDfonction.TextTransmission:
                    RichTextBox.Dispatcher.BeginInvoke(new Action(() =>
                    RichTextBox.Text += "0x" + msgFunction.ToString("X") + " " + Encoding.ASCII.GetString(msgPayload, 0, msgPayloadLength)));
                    break;
                case (byte)IDfonction.SetLed:
                    break;
                case (byte)IDfonction.IRdistance:
                    RichTextBox.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        IRpg.Content = msgPayload[4];
                        IRg.Content = msgPayload[3];
                        IRc.Content = msgPayload[2];
                        IRd.Content = msgPayload[1];
                        IRpd.Content = msgPayload[0];
                    }));
                    break;

                case (byte)IDfonction.SpeedRule:
                    byte valG = msgDecodedPayload[0];
                    byte valD = msgDecodedPayload[1];
                    if (valG == 0 && valD == 0)
                    {
                        vitg.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            vitg.Content = valG;
                        }));
                        vitd.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            vitd.Content = valD;
                        }));
                    }
                    else
                    {
                        Console.WriteLine(valG.ToString() + " | " + valD.ToString());
                        //RichTextBox.Dispatcher.BeginInvoke(new Action(() =>
                        //          RichTextBox.Text += "0x" + msgFunction.ToString("X")));
                    }

                    break;
                case (byte)IDfonction.RobotState:
                    int instant = (((int)msgPayload[1]) << 24) + (((int)msgPayload[2]) << 16)
                    + (((int)msgPayload[3]) << 8) + ((int)msgPayload[4]);

                    RichTextBox.Dispatcher.BeginInvoke(new Action(() =>
                    RichTextBox.Text += "\nRobot␣State␣:␣" + ((StateRobot)(msgPayload[0])).ToString() + "␣-␣" + instant.ToString() + "␣ms"));
                    break;

                case (byte)IDfonction.QEIReception:
                    robot.positionXOdo = BitConverter.ToSingle(msgPayload, 4);
                    robot.positionYOdo = BitConverter.ToSingle(msgPayload, 8);
                    robot.angleRadianFromOdometry = BitConverter.ToSingle(msgPayload, 12);
                    robot.vitesseLineaireFromOdometry = BitConverter.ToSingle(msgPayload, 16);
                    robot.vitesseAngulaireFromOdometry = BitConverter.ToSingle(msgPayload, 20);
                    robot.vitesseGaucheFromOdometry = BitConverter.ToSingle(msgPayload, 24);
                    robot.vitesseDroitFromOdometry = BitConverter.ToSingle(msgPayload, 28);
                   


                    WpfWorldMap.UpdatePosRobot(robot.positionXOdo+0.1,robot.positionYOdo+1);
                    WpfWorldMap.UpdateOrientationRobot(robot.angleRadianFromOdometry * 180.0 / Math.PI);
                   break;
                case (byte)IDfonction.SetPid:
                    float kp = BitConverter.ToSingle(msgPayload, 0);
                    float ki = BitConverter.ToSingle(msgPayload, 4);
                    float kd = BitConverter.ToSingle(msgPayload, 8);

                    asservSpeedDisplay.UpdatePolarSpeedCorrectionGains(kp, msgPayload[3], ki, msgPayload[4],kd , msgPayload[5]);
                    //test.UpdatePolarSpeedCorrectionGains(msgPayload[0], msgPayload[1], msgPayload[2], msgPayload[3], msgPayload[4], msgPayload[5]);
                    break;
                case (byte)PID_val.info:
                    robot.PidX.Consigne = BitConverter.ToSingle(msgPayload, 0);
                    robot.PidX.Measure = BitConverter.ToSingle(msgPayload, 4);
                    robot.PidX.Error = BitConverter.ToSingle(msgPayload, 8);
                    robot.PidX.Command = BitConverter.ToSingle(msgPayload, 12);
                    robot.PidX.Kp = BitConverter.ToSingle(msgPayload, 16);
                    robot.PidX.CorrecP = BitConverter.ToSingle(msgPayload, 20);
                    robot.PidX.CorrecP_Max = BitConverter.ToSingle(msgPayload, 24);
                    robot.PidX.Ki = BitConverter.ToSingle(msgPayload, 28);
                    robot.PidX.CorrecI = BitConverter.ToSingle(msgPayload, 32);
                    robot.PidX.CorrecI_Max = BitConverter.ToSingle(msgPayload, 36);
                    robot.PidX.Kd = BitConverter.ToSingle(msgPayload, 40);
                    robot.PidX.CorrecD = BitConverter.ToSingle(msgPayload, 44);
                    robot.PidX.CorrecD_Max = BitConverter.ToSingle(msgPayload, 48);

                    robot.PidTheta.Consigne = BitConverter.ToSingle(msgPayload, 52);
                    robot.PidTheta.Measure = BitConverter.ToSingle(msgPayload, 56);
                    robot.PidTheta.Error = BitConverter.ToSingle(msgPayload, 60);
                    robot.PidTheta.Command = BitConverter.ToSingle(msgPayload, 64);
                    robot.PidTheta.Kp = BitConverter.ToSingle(msgPayload, 68);
                    robot.PidTheta.CorrecP = BitConverter.ToSingle(msgPayload, 72);
                    robot.PidTheta.CorrecP_Max = BitConverter.ToSingle(msgPayload, 76);
                    robot.PidTheta.Ki = BitConverter.ToSingle(msgPayload, 80);
                    robot.PidTheta.CorrecI = BitConverter.ToSingle(msgPayload, 84);
                    robot.PidTheta.CorrecI_Max = BitConverter.ToSingle(msgPayload, 88);
                    robot.PidTheta.Kd = BitConverter.ToSingle(msgPayload, 92);
                    robot.PidTheta.CorrecD = BitConverter.ToSingle(msgPayload, 96);
                    robot.PidTheta.CorrecD_Max = BitConverter.ToSingle(msgPayload, 100);

              

                    asservSpeedDisplay.UpdatePolarSpeedCorrectionGains(robot.PidX.Kp, robot.PidTheta.Kp, robot.PidX.Ki, robot.PidTheta.Ki, robot.PidX.Kd, robot.PidTheta.Kd);
                    asservSpeedDisplay.UpdatePolarSpeedCorrectionLimits(robot.PidX.CorrecP_Max, robot.PidTheta.CorrecP_Max, robot.PidX.CorrecI_Max, robot.PidTheta.CorrecI_Max, robot.PidX.CorrecD_Max, robot.PidTheta.CorrecD_Max);
                    asservSpeedDisplay.UpdatePolarSpeedCorrectionValues(robot.PidX.CorrecP, robot.PidTheta.CorrecP, robot.PidX.CorrecI, robot.PidTheta.CorrecI, robot.PidX.CorrecD , robot.PidTheta.CorrecD);
                    asservSpeedDisplay.UpdatePolarSpeedErrorValues(robot.PidX.Error, robot.PidTheta.Error);
                    asservSpeedDisplay.UpdatePolarOdometrySpeed(robot.PidX.Measure, robot.PidTheta.Measure);
                    asservSpeedDisplay.UpdatePolarSpeedCommandValues(robot.PidX.Command , robot.PidTheta.Command);
                    asservSpeedDisplay.UpdatePolarSpeedConsigneValues(robot.PidX.Consigne, robot.PidTheta.Consigne);


                    break;
                case (byte)IDfonction.Ghost_angle:

                    robot.ThetaGhost = BitConverter.ToSingle(msgPayload, 0);
                   WpfWorldMap.UpdateOrientationRobotGhost(robot.ThetaGhost);

                    break;
                case (byte)IDfonction.Ghost_position:
                    WpfWorldMap.UpdatePosRobotGhost(BitConverter.ToSingle(msgPayload, 0), BitConverter.ToSingle(msgPayload, 4) );
                    break;


                case 0x00FF:
                    float test = BitConverter.ToSingle(msgPayload, 0);
                    float test1 = BitConverter.ToSingle(msgPayload, 4);
                    asservSpeedDisplay.UpdateIndependantSpeedCommandValues(test, test1);
                    break;




            }

        }

        private void TextBoxEmission_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //sendMessage();
                byte[] array = Encoding.ASCII.GetBytes(TextBoxEmission.Text);
                UartEncodeAndSendMessage(0x0080, (Int16)array.Length, array);
                TextBoxEmission.Text = "";

            }

        }

        private void KpEntry_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {

                

            }

        }
        private void KiEntry_KeyUp(object sender, KeyEventArgs e)
        {


        }
        private void KdEntry_KeyUp(object sender, KeyEventArgs e)
        {


        }
        public enum StateReception
        {
            Waiting,
            FunctionMSB,
            FunctionLSB,
            PayloadLengthMSB,
            PayloadLengthLSB,
            Payload,
            CheckSum
            
        }
       

        public enum StateRobot
        {

            STATE_ATTENTE = 0,
            STATE_ATTENTE_EN_COURS = 1,
            STATE_AVANCE = 2,
            STATE_AVANCE_EN_COURS = 3,
            STATE_TOURNE_GAUCHE = 4,
            STATE_TOURNE_GAUCHE_EN_COURS = 5,
            STATE_TOURNE_DROITE_PLUS = 6,
            STATE_TOURNE_DROITE_PLUS_EN_COURS = 7,
            STATE_TOURNE_GAUCHE_PLUS = 8,
            STATE_TOURNE_GAUCHE_PLUS_EN_COURS = 9,
            STATE_TOURNE_DROITE = 10,
            STATE_TOURNE_DROITE_EN_COURS = 11,
            STATE_TOURNE_SUR_PLACE_GAUCHE = 12,
            STATE_TOURNE_SUR_PLACE_GAUCHE_EN_COURS = 13,
            STATE_TOURNE_SUR_PLACE_DROITE = 14,
            STATE_TOURNE_SUR_PLACE_DROITE_EN_COURS = 15,
            STATE_ARRET = 16,
            STATE_ARRET_EN_COURS = 17,
            STATE_RECULE = 18,
            STATE_RECULE_EN_COURS = 19
        }

        StateReception rcvState = StateReception.Waiting;
        int msgDecodedFunction = 0;
        int msgDecodedPayloadLength = 0;
        byte[] msgDecodedPayload = { };
        int msgDecodedPayloadIndex = 0;

        private void DecodeMessage(byte c)
        {
            switch (rcvState)
            {
                case StateReception.Waiting:
                    if (c == 0xFE)
                    {
                        rcvState = StateReception.FunctionMSB;
                    }
                    break;
                case StateReception.FunctionMSB:
                    msgDecodedFunction = c<<8;
                    rcvState = StateReception.FunctionLSB;
                    //RichTextBox.Text += "functionMSB";
                    break;
                case StateReception.FunctionLSB:
                    msgDecodedFunction += c;
                    rcvState = StateReception.PayloadLengthMSB;
                    //RichTextBox.Text += "functionLSB";
                    break;
                case StateReception.PayloadLengthMSB:
                    msgDecodedPayloadLength = c<<8;
                    rcvState = StateReception.PayloadLengthLSB;
                    break;
                case StateReception.PayloadLengthLSB:
                    msgDecodedPayloadLength += c;
                    if (msgDecodedPayloadLength == 0)
                        rcvState = StateReception.CheckSum;
                    else if (msgDecodedPayloadLength < 1024)
                    {
                        rcvState = StateReception.Payload;
                        msgDecodedPayloadIndex = 0;
                        msgDecodedPayload = new byte[msgDecodedPayloadLength];
                    }
                    else
                        rcvState = StateReception.Waiting;
                    break;
                case StateReception.Payload:
                    msgDecodedPayload[msgDecodedPayloadIndex++] = c;
                    if (msgDecodedPayloadIndex >= msgDecodedPayloadLength)
                    {
                        rcvState = StateReception.CheckSum;
                    }                    
                    break;
                case StateReception.CheckSum:

                    byte calculatedChecksum =  CalculateChecksum((byte)msgDecodedFunction, msgDecodedPayloadLength, msgDecodedPayload);
                    //RichTextBox.Text += calculatedChecksum;
                    byte receivedChecksum = c;
                    if (calculatedChecksum == receivedChecksum)
                    {
                        //RichTextBox.Text += "ça marche";
                        rcvState = StateReception.Waiting;
                     
                       ProcessDecodedMessage(msgDecodedFunction, msgDecodedPayloadLength, msgDecodedPayload);  
                    }
                    else
                    {
                        rcvState = StateReception.Waiting;
                    }
                    msgDecodedFunction = 0;
                    msgDecodedPayloadLength = 0;
                    msgDecodedPayloadIndex = 0;
                    for (int i = 0; i < msgDecodedPayloadLength; i++)
                    {
                        msgDecodedPayload[i] = 0;
                    }
                    break;
                default:
                    rcvState = StateReception.Waiting;
                    break;
            }
        }

        public enum IDfonction
        {
            TextTransmission = 0x0080,
            SetLed = 0x0020,
            IRdistance = 0x0030,
            SpeedRule = 0x0040,
            RobotState = 0x0050,
            QEIReception = 0x0060,
            SetPid = 0x0070,
            Ghost_angle=0x0090,
            Ghost_position = 0x0091,
            functionTestValue
        }

        public enum PID_val
        {
            info = 0x0071,
            SetPidX = 0x0072,
            SetPidT = 0x0073,
            setConsigne = 0x0074,
            ResetPid = 0x0075,
            ghostSetPid = 0x0076,

        }

        public enum GhostState
        {
            Idle,
            Rotation,
            DeplacementLineaire
        }
        private void buttonTest_Click(object sender, RoutedEventArgs e)
        {
         
            byte[] kpX = BitConverter.GetBytes(3.0f);
            byte[] kiX = BitConverter.GetBytes(25.0f);
            byte[] kdX = BitConverter.GetBytes(0.0f);

            byte[] resultX = kpX.Concat(kiX).Concat(kdX).ToArray();
          
            UartEncodeAndSendMessage((byte)PID_val.SetPidX, 12 , resultX);



            byte[] kpT = BitConverter.GetBytes(2.0f);
            byte[] kiT = BitConverter.GetBytes(10.0f);
            byte[] kdT = BitConverter.GetBytes(0.0f);

            byte[] resultT = kpT.Concat(kiT).Concat(kdT).ToArray();

            UartEncodeAndSendMessage((byte)PID_val.SetPidT, 12, resultT);

            byte[] kp_PD_Lin = BitConverter.GetBytes(0.2f);
            byte[] ki_PD_Lin = BitConverter.GetBytes(0.0f);
            byte[] kd_PD_Lin = BitConverter.GetBytes(0.0f);
            byte[] result_PD_Lin = kp_PD_Lin.Concat(ki_PD_Lin).Concat(kd_PD_Lin).ToArray();

            byte[] kp_PD_Ang = BitConverter.GetBytes(0.0f);
            byte[] ki_PD_Ang = BitConverter.GetBytes(0.0f);
            byte[] kd_PD_Ang = BitConverter.GetBytes(0.0f);
            byte[] result_PD_Ang = kp_PD_Ang.Concat(ki_PD_Ang).Concat(kd_PD_Ang).ToArray();

            byte[] result_ghost_PD = result_PD_Lin.Concat(result_PD_Ang).ToArray();

            UartEncodeAndSendMessage((byte)PID_val.ghostSetPid, 24, result_ghost_PD);

        }

          
        private void buttonConsigne_Click(object sender, RoutedEventArgs e)
        {

            byte[] consigneAngulaire = BitConverter.GetBytes(0.00f);
            byte[] consigneLineaire = BitConverter.GetBytes(0.00f);

            byte[] consigne = consigneLineaire.Concat(consigneAngulaire).ToArray();
            UartEncodeAndSendMessage((byte)PID_val.setConsigne, 8, consigne);

        }


        private void ResetConsigne_Click(object sender, RoutedEventArgs e)
        {
            byte[] consigneAngulaire = BitConverter.GetBytes(0.0f);
            byte[] consigneLineaire = BitConverter.GetBytes(0.0f);

            byte[] consigne = consigneLineaire.Concat(consigneAngulaire).ToArray();
            UartEncodeAndSendMessage((byte)PID_val.setConsigne, 8, consigne);
        }

        private void ResetPid_Click(object sender, RoutedEventArgs e)
        {

            byte[] reset = new byte[] {0x00 };

            UartEncodeAndSendMessage((byte)PID_val.ResetPid, 1, reset);


        }





        private void TextBlock_TextInput(object sender, TextCompositionEventArgs e)
        {
           
            
        }


        bool L1 = false;
        bool L2 = false;
        bool L3 = false;
        bool L4 = false;
        bool L5 = false;
        byte[] ledList = new byte[] { 0x00, 0x00};
        private void Led1_Checked(object sender, RoutedEventArgs e)
        {
            
            L1 = !L1;
            ledList[0]=0x01;
            if ( L1)
            {
                ledList[1] = 0x01;
            }
            else
            {
                ledList[1] = 0x00;
            }
      
     
                UartEncodeAndSendMessage(0x0020, 2, ledList);
           
           
        }
        private void Led2_Checked(object sender, RoutedEventArgs e)
        {
            L2 = !L2;
            ledList[0] = 0x02;
            if (L2)
            {
                ledList[1] = 0x01;
            }
            else
            {
                ledList[1] = 0x00;
            }

            UartEncodeAndSendMessage(0x0020, 2, ledList);
        }
        private void Led3_Checked(object sender, RoutedEventArgs e)
        {
            L3 = !L3;
            ledList[0] = 0x03;
            if (L3)
            {
                ledList[1] = 0x01;
            }
            else
            {
                ledList[1] = 0x00;
            }

            UartEncodeAndSendMessage(0x0020, 2, ledList);
        }

        private void Led4_Checked(object sender, RoutedEventArgs e)
        {
            L4 = !L4;
            ledList[0] = 0x04;
            if (L4)
            {
                ledList[1] = 0x01;
            }
            else
            {
                ledList[1] = 0x00;
            }

            UartEncodeAndSendMessage(0x0020, 2, ledList);
        }

        private void Led5_Checked(object sender, RoutedEventArgs e)
        {
            L5 = !L5;
            ledList[0] = 0x05;
            if (L5)
            {
                ledList[1] = 0x01;
            }
            else
            {
                ledList[1] = 0x00;
            }

            UartEncodeAndSendMessage(0x0020, 2, ledList);
        }
        GhostState Ghoststate = GhostState.Idle;
        private void UpdateGhost()
        {
            switch (Ghoststate)
            {
                case GhostState.Idle:
                    if (WpfWorldMap.Start)
                    { 
                        Ghoststate = GhostState.Rotation;
                    }
                    break;
                case GhostState.Rotation:
                    Point Robot = new Point(WpfWorldMap.pos_X_ghost, WpfWorldMap.pos_Y_ghost);
                    Point Target = new Point(WpfWorldMap.xDataValue, WpfWorldMap.yDataValue);
                    double AnglePoint = AngleVersCible(Robot, Target);
                    SetWaypointAngle(AnglePoint);
                    UpdateRotation();
               
                    if ( WpfWorldMap._angleghost == AnglePoint)
                    {
                        Ghoststate = GhostState.DeplacementLineaire;
                        SetWaypointDistance();
                    }
                    break;
                case GhostState.DeplacementLineaire:

                    //double distance = DistancePointToSegment();
                    UpdateDeplacementGhost();
                    if (distanceRestante < 1e-6)
                    {
                        vLin = 0;
                        positionGhost = positionWaypoint;
                        WpfWorldMap.UpdatePosRobotGhost(positionGhost.X, positionGhost.Y);
                        WpfWorldMap.Start = false;
                        Ghoststate = GhostState.Idle;
                    }

                    break;
            }
        }
        void UpdateRotation()
        {
            double thetaRestant = ModuloByAngle(thetaWaypoint - thetaGhost);
            thetaRestant = NormalizeAngle(thetaRestant); 

            double thetaArret = (vTheta * vTheta) / (2 * AccTheta);
            if (vTheta < 0)
                thetaArret = -thetaArret;

            double incrementTheta = vTheta * Tsampling;

            if (((thetaArret >= 0 && thetaRestant >= 0) || (thetaArret <= 0 && thetaRestant <= 0)) &&
                Math.Abs(thetaRestant) >= Math.Abs(thetaArret))
            {
                if (thetaRestant > 0)
                    vTheta = Math.Min(vTheta + (AccTheta * Tsampling), VThetaMax);
                else if (thetaRestant < 0)
                    vTheta = Math.Max(vTheta - (AccTheta * Tsampling), -VThetaMax);
            }
            else 
            {
                if (vTheta > 0)
                    vTheta = Math.Max(vTheta - (AccTheta * Tsampling), 0);
                else if (vTheta < 0)
                    vTheta = Math.Min(vTheta + (AccTheta * Tsampling), 0);
            }

            if (Math.Abs(thetaRestant) < Math.Abs(incrementTheta))
                incrementTheta = thetaRestant;

            thetaGhost += incrementTheta;
            thetaGhost = NormalizeAngle(thetaGhost);

            if (vTheta == 0 && Math.Abs(thetaRestant) < 0.01)
                thetaGhost = thetaWaypoint;

          WpfWorldMap.UpdateOrientationRobotGhost(thetaGhost);
        }


      

        private void UpdateDeplacementGhost()
        {
            WpfWorldMap.UpdatePosRobotGhost(positionGhost.X, positionGhost.Y);

            double dx = positionWaypoint.X - positionGhost.X;
            double dy = positionWaypoint.Y - positionGhost.Y;

            distanceRestante = Math.Sqrt(dx * dx + dy * dy);


            double distanceArret = (vLin * vLin) / (2 * accLin);
            if (vLin < 0)
                distanceArret = -distanceArret;

            double incrementD = vLin * Tsampling;

            double dirX = dx / distanceRestante;
            double dirY = dy / distanceRestante;

            if (((distanceArret >= 0 && distanceRestante >= 0) || (distanceArret <= 0 && distanceRestante <= 0)) &&
                Math.Abs(distanceRestante) >= Math.Abs(distanceArret))
            {
                vLin = Math.Min(vLin + accLin * Tsampling, vLinMax);
            }
            else
            {
                vLin = Math.Max(vLin - accLin * Tsampling, 0);
            }

            if (Math.Abs(distanceRestante) < Math.Abs(incrementD))
            {
                incrementD = distanceRestante;
            }

            positionGhost.X += (float)(dirX * incrementD);
            positionGhost.Y += (float)(dirY * incrementD);

            if (vLin == 0 && distanceRestante < 0.01)
            {
                positionGhost = positionWaypoint;
            }

        }



        public void SetWaypointDistance()
        {
            positionWaypoint =  new Point(WpfWorldMap.xDataValue, WpfWorldMap.yDataValue);
        }




        //private static double DistancePointToSegment(Point p, Point a, Point b, out Point projection)
        //{
        //    Vector ab = b - a;
        //    Vector ap = p - a;

        //    double abLengthSquared = ab.LengthSquared;

        //    if (abLengthSquared == 0)
        //    {
        //        projection = a;
        //        return (p - a).Length;
        //    }

        //    double t = Vector.Multiply(ap, ab) / abLengthSquared;

        //    t = Math.Max(0, Math.Min(1, t));

        //    projection = a + ab * t;

        //    return (p - projection).Length;
        //}


        private double ModuloByAngle(double angle)
        {
            return (angle + 360) % 360;
        }


        private double NormalizeAngle(double angle)
        {
            angle = ModuloByAngle(angle);
            if (angle > 180)
                angle -= 360;
            return angle;
        }

        private double AngleVersCible(Point robot, Point target)
        {
            double dx = target.X - robot.X;
            double dy = -(target.Y - robot.Y); 

            double angleDeg = Math.Atan2(dy, dx) * 180.0 / Math.PI;
            return NormalizeAngle(angleDeg+90);
        }


        bool is_waypoint_ahead(Point robot, double theta, Point waypoint)
        {
            double dx = Math.Cos(theta);
            double dy = Math.Sin(theta);

            double wx = waypoint.X - robot.X;
            double wy = waypoint.Y - robot.Y;

            double dot = dx * wx + dy * wy;
            return dot >= 0.0;
        }


        public void SetWaypointAngle(double angleDeg)
        {
            thetaWaypoint = angleDeg;
        }

        private void testGhost_Click(object sender, RoutedEventArgs e)
        {

            byte[] posX = BitConverter.GetBytes((float)WpfWorldMap.xDataValue);
            byte[] posy = BitConverter.GetBytes((float)WpfWorldMap.yDataValue);
            byte[] position = posX.Concat(posy).ToArray();


            Point Robot = new Point(WpfWorldMap.pos_X_robot, WpfWorldMap.pos_Y_robot);
            Point Target = new Point(WpfWorldMap.xDataValue, WpfWorldMap.yDataValue);
            double AnglePoint = AngleVersCible(Robot, Target);


            //byte[] thetaW = BitConverter.GetBytes(-AnglePointf);
            UartEncodeAndSendMessage((byte)IDfonction.Ghost_angle, 8, position);

        }

     
    }















}
