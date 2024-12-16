using System.Collections.Generic;

namespace Constants
{
    public enum RefBoxCommand
    {
        START,
        STOP,
        XBOX_CONTROL,
        DROP_BALL,
        HALF_TIME,
        END_GAME,
        GAME_OVER,
        PARK,
        FIRST_HALF,
        SECOND_HALF,
        FIRST_HALF_OVER_TIME,
        RESET,
        WELCOME,
        KICKOFF,
        FREEKICK,
        GOALKICK,
        THROWIN,
        CORNER,
        PENALTY,
        GOAL,
        SUBGOAL,
        REPAIR,
        YELLOW_CARD,
        DOUBLE_YELLOW,
        RED_CARD,
        SUBSTITUTION,
        IS_ALIVE,


        //Added commands for debug
        GOTO,
        SCORING_ON_RIGHT,
        SCORING_ON_LEFT,
        SHOOT,
    }

    public enum RoboCupPoste
    {
        Unassigned = 0,
        GoalKeeper = 1,
        DefenderLeft = 2,
        DefenderCenter = 3,
        DefenderRight = 4,
        MidfielderLeft = 5,
        MidfielderCenter = 6,
        MidfielderRight = 7,
        ForwardLeft = 8,
        ForwardCenter = 9,
        ForwardRight = 10,
    }

    public enum RobotBallHandlingState
    {
        NoBall = 0,
        HasBall = 1,
        //IsPassing = 2,
        //IsShooting = 3,
        //IsDribbling = 4,
    }
    public enum TeamBallHandlingState
    {
        NoBall,
        RobotHasBall,
        TeamMateHasBall,
        //RobotIsPassing,
        //TeamMateIsPassing,
        //RobotIsShooting,
        //TeamMateIsShooting,
        //RobotIsDribbling,
        //TeamMateIsDribbling
    }

    public static class RoboCupField
    {
        public static double FieldLength = 22;
        public static double FieldWidth = 14;
        public static double StadiumLength = 26;
        public static double StadiumWidth = 18;
    }

    public enum GameState
    {
        STOPPED,
        STOPPED_GAME_POSITIONING,
        PLAYING,
        XBOX_CONTROL,
    }

    public enum StoppedGameAction
    {
        NONE,
        KICKOFF,
        KICKOFF_OPPONENT,
        FREEKICK,
        FREEKICK_OPPONENT,
        GOALKICK,
        GOALKICK_OPPONENT,
        THROWIN,
        THROWIN_OPPONENT,
        CORNER,
        CORNER_OPPONENT,
        PENALTY,
        PENALTY_OPPONENT,
        PARK,
        DROPBALL,
        GOTO,
        GOTO_OPPONENT,
        IS_ALIVE,
    }

    public enum Actions
    {
        /// Les actions potentielles sont : 
        /// 
        /// -> En défense
        /// -> -> Aller sur le ballon
        /// -> -> -> dépend de la position du ballon p/r à l'ensemble des joueurs
        /// -> -> -> dépend du risque de collision sur le segment robot-ballon
        /// -> -> Marquer un adversaire sans ballon
        /// -> -> -> dépend de la position de l'adversaire sur le terrain
        /// -> -> -> dépend de la distance de celui-ci au ballon
        /// -> -> -> dépend de l aposition du ballon par rapport à l'adversaire
        /// -> -> Couper les lignes de passe
        /// -> -> -> dépend de la position de l'adversaire visé sur le terrain
        /// -> -> -> dépend de la distance de l'adversaire au ballon
        /// -> -> Couper les lignes de tir
        /// -> -> -> dépend de la position de l'adversaire considéré
        /// 
        /// -> En attaque avec ballon
        /// -> -> Passer à un partenaire
        /// -> -> -> dépend de la position du partenaire sur le terrain
        /// -> -> -> dépend du risque d'interception sur le segment robot-ballon            
        /// -> -> -> dépend du temps passé avec la balle conservée
        /// -> -> Aller au but pour tirer
        /// -> -> -> dépend du risque de collision sur le segment robot-but
        /// -> -> -> dépend de la position du robot sur le terrain
        /// -> -> Se déplacer avec le ballon
        /// -> -> -> dépend du risque de collision sur le segment robot-lieu de déplacement
        /// -> -> -> dépend de la position du lieu de déplacement
        /// -> -> -> dépend du risque lié au dribble
        /// -> -> Conserver et protéger son ballon 
        /// -> -> -> dépend de la proximité des adversaires            
        /// -> -> -> dépend du temps passsé à conserver la balle
        /// 
        /// -> En attaque sans balle
        /// -> -> Se démarquer
        /// -> -> -> dépend du risque de collision sur le segment robot-lieu de déplacement
        /// -> -> -> dépend de la position du lieu de déplacement
        /// -> -> Soutenir son équipier à proximité
        /// -> -> -> dépend de la proximité des adversaires            
        /// -> -> -> dépend de la position du joueur à soutenir
        /// -> -> Se rapprocher d'un joueur adverse pour l'attirer dans une zone (stratégie avancée...)
        GoOnBall,
        MarkOpponent,
        BlockPassLine,
        BlockShootingLine,
        PassToTeamMate,
        TryToScore,
        MoveWithBall,
        ProtectBall,
        UnMark,
        AssistTeamMate,
        TryToAttractOpponent,
    }


    public enum TeamFormation
    {
        UnUnDeux,
        UnDeuxUn,
        UnTrois,
        DeuxUnUn,
        DeuxDeux,
        TroisUn,
    }

    public enum TeamOffensiveness
    {
        UltraDefensive,
        Defensive,
        Medium,
        Offensive,
        UltraOffensive,
    }

    public enum TeamBehaviour
    {
        Stone,
        Normal,
    }

    public enum PlayingSituation
    {
        Stopped,
        RobotHasBall,
        TeamHasBall,
        TeamIsPassing,
        OpponentHasBall,
        KickOffFor,
        KickOffAgainst,
        FreeKickFor,
        FreeKickAgainst,
        ThrowInFor,
        ThrowInAgainst,
        CornerFor,
        CornerAgainst,
        PenaltyFor,
        PenaltyAgainst,
        GoalKickFor,
        GoalKickAgainst,
        Park,
        Goto,
        Any,
        TeamOrRobotHasBall,
        PositioningFor,
        PositioningAgainst,
    }
    public enum PlayingRole
    {
        GoalKeeper,
        Defender,
        Midfielder,
        Forward
    }
    public enum PlayingAction
    {
        Unknown, /// Sert à déclarer que l'on ne connait pas l'action en cours (typiquement quand on se met à la place d'un autre robot)
        None,
        /// Arrêt
        Stopped,
        /// Positionnement durant les arrêts de jeu
        PositioningPlayingBall,
        PositioningAssist,
        PositioningDefense,
        PositioningStrategy,
        PositioningStrategyFixed,
        Positioning,
        /// Gardien
        GoalKeeping,
        /// Attaque

        TryToCatchBall,
        TryToPass,
        TryToShoot,
        Assist,
        MovingWithBall,
        TryToDribble,
        Defend,
        //ProtectBall,

        /// Défense
        //BlockPass,
        //MarkOpponent,
        //Discovering,
    }


    public static class TeamRobotToolbox
    {
        public static int GetRobotId(TeamId teamId, int robotNumber)
        {
            return (int)teamId * 10 + robotNumber;
        }
        public static int GetRobotNumber(int robotId)
        {
            return robotId % 10;
        }

        public static TeamId GetTeamId(int robotId)
        {
            return (TeamId)(robotId / 10);
        }
    }

}
