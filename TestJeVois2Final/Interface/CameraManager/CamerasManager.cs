using CameraJeVoisProAdapterNS;
using EventArgsLibrary;
using ExtendedSerialPort;
using System.Diagnostics;
using System.Timers;
using Timer = System.Timers.Timer;

namespace CamerasManager_NS
{


    public class CamerasManager
    {
        Timer timerEnvoi;
        double freqScanPorts = 0.5;
        Dictionary<string, CameraJeVoisProAdapter> dictionaryCameras = new Dictionary<string, CameraJeVoisProAdapter> ();
        bool LogReplayActivated = false;

        Timer timerTestUnitaire = new Timer(20);

        //CameraJeVoisProAdapter camAdapterTest;// = new CameraJeVoisProAdapter("COM31");

        public CamerasManager()
        {
            timerEnvoi = new Timer(1000 / freqScanPorts);
            timerEnvoi.Elapsed += TimerEnvoi_Elapsed; 
            timerEnvoi.Start();
        }

        private void TimerEnvoi_Elapsed(object sender, ElapsedEventArgs e)
        {
            var portNameList = ReliableSerialPort.SearchPortsWithName("VID_0525&PID_A4A7"); /// Code des caméras JeVois
            
            Console.Write("\nListe des COM des caméras trouvées : ");
            foreach(var port in portNameList)
            {
                Console.Write(port + " " );
                /// Si le dictionnaire des caméras ne contient pas la caméra trouvée, on l'ajoute
                if(!dictionaryCameras.ContainsKey(port)) 
                {
                    var camAdapter = new CameraJeVoisProAdapter(port);
                    camAdapter.OnCameraDataReceivedWithInfoEvent += OnCameraDataWithInfoReceivedEvent;

                    /// Forward vers le reste du robot
                    camAdapter.OnDnnListDetectionEvent += OnDnnExtractionForward;// OnDnnExtractionEventReceived;
                    camAdapter.OnCameraOffsetEvent += OnCameraOffsetForward;

                    dictionaryCameras.Add(port, camAdapter);
                }
                else
                {
                    var camAdapter = dictionaryCameras[port];
                    Console.WriteLine("Cam " + port + " nb Bytes Received : " + camAdapter.nbBytesReceived);
                    if(camAdapter.nbBytesReceived == 0)
                    {
                        //On a perdu la caméra et c'est pas cool :
                        try
                        {
                            camAdapter.serialPort.Close();
                            dictionaryCameras.Remove(port);
                        }
                        catch
                        {
                            Console.WriteLine("Exception Camera Manager - Timer_Envoi_Elapsed");
                        }
                    }
                    camAdapter.nbBytesReceived = 0;
                }
            }
        }

        //double t = 0;
        //int i = 0;
        //private void TimerTest_Elapsed(object sender, ElapsedEventArgs e)
        //{
        //    t += 0.05;
        //    DataReceivedWithInfoArgs elt = new DataReceivedWithInfoArgs();
        //    elt.Data = Encoding.ASCII.GetBytes("N2 ballon 66 "+(123+50*Math.Sin(2*Math.PI*0.5*t)).ToString("N")+ " 224 89 92\n");
        //    elt.Info = "COM13";
        //    camAdapterTest.OnDataReceivedWithInfo(elt);

        //    if (i++ % 1 == 0)
        //    {

        //        elt.Data = Encoding.ASCII.GetBytes("MARK START\nengine 0 RobocupFront 0 0 0 0\n");
        //        elt.Info = "COM99";
        //        camAdapterTest.OnDataReceivedWithInfo(elt);
        //    }
        //}


        private void ProcessReceivedDatafromCameraWithInfo(string port, byte[] data)
        {
            if (!dictionaryCameras.ContainsKey(port))
            {
                var newCamAdapter = new CameraJeVoisProAdapter(port);
                /// Forward vers le reste du robot
                newCamAdapter.OnDnnListDetectionEvent += OnDnnExtractionForward;
                dictionaryCameras.Add(port, newCamAdapter);
            }

            CameraJeVoisProAdapter camAdapter = dictionaryCameras[port];

            /// On gère le flux de données 
            camAdapter.nbBytesReceived += data.Length;
            foreach (var c in data)
            {
                camAdapter.ProcessJeVoisData(c);
            }

            //Console.Write(Encoding.ASCII.GetString(data));
        }

        public void OnCameraDataWithInfoReceivedEvent(object sender, DataReceivedWithInfoArgs e)
        {
            Debug.WriteLine("Nb bytes received : " + e.Data.Length);
            //if (!LogReplayActivated)
            //{
            //    /// On ajoute le cameraAdapter si besoin à la liste des cameraAdapter
            //    string port = e.Info;
            //    byte[] data = e.Data;
            //    ProcessReceivedDatafromCameraWithInfo(port, data);

            //    //Console.Write(Encoding.ASCII.GetString(data));
            //}
        }
        public void OnCameraDataWithInfoForwardToLogRecorderEvent(object sender, DataReceivedWithInfoArgs e)
        {
            ///Fait un forward pour le LogRecorder
            OnCameraDataReceivedWithInfoForwardToLogRecorder(sender, e);
        }
        
        public void OnLogReplayCameraDataReceivedWithInfoEvent(object sender, DataReceivedWithInfoArgs e)
        {
            if (LogReplayActivated)
            {
                /// On ajoute le cameraAdapter si besoin à la liste des cameraAdapter
                string port = e.Info;
                byte[] data = e.Data;
                ProcessReceivedDatafromCameraWithInfo(port, data);

                //Console.Write(Encoding.ASCII.GetString(data));
            }
        }


        public void OnEnableDisableLogReplay(object sender, BoolEventArgs e)
        {
            LogReplayActivated = e.value;
        }

        //public void OnDnnExtractionEventReceived(object sender, DnnDetectionArgs e)
        //{
        //    OnDnnExtractionForward(sender, e.EltList, e.CameraPosition);
        //}

        public event EventHandler<DnnDetectionArgs> OnDnnExtractionEvent;
        public virtual void OnDnnExtractionForward(object initialSender, DnnDetectionArgs e)//, List<DnnElement> eltList, CameraPosition cameraPosition)
        {
            var handler = OnDnnExtractionEvent;
            if (handler != null)
            {
                handler(initialSender, new DnnDetectionArgs { EltList = e.EltList, CameraPosition = e.CameraPosition });
            }
        }

        public event EventHandler<CameraOffsetArgs> OnCameraOffsetEvent;
        public virtual void OnCameraOffsetForward(object initialSender, CameraOffsetArgs e)//, List<DnnElement> eltList, CameraPosition cameraPosition)
        {
            var handler = OnCameraOffsetEvent;
            if (handler != null)
            {
                handler(initialSender, e);
            }
        }

        public event EventHandler<DataReceivedWithInfoArgs> OnDataReceivedWithInfoForwardToLogRecorderEvent;
        public void OnCameraDataReceivedWithInfoForwardToLogRecorder(object sender, DataReceivedWithInfoArgs e)
        {
            var handler = OnDataReceivedWithInfoForwardToLogRecorderEvent;
            if (handler != null)
            {
                handler(sender, new DataReceivedWithInfoArgs { Data = e.Data, Info=e.Info });
            }
        }
    }
}
