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

        public MainWindow()
        {
            InitializeComponent();
            
            serialPort1 = new ExtendedSerialPort("COM9", 115200, Parity.None, 8, StopBits.One);
            serialPort1.DataReceived += SerialPort1_DataReceived;
            serialPort1.Open();
            DispatcherTimer timerAffichage;
            timerAffichage = new DispatcherTimer();
            timerAffichage.Interval = new TimeSpan(0, 0, 0, 0, 100);
            timerAffichage.Tick += TimerAffichage_Tick;
            timerAffichage.Start();
        }

        private void TimerAffichage_Tick(object sender, EventArgs e)
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

        }
       
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }
        public void SerialPort1_DataReceived(object sender, DataReceivedArgs e)
        {
            for (int i = 0;i <e.Data.Length;i++) {
               robot.byteListReceived.Enqueue(e.Data[i]);
            }
            robot.receivedText += Encoding.ASCII.GetString(e.Data, 0, e.Data.Length);

        }
        private void buttonEnvoyer_Click(object sender, RoutedEventArgs e)
        {
            //byte[] byteList = new byte[] {20,30,40,50};
            //UartEncodeAndSendMessage(20, 4,byteList);
            byte start = 0xFE;
            DecodeMessage(start);

        }
        private void sendMessage()
        {
            serialPort1.Write(TextBoxEmission.Text);
        }
        public byte CalculateChecksum(Int16 msgFunction,int msgPayloadLength, byte[] msgPayload)
        {
           
            // Todo faire checksum pour sof command and playload
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
        private void TextBoxEmission_TextChanged(object sender, TextChangedEventArgs e)
        {
          
        }

        private void TextBoxEmission_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //sendMessage();
                byte[] array = Encoding.ASCII.GetBytes(TextBoxEmission.Text);
                UartEncodeAndSendMessage(20, (Int16)array.Length, array);
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
                    byte receivedChecksum = c;
                        if (calculatedChecksum == receivedChecksum){
                            RichTextBox.Text += "ça marche";
                            rcvState = StateReception.Waiting;
                            }
                        else{ 
                            RichTextBox.Text += "les problèmes";
                            }
                    break;
                default:
                    rcvState = StateReception.Waiting;
                    break;
            }
        }



    }
}
