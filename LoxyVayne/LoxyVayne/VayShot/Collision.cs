﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SharpDX;
using EloBuddy;
using EloBuddy.SDK;

namespace VayShot
{
    public enum CollisionObjectTypes
    {
        Minion,
        Champion,
        YasuoWall
    }

    public class FastPredictionResult
    {
        public Vector2 CurrentPosVector2;
        public bool IsMoving;
        public Vector2 PredictedPosVector2;
    }

    public class DetectedCollision
    {
        public float Difference;
        public float Distance;
        public Vector2 PositionVector2;
        public CollisionObjectTypes Type;
        public Obj_AI_Base UnitAiBase;
    }

    public static class Collision
    {
        private static int _wallCastTick;
        private static Vector2 _yasuoWallVector2;

        public static void Init()
        {
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsValid || sender.Team != GameObjectTeam.Neutral || !args.SData.Name.Equals("YasuoWMovingWall"))
            {
                return;
            }

            _wallCastTick = Environment.TickCount;
            _yasuoWallVector2 = sender.ServerPosition.To2D();
        }

        internal static class WaypointTracker
        {
            public static readonly Dictionary<int, List<Vector2>> StoredPaths = new Dictionary<int, List<Vector2>>();
            public static readonly Dictionary<int, int> StoredTick = new Dictionary<int, int>();
        }

        public static List<Vector2> GetWaypoints(this Obj_AI_Base unit)
        {
            var result = new List<Vector2>();

            if (unit.IsVisible)
            {
                result.Add(unit.ServerPosition.To2D());
                var path = unit.Path;
                if (path.Length > 0)
                {
                    var first = path[0].To2D();
                    if (first.Distance(result[0], true) > 40)
                    {
                        result.Add(first);
                    }

                    for (int i = 1; i < path.Length; i++)
                    {
                        result.Add(path[i].To2D());
                    }
                }
            }
            else if (WaypointTracker.StoredPaths.ContainsKey(unit.NetworkId))
            {
                var path = WaypointTracker.StoredPaths[unit.NetworkId];
                var timePassed = (Core.GameTickCount - WaypointTracker.StoredTick[unit.NetworkId]) / 1000f;
                if (path.PathLength() >= unit.MoveSpeed * timePassed)
                {
                    result = CutPath(path, (int)(unit.MoveSpeed * timePassed));
                }
            }

            return result;
        }

        public static List<Vector2> CutPath(this List<Vector2> path, float distance)
        {
            var result = new List<Vector2>();
            var Distance = distance;
            if (distance < 0)
            {
                path[0] = path[0] + distance * (path[1] - path[0]).Normalized();
                return path;
            }

            for (var i = 0; i < path.Count - 1; i++)
            {
                var dist = path[i].Distance(path[i + 1]);
                if (dist > Distance)
                {
                    result.Add(path[i] + Distance * (path[i + 1] - path[i]).Normalized());
                    for (var j = i + 1; j < path.Count; j++)
                    {
                        result.Add(path[j]);
                    }

                    break;
                }
                Distance -= dist;
            }
            return result.Count > 0 ? result : new List<Vector2> { path.Last() };
        }

        public static float PathLength(this List<Vector2> path)
        {
            var distance = 0f;
            for (var i = 0; i < path.Count - 1; i++)
            {
                distance += path[i].Distance(path[i + 1]);
            }
            return distance;
        }

        public static FastPredictionResult FastPrediction(Vector2 fromVector2,
            Obj_AI_Base unitAiBase,
            int delay,
            int speed)
        {
            var tickDelay = delay / 1000f + (fromVector2.Distance(unitAiBase) / speed);
            var moveSpeedF = tickDelay * unitAiBase.MoveSpeed;
            var path = unitAiBase.GetWaypoints();

            if (path.PathLength() > moveSpeedF)
            {
                return new FastPredictionResult
                {
                    IsMoving = true,
                    CurrentPosVector2 = unitAiBase.ServerPosition.To2D(),
                    PredictedPosVector2 = path.CutPath((int) moveSpeedF)[0]
                };
            }

            if (path.Count == 0)
            {
                return new FastPredictionResult
                {
                    IsMoving = false,
                    CurrentPosVector2 = unitAiBase.ServerPosition.To2D(),
                    PredictedPosVector2 = unitAiBase.ServerPosition.To2D()
                };
            }

            return new FastPredictionResult
            {
                IsMoving = false,
                CurrentPosVector2 = path[path.Count - 1],
                PredictedPosVector2 = path[path.Count - 1]
            };
        }

        public static Vector2 GetCollisionPoint(Skillshot skillshot)
        {
            var collisions = new List<DetectedCollision>();
            var from = skillshot.GetMissilePosition(0);
            skillshot.ForceDisabled = false;
            foreach (var cObject in skillshot.SpellData.CollisionObjects)
            {
                switch (cObject)
                {
                    case CollisionObjectTypes.Minion:

                        collisions.AddRange(
                            from minion in
                                EntityManager.MinionsAndMonsters.AlliedMinions.Where(m => m.Distance(Player.Instance) <= 1200)
                            let pred =
                                FastPrediction(
                                    @from, minion,
                                    Math.Max(
                                        0, skillshot.SpellData.Delay - (Environment.TickCount - skillshot.StartTick)),
                                    skillshot.SpellData.MissileSpeed)
                            let pos = pred.PredictedPosVector2
                            let w =
                                skillshot.SpellData.RawRadius + (!pred.IsMoving ? (minion.BoundingRadius - 15) : 0) -
                                pos.Distance(@from, skillshot.End, true)
                            where w > 0
                            select
                                new DetectedCollision
                                {
                                    PositionVector2 =
                                        pos.ProjectOn(skillshot.End, skillshot.Start).LinePoint +
                                        skillshot.Direction * 30,
                                    UnitAiBase = minion,
                                    Type = CollisionObjectTypes.Minion,
                                    Distance = pos.Distance(@from),
                                    Difference = w
                                });

                        break;

                    case CollisionObjectTypes.Champion:
                        collisions.AddRange(
                            from hero in
                                ObjectManager.Get<AIHeroClient>()
                                    .Where(
                                        h =>
                                            (h.IsValidTarget(1200, false) && h.Team == ObjectManager.Player.Team &&
                                             !h.IsMe || h.Team != ObjectManager.Player.Team))
                            let pred =
                                FastPrediction(
                                    @from, hero,
                                    Math.Max(
                                        0, skillshot.SpellData.Delay - (Environment.TickCount - skillshot.StartTick)),
                                    skillshot.SpellData.MissileSpeed)
                            let pos = pred.PredictedPosVector2
                            let w = skillshot.SpellData.RawRadius + 30 - pos.Distance(@from, skillshot.End, true)
                            where w > 0
                            select
                                new DetectedCollision
                                {
                                    PositionVector2 =
                                        pos.ProjectOn(skillshot.End, skillshot.Start).LinePoint +
                                        skillshot.Direction * 30,
                                    UnitAiBase = hero,
                                    Type = CollisionObjectTypes.Minion,
                                    Distance = pos.Distance(@from),
                                    Difference = w
                                });
                        break;

                    case CollisionObjectTypes.YasuoWall:
                        if (
                            !ObjectManager.Get<AIHeroClient>()
                                .Any(
                                    hero =>
                                        hero.IsValidTarget(float.MaxValue, false) &&
                                        hero.Team == ObjectManager.Player.Team && hero.ChampionName == "Yasuo"))
                        {
                            break;
                        }
                        GameObject wall = null;
                        foreach (
                            var gameObject in
                                ObjectManager.Get<GameObject>()
                                    .Where(
                                        gameObject =>
                                            gameObject.IsValid &&
                                            Regex.IsMatch(
                                                gameObject.Name, "_w_windwall.\\.troy", RegexOptions.IgnoreCase)))
                        {
                            wall = gameObject;
                        }
                        if (wall == null)
                        {
                            break;
                        }
                        var level = wall.Name.Substring(wall.Name.Length - 6, 1);
                        var wallWidth = (300 + 50 * Convert.ToInt32(level));


                        var wallDirection = (wall.Position.To2D() - _yasuoWallVector2).Normalized().Perpendicular();
                        var fraction = wallWidth / 0x2; // 0x2 = 2
                        var wallStart = wall.Position.To2D() + fraction * wallDirection;
                        var wallEnd = wallStart - wallWidth * wallDirection;
                        var wallPolygon = new Geometry.Rectangle(wallStart, wallEnd, 75).ToPolygon();
                        var intersections = new List<Vector2>();

                        for (var i = 0; i < wallPolygon.Points.Count; i++)
                        {
                            var inter =
                                wallPolygon.Points[i].Intersection(
                                    wallPolygon.Points[i != wallPolygon.Points.Count - 1 ? i + 1 : 0], from,
                                    skillshot.End);
                            if (inter.Intersects)
                            {
                                intersections.Add(inter.Point);
                            }
                        }

                        if (intersections.Count > 0)
                        {
                            var intersection = intersections.OrderBy(item => item.Distance(from)).ToList()[0];
                            var collisionT = Environment.TickCount +
                                             Math.Max(
                                                 0,
                                                 skillshot.SpellData.Delay -
                                                 (Environment.TickCount - skillshot.StartTick)) + 100 +
                                             (1000 * intersection.Distance(from)) / skillshot.SpellData.MissileSpeed;
                            if (collisionT - _wallCastTick < 4000)
                            {
                                if (skillshot.SpellData.Type != SkillShotType.SkillshotMissileLine)
                                {
                                    skillshot.ForceDisabled = true;
                                }
                                return intersection;
                            }
                        }

                        break;
                }
            }

            return collisions.Count > 0
                ? collisions.OrderBy(c => c.Distance).ToList()[0].PositionVector2
                : new Vector2();
        }
    }
}