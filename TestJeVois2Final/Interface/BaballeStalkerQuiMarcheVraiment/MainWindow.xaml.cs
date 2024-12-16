using ExtendedSerialPort;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;
using Position;
using MathNet.Spatial.Euclidean;
using SciChart.Charting.Visuals;
using SciChart.Charting3D.Model;
using SciChart.Charting3D.PointMarkers;
using System.Windows.Controls;
using System.Windows.Media;
using SciChart.Charting3D;
using SciChart.Charting.Model.DataSeries;
using System.Diagnostics;
using System.Linq;
using MathNet.Numerics;
using System.Windows.Documents;

namespace BaballeStalkerQuiMarcheVraiment
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : UserControl
    {
        //public lababalle balle = new lababalle();

        ReliableSerialPort serialPort1;

        Queue<byte> queueReceptionUart = new Queue<byte>();
        Position.Position position = new Position.Position();
        XyzDataSeries3D<double>? xyzDataSeries3D = new XyzDataSeries3D<double>();
        XyzDataSeries3D<double>? xyzSimu3D = new XyzDataSeries3D<double>();
        double[] linearRegX=new double [30];
        double[] linearRegY=new double [30];
        double[] linearRegZ=new double [30];

        public MainWindow()
        {
            SciChartSurface.SetRuntimeLicenseKey("qXA9gxzWx1MEaZzdvhzZUytiy/Qb1zMDcr2FtXsxHbxNaZ46WtKxf4vHB/rj54IXB0TnTIzx4db3xQ2euI4eTJGywGOBYp7rmYRyUJkfiOaMzc/l1WENSoNvApT4SQIiZqcbTCltHIcv3wUgrSUAqpn2gCjN8rXYIQ/IdHjfTqstJhBMLsnj1v1a3anB9xW6zBfVW8E6k4pgNsUdxlvQf2COgHy2R/BQEPaOShNxEfLi0OJTwW24Cd/50jVg1upPLL4Ii3ceMog7rBRZzl0kehO0x4WkNYDb51KmCZpqmQbjCxMCTHFIlH1q8qPu40/VMD6h8IXwaQv940zTly854ewLxhaZO4f8ZOrARNL9rHTNuDKHdkUcjIXK1dWVocFrS+gzGJq3c9VGbNV4cP5ngOa/ByzLaZ8WkjRjS2bOLLNF8TdcVojC0xdcD1Iu8hQIilLMCNwsizkQVA071+Ei2wF4fqOr6oy+iFF7lno6CK2E6TltmiPsz64Q2mOcZgKbtqwlSA7m");            //CamerasManager camera = new CamerasManager();
            InitializeComponent();

            DispatcherTimer timerAffichage;
            timerAffichage = new DispatcherTimer();
            timerAffichage.Interval = new TimeSpan(0, 0, 0, 0, 100);
            timerAffichage.Tick += TimerAffichage_Tick;
            timerAffichage.Start();

            serialPort1 = new ReliableSerialPort("COM6", 115200, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One);
            serialPort1.OnDataReceivedWithInfoEvent += SerialPort1_OnDataReceivedWithInfoEvent;
            serialPort1.Open();

        }

        private void SerialPort1_OnDataReceivedWithInfoEvent(object? sender, EventArgsLibrary.DataReceivedWithInfoArgs e)
        {
            //Debug.WriteLine(Encoding.ASCII.GetString(e.Data));
            foreach (var c in e.Data)
            {
                queueReceptionUart.Enqueue(c);
            }
        }


        string currentString = "";
        public void TimerAffichage_Tick(object sender, EventArgs e)
        {
            while (queueReceptionUart.Count > 0)
            {
                var c = queueReceptionUart.Dequeue();
                if (c != '\r')
                    currentString += System.Text.Encoding.ASCII.GetString(new[] { c });
                else
                {
                    //Debug.WriteLine(currentString);
                    string[] ball = currentString.Split(' ');
                    //if (char.IsDigit(ball[0][0]))
                    //{
                    int i = 0;
                    if (int.TryParse(ball[0], out i))
                    {
                        int x = Int32.Parse(ball[0]);
                        int y = Int32.Parse(ball[1]);
                        int rayon = Int32.Parse(ball[2]);
                        Debug.WriteLine("x: "+x+" y: "+y+" rayon: "+rayon);
                        //SerialReceive.Text = "x: " + x + "\ny: " + y + "\nRayon: " + rayon + "\n";
                        //SerialReceive.ScrollToEnd();
                        //}
                        currentString = "";

                        position.ProcessJeVoisData(x, y, rayon);

                        AnalyzeAndDisplayTrajectory(lababalle.posObj);

                    }
                    else currentString = "";
                }
            }
            //while (Port.Text == "")
            //{
            //    Port.Text = lababalle.port;
            //}
            //SerialReceive.Text += "Coordonnees X:" + lababalle.x+"\n";
            //SerialReceive.Text += "Coordonnees Y:" + lababalle.y + "\n";
            //SerialReceive.Text += "Rayon:" + lababalle.radius + "\n";

        }


        List<Point3D> trajectoireEnCours = new List<Point3D>();
        double[] listAncien= {0,0,0};

        int numberPtUsedForTrajCheck = 4;
        int numberPtUsedForIntercept = 30;
        public void AnalyzeAndDisplayTrajectory(Point3D? posObjet)
        {
            // Gestion de l'affichage de 30 dernières valeurs
            if (xyzDataSeries3D.Count>=30)
            {
                xyzDataSeries3D.RemoveAt(0);
            }
            if (trajectoireEnCours.Count >= 50)
            {
                trajectoireEnCours.RemoveAt(0);
            }
            xyzDataSeries3D.Append(posObjet.Value.X, posObjet.Value.Z, posObjet.Value.Y);
            trajectoireEnCours.Add(posObjet.Value);

            /// Gestion des trajectoires segmentées
            if (trajectoireEnCours.Count > numberPtUsedForTrajCheck)
            {
                /// On calcule la régression linéaire sur les 4 dernières valeurs
                var listPtUtilises = trajectoireEnCours.GetRange(trajectoireEnCours.Count - (numberPtUsedForTrajCheck + 1), numberPtUsedForTrajCheck); // +1 pour laisse le dernier dispo pour valider ou pas l'extrapolation

                var listT = new double[numberPtUsedForTrajCheck];
                for (int i = -numberPtUsedForTrajCheck; i < 0; i++)
                {
                    listT[numberPtUsedForTrajCheck + i] = i;
                }
                var listX = listPtUtilises.Select(o => o.X).ToArray();
                var listY = listPtUtilises.Select(o => o.Y).ToArray();
                var listZ = listPtUtilises.Select(o => o.Z).ToArray();

                bool debug = false;
                if (debug)
                {
                    listX = new double[] { 1.19, 1.15, 1.23, 1.20 };
                    listY = new double[] { 0.03, -0.06, -0.15, -0.24 };
                    listZ = new double[] { 2.12, 2.09, 2.10, 2.05 };
                }

                (double Ax, double Bx) = Fit.Line(listT, listX);
                (double Ay, double By) = Fit.Line(listT, listY);
                (double Az, double Bz) = Fit.Line(listT, listZ);

                //Calcul des résidus
                var ListResiduX = new double[listX.Length];
                var ListResiduY = new double[listX.Length];
                var ListResiduZ = new double[listX.Length];

                double sumError = 0;
                for (int i = 0; i < listX.Length; i++)
                {
                    ListResiduX[i] = listX[i] - (Ax + Bx * listT[i]);
                    ListResiduY[i] = listY[i] - (Ay + By * listT[i]);
                    ListResiduZ[i] = listZ[i] - (Az + Bz * listT[i]);
                    sumError += ListResiduX[i] * ListResiduX[i];
                    sumError += ListResiduY[i] * ListResiduY[i];
                    sumError += ListResiduZ[i] * ListResiduZ[i];
                }

                sumError /= listX.Length;
                double incertitude = Math.Sqrt(sumError);

                Point3D ExpectedPoint = new Point3D(Ax, Ay, Az);
                Point3D RealPoint = trajectoireEnCours[trajectoireEnCours.Count - 1];

                double criterion = incertitude * 7 + 0.05;

                if (Distance(ExpectedPoint, RealPoint) > criterion) xyzDataSeries3D.Clear();


                xyzSimu3D.Clear();
                xyzSimu3D.Append(Ax - Bx * numberPtUsedForTrajCheck, Az - Bz * numberPtUsedForTrajCheck, Ay - By * numberPtUsedForTrajCheck); //Point de référence pour la droite
                xyzSimu3D.Append(Ax, Az, Ay); //Point qui prédit le suivant avec droite
            }
                //Partie d'interception-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
                if (trajectoireEnCours.Count > numberPtUsedForIntercept)
                {
                    var listPtInter = trajectoireEnCours.GetRange(trajectoireEnCours.Count - (numberPtUsedForIntercept + 1), numberPtUsedForIntercept); // +1 pour laisse le dernier dispo pour valider ou pas l'extrapolation

                var listT2 = new double[numberPtUsedForIntercept];
                for (int i = -numberPtUsedForIntercept; i < 0; i++)
                {
                    listT2[numberPtUsedForIntercept + i] = i;
                }
                var listX2 = listPtInter.Select(o => o.X).ToArray();
                var listY2 = listPtInter.Select(o => o.Y).ToArray();
                var listZ2 = listPtInter.Select(o => o.Z).ToArray();

                (double Ax2, double Bx2) = Fit.Line(listT2, listX2);
                (double Ay2, double By2) = Fit.Line(listT2, listY2);
                double[] polyTZ = Fit.Polynomial(listT2, listZ2, 2);

                Point3D InterceptPoint = new Point3D(0, By2*(-Ax2/Bx2)+Ay2, polyTZ[2]*Math.Pow((-Ax2 / Bx2), 2) + polyTZ[1]* (-Ax2 / Bx2) + polyTZ[0]);
                Debug.WriteLine(InterceptPoint);
            }
            //Fit.multidim = Point d'intersection ?
            PointLineSeries3D.DataSeries = xyzSimu3D;
            ScatterSeries3D.DataSeries = xyzDataSeries3D;        
        }

        private void OnUseRhsCoordinates(object sender, RoutedEventArgs e)
        { 
            if (sciChart != null) sciChart.CoordinateSystem = CoordinateSystem3D.RightHanded;
        }
        
        private void ExtrapoleTraj(XyzDataSeries3D<double>? DataSeriesEX3D)
        {

        }

        public static double Distance(Point3D pt1, Point3D pt2)
        {
            if (pt1 != null && pt2 != null)
                return Math.Sqrt((pt2.X - pt1.X) * (pt2.X - pt1.X) + (pt2.Y - pt1.Y) * (pt2.Y - pt1.Y) + (pt2.Z - pt1.Z) * (pt2.Z - pt1.Z));
            else
                return double.PositiveInfinity;
        }

    }

}
