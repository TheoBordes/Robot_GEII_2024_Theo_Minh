using MadWizard.WinUSBNet;
using System.Management;
using System.Runtime.InteropServices;
using System.Timers;
using Constants;
namespace USBDriverNS
{
    public class USBDriver
    {
        private string _deviceInterfaceGuid = "{58D07210-27C1-11DD-BD0B-0800200C9a66}"; //GUID inclut dans le fichier .inf du driver Microchip
        private const int VendorID = 0x04D8;
        private const int ProductID = 0x0053;
        private string _serialNumber = "";

        private bool useMultipleUSBInstance = true;

        USBDevice device;
        USBInterface usbInterfaceMicrochip;
        const int rcvBufferSize = 2048;

        byte[] rcvBuffer = new byte[rcvBufferSize];

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        private ManagementEventWatcher _deviceArrivedWatcher;
        private ManagementEventWatcher _deviceRemovedWatcher;


        System.Threading.Thread _usbThread;

        public USBDriver(string SerialNumber)
        {
            _serialNumber = SerialNumber;
            //_deviceInterfaceGuid = GUID;
            _usbThread = new System.Threading.Thread(USBThread);
            _usbThread.IsBackground = true;
            _usbThread.Start();

            AddDeviceArrivedHandler();
            AddDeviceRemovedHandler();
            if (useMultipleUSBInstance)
                UpdateUsbConnection2();
            else
                UpdateUsbConnection();

            System.Timers.Timer timerReconnectionIn = new System.Timers.Timer(100);
            timerReconnectionIn.Elapsed += TimerReconnectionIn_Elapsed;
            timerReconnectionIn.Start();

            System.Timers.Timer timerUsbReceptionWatchdog = new System.Timers.Timer(1000);
            timerUsbReceptionWatchdog.Elapsed += TimerUsbReceptionWatchdog_Elapsed;
            timerUsbReceptionWatchdog.Start();
        }

        private void USBThread()
        {
            while (true)
            {
                try
                {
                    if (usbInterfaceMicrochip != null)
                    {
                        var nbBytesTransfered = usbInterfaceMicrochip.InPipe.Read(rcvBuffer, 0, 128);

                        byte[] usbReceivedBuffer = new byte[nbBytesTransfered];
                        Buffer.BlockCopy(rcvBuffer, 0, usbReceivedBuffer, 0, nbBytesTransfered);

                        nbUsbPacketReceived++;
                        nbUSBBytesReceived += nbBytesTransfered;

                        OnUSBDataReceived(usbReceivedBuffer);
                    }
                }
                catch (Exception e)
                {

                    /// On a une erreur, potentiellement due au débranchement de l'USB, du coup on n'écoute plus
                    Console.WriteLine("Exception USBDriver : USB Thread \n" + e.ToString());
                    isListening = false;
                }

                Thread.Sleep(1);
            }
        }

        int nbUsbPacketReceived = 0;
        int nbUSBBytesReceived = 0;
        private void TimerUsbReceptionWatchdog_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (nbUsbPacketReceived == 0 && _deviceInterfaceGuid == SerialNumbers.CARTE_MOTEUR)
                if (useMultipleUSBInstance)
                    UpdateUsbConnection2();
                else
                    UpdateUsbConnection();
            OnUsbStats(nbUsbPacketReceived, nbUSBBytesReceived);
            nbUsbPacketReceived = 0;
            nbUSBBytesReceived = 0;
        }

        bool isListening = false;
        private void UpdateUsbConnection()
        {
            try
            {
                //On peut avoir un device déjà en usage et donc inaccessible
                device = USBDevice.GetSingleDevice(Guid.Parse(_deviceInterfaceGuid));

                //var devList = USBDevice.GetDevices(Guid.Parse(_deviceInterfaceGuid));
                //device =new USBDevice(devList[0]);
            }
            catch
            {
                Console.WriteLine("Exception USBDriver : UpdateUSBConnection");
            }
            if (device != null)
            {
                /// Si on a trouvé le device
                var usbList = device.Interfaces.Where(usbIf => usbIf.BaseClass == USBBaseClass.VendorSpecific).ToList();
                if (usbList.Count > 0)
                {
                    /// Si il existe une interface de type VendorId
                    var interfaceMatchingListe = usbList.Where(dev => dev.Device.Descriptor.SerialNumber == _serialNumber || dev.Device.Descriptor.SerialNumber == null);
                    if (interfaceMatchingListe.Count() > 0)
                        usbInterfaceMicrochip = interfaceMatchingListe.First();// usbList[0].InPipe.Interface;
                    if (usbInterfaceMicrochip != null)
                    {
                        try
                        {
                            if (!_usbThread.IsAlive)
                                _usbThread.Start();
                            //usbInterfaceMicrochip.InPipe.BeginRead(rcvBuffer, 0, rcvBufferSize, new AsyncCallback(ReceiveUsbDataCallback), null);
                            isListening = true;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("USBDriver - USB problem : " + e.ToString());
                        }
                    }
                }
            }
        }

        int counterException = 0;
        private void UpdateUsbConnection2()
        {
            USBDeviceInfo[] devList = null;
            try
            {
                //On recupere la liste des peripheriques disponibles ayant l'UUID specifié
                devList = USBDevice.GetDevices(Guid.Parse(_deviceInterfaceGuid));
            }
            catch
            {
                if (counterException++ % 20 == 0)
                    Console.WriteLine("Exception USBDriver : UpdateUSBConnection2");

            }
            if (devList != null)
            {
                //On balaye la liste afin de trouver le peripherique qui nous interresse
                foreach (USBDeviceInfo devInfo in devList)
                {
                    try
                    {
                        //On construit le Device a partir des USBDeviceInfo
                        USBDevice dev = new USBDevice(devInfo);

                        //Si le device contient le champ serialNumber
                        if (dev.Descriptor.SerialNumber != null)
                        {
                            //On verifie s'il correspond au peripherique recherché
                            if (dev.Descriptor.SerialNumber == _serialNumber)
                            {
                                switch (dev.Descriptor.SerialNumber)
                                {
                                    case SerialNumbers.CARTE_MOTEUR:
                                        Console.WriteLine("USB: Peripherique trouvé: " + dev.Descriptor.SerialNumber + ": CARTE_MOTEUR");
                                        break;
                                    case SerialNumbers.CONCENTRATEUR_UART:
                                        Console.WriteLine("USB: Peripherique trouvé: " + dev.Descriptor.SerialNumber + ": CONCENTRATEUR_UART");
                                        break;
                                    case SerialNumbers.LED_STRIP:
                                        Console.WriteLine("USB: Peripherique trouvé: " + dev.Descriptor.SerialNumber + ": LED_STRIP");
                                        break;
                                    default:
                                        Console.WriteLine("USB: Peripherique trouvé: " + dev.Descriptor.SerialNumber + ": INCONNU");
                                        break;
                                }

                                //On a trouvé le peripherique recherché
                                usbInterfaceMicrochip = dev.Interfaces.First();
                                break;
                            }
                            else
                            {
                                dev.Dispose();
                            }
                        }
                        else
                        {
                            //Le champ SerialNumber est pas present, il peut s'agir d'une carte moteur
                            //On verifie s'il correspond au peripherique recherché
                            if (_serialNumber == SerialNumbers.CARTE_MOTEUR)
                            {
                                //On a trouvé le peripherique recherché
                                usbInterfaceMicrochip = dev.Interfaces.First();
                                Console.WriteLine("USB: Peripherique trouvé: " + dev.Descriptor.PathName.Split('#')[2] + ": Carte Moteur");
                                break;
                            }
                            else
                            {
                                dev.Dispose();
                            }
                        }
                    }
                    catch
                    {
                        if (counterException++ % 20 == 0)
                            Console.WriteLine("Exception USBDriver : UpdateUSBConnection2, peripherique:" + devInfo.DevicePath.Split('#')[2] + " deja utilisé");
                    }
                }

                //Si on a trouvé une interface
                if (usbInterfaceMicrochip != null)
                {
                    try
                    {
                        if (!_usbThread.IsAlive)
                            _usbThread.Start();
                        isListening = true;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception USBDriver : USB problem - " + e.ToString());
                    }
                }
            }
        }
        //public void ReceiveUsbDataCallback(IAsyncResult result)
        //{
        //    //try
        //    //{
        //    //   var nbBytesTransfered = usbInterfaceMicrochip.InPipe.EndRead(result);
        //    //    //Console.WriteLine(rcvBuffer);

        //    //    byte[] usbReceivedBuffer = new byte[nbBytesTransfered];
        //    //    Buffer.BlockCopy(rcvBuffer, 0, usbReceivedBuffer, 0, nbBytesTransfered);

        //    //    nbUsbPacketReceived++;
        //    //    nbUSBBytesReceived += nbBytesTransfered;

        //    //    var v1=usbInterfaceMicrochip.InPipe.MaximumPacketSize;
        //    //   usbInterfaceMicrochip.InPipe.BeginRead(rcvBuffer, 0, rcvBufferSize, new AsyncCallback(ReceiveUsbDataCallback), null);
        //    //   OnUSBDataReceived(usbReceivedBuffer);

        //    //}
        //    //catch (Exception e)
        //    //{
        //    //    /// On a une erreur, potentiellement due au débranchement de l'USB, du coup on n'écoute plus
        //    //    Console.WriteLine("Exception USB ReceiveUsbDataCallback :\n" + e.ToString());
        //    //    isListening = false;
        //    //}
        //}

        private void TimerReconnectionIn_Elapsed(object sender, ElapsedEventArgs e)
        {
            /// On réarme l'USB quoiqu'il arrive si on est pas listening
            if (!isListening)
            {
                if (useMultipleUSBInstance)
                    UpdateUsbConnection2();
                else
                    UpdateUsbConnection();
                //Console.WriteLine("Rénitialisation de la connexion USB sur erreur de réception");
            }
        }




        public event EventHandler<DataArgs> OnUSBDataReceivedEvent;
        public virtual void OnUSBDataReceived(byte[] data)
        {
            var handler = OnUSBDataReceivedEvent;
            if (handler != null)
            {
                handler(this, new DataArgs { Data = data });
            }
        }

        public void SendUSBMessage(object sender, DataArgs e)
        {
            if (usbInterfaceMicrochip != null)
            {
                try
                {
                    usbInterfaceMicrochip.OutPipe.Write(e.Data);
                    //Console.WriteLine("Envois:" +e.Msg.ToString());
                }
                catch
                {
                    /// On a une une déconnexion pas encore notifiée
                    Console.WriteLine("Exception USBDriver : SendUSBMessage");
                }
            }
            else
            {
                //Console.WriteLine(" USB Interface is null");
            }
        }

        ///  <summary>
        ///  Called on removal of any device.
        ///  Calls a routine that searches to see if the desired device is still present.
        ///  </summary>
        /// 
        private void DeviceRemoved(object sender, EventArgs e)
        {
            //try
            {
                Console.WriteLine("A USB device has been removed");
                if (useMultipleUSBInstance)
                    UpdateUsbConnection2();
                else
                    UpdateUsbConnection();
            }
            //catch (Exception ex)
            //{
            //    //DisplayException(Name, ex);
            //    throw;
            //}
        }

        ///  <summary>
        ///  Add a handler to detect removal of devices.
        ///  </summary>

        private void AddDeviceRemovedHandler()
        {
            const Int32 pollingIntervalMilliseconds = 3000;
            var scope = new ManagementScope("root\\CIMV2");
            scope.Options.EnablePrivileges = true;

            try
            {
                var q = new WqlEventQuery();
                q.EventClassName = "__InstanceDeletionEvent";
                q.WithinInterval = new TimeSpan(0, 0, 0, 0, pollingIntervalMilliseconds);
                q.Condition = @"TargetInstance ISA 'Win32_USBControllerdevice'";
                _deviceRemovedWatcher = new ManagementEventWatcher(scope, q);
                _deviceRemovedWatcher.EventArrived += DeviceRemoved;
                _deviceRemovedWatcher.Start();
            }
            catch (Exception e)
            {

                Console.WriteLine("Exception USBDriver : AddDeviceRemovedHandler");
                Console.WriteLine(e.Message);
                if (_deviceRemovedWatcher != null)
                    _deviceRemovedWatcher.Stop();
            }
        }

        private void DeviceAdded(object sender, EventArrivedEventArgs e)
        {
            //try
            {
                Console.WriteLine("A USB device has been inserted");
                if (useMultipleUSBInstance)
                    UpdateUsbConnection2();
                else
                    UpdateUsbConnection();

                //FindMyDevice();
                //_deviceDetected = FindDeviceUsingWmi();
            }
            //catch (Exception ex)
            //{
            //    //DisplayException(Name, ex);
            //    throw;
            //}
        }

        private void AddDeviceArrivedHandler()
        {
            const Int32 pollingIntervalMilliseconds = 3000;
            var scope = new ManagementScope("root\\CIMV2");
            scope.Options.EnablePrivileges = true;

            try
            {
                var q = new WqlEventQuery();
                q.EventClassName = "__InstanceCreationEvent";
                q.WithinInterval = new TimeSpan(0, 0, 0, 0, pollingIntervalMilliseconds);
                q.Condition = @"TargetInstance ISA 'Win32_USBControllerdevice'";
                _deviceArrivedWatcher = new ManagementEventWatcher(scope, q);
                _deviceArrivedWatcher.EventArrived += DeviceAdded;

                _deviceArrivedWatcher.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception USBDriver : AddDeviceArrivedHandler - " + e.Message);
                if (_deviceArrivedWatcher != null)
                    _deviceArrivedWatcher.Stop();
            }
        }

        public event EventHandler<USBStatsArgs> OnUsbStatsEvent;
        public virtual void OnUsbStats(int nbPacketsReceived, int nbBytesReceived)
        {
            var handler = OnUsbStatsEvent;
            if (handler != null)
            {
                handler(this, new USBStatsArgs { nbBytesReceived = nbBytesReceived, nbPacketReceived = nbPacketsReceived });
            }
        }
    }


    public class DataArgs : EventArgs
    {
        public byte[] Data { get; set; }
    }

    public class USBStatsArgs : EventArgs
    {
        public int nbPacketReceived { get; set; }
        public int nbBytesReceived { get; set; }
    }
}
