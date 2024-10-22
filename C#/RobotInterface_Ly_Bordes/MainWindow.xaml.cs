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
            if (robot.byteListReceived.Count != 0)
            {
                RichTextBox.Text += "0x" + robot.byteListReceived.Dequeue().ToString("X2") + " ";

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
            byte[] byteList = new byte[] { 115, 97, 108, 117, 116, 32, 108, 121, 32, 109, 105, 110, 104 };
          
            ///for (int i = 0; i < 20; i++)
            ///{
            /// byteList[i] = (byte)(2 * i);
            ///}
            serialPort1.Write(byteList, 0, byteList.Length);
        }
        private void sendMessage()
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(TextBoxEmission.Text);
            for (int i = 0; i < TextBoxEmission.Text.Length; i++)
            {
                robot.byteListReceived.Enqueue(byteArray[i]);
            }
        }
        private void TextBoxEmission_TextChanged(object sender, TextChangedEventArgs e)
        {
          
        }

        private void TextBoxEmission_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                sendMessage();

                TextBoxEmission.Text = "";

            }
            
        }
    }
}
