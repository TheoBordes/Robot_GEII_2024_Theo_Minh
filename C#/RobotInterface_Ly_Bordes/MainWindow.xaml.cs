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
using SharpDX.XInput;
using System.Reflection.Metadata;
using System.Linq.Expressions;

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
        private Timer TimerAffichage;
        private Controller gamepad;

        public MainWindow()
        {
            InitializeComponent();

            serialPort1 = new ExtendedSerialPort("COM5", 115200, Parity.None, 8, StopBits.One);
            serialPort1.DataReceived += SerialPort1_DataReceived;
            serialPort1.Open();
            DispatcherTimer timerAffichage;
            timerAffichage = new DispatcherTimer();
            timerAffichage.Interval = new TimeSpan(0, 0, 0, 0, 1);
            timerAffichage.Tick += TimerAffichage_Tick;
            timerAffichage.Start();
            gamepad = new Controller(UserIndex.One);
            // Create a thread
            //Thread backgroundThread = new Thread(new ThreadStart(ProcessQueue));
            //// Start thread
            //backgroundThread.IsBackground = true;
            //backgroundThread.Start();
        }

        bool gamepad_state = true;
        int compteur = 0;

        private void TimerAffichage_Tick(object sender, EventArgs e)
        {
            
            //ProcessQueue();
            //RichTextBox.Text += "100";
            if (!gamepad.IsConnected)
            {
                gamepad_state = false ;
                //RichTextBox.Text += "Gamepad not connected.\n";
                byte[] mode = new byte[] {1};
                UartEncodeAndSendMessage(0x0052, 1, mode);
            }   
            else if ( gamepad.IsConnected ) {
                byte[] mode = new byte[] { 0 };
                UartEncodeAndSendMessage(0x0052, 1, mode);
                var state = gamepad.GetState();
                var gamepadState = state.Gamepad;
                byte rightTrigger = gamepadState.RightTrigger; 
                byte dividedValueR = (byte)(rightTrigger / 3f);
                short rightThumbStickX = gamepadState.RightThumbX;
                byte dividedValueSX = (byte)(rightThumbStickX / 650f);
                byte leftTrigger = gamepadState.LeftTrigger;
                byte dividedValueL = (byte)(leftTrigger / 3f);
                //RichTextBox.Dispatcher.BeginInvoke(new Action(() =>
                //           RichTextBox.Text += $"Buttons: {dividedValue}\n"));
                byte[] vit = new byte[] { dividedValueR,dividedValueL };
               
                UartEncodeAndSendMessage(0X0090, 2, vit);
                compteur += 1;
                RichTextBox.Dispatcher.BeginInvoke(new Action(() =>
                         // RichTextBox.Text += $"vitesse: {dividedValue}\n"));
                         RichTextBox.Text = $"compteur {compteur}\n"
                         ));

                if (gamepadState.Buttons != GamepadButtonFlags.None)
                {
                    RichTextBox.Dispatcher.BeginInvoke(new Action(() =>
                           RichTextBox.Text += $"Buttons: {gamepadState.Buttons}\n"));
                }
            }
           

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
            byte checksum = 0xFE;
            checksum = (byte) ( msgFunction ^ checksum) ;

            for (int j = 0; j < msgPayloadLength; j++)
            {
                checksum = (byte)(checksum ^ msgPayload[j]);
                
            }
            return checksum;
        }
        void UartEncodeAndSendMessage(Int16 msgFunction, Int16 msgPayloadLength, byte[] msgPayload)
        {
            byte start = 0xFE;
            byte[] checksum = { CalculateChecksum((byte)msgFunction, msgPayloadLength, msgPayload) };
            byte[] command = new byte[] { start };
            byte[] codeFunction = BitConverter.GetBytes(msgFunction);
            /*Array.Reverse(codeFunction);*/
            byte[] PayloadLength = BitConverter.GetBytes(msgPayloadLength);
            //Array.Reverse(PayloadLength);
            serialPort1.Write(command, 0, command.Length);
            serialPort1.Write(codeFunction, 0, codeFunction.Length); 
            serialPort1.Write(PayloadLength, 0, PayloadLength.Length);
            serialPort1.Write(msgPayload, 0, msgPayload.Length);
            serialPort1.Write(checksum, 0, checksum.Length);
        }
        void ProcessDecodedMessage(int msgFunction, int msgPayloadLength, byte[] msgPayload)
        {
            switch (msgFunction)
            {
                case (byte)IDfonction.TextTransmission:
                    RichTextBox.Dispatcher.BeginInvoke(new Action(() =>
                                   RichTextBox.Text += "0x" + msgFunction.ToString("X") + " " + Encoding.ASCII.GetString(msgPayload, 0, msgPayloadLength))); break;
                case (byte)IDfonction.SetLed:

                    break;
                case (byte)IDfonction.IRdistance:
                    RichTextBox.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        IRpg.Content = msgDecodedPayload[4];
                        IRg.Content = msgDecodedPayload[3];
                        IRc.Content = msgDecodedPayload[2];
                        IRd.Content = msgDecodedPayload[1];
                        IRpd.Content = msgDecodedPayload[0];
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
                    RichTextBox.Text += "\nRobot␣State␣:␣" +((StateRobot)(msgPayload[0])).ToString() +"␣-␣" + instant.ToString() + "␣ms"));
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
                        rcvState = StateReception.FunctionLSB;
                        //RichTextBox.Text += "IN";
                    }
                    break;
                case StateReception.FunctionMSB:
                    msgDecodedFunction += c;
                    rcvState = StateReception.PayloadLengthLSB;
                    //RichTextBox.Text += "functionMSB";
                    break;
                case StateReception.FunctionLSB:
                    msgDecodedFunction += c;
                    rcvState = StateReception.FunctionMSB;
                    //RichTextBox.Text += "functionLSB";
                    break;
                case StateReception.PayloadLengthMSB:
                    msgDecodedPayloadLength += c;
                    rcvState = StateReception.Payload;
                    Array.Resize(ref msgDecodedPayload, msgDecodedPayload.Length + msgDecodedPayloadLength);
                    break;
                case StateReception.PayloadLengthLSB:
                    msgDecodedPayloadLength += c;
                    rcvState = StateReception.PayloadLengthMSB;
                    break;
                case StateReception.Payload:
                    //RichTextBox.Text += msgDecodedPayloadIndex;
                    msgDecodedPayloadIndex += 1;
                    if (msgDecodedPayloadIndex == msgDecodedPayloadLength){
                        rcvState = StateReception.CheckSum;
                        }
                    msgDecodedPayload[msgDecodedPayloadIndex-1] = c;
                    
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
            functionTestValue
        }


        IDfonction func = IDfonction.TextTransmission;
        private void buttonTest_Click(object sender, RoutedEventArgs e)
        {
            byte[] byteList = new byte[] { 0x42, 0x6f, 0x6e, 0x6a, 0x6f, 0x75, 0x72, 0x0a, 0x0d };
            while (func != IDfonction.functionTestValue)
            {
                switch (func)
                {

                    case IDfonction.TextTransmission:
                        UartEncodeAndSendMessage((byte)func, 9, byteList);
                        func = IDfonction.SetLed;
                        break;
                    case IDfonction.SetLed:
                        UartEncodeAndSendMessage((byte)func, 9, byteList);
                        func = IDfonction.IRdistance;
                        break;
                    case IDfonction.IRdistance:
                        UartEncodeAndSendMessage((byte)func, 9, byteList);
                        func = IDfonction.SpeedRule;
                        break;
                    case IDfonction.SpeedRule:
                        UartEncodeAndSendMessage((byte)func, 9, byteList);
                        func = IDfonction.functionTestValue;
                        break;
                    default:
                        func = IDfonction.TextTransmission;
                        break;

                }
            }
            func = IDfonction.TextTransmission;
        }

        private void buttonClear_Click(object sender, RoutedEventArgs e)
        {
            RichTextBox.Text = "";
            byte[] byteList = new byte[] { 0x10, 0x10, 0x10, 0x10, 0x10, 0x10, 0x10, 0x10, 0x10 };
            UartEncodeAndSendMessage(0x0090, 9, byteList);
        }

        private void TextBlock_TextInput(object sender, TextCompositionEventArgs e)
        {

        }
    }
}
