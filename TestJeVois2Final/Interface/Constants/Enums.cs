namespace Constants
{
    //public enum Equipe
    //{
    //    Jaune,
    //    Bleue,
    //}

    public enum CompetitionType
    {
        RoboCup,
        Eurobot2021,
        Eurobot2022,
        Cachan
    }

    public enum GameMode
    {
        Normal,
        Discovery,
        BallSearching,
    }

    public enum RoboCupIndividualMotors
    {
        BallHandlingLeft = 5,
        BallHandlingRight = 6,
        KeeperArmLeft = 7,
        KeeperArmRight = 8,
    }

    public enum RobotDrivingMode
    {
        AutonomousPosition,
        AutonomousSpeed,
        XBoxSpeed,
        XBoxPosition,
    }

    public enum CameraPosition
    {
        RoboCupFront,
        RoboCupLeft,
        RoboCupBack,
        RoboCupRight,
        EurobotFront,
        EurobotBack,
        EurobotRight,
        EurobotLeft,
    }

    public enum PlayingSide
    {
        NotDefined,
        ScoringOnRight,
        ScoringOnLeft
    }

    public enum ObjectType
    {
        ObstacleLidar,
        Balise,
        Aruco,
        RawPoteau,
        Poteau,
        PoteauBut,
        Balle,
        RawRobotTeam1,
        RobotTeam1,
        RobotTeam2,
        Valise,
        Cible,
        But,
        TagRouge,
        TagBleu,
        Humain,
        BrassardRouge,
        BrassardBleu,
        CentreTerrain,
        VirtualObject,
        GateauBrun,
        GateauRose,
        GateauJaune,
        BaliseVerte,
        BaliseBleue,
        BaliseRouge,
        Cerise,
        PositionTheorique
    }

    public enum Eurobot2022ArucoId
    {
        Rouge = 47,
        Vert = 36,
        Bleu = 13,
        Brun = 17,
    }

    public enum Eurobot2022TurbineId
    {
        Turbine_Aspiration_Balles = 17,
        Turbine_Tir = 16,
        Turbine_Aspiration_Gateau = 18,
        Turbine_Soufflage = 19,
    }

    public enum IMUReferential
    {
        XYZ = 0,
        RoboCup = 1,
        Eurobot = 2
    }

    public enum Eurobot2022SideColor { Blue, Green };
}
