using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using LoxyVay.Managers;
using SharpDX;
using LoxyVayne;

namespace LoxyVay.Extensions
{
    public static class ExtensionsHelpers
    {

        public static bool Condenable(this AIHeroClient target)
        {
            if (MenuManager.Modes.Condemn.CondemnHero(target))
            {
                return false;
            }

            if (target.HasBuffOfType(BuffType.SpellImmunity) || target.HasBuffOfType(BuffType.SpellShield) || Player.Instance.IsDashing()) return false;

            return collisionTypeCheckage(target, MenuManager.Modes.Condemn.MinChanceCondemn);
        }

        public static bool collisionTypeCheckage(Obj_AI_Base target, int value)
        {
            switch (value)
            {
                case 1:
                    {
                        var position = Player.Instance.Position.Extend(target.Position, Player.Instance.Distance(target));
                        for (int i = 0; i < 470; i += 10)
                        {
                            var cPos = Player.Instance.Position.Extend(position, Player.Instance.Distance(position) + i).To3D();

                            var collideCurrent = (cPos.ToNavMeshCell().CollFlags.HasFlag(CollisionFlags.Wall) ||
                                                  cPos.ToNavMeshCell().CollFlags.HasFlag(CollisionFlags.Building));

                            if (collideCurrent) return true;
                        }
                    }
                    break;
                case 2:
                    {
                        var position = Player.Instance.ServerPosition.Extend(target.ServerPosition, Player.Instance.Distance(target));
                        for (int i = 0; i < 470; i += 10)
                        {
                            var cPos = Player.Instance.ServerPosition.Extend(position, Player.Instance.Distance(position) + i).To3D();

                            var collideCurrent = (cPos.ToNavMeshCell().CollFlags.HasFlag(CollisionFlags.Wall) ||
                                                  cPos.ToNavMeshCell().CollFlags.HasFlag(CollisionFlags.Building));

                            if (collideCurrent) return true;
                        }
                    }
                    break;
                case 3:
                    {
                        var predictPos = Prediction.Position.PredictUnitPosition(target, 500);
                        for (int i = 0; i < 470; i += 10)
                        {
                            var cPredPos = Player.Instance.ServerPosition.Extend(predictPos, Player.Instance.Distance(predictPos) + i).To3D();

                            var collidePredict = (cPredPos.ToNavMeshCell().CollFlags.HasFlag(CollisionFlags.Wall) ||
                                                  cPredPos.ToNavMeshCell().CollFlags.HasFlag(CollisionFlags.Building));
                            if (collidePredict) return true;
                        }
                    }
                    break;
                case 4:
                    {
                        var predPos = Prediction.Position.PredictUnitPosition(target, 500);
                        var position = Player.Instance.ServerPosition.Extend(target.ServerPosition, Player.Instance.Distance(target));
                        for (int i = 0; i < 470; i += 10)
                        {
                            var cPos = Player.Instance.ServerPosition.Extend(position, Player.Instance.Distance(position) + i).To3D();
                            var cPredPos = Player.Instance.ServerPosition.Extend(predPos, Player.Instance.Distance(predPos) + i).To3D();

                            var collidePredict = (cPredPos.ToNavMeshCell().CollFlags.HasFlag(CollisionFlags.Wall) ||
                                                  cPredPos.ToNavMeshCell().CollFlags.HasFlag(CollisionFlags.Building));

                            var collideCurrent = (cPos.ToNavMeshCell().CollFlags.HasFlag(CollisionFlags.Wall) ||
                                                  cPos.ToNavMeshCell().CollFlags.HasFlag(CollisionFlags.Building));

                            if (collideCurrent && collidePredict) return true;
                        }
                    }
                    break;
                default:
                    {
                        var collisionCount = 0;
                        for (var checkCount = 0; checkCount < 5; checkCount++)
                        {
                            var predPos = Prediction.Position.PredictUnitPosition(target, 500);
                            var position = Player.Instance.ServerPosition.Extend(target.ServerPosition, Player.Instance.Distance(target));
                            for (int i = 0; i < 470; i += 47)
                            {
                                var cPos = Player.Instance.ServerPosition.Extend(position, Player.Instance.Distance(position) + i).To3D();
                                var cPredPos = Player.Instance.ServerPosition.Extend(predPos, Player.Instance.Distance(predPos) + i).To3D();

                                var collidePredict = (cPredPos.ToNavMeshCell().CollFlags.HasFlag(CollisionFlags.Wall) ||
                                                      cPredPos.ToNavMeshCell().CollFlags.HasFlag(CollisionFlags.Building));

                                var collideCurrent = (cPos.ToNavMeshCell().CollFlags.HasFlag(CollisionFlags.Wall) ||
                                                      cPos.ToNavMeshCell().CollFlags.HasFlag(CollisionFlags.Building));

                                if (collidePredict && collideCurrent) collisionCount++;

                                if (collisionCount >= 2) return true;
                            }
                        }

                    }
                    break;
            }

            return false;
        }

        public static bool collisionTypeCheckage(Vector3 position)
        {
            var collisionCount = 0;
            for (var checkCount = 0; checkCount < 5; checkCount++)
            {
                for (var i = 0; i < 470; i += 47)
                {
                    var cPos = Player.Instance.ServerPosition.Extend(position, Player.Instance.Distance(position) + i).To3D();

                    var collideCurrent = (cPos.ToNavMeshCell().CollFlags.HasFlag(CollisionFlags.Wall) ||
                                            cPos.ToNavMeshCell().CollFlags.HasFlag(CollisionFlags.Building));

                    if (collideCurrent) collisionCount++;

                    if (collisionCount >= 2) return true;
                }
            }

            return false;
        }


        public static Vector3 BestTumblePost(Obj_AI_Base target)
        {
            if (MenuManager.Modes.Misc.AutomaticPositionTumble && target.CountEnemiesInRange(800) <= 1)
            {  
                if (SpellManager.E.IsReady() && MenuManager.Modes.Condemn.TumbleIfCondemn)
                {
                    var direction = (target.ServerPosition - Player.Instance.ServerPosition).To2D().Normalized();

                    for (var i = 0; i < 180; i++)
                    {
                        var angle = (float)(i * Math.PI / 180);
                        var position = Player.Instance.ServerPosition.To2D() + direction.Rotated(-angle) * 300;

                        if (collisionTypeCheckage(position.To3D()) && position.Distance(Player.Instance.ServerPosition) >= 300 && position.Distance(target) < Player.Instance.Distance(target))
                            return position.To3D();
                    }
                }

                return Game.CursorPos;
            }
            else
            {
                return Game.CursorPos;
            }
        }

        public static Vector3 GetTumblePos(this Obj_AI_Base target)
        {

            if (target.IsFacing(Player.Instance))
            {
                return target.ServerPosition.Shorten(Player.Instance.ServerPosition, 300);
            }
            else
            {
                var aRc = new Geometry.Polygon.Circle(Player.Instance.ServerPosition.To2D(), 300).ToClipperPath();
                var cursorPos = Game.CursorPos;
                var targetPosition = target.ServerPosition;
                var pList = new List<Vector3>();
                var additionalDistance = (0.106 + Game.Ping / 2000f) * target.MoveSpeed;
                foreach (var v3 in aRc.Select(p => new Vector2(p.X, p.Y).To3D()))
                {
                    if (target.IsFacing(Player.Instance))
                    {
                        if (v3.Distance(targetPosition) < 350) pList.Add(v3);
                    }
                    else
                    {
                        if (v3.Distance(targetPosition) < 350 - additionalDistance) pList.Add(v3);
                    }
                }
                return pList.Count > 1
                    ? pList.OrderByDescending(el => el.Distance(cursorPos)).FirstOrDefault()
                    : Vector3.Zero;
            }
        }
    }
}
