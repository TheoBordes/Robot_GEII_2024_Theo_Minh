using Constants;
using System.Collections.Concurrent;
using System.Configuration;
using System.Drawing;
using System.Numerics;
using Utilities;

namespace EventArgsLibrary
{
    public class DataReceivedArgs : EventArgs
    {
        public byte[] Data { get; set; }
    }
    public class DataReceivedWithInfoArgs : EventArgs
    {
        public byte[] Data { get; set; }
        public string Info { get; set; }
    }

    public class StringArgs : EventArgs
    {
        public string Value { get; set; }
    }

    public class BoolEventArgs : EventArgs
    {
        public bool value { get; set; }
    }
    public class ByteBoolEventArgs : EventArgs
    {
        public byte byteValue { get; set; }
        public bool boolvalue { get; set; }
    }
    public class ByteDoubleEventArgs : EventArgs
    {
        public byte byteValue { get; set; }
        public bool doublevalue { get; set; }
    }

    public class ConfigurationArgs : EventArgs 
    { 
        public Configuration config { get; set; }
    }

    public class SimulatorActionArgs : EventArgs 
    { 
        public StoppedGameAction action { get; set; }
    }

    public class TeamStrategyParametersArgs : EventArgs
    {
        public double weightTir { get; set; }
        public double weightPasse { get; set; }
        public double weightDribble { get; set; }
        public double weightMoveWithBall { get; set; }
    }

    public class RobotDrivingModeEventArgs : EventArgs
    {
        public RobotDrivingMode value { get; set; }
    }
    public class AsservissementModeEventArgs : EventArgs
    {
        public AsservissementSpeedMode mode { get; set; }
    }
    public class IndividualMotorPositionModeEnableDisableEventArgs : EventArgs
    {
        public byte MotorNumber { get; set; }
        public bool positionModeActivated { get; set; }
    }

    public class IndividualMotorPositionTargetEventArgs : EventArgs
    {
        public byte MotorNumber { get; set; }
        public double targetPosition { get; set; }
    }
    public class GoalKeeperPositionEventArgs : EventArgs
    {
        public double targetPositionRight { get; set; }
        public double targetPositionLeft { get; set; }
    }

    public class IndividualMotorAsservissementModeEventArgs : EventArgs
    {
        public byte motorNum { get; set; }
        public IndividualAsservissementMode mode { get; set; }
    }
    public class ByteEventArgs : EventArgs
    {
        public byte Value { get; set; }
    }
    public class IntEventArgs : EventArgs
    {
        public int Value { get; set; }
    }

    public class RobotIdEventArgs : EventArgs
    {
        public int RobotId { get; set; }
        public TeamId TeamId { get; set; }
    }

    public class TorqueEventArgs : EventArgs
    {
        public byte servoID { get; set; }
        public int Value { get; set; }
    }

    public class HerkulexPositionEventArgs : EventArgs
    {
        public byte servoID { get; set; }
        public int Value { get; set; }
    }

    public class LidarMessageArgs : EventArgs
    {
        public string Value { get; set; }
        public int Line { get; set; }
    }
    public class LEDColorArgs : EventArgs
    {
        public byte r { get; set; }
        public byte g { get; set; }
        public byte b { get; set; }
    }
    public class PixelLedColorArgs : EventArgs
    {
        public int pixelIndex { get; set; }
        public byte r { get; set; }
        public byte g { get; set; }
        public byte b { get; set; }
    }
    public class PixelBufferArgs : EventArgs
    {
        public uint[] pixelBuffer { get; set; }
    }

    public class LEDAnimationArgs : EventArgs
    {
        object animation;
    }

    public class DoubleEventArgs : EventArgs
    {
        public double Value { get; set; }
    }
    public class IndividualMotorSpeedToPercentEventArgs : EventArgs
    {
        public byte motorNum { get; set; }
        public double Value { get; set; }
    }
    public class IndividualMotorPointToPositionEventArgs : EventArgs
    {
        public byte motorNum { get; set; }
        public double pointToPositionConstant { get; set; }
    }

    public class MotorsPositionGhostErrorEventArgs : EventArgs
    {
        public byte flags;
        public double motor1CurrentPosition;
        public double motor2CurrentPosition;
        public double motor3CurrentPosition;
        public double motor4CurrentPosition;
        public double motor5CurrentPosition;
        public double motor6CurrentPosition;
        public double motor7CurrentPosition;
        public double motor8CurrentPosition;
    }
    public class IndividualMotorPositionEventArgs : EventArgs
    {
        public byte motorNumber;
        public double position;
    }

    public class IndividualMotorPositionErrorMaxArgs : EventArgs
    {
        public byte MotorNumber;
        public double errorMax;
    }

    public class DoubleIntEventArgs : EventArgs
    {
        public double dValue { get; set; }
        public double iValue { get; set; }
    }

    public class StrategyFileArgs : EventArgs
    {
        public string StrategyFileName { get; set; }
        public TeamId TeamId { get; set; }
    }

    public class CameraImageArgs : EventArgs
    {
        public Bitmap ImageBmp { get; set; }
    }

    public class FieldSizeArgs : EventArgs
    {
        public double? stadiumLength;
        public double? stadiumWidth;
        public double? fieldLength;
        public double? fieldWidth;
    }

  
    //public class RefBoxMessageArgs : EventArgs
    //{
    //    public RefBoxMessage msg { get; set; }
    //}

    public class MessageDecodedArgs : EventArgs
    {
        public int MsgFunction { get; set; }
        public int MsgPayloadLength { get; set; }
        public byte[] MsgPayload { get; set; }
    }

    public class MessageEncodedArgs : EventArgs
    {
        public byte[] Msg { get; set; }
    }

    public class MessageToRobotArgs : EventArgs
    {
        public Int16 MsgFunction { get; set; }
        public Int16 MsgNumber { get; set; }
        public Int16 MsgPayloadLength { get; set; }
        public byte[] MsgPayload { get; set; }
    }
    public class PolarSpeedArgs : EventArgs
    {
        public int RobotId { get; set; }
        public double Vx { get; set; }
        public double Vy { get; set; }
        public double Vtheta { get; set; }
    }
    public class IndependantSpeedArgs : EventArgs
    {
        public int RobotId { get; set; }
        public double VitesseMoteur1 { get; set; }
        public double VitesseMoteur2 { get; set; }
        public double VitesseMoteur3 { get; set; }
        public double VitesseMoteur4 { get; set; }
    }
    public class AuxiliarySpeedArgs : EventArgs
    {
        public int RobotId { get; set; }
        public double VitesseMoteur5 { get; set; }
        public double VitesseMoteur6 { get; set; }
        public double VitesseMoteur7 { get; set; }
        public double VitesseMoteur8 { get; set; }
    }
    public class GyroArgs : EventArgs
    {
        public double Vx { get; set; }
        public double Vy { get; set; }
        public double Vtheta { get; set; }
    }

    public class BallHandlingSensorArgs : EventArgs
    {
        public int RobotId { get; set; }
        public bool IsHandlingBall { get; set; }
    }
    public class PolarSpeedEventArgs : PolarSpeedArgs
    {
        public uint timeStampMs;
    }
    public class IndependantSpeedEventArgs : IndependantSpeedArgs
    {
        public uint timeStampMs;
    }
    public class AuxiliarySpeedEventArgs : AuxiliarySpeedArgs
    {
        public uint timeStampMs;
    }
    public class TirEventArgs : EventArgs
    {
        //public int RobotId { get; set; }
        public ushort frontCoilDurationUs { get; set; }
        public ushort rearCoilDurationUs { get; set; }
        public ushort coil3US { get; set; }
        public ushort coil4US { get; set; }
        public ushort frontCoilOffsetUs { get; set; }
        public ushort rearCoilOffsetUs { get; set; }
        public ushort coil3OffsetUS { get; set; }
        public ushort coil4OffsetUS { get; set; }
    }
    public class GotoEventArgs : EventArgs
    {
        public double xPercent { get; set; }
        public double yPercent { get; set; }
    }

    public class IMUDataEventArgs : EventArgs
    {
        public uint EmbeddedTimeStampInMs;
        public double accelX;
        public double accelY;
        public double accelZ;
        public double gyroX;
        public double gyroY;
        public double gyroZ;
        public double magX;
        public double magY;
        public double magZ;

        //public IMUDataEventArgs(double aX, double aY, double aZ, double gX, double gY, double gZ)
        //{
        //    accelX = aX;
        //    accelY = aY;
        //    accelZ = aZ;
        //    gyroX = gX;
        //    gyroY = gY;
        //    gyroZ = gZ;
        //}
    }
    public class MotorsCurrentsEventArgs : EventArgs
    {
        public uint timeStampMS;
        public double motor1;
        public double motor2;
        public double motor3;
        public double motor4;
        public double motor5;
        public double motor6;
        public double motor7;
        public double motor8;
    }
    public class EncodersRawDataEventArgs : EventArgs
    {
        public uint timeStampMS;
        public int motor1;
        public int motor2;
        public int motor3;
        public int motor4;
        public int motor5;
        public int motor6;
        public int motor7;
        public int motor8;
    }
    public class IOValuesEventArgs : EventArgs
    {
        public uint timeStampMS;
        public int ioValues;
    }
    public class PowerMonitoringValuesEventArgs : EventArgs
    {
        public uint timeStampMS;
        public double battCMDVoltage;
        public double battCMDCurrent;
        public double battPWRVoltage;
        public double battPWRCurrent;
    }
    public class MotorsPositionDataEventArgs : MotorsCurrentsEventArgs
    {

    }
    public class PrehensionAngleArgs : EventArgs
    {
        public uint timeStampMS;
        public double angleLeftArm;
        public double angleRightArm;
    }
    public class PrehensionMotorSpeedArgs : EventArgs
    {
        public double speedLeftMotor;
        public double speedRightMotor;
    }
    public class TwoWheelsAngleArgs : EventArgs
    {
        public double angleMotor1;
        public double angleMotor2;
    }
    public class TwoWheelsToPolarMatrixArgs : EventArgs
    {
        public double mx1;
        public double mx2;
        public double mtheta1;
        public double mtheta2;
    }
    public class FourWheelsAngleArgs : EventArgs
    {
        public double angleMotor1;
        public double angleMotor2;
        public double angleMotor3;
        public double angleMotor4;
    }
    public class PololuServoUsArgs : EventArgs
    {
        public ushort servoUs;
        public byte servoChannel;
    }
    public class FourWheelsToPolarMatrixArgs : EventArgs
    {
        public double mx1;
        public double mx2;
        public double mx3;
        public double mx4;
        public double my1;
        public double my2;
        public double my3;
        public double my4;
        public double mtheta1;
        public double mtheta2;
        public double mtheta3;
        public double mtheta4;
    }
    public class PolarToFourWheelsMatrixArgs : EventArgs
    {
        public double m1x;
        public double m1y;
        public double m1theta;
        public double m2x;
        public double m2y;
        public double m2theta;
        public double m3x;
        public double m3y;
        public double m3theta;
        public double m4x;
        public double m4y;
        public double m4theta;
    }
    public class AuxiliaryMotorsVitesseDataEventArgs : EventArgs
    {
        public uint timeStampMS;
        public double vitesseMotor5;
        public double vitesseMotor6;
        public double vitesseMotor7;
        public double vitesseMotor8;
    }
    public class PidErrorCorrectionConsigneDataArgs : EventArgs
    {
        public uint timeStampMS;
        public double Erreur;
        public double Correction;
        public double ConsigneFromRobot;
    }
    public class Polar4WheelsPidErrorCorrectionConsigneDataArgs : EventArgs
    {
        public uint timeStampMS;
        public double xErreur;
        public double yErreur;
        public double thetaErreur;

        public double xCorrection;
        public double yCorrection;
        public double thetaCorrection;

        public double xConsigneFromRobot;
        public double yConsigneFromRobot;
        public double thetaConsigneFromRobot;
    }

    public class Polar2WheelsPidErrorCorrectionConsigneDataArgs : EventArgs
    {
        public uint timeStampMS;
        public double xErreur;
        public double thetaErreur;

        public double xCorrection;
        public double thetaCorrection;

        public double xConsigneFromRobot;
        public double thetaConsigneFromRobot;
    }

    public class Independant4WheelsPidErrorCorrectionConsigneDataArgs : EventArgs
    {
        public uint timeStampMS;
        public double M1Erreur;
        public double M2Erreur;
        public double M3Erreur;
        public double M4Erreur;

        public double M1Correction;
        public double M2Correction;
        public double M3Correction;
        public double M4Correction;

        public double M1ConsigneFromRobot;
        public double M2ConsigneFromRobot;
        public double M3ConsigneFromRobot;
        public double M4ConsigneFromRobot;
    }

    public class Independant2WheelsPidErrorCorrectionConsigneDataArgs : EventArgs
    {
        public uint timeStampMS;
        public double M1Erreur;
        public double M2Erreur;

        public double M1Correction;
        public double M2Correction;

        public double M1ConsigneFromRobot;
        public double M2ConsigneFromRobot;
    }
    //public class IndividualMotorPIDSetupArgs : EventArgs
    //{
    //    public double P;
    //    public double I;
    //    public double D;
        
    //    public double P_Limit;
    //    public double I_Limit;
    //    public double D_Limit;
    //}
    public class PolarPIDSetupArgs : EventArgs
    {
        public double P_x;
        public double I_x;
        public double D_x;
        public double P_y;
        public double I_y;
        public double D_y;
        public double P_theta;
        public double I_theta;
        public double D_theta;
        public double P_x_Limit;
        public double I_x_Limit;
        public double D_x_Limit;
        public double P_y_Limit;
        public double I_y_Limit;
        public double D_y_Limit;
        public double P_theta_Limit;
        public double I_theta_Limit;
        public double D_theta_Limit;
    }
    public class KalmanAlphasArgs : EventArgs
    {
        public double alphaTheta;
        public double alphaSpeed;
        public double alphaPosition;
    }
    public class IndividualMotorPIDSetupArgs : EventArgs
    {
        public byte MotorNumber;
        public double P;
        public double I;
        public double D;
        public double P_Limit;
        public double I_Limit;
        public double D_Limit;
    }
    public class IMUReferentialArgs : EventArgs
    {
        public IMUReferential referential;
    }
    public class IMUOffsetsArgs : EventArgs
    {
        public double accelXOffset;
        public double accelYOffset;
        public double accelZOffset;
        public double gyroXOffset;
        public double gyroYOffset;
        public double gyroZOffset;
    }
    public class IndependantPIDSetupArgs : EventArgs
    {
        public double P_M1;
        public double I_M1;
        public double D_M1;
        public double P_M2;
        public double I_M2;
        public double D_M2;
        public double P_M3;
        public double I_M3;
        public double D_M3;
        public double P_M4;
        public double I_M4;
        public double D_M4;
        public double P_M1_Limit;
        public double I_M1_Limit;
        public double D_M1_Limit;
        public double P_M2_Limit;
        public double I_M2_Limit;
        public double D_M2_Limit;
        public double P_M3_Limit;
        public double I_M3_Limit;
        public double D_M3_Limit;
        public double P_M4_Limit;
        public double I_M4_Limit;
        public double D_M4_Limit;
    }

    public class PolarPidCorrectionArgs : EventArgs
    {
        public double CorrPx;
        public double CorrIx;
        public double CorrDx;
        public double CorrPy;
        public double CorrIy;
        public double CorrDy;
        public double CorrPTheta;
        public double CorrITheta;
        public double CorrDTheta;
    }
    public class Independant4WheelsPidCorrectionArgs : EventArgs
    {
        public double CorrPM1;
        public double CorrIM1;
        public double CorrDM1;
        public double CorrPM2;
        public double CorrIM2;
        public double CorrDM2;
        public double CorrPM3;
        public double CorrIM3;
        public double CorrDM3;
        public double CorrPM4;
        public double CorrIM4;
        public double CorrDM4;
    }
    public class PidCorrectionArgs : EventArgs
    {
        public double CorrP;
        public double CorrI;
        public double CorrD;
    }
    public class Independant2WheelsPidCorrectionArgs : EventArgs
    {
        public double CorrPM1;
        public double CorrIM1;
        public double CorrDM1;
        public double CorrPM2;
        public double CorrIM2;
        public double CorrDM2;
    }

    public class AccelEventArgs : EventArgs
    {
        public int timeStampMS;
        public double accelX;
        public double accelY;
        public double accelZ;
    }

    public class CollisionEventArgs : EventArgs
    {
        public int RobotId { get; set; }
        public Location RobotRealPositionRefTerrain { get; set; }
    }

    public class IndividualMotorCollisionEventArgs : EventArgs
    {
        public byte MotorNumber { get; set; }
        public double Position { get; set; }
    }

    public class ShootEventArgs : EventArgs
    {
        public int RobotId { get; set; }
        public double shootingSpeed { get; set; }
    }

    public class StringEventArgs : EventArgs
    {
        public string value { get; set; }
    }
    public class IndividualMotorSpeedConsigneArgs : EventArgs
    {
        public byte MotorNumber { get; set; }
        public double V { get; set; }
    }
    public class IndividualMotorTrajectoryConstantsArgs : EventArgs
    {
        public byte MotorNumber { get; set; }
        public double Accel { get; set; }
        public double Speed { get; set; }
    }
    public class PositionEventArgs : EventArgs
    {
        public int RobotId { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Theta { get; set; }
        public double Reliability { get; set; }
    }


    public class StrategyMapEventArgs : EventArgs
    {
        public int RobotId { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public bool IsAttack { get; set; }
    }

    public class LocationArgs : EventArgs
    {
        public int RobotId { get; set; }

        public Location Location { get; set; }
    }

    public class IndividualStrategyParametersArgs : EventArgs
    {
        public IndividualStrategyParameters parameters { get; set; }
    }

    public class LocationEventArgs : LocationArgs
    {
        public uint timeStampMS;
    }

    public class SpeedArgs : EventArgs
    {
        public Vector3 Speed { get; set; }
    }
    public class AccelArgs : EventArgs
    {
        public Vector3 Accel { get; set; }
    }

    public class PlayingActionArgs : EventArgs
    {
        public int RobotId { get; set; }

        public PlayingAction action { get; set; }
    }

    //public class BallHandlingStateArgs : EventArgs
    //{
    //    public int RobotId { get; set; }

    //    public BallHandlingState State { get; set; }
    //}

    public class MessageDisplayArgs : EventArgs
    {
        public int RobotId { get; set; }
        public string Message { get; set; }
        public MessageType Type { get; set; }
    }

    public enum MessageType
    {
        CurrentAction,
        BallHandlingStatus,
        PrehensionStatus,
    }

    public class PlayingSideArgs : EventArgs
    {
        public int RobotId { get; set; }

        public PlayingSide PlaySide { get; set; }
    }
    public class LocationListArgs : EventArgs
    {
        public List<Location> LocationList { get; set; }
    }
    public class LocationExtendedListArgs : EventArgs
    {
        public List<LocationExtended> LocationExtendedList { get; set; }
    }
    public class LocationExtendedConcurrentBagArgs : EventArgs
    {
        public ConcurrentBag<LocationExtended> LocationExtendedList { get; set; }
    }
    public class LocationExtendedWithPolarConcurrentBagArgs : EventArgs
    {
        public ConcurrentBag<LocationExtendedWithPolar> LocationExtendedList { get; set; }
    }
   

 

    //public class CameraOffsetArgs : EventArgs
    //{
    //    public CameraPosition cameraPosition;
    //    public int xOffset;
    //    public int yOffset;
    //}


    public enum LidarDataType
    {
        RawPtsList = 0,
        ProcessedPtsList = 1,
        StrategyPtsList = 2,
        BalisesPoints = 3,
        ObjectPoints = 4,
    }
    public class RawLidarArgs : EventArgs
    {
        public List<PolarPointRssi> PtList { get; set; }
        public int LidarFrameNumber { get; set; }
        public LidarDataType Type { get; set; }
    }
    public class LidarPolarPtListExtendedArgs : EventArgs
    {
        public List<PolarPointRssiExtended> PtList { get; set; }
        public int LidarFrameNumber { get; set; }
        public LidarDataType Type { get; set; }
    }
    public class PointDExtendedListArgs : EventArgs
    {
        public List<PointDColored> PtList { get; set; }
    }
    public class PolarPointListExtendedListArgs : EventArgs
    {
        public List<PolarPointListExtended> ObjectList { get; set; }
    }

    public class SegmentExtendedListArgs : EventArgs
    {
        public ConcurrentBag<SegmentExtended> SegmentList { get; set; }
    }


    public class BitmapImageArgs : EventArgs
    {
        public Bitmap Bitmap { get; set; }
        public string Descriptor { get; set; }
    }
    public class MsgCounterArgs : EventArgs
    {
        public int nbMessageIMU { get; set; }
        public int nbMessageOdometry { get; set; }
        public int fluxUart1 { get; set; }
        public int fluxUart2 { get; set; }
    }
    public class MsgDecoderCounterArgs : EventArgs
    {
        public int nbMessageReceived { get; set; }
        public int nbMessageFailed { get; set; }
    }
    public class USBStatsArgs : EventArgs
    {
        public int nbPacketReceived { get; set; }
        public int nbBytesReceived { get; set; }
    }
    public class GameStateArgs : EventArgs
    {
        public int RobotId { get; set; }
        public GameState gameState { get; set; }
    }

    public class GameModeArgs : EventArgs
    {
        public GameMode Mode { get; set; }
    }

    public class TurbineEventArgs : EventArgs
    {
        public Eurobot2022TurbineId turbineId { get; set; }
        public ushort speed { get; set; }
    }

    public class CameraCalibrationFileArgs : EventArgs
    {
        public string CalibrationFileName{ get; set; }
        public CameraPosition CameraPosition { get; set; }
    }

    public class TrajectoryGeneratorHolonomeConstants : EventArgs
    {
        public int RobotId { get; set; }
        public double accelLineaireMax { get; set; }
        public double accelLineaireFreinageMax { get; set; }
        public double accelRotationCapVitesseMax { get; set; }
        public double accelRotationOrientationRobotMax { get; set; }
        public double vitesseLineaireMax { get; set; }
        public double vitesseRotationCapVitesseMax { get; set; }
        public double vitesseRotationOrientationRobotMax { get; set; }
    }
}
