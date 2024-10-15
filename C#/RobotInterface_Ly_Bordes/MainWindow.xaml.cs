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
        String ReceivedText = "";
        
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

        private void TimerAffichage_Tick(object? sender, EventArgs e)
        {
            RichTextBox.Text = ReceivedText;
            
        }
       
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }
        public void SerialPort1_DataReceived(object sender, DataReceivedArgs e)
        {
             ReceivedText += Encoding.UTF8.GetString(e.Data, 0, e.Data.Length);
        }
        private void buttonEnvoyer_Click(object sender, RoutedEventArgs e)
        {

           if (buttonEnvoyer.Background == Brushes.Beige || buttonEnvoyer.Background != Brushes.RoyalBlue)
            buttonEnvoyer.Background = Brushes.RoyalBlue;
           else if (buttonEnvoyer.Background == Brushes.RoyalBlue)
            {
                buttonEnvoyer.Background = Brushes.Beige;
            }
            
        }
        private void sendMessage()
        {
            serialPort1.WriteLine(TextBoxEmission.Text);

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
