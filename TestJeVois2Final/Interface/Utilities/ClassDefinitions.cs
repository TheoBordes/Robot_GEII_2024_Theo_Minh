using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Utilities
{
    [Serializable]
    public class TeamStrategyParameters
    {
        public double WeightDribble = 5;
        public double WeightPasse = 5;
        public double WeightTir = 5;
        public double WeightMoveWithBall = 5;
        public double RobotSpeed = 5;
        public double RobotAngularSpeed = 5;
        public double PassingSpeed = 5;
        public double DistanceBlock = 5;
        public double CollisionAvoidance = 5;
        public double Assist = 5;

        public ConcurrentDictionary<int, IndividualStrategyParameters> dictionaryIndividualStrategies = new ConcurrentDictionary<int, IndividualStrategyParameters>();

        public ConcurrentDictionary<int, PointD> dictionaryTheoreticalPositionsInAttackFieldPercent = new ConcurrentDictionary<int, PointD>();
        public ConcurrentDictionary<int, PointD> dictionaryTheoreticalPositionsInDefenseFieldPercent = new ConcurrentDictionary<int, PointD>();

        public ConcurrentDictionary<int, PointD> dictionaryTheoreticalPositionsInAttack = new ConcurrentDictionary<int, PointD>();
        public ConcurrentDictionary<int, PointD> dictionaryTheoreticalPositionsInDefense = new ConcurrentDictionary<int, PointD>();
        

        public TeamStrategyParameters()
        {
            ;
        }

        //public TeamStrategyParameters(TeamStrategyParameters p)
        //{
        //    Espacement = p.Espacement;
        //    RushPasse = p.RushPasse;
        //    DefenseAttaque = p.DefenseAttaque;
        //    foreach (var i in p.dictionaryIndividualStrategies)
        //        dictionaryIndividualStrategies.AddOrUpdate(i.Key, i.Value, (key, value)=>i.Value);
        //    foreach (var i in p.dictionaryTheoreticalPositionsInAttack)
        //        dictionaryTheoreticalPositionsInAttack.AddOrUpdate(i.Key, i.Value, (key, value) => i.Value);
        //    foreach (var i in p.dictionaryTheoreticalPositionsInDefense)
        //        dictionaryTheoreticalPositionsInAttack.AddOrUpdate(i.Key, i.Value, (key, value) => i.Value);
        //}
    }

    [Serializable]
    public class IndividualStrategyParameters
    {
        public double Espacement = 0.5;
        public double RushPasse = 0.5;
        //public double DefenseAttaque = 0.5;
        //public double DroiteGauche = 0.5;
    }
}
