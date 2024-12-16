using BaballeStalkerQuiMarcheVraiment;
using Constants;
using EventArgsLibrary;
using ExtendedSerialPort;
using MathNet.Numerics.Interpolation;
using MathNet.Spatial.Euclidean;
using System.Text;
using System.Timers;
using Utilities;
using Timer = System.Timers.Timer;

namespace CameraJeVoisProAdapterNS
{
    public class CameraJeVoisProAdapter
    {
// public lababalle balle = new lababalle();

        string port;
        public ReliableSerialPort serialPort;

        public CameraPosition cameraPosition;
        double _xCameraRefRobot, _yCameraRefRobot, _thetaCameraRefRobot;

        double freqConfig = 0.5;

        int offsetX;
        int offsetY;

        Timer timerConfig;

        Dictionary<double, ReferencePointRefCameraList> LUT_RoboCup = new Dictionary<double, ReferencePointRefCameraList>();

        List<DnnElement> dnnElementsList = new List<DnnElement>();

        public CameraJeVoisProAdapter(string port)
        {
            /// La logique du module CameraJeVoisProAdapter est un peu particulière :
            /// Le module se connecte sur le port indiqué, puis transfère les données vers le camera manager
            /// afin que celui-ci puisse lancer le processing dans le cameraAdapter. 
            /// La raison vient du fait que le camera manager est le point d'entrée des données issues des replay
            /// Les extractions DNN sont ensuite transmies au Camera Manager puis au reste du code
            if (port != null)
            {
                this.port = port;
                serialPort = new ReliableSerialPort(this.port, 115200, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One);
                serialPort.OnDataReceivedWithInfoEvent += SerialPort_OnDataReceivedWithInfoEvent; 
                serialPort.Open();

                timerConfig = new Timer(1000 / freqConfig);
                timerConfig.Elapsed += TimerConfig_Elapsed;
                timerConfig.Start();
            }

        }

        private void SerialPort_OnDataReceivedWithInfoEvent(object sender, DataReceivedWithInfoArgs e)
        {
            /// On forward l'event de réception de data sur le port série
            OnDataReceivedWithInfo(e);
        }

        string jeVoisCurrentFrame = "";

        List<byte> listByteReceived = new List<byte>();

        public int nbBytesReceived { get; set; }

        public void ProcessJeVoisData(byte c)
        {
            listByteReceived.Add(c);

            if (c == 0x0A)
            {
                /// On a une fin de trame
                /// on convertit la liste coourante en string et on process1152
                /// #-

                var tab = listByteReceived.ToArray();

                jeVoisCurrentFrame = Encoding.UTF8.GetString(tab, 0, tab.Length);

                //Console.WriteLine("Cam " + port + " : " + jeVoisCurrentFrame);

                var splittedFrame = jeVoisCurrentFrame.Split(new char[] { ' ', '\r', '\n', ':' });

                if (splittedFrame.Length > 0)
                {
                    lababalle.x = splittedFrame[0];
                    lababalle.y= splittedFrame[1];
                    lababalle.radius = splittedFrame[2];

                    switch (splittedFrame[0])
                    {
                        case "engine":
                            if (splittedFrame.Length >= 7)
                            {
                                switch (splittedFrame[2])
                                {
                                    case "RobocupFront":
                                        cameraPosition = CameraPosition.RoboCupFront;
                                        _xCameraRefRobot = 0.161;
                                        _yCameraRefRobot = -0.011;
                                        _thetaCameraRefRobot = 0;
                                        break;
                                    case "RobocupLeft":
                                        cameraPosition = CameraPosition.RoboCupLeft;
                                        _xCameraRefRobot = -0.049;
                                        _yCameraRefRobot = 0.195;
                                        _thetaCameraRefRobot = Math.PI / 2;
                                        break;
                                    case "RobocupRight":
                                        cameraPosition = CameraPosition.RoboCupRight;
                                        _xCameraRefRobot = -0.071;
                                        _yCameraRefRobot = -0.196;
                                        _thetaCameraRefRobot = -Math.PI / 2;
                                        break;
                                    case "RobocupBack":
                                        cameraPosition = CameraPosition.RoboCupBack;
                                        _xCameraRefRobot = -0.176;
                                        _yCameraRefRobot = 0.011;
                                        _thetaCameraRefRobot = Math.PI;
                                        break;
                                    case "EurobotFront":
                                        cameraPosition = CameraPosition.EurobotFront;
                                        _xCameraRefRobot = 0.10;
                                        _yCameraRefRobot = 0.11;
                                        _thetaCameraRefRobot = Math.PI / 6;
                                        break;
                                    case "EurobotBack":
                                        cameraPosition = CameraPosition.EurobotBack;
                                        _xCameraRefRobot = -0.75;
                                        _yCameraRefRobot = 0.13;
                                        _thetaCameraRefRobot = 5*Math.PI / 6;
                                        break;
                                    //case "EurobotLeft":
                                    //    cameraPosition = CameraPosition.EurobotLeft;
                                    //    _xCameraRefRobot = 0.015;
                                    //    _yCameraRefRobot = 0.14;
                                    //    _thetaCameraRefRobot = Math.PI / 2;
                                    //    break;
                                    case "EurobotRight":
                                        cameraPosition = CameraPosition.EurobotRight;
                                        _xCameraRefRobot = -0.015;
                                        _yCameraRefRobot = -0.14;
                                        _thetaCameraRefRobot = -Math.PI / 2;
                                        break;
                                    default:
                                        break;
                                }

                                int.TryParse(splittedFrame[3], out offsetX);
                                int.TryParse(splittedFrame[4], out offsetY);
                                OnCameraOffset(cameraPosition, offsetX, offsetY);
                            }
                            break;
                        case "MARK":
                            if(splittedFrame.Length >=2)
                            {
                                if (splittedFrame[1] == "START")
                                {
                                    /// On fait une copie de la liste, on la transmet et on la reset
                                    List<DnnElement> transferList = new List<DnnElement>();
                                    foreach(var elt in dnnElementsList)
                                    {
                                        transferList.Add(elt);
                                    }
                                    dnnElementsList.Clear();
                                    //Console.WriteLine("Camera " + port + " : nb elements = " + transferList.Count.ToString());
                                    OnDnnExtraction(transferList, cameraPosition);
                                }
                            }
                            break;
                        case "N2":
                            if (splittedFrame.Length >= 7)
                            {
                                try
                                {
                                    DnnElement elt = new DnnElement();
                                    elt.type = splittedFrame[1];
                                    switch (elt.type)
                                    {
                                        case "ballon":
                                            elt.TheoreticalWidthObject = 0.22;
                                            elt.TheoreticalHeightObject = 0.22;
                                            elt.IsObjectOnGround = true;
                                            break;
                                        case "poteau":
                                            //elt.TheoreticalWidthObject = 0.15;
                                            elt.TheoreticalHeightObject = 1.0;
                                            elt.IsObjectOnGround = true;
                                            break;
                                        case "valise":
                                            elt.IsObjectOnGround = true;
                                            elt.TheoreticalHeightObject = 0.7;
                                            break;
                                        case "robot_rct":
                                            elt.IsObjectOnGround = true;
                                            elt.TheoreticalHeightObject = 0.7;
                                            //elt.TheoreticalWidthObject = 0.6;
                                            break;
                                        case "robot":
                                            elt.IsObjectOnGround = true;
                                            elt.TheoreticalHeightObject = 0.6;
                                            //elt.TheoreticalWidthObject = 0.6;
                                            break;
                                        case "but":
                                            elt.TheoreticalWidthObject = 2.2;
                                            //elt.TheoreticalHeightObject = 1.1;
                                            elt.IsObjectOnGround = true;
                                            break;
                                        case "tag_bleu":
                                            elt.TheoreticalWidthObject = 0.1;
                                            elt.TheoreticalHeightObject = 0.1;
                                            elt.IsObjectOnGround = false;
                                            break;
                                        case "tag_rouge":
                                            elt.TheoreticalWidthObject = 0.1;
                                            elt.TheoreticalHeightObject = 0.1;
                                            elt.IsObjectOnGround = false;
                                            break;
                                        case "gateau_brun":
                                            elt.TheoreticalWidthObject = 0.12;
                                            elt.IsObjectOnGround = true;
                                            break;
                                        case "gateau_rose":
                                            elt.TheoreticalWidthObject = 0.12;
                                            elt.IsObjectOnGround = true;
                                            break;
                                        case "gateau_jaune":
                                            elt.TheoreticalWidthObject = 0.12;
                                            elt.IsObjectOnGround = true;
                                            break;
                                        case "balise_verte":
                                            elt.TheoreticalWidthObject = 0.12;
                                            elt.IsObjectOnGround = false;
                                            break;
                                        case "balise_bleue":
                                            elt.TheoreticalWidthObject = 0.12;
                                            elt.IsObjectOnGround = false;
                                            break;
                                        case "balise_rouge":
                                            elt.TheoreticalWidthObject = 0.12;
                                            elt.IsObjectOnGround = false;
                                            break;
                                        case "balle":
                                            elt.TheoreticalWidthObject = 0.022;
                                            elt.IsObjectOnGround = true;
                                            break;
                                        default:
                                            break;
                                    }
                                    elt.confidence = double.Parse(splittedFrame[2]);

                                    /// Centre 0;0 au centre de l'image
                                    /// Negatif en X à Gauche
                                    /// Negatif en Y en haut
                                    /// On récupère en premier le point haut gauche de la Bounding Box
                                    /// Et ensuite, la largeur et hauteur
                                    /// Tout est en pixels

                                    var XHautGauche = double.Parse(splittedFrame[3]);
                                    var YHautGauche = -double.Parse(splittedFrame[4]);
                                    var eltWidth = double.Parse(splittedFrame[5]);
                                    var eltHeight = double.Parse(splittedFrame[6]);

                                    /// Vérfication pour savoir si l'objet est coupé en haut / bas ou sur les côtés
                                    int largeurImagePixel = 1920;
                                    int hauteurImagePixel = 1080;
                                    if (XHautGauche + eltWidth >= largeurImagePixel / 2 - 1 || XHautGauche <= -largeurImagePixel / 2 + 1)
                                        elt.croppedInWidth = true;
                                    if (YHautGauche >= +hauteurImagePixel / 2 - 1 || YHautGauche - eltHeight <= -hauteurImagePixel / 2 + 1)
                                        elt.croppedInHeight = true;

                                    /// Correction de l'offset : ATTENTION au plus du au signe moins sur Y
                                    XHautGauche -= offsetX;
                                    YHautGauche += offsetY;

                                    elt.SetPositionAndSize(XHautGauche, YHautGauche, eltWidth, eltHeight);

                                    /// Calcul de la distance en utilisant les valeurs théoriques si elles sont connues et que les objets ne sont pas croppés
                                    /// On prend préférentiellement la distance obtenue par le largeur, puis par la hauteur si elles ne sont pas bonnes.
                                    double? distanceObjet = null;
                                    if (elt.TheoreticalHeightObject != null && !elt.croppedInHeight)
                                    {
                                        /// On connait la largeur théorique de l'élément et il n'est pas croppé en hauteur
                                        distanceObjet = GetDistanceObjet(eltHeight, (double)elt.TheoreticalHeightObject);
                                    }
                                    else if (elt.TheoreticalWidthObject!=null && !elt.croppedInWidth)
                                    {
                                        /// On connait la largeur théorique de l'élément et il n'est pas croppé en largeur
                                        distanceObjet = GetDistanceObjet(eltWidth, (double)elt.TheoreticalWidthObject);
                                    }

                                    MathNet.Spatial.Euclidean.Point3D? posObjet = null;
                                    //if (elt.type == "ballon")
                                    //    distanceObjet = null;

                                    if (distanceObjet != null)
                                    {
                                        posObjet = DeterminePositionDansEspace(new PointD(elt.xCenterRefCamera, elt.yBottomRefCamera), (double)distanceObjet, cameraPosition);
                                    }
                                    else
                                    {
                                        posObjet = DeterminePositionAuSol(new PointD(elt.xCenterRefCamera, elt.yBottomRefCamera), cameraPosition);
                                    }

                                    //if (elt.type == "ballon" && elt.confidence > 70)
                                    //    Console.WriteLine("ballon - Confidence : " + elt.confidence + " - X : " + posObjet.Value.X.ToString("N2") + " - Y : " + posObjet.Value.Y.ToString("N2") + " - Z : " + posObjet.Value.Z.ToString("N2"));

                                    if (posObjet != null)
                                    {
                                        elt.xRefRobot = posObjet.Value.X;
                                        elt.yRefRobot = posObjet.Value.Y;
                                        elt.zRefRobot = posObjet.Value.Z;
                                        distanceObjet = Math.Sqrt(Math.Pow(elt.xRefRobot, 2) + Math.Pow(elt.yRefRobot, 2) + Math.Pow(elt.zRefRobot, 2));

                                        elt.widthObject = GetWidthObjet(elt.widthRefCamera, (double)distanceObjet);
                                        elt.heightObject = GetWidthObjet(elt.heightRefCamera, (double)distanceObjet);

                                        ///On fait bouger les éléments détectés en fonction de la position de la caméra
                                        var ptRotated = Toolbox.Rotate(new PointD(elt.xRefRobot, elt.yRefRobot), new PointD(0, 0), _thetaCameraRefRobot);
                                        var ptRefRobot = new PointD(ptRotated.X + _xCameraRefRobot, ptRotated.Y + _yCameraRefRobot);

                                        elt.distanceRefRobot = Math.Sqrt(Math.Pow(ptRefRobot.X, 2) + Math.Pow(ptRefRobot.Y, 2));
                                        elt.angleRefRobot = Math.Atan2(ptRefRobot.Y, ptRefRobot.X);

                                        elt.xRefRobot = ptRefRobot.X;
                                        elt.yRefRobot = ptRefRobot.Y;

                                        if (elt.type.Contains("balise") && elt.confidence>35)   /// Très très très important sur les balises !!!!!
                                        {
                                            dnnElementsList.Add(elt);
                                            //Console.WriteLine("RSSI balise : " + elt.confidence);
                                        }
                                        else if (!elt.type.Contains("balise"))
                                            dnnElementsList.Add(elt);
                                    }
                                    else
                                    { 
                                        //Console.WriteLine("Objet non positionné : "+elt.type); 
                                    }
                                }
                                catch
                                {
                                    Console.WriteLine("Exception CameraJeVoisProAdapter : ProcessJeVoisData - N2");
                                }
                            }
                            break;
                        case "OK":
                            break;
                        default:
                            break;
                    }
                }

                listByteReceived.Clear();
            }
        }


        static public double GetDistanceObjet(double tailleApparente, double tailleTheorique)
        {
            double largeurApparenteBallon22cmA2m = 140; //A mesurer
            double tailleTheoriqueballon = 0.22;    
            double distanceObjet = largeurApparenteBallon22cmA2m / tailleApparente * 2 * (tailleTheorique / tailleTheoriqueballon);
            return distanceObjet;
        }

        static public double GetWidthObjet(double largeurObjetImage, double distance)
        {
            double largeurApparenteBallon22cmA2m = 140; //A mesurer
            double largeurObjet = 0.22 * largeurObjetImage / largeurApparenteBallon22cmA2m * distance / 2;
            return largeurObjet;
        }

        static public MathNet.Spatial.Euclidean.Point3D? DeterminePositionAuSol(PointD ptObjetImage, CameraPosition camPos)
        {
            //double zCamera = 0.389;
            //if (camPos ==  CameraPosition.EurobotLeft || camPos ==  CameraPosition.EurobotFront || camPos ==  CameraPosition.EurobotRight || camPos ==  CameraPosition.EurobotBack)
            //    zCamera = 0.305;
            //double angleCamera = GetCameraAngle(camPos);

            ////Alpha : angle entre l'axe optique et l'axe du ptObjetImage
            //double distancePtObjetAxeOptique = Toolbox.Distance(ptObjetImage, new PointD(0, 0));

            ////La valeur distancePixel45Horizontal est la distance au centre de l'image du point sur la ligne 45°
            ////situé sur la ligne horizontale passant par le centre
            //double distancePixel45Horizontal = 743.0;

            //double alpha = Toolbox.DegToRad(distancePtObjetAxeOptique * 45 / distancePixel45Horizontal);
            ////Theta : angle entre la verticale de l'image et la droite origine-camera vers ptObjetImage
            //var theta = -(Math.Atan2(ptObjetImage.Y, ptObjetImage.X) - Math.PI / 2);

            //// Définition du vecteur directeur de l'axe optique
            //Vector3D axeRobot = new Vector3D(1, 0, 0);

            //// Création des matrices de rotation autour de l'axe y
            //var matRotationPlongeeCamera = Matrix3D.RotationAroundYAxis(MathNet.Spatial.Units.Angle.FromRadians(angleCamera));
            //var matRotationAlpha = Matrix3D.RotationAroundYAxis(MathNet.Spatial.Units.Angle.FromRadians(-alpha));

            //// Appliquer la rotation à la ligne
            //var axeOptique = axeRobot.TransformBy(matRotationPlongeeCamera);
            //var axeAlpha = axeOptique.TransformBy(matRotationAlpha);
            //var matRotationTheta = Matrix3D.RotationAroundArbitraryVector(axeOptique.Normalize(), MathNet.Spatial.Units.Angle.FromRadians(theta));
            //var axeObjet = axeAlpha.TransformBy(matRotationTheta);

            var distanceMax = 100;

            double zCamera;
            Vector3D axeObjet;
            DeterminePositionObjetCameraBase(ptObjetImage, camPos, out zCamera, out axeObjet);

            Line3D ligneObjet = new Line3D(new MathNet.Spatial.Euclidean.Point3D(0, 0, zCamera),
                new MathNet.Spatial.Euclidean.Point3D(0 + distanceMax * axeObjet.X, 0 + distanceMax * axeObjet.Y, zCamera + distanceMax * axeObjet.Z));
            var ptOnFloor = ligneObjet.IntersectionWith(new MathNet.Spatial.Euclidean.Plane(0, 0, 1, 0));
            if (ptOnFloor == null)
                ;
            return ptOnFloor;
        }

        static public MathNet.Spatial.Euclidean.Point3D? DeterminePositionDansEspace(PointD ptObjetImage, double distance, CameraPosition camPos)
        {
            /// L'objectif est de déterminer l'axe du référentiel robot dans lequel se situe l'objet observé sur l'écran.
            /// Ainsi on pourra déterminer le pt d'intersection de cet axe avec le sol pour trouver le position d'un objet au sol ou
            /// déterminer l'emplacement de l'objet en fonction de sa distance si celui ci est en l'air

            double zCamera;
            Vector3D axeObjet;
            DeterminePositionObjetCameraBase(ptObjetImage, camPos, out zCamera, out axeObjet);

            var ptObjet = new MathNet.Spatial.Euclidean.Point3D(0 + distance * axeObjet.X, 0 + distance * axeObjet.Y, zCamera + distance * axeObjet.Z);

            return ptObjet;
        }

        private static void DeterminePositionObjetCameraBase(PointD ptObjetImage, CameraPosition camPos, out double zCamera, out Vector3D axeObjet)
        {
            /// Pour construire la direction de l'objet :
            /// On part de l'axe X du robot (en face de la face avant, plan horizontal), Y est sur le côté Gauche et Z en haut : repère direct
            /// L'axe optique est celui du robot avec une rotation d'angle angleCamera par rapport à l'axe Y
            /// Alpha est l'angle entre l'axe optique et l'axe de l'objet, il est lié à la distance apparente entre l'objet dans l'image et le centre de l'image.
            /// Theta est l'angle entre la plan vertical passant par l'axe optique et le plan (axe optique - pt objet). Dans l'image, theta est l'angle entre la verticale 
            /// passant le centre optique et la droite centre optique-pt objet.
            /// 
            /// Pour trouver l'axe de l'objet, on part de l'axe optique calculé précédemment, puis on effectue une rotation d'axe Y et d'angle Alpha 
            /// permettant de se déplacer sur le cercle de centre axe optique et passant par l'objet.
            /// Ensuite, on effectue une rotation d'axe l'axe optique et d'angle theta, le nouvel axe est l'axe de l'objet.


            zCamera = 0.389;
            if (camPos == CameraPosition.EurobotLeft || camPos == CameraPosition.EurobotFront || camPos == CameraPosition.EurobotRight || camPos == CameraPosition.EurobotBack)
                zCamera = 0.305;
            double angleCamera = GetCameraAngle(camPos);

            //Alpha : angle entre l'axe optique et l'axe du ptObjetImage
            double distancePtObjetAxeOptique = Toolbox.Distance(ptObjetImage, new PointD(0, 0));

            //La valeur distancePixel45Horizontal est la distance au centre de l'image du point sur la ligne 45°
            //situé sur la ligne horizontale passant par le centre
            double distancePixel45Horizontal = 950; //Validé sur des tests RoboCup

            double alpha = Math.Atan2(distancePtObjetAxeOptique, distancePixel45Horizontal);// Toolbox.DegToRad(distancePtObjetAxeOptique * 45 / distancePixel45Horizontal);
            //Theta : angle entre la verticale de l'image et la droite origine-camera vers ptObjetImage
            var theta = -(Math.Atan2(ptObjetImage.Y, ptObjetImage.X) - Math.PI / 2);

            // Définition du vecteur directeur de l'axe optique
            Vector3D axeRobot = new Vector3D(1, 0, 0);

            // Création des matrices de rotation autour de l'axe y
            var matRotationPlongeeCamera = Matrix3D.RotationAroundYAxis(MathNet.Spatial.Units.Angle.FromRadians(angleCamera));
            var matRotationAlpha = Matrix3D.RotationAroundYAxis(MathNet.Spatial.Units.Angle.FromRadians(-alpha));

            // Appliquer la rotation à la ligne
            var axeOptique = axeRobot.TransformBy(matRotationPlongeeCamera);
            var axeAlpha = axeOptique.TransformBy(matRotationAlpha);
            var matRotationTheta = Matrix3D.RotationAroundArbitraryVector(axeOptique.Normalize(), MathNet.Spatial.Units.Angle.FromRadians(theta));
            axeObjet = axeAlpha.TransformBy(matRotationTheta);
        }

        public static double GetCameraAngle(CameraPosition camPos)
        {
            double angleCamera = 0;
            if (camPos == CameraPosition.RoboCupLeft || camPos == CameraPosition.RoboCupRight || camPos == CameraPosition.RoboCupBack || camPos == CameraPosition.RoboCupFront)
                angleCamera = Toolbox.DegToRad(8);
            else if (camPos == CameraPosition.EurobotFront || camPos == CameraPosition.EurobotLeft || camPos == CameraPosition.EurobotRight || camPos == CameraPosition.EurobotBack)
                angleCamera = Toolbox.DegToRad(30);
            else
                Console.WriteLine("CameraJeVoisProAdapter : camera de type inconnu");
            return angleCamera;
        }

        private void TimerConfig_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (serialPort != null)
            {
                if (serialPort.IsOpen)
                {
                    try
                    {
                        serialPort.WriteLine("setpar serout USB");
                        serialPort.WriteLine("getpar nickname");
                        serialPort.WriteLine("setpar serstyle Normal");
                        serialPort.WriteLine("setpar sermark Start");
                    }
                    catch
                    {
                        Console.WriteLine("Exception CameraJeVoisProAdapter - TimerConfig_Elapsed");
                    }
                }
            }
        }

        //public void SetOffSet(int offsetX, int offsetY)
        //{
        //    this.offsetX = offsetX;
        //    this.offsetY = offsetY;
        //}

        //public void OnCameraOffsetReceived(object sender, CameraOffsetArgs e)
        //{
        //    if (e.cameraPosition == cameraPosition)
        //    {
        //        offsetX = e.xOffset;
        //        offsetY = e.yOffset;
        //    }
        //}

        //public void OnCameraCalibrationFileReceived(object sender, CameraCalibrationFileArgs e)
        //{
        //    if (cameraPosition == e.CameraPosition)
        //        GenerateCameraLookUpTable(e.CalibrationFileName);
        //}

        //List<ArucoElement> ArucoLut = new List<ArucoElement>();


        //public void GenerateCameraLookUpTable(string lutFileName)
        //{
        //    ArucoLut = new List<ArucoElement>();
        //    using (var reader = new StreamReader(lutFileName))
        //    {
        //        List<int> ThetaAruco = new List<int>();
        //        List<int> x1List = new List<int>();
        //        List<int> x2List = new List<int>();
        //        List<int> x3List = new List<int>();
        //        List<int> x4List = new List<int>();
        //        List<int> y1List = new List<int>();
        //        List<int> y2List = new List<int>();
        //        List<int> y3List = new List<int>();
        //        List<int> y4List = new List<int>();

        //        while (!reader.EndOfStream)
        //        {
        //            var line = reader.ReadLine();
        //            var values = line.Replace('.', ',').Split(';');
        //            int pos = 0;

        //            ArucoElement lutElement = new ArucoElement();
        //            lutElement.xRefProjection = double.Parse(values[pos++]);
        //            lutElement.yRefProjection = double.Parse(values[pos++]);
        //            lutElement.thetaRefProjection = Toolbox.DegToRad(double.Parse(values[pos++]));

        //            double x = int.Parse(values[pos++]);
        //            double y = int.Parse(values[pos++]);
        //            lutElement.corner1RefCameraImage = new PointD(x, y);
        //            x = int.Parse(values[pos++]);
        //            y = int.Parse(values[pos++]);
        //            lutElement.corner2RefCameraImage = new PointD(x, y);
        //            x = int.Parse(values[pos++]);
        //            y = int.Parse(values[pos++]);
        //            lutElement.corner3RefCameraImage = new PointD(x, y);
        //            x = int.Parse(values[pos++]);
        //            y = int.Parse(values[pos++]);
        //            lutElement.corner4RefCameraImage = new PointD(x, y);

        //            lutElement.xRefCameraImage = (lutElement.corner1RefCameraImage.X + lutElement.corner2RefCameraImage.X + lutElement.corner3RefCameraImage.X + lutElement.corner4RefCameraImage.X) / 4.0;
        //            lutElement.yRefCameraImage = (lutElement.corner1RefCameraImage.Y + lutElement.corner2RefCameraImage.Y + lutElement.corner3RefCameraImage.Y + lutElement.corner4RefCameraImage.Y) / 4.0;

        //            ArucoLut.Add(lutElement);

        //            /// Génération du symétrique par rapport à l'axe vertical
        //            if (lutElement.xRefProjection != 0)
        //            {
        //                ArucoElement lutElementSym = new ArucoElement();

        //                lutElementSym.xRefProjection = -lutElement.xRefProjection;
        //                lutElementSym.yRefProjection = lutElement.yRefProjection;
        //                lutElementSym.xRefCameraImage = -lutElement.xRefCameraImage;
        //                lutElementSym.yRefCameraImage = lutElement.yRefCameraImage;
        //                lutElementSym.thetaRefProjection = 180 - lutElement.thetaRefProjection;
        //                lutElementSym.corner1RefCameraImage = new PointD(-lutElement.corner1RefCameraImage.X, lutElement.corner1RefCameraImage.Y);
        //                lutElementSym.corner2RefCameraImage = new PointD(-lutElement.corner2RefCameraImage.X, lutElement.corner2RefCameraImage.Y);
        //                lutElementSym.corner3RefCameraImage = new PointD(-lutElement.corner3RefCameraImage.X, lutElement.corner3RefCameraImage.Y);
        //                lutElementSym.corner4RefCameraImage = new PointD(-lutElement.corner4RefCameraImage.X, lutElement.corner4RefCameraImage.Y);

        //                ArucoLut.Add(lutElementSym);
        //            }
        //        }
        //    }
        //}

        //Output events
        public event EventHandler<DataReceivedWithInfoArgs> OnCameraDataReceivedWithInfoEvent;
        public virtual void OnDataReceivedWithInfo(DataReceivedWithInfoArgs evt)
        {
            var handler = OnCameraDataReceivedWithInfoEvent;
            if (handler != null)
            {
                handler(this, evt);
            }
        }

        public event EventHandler<DnnDetectionArgs> OnDnnListDetectionEvent;
        public virtual void OnDnnExtraction(List<DnnElement> elementsList, CameraPosition cameraPosition)
        {
            var handler = OnDnnListDetectionEvent;
            if (handler != null)
            {
                handler(this, new DnnDetectionArgs { EltList = elementsList, CameraPosition = cameraPosition });
            }
        }

        public event EventHandler<CameraOffsetArgs> OnCameraOffsetEvent;
        public virtual void OnCameraOffset(CameraPosition camPos, int offsetX, int offSetY)
        {
            var handler = OnCameraOffsetEvent;
            if (handler != null)
            {
                handler(this, new CameraOffsetArgs { CameraPosition = camPos, xOffset = offsetX, yOffset = offsetY });
            }
        }

        //public event EventHandler<ArucoListDetectionArgs> OnCameraArucoExtractionEvent;
        //public virtual void OnCameraArucoExtraction(ConcurrentBag<LocationExtended> arucoBag, CameraPosition cameraPosition)
        //{
        //    var handler = OnCameraArucoExtractionEvent;
        //    if (handler != null)
        //    {
        //        handler(this, new ArucoListDetectionArgs { Value = arucoBag, CameraPosition = cameraPosition });
        //    }
        //}



        
    }
    public class ArucoElement
    {
        public int id;
        public double xRefProjection;
        public double yRefProjection;
        public double thetaRefProjection;
        public double xRefCameraImage;
        public double yRefCameraImage;
        public PointD corner1RefCameraImage;
        public PointD corner2RefCameraImage;
        public PointD corner3RefCameraImage;
        public PointD corner4RefCameraImage;

        public void CopyArucoElement(ArucoElement elt)
        {
            xRefProjection = elt.xRefProjection;
            yRefProjection = elt.yRefProjection;
            thetaRefProjection = elt.thetaRefProjection;
            id = elt.id;
        }
    }

    public class PointCamera
    {
        public PointD PtRefCamera;
        public double distanceRefCamera;
        //public double angleRefCamera;

        public PointCamera(PointD ptRefCamera, double distance)
        {
            PtRefCamera = ptRefCamera;
            distanceRefCamera = distance;
            //angleRefCamera = angle;
        }
    }

    public class ReferencePointRefCameraList
    {
        public List<PointCamera> ptList;
        public IInterpolation InterpolFunction;

        public ReferencePointRefCameraList()
        {
            ptList = new List<PointCamera>();
        }
    }

    public class DnnElement
    {
        /// Variables récupérées
        public string type { get;  set; }
        public double confidence { get; set; }
        public double xCenterRefCamera { get; set; }
        public double yCenterRefCamera { get; set; }
        public double yBottomRefCamera { get; set; }
        public double widthRefCamera { get; set; }
        public double heightRefCamera { get; set; }

        public bool croppedInWidth = false;
        public bool croppedInHeight = false;

        public double? TheoreticalWidthObject = null;
        public double? TheoreticalHeightObject = null;
        public bool IsObjectOnGround = true;

        /// Variables générées
        public double angleRefRobot { get; set; }
        public double distanceRefRobot { get; set; }
        public double xRefRobot { get; set; }
        public double yRefRobot { get; set; }
        public double zRefRobot { get; set; }
        public double widthObject { get; set; }
        public double heightObject { get; set; }

        public override string ToString()
        {
            return "\nDNN : " + type.ToString() + " - xCenterRefCam : " + xCenterRefCamera.ToString() + " - yBottomRefCam : " + yBottomRefCamera.ToString()
                + " - distance : " + distanceRefRobot.ToString() + " - xRefRobot : " + xRefRobot.ToString() + " - yRefRobot : " + yRefRobot.ToString(); 
;
        }

        internal void SetPositionAndSize(double xHautGauche, double yHautGauche, double eltWidth, double eltHeight)
        {
            widthRefCamera = eltWidth;
            heightRefCamera = eltHeight;

            /// changement référentiel vers origine (0;0) au centre de l'image et axe x positif vers la droite, y positif vers le haut
            xCenterRefCamera = xHautGauche + widthRefCamera / 2;
            yCenterRefCamera = yHautGauche - heightRefCamera / 2;
            yBottomRefCamera = yHautGauche - heightRefCamera;
        }

    }

    public class DnnDetectionArgs : EventArgs
    {
        public CameraPosition CameraPosition;
        public List<DnnElement> EltList { get; set; }
    }

    public class CameraOffsetArgs : EventArgs
    {
        public CameraPosition CameraPosition;
        public int xOffset { get; set; }
        public int yOffset { get; set; }
    }

    //public class ArucoListDetectionArgs : EventArgs
    //{
    //    public CameraPosition CameraPosition;
    //    public ConcurrentBag<LocationExtended> Value { get; set; }
    //}
}
