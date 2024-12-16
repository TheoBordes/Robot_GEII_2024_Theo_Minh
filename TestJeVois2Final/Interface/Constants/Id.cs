namespace Constants
{
    public enum TeamId
    {
        Team1 = 1,
        Team2 = 2,
        Opponents = 10,
    }

    public static class TeamMulticastIP
    {
        public static string TeamIpRCT = "224.16.32.79";
        public static string TeamIpTechUnited = "224.16.32.63";
    }
    public static class RobotIP
    {
        public static string RobotIpRCT1 = "172.16.79.2";
        public static string RobotIpRCT2 = "172.16.79.3";
        public static string RobotIpRCT3 = "172.16.79.4";
        public static string RobotIpRCT4 = "172.16.79.5";
        public static string RobotIpRCT5 = "172.16.79.6";
        public static string RobotIpRCT6 = "172.16.79.7";
        public static string RobotIpTechUnited1 = "172.16.63.2";
        public static string RobotIpTechUnited2 = "172.16.63.3";
        public static string RobotIpTechUnited3 = "172.16.63.4";
        public static string RobotIpTechUnited4 = "172.16.63.5";
        public static string RobotIpTechUnited5 = "172.16.63.6";
        public static string RobotIpTechUnited6 = "172.16.63.7";
    }
    public static class BaseStationIP
    {
        public static string BaseStationIpRCT = "172.16.79.1";
        public static string BaseStationIpTechUnited = "172.16.63.1";
    }
    public static class MulticastPort
    {
        public static int PortRCT = 4567;
        public static int PortTechUnited = 4568;
    }

    public enum BallId
    {
        Ball = 1000,
    }
    public enum ObstacleId
    {
        Obstacle = 2000,
    }
    public enum RobotId
    {
        Robot1 = 0,
        Robot2 = 1,
        Robot3 = 2,
        Robot4 = 3,
        Robot5 = 4,
        Robot6 = 5,
    }
    public enum Caracteristique
    {
        Speed = 100,
        Destination = 200,
        WayPoint = 300,
        Ghost = 400,
        Ball = 500,
    }

    public enum Terrain
    {
        ZoneProtegee = 1,
        DemiTerrainDroit = 2,
        DemiTerrainGauche = 3,
        SurfaceButGauche = 4,
        SurfaceButDroit = 5,
        SurfaceGardienDroit = 6,
        SurfaceGardienGauche = 7,
        ButGauche = 8,
        ButDroit = 9,
        ZoneTechniqueGauche = 10,
        ZoneTechniqueDroite = 11,
        RondCentral = 12,
        CornerBasGauche = 13,
        CornerBasDroite = 14,
        CornerHautDroite = 15,
        CornerHautGauche = 16,
        PtAvantSurfaceGauche = 17,
        PtAvantSurfaceDroit = 18,


        TerrainComplet = 19,
        LigneTerrainGauche = 20,
        LigneTerrainDroite = 21,
        LigneCentraleEpaisse = 22,
        LigneCentraleFine = 23,
        BaliseGaucheHaut = 24,
        BaliseGaucheCentre = 25,
        BaliseGaucheBas = 26,
        BaliseDroiteHaut = 27,
        BaliseDroiteCentre = 28,
        BaliseDroiteBas = 29,

        FlecheSensStrategie = 30,
    }
}
