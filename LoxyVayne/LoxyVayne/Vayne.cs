using System;
using EloBuddy;
using EloBuddy.SDK;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VayShot; 
using LoxyVay.Managers;
using LoxyVay.Extensions;

namespace LoxyVayne
{
    class Vayne
    {
        private readonly ActionQueueList comboQueue;
        private readonly ActionQueueList lastHitQueue;
        private readonly ActionQueueList harassQueue;
        private readonly ActionQueueList laneQueue;
        private readonly ActionQueueList jungleQueue;

        private readonly ActionManager actionManager;

        private Orbwalker.ActiveModes activeMode;

        public Vayne()
        {
            actionManager = new ActionManager();
            comboQueue = new ActionQueueList();
            lastHitQueue = new ActionQueueList();
            harassQueue = new ActionQueueList();
            laneQueue = new ActionQueueList();
            jungleQueue = new ActionQueueList();
        }

        public void Initialize()
        {
            Drawing.OnDraw += OnDraw;
            Game.OnTick += OnTick;
            Game.OnUpdate += OnUpdate;
            Game.OnNotify += GameOnOnNotify;
            Orbwalker.OnPostAttack += OnAfterAa;

            if (MenuManager.Modes.Misc.Evade)
                EvadeHelper.OnLoad();
        }

        private static void GameOnOnNotify(GameNotifyEventArgs args)
        {
            if (args.EventId == GameEventId.OnChampionKill)
            {
                var killer = ObjectManager.GetUnitByNetworkId<Obj_AI_Base>(args.NetworkId);

                if (killer.IsMe)
                {
                    if (MenuManager.Modes.Misc.MasteryBadge)
                        Chat.Say("/masterybadge");
                    if (MenuManager.Modes.Misc.Laugh)
                        Core.DelayAction(() => Chat.Say("/l"), 0x3e8);
                }
            }
        }

        private void OnUpdate(EventArgs args)
        {
            if (!MenuManager.Modes.Misc.GameUpdate)
            {
                Game.OnTick += OnTick;
                Game.OnUpdate -= OnUpdate;
            }
            PermaActive();
        }

        private void OnTick(EventArgs args)
        {
            if (MenuManager.Modes.Misc.GameUpdate)
            {
                Game.OnUpdate += OnUpdate;
                Game.OnTick -= OnTick;
            }
            PermaActive();
        }

        private void OnDraw(EventArgs args)
        {

        }

        private void OnAfterAa(AttackableUnit target, EventArgs args)
        {
            if (MenuManager.Modes.Misc.WaitAAForQ)
                if (target != null && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) && target.IsValidTarget(Player.Instance.GetAutoAttackRange() + SpellManager.Q.Range))
                {
                    var enemy = ObjectManager.GetUnitByNetworkId<Obj_AI_Base>((uint)target.NetworkId);
                    actionManager.EnqueueAction(
                        comboQueue,
                        () => CanCastQ(Orbwalker.ActiveModes.Combo),
                        () => SpellManager.Q.Cast(Game.CursorPos),
                        () => !CanCastQ(Orbwalker.ActiveModes.Combo));
                }
            if (target != null && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass) && target.IsValidTarget(Player.Instance.GetAutoAttackRange() + SpellManager.Q.Range))
            {
                var enemy = ObjectManager.GetUnitByNetworkId<Obj_AI_Base>((uint)target.NetworkId);
                actionManager.EnqueueAction(
                    harassQueue,
                    () => CanCastQ(Orbwalker.ActiveModes.Harass),
                    () => SpellManager.Q.Cast(ExtensionsHelpers.BestTumblePost(enemy)),
                    () => !CanCastQ(Orbwalker.ActiveModes.Harass));
            }
            else if (target != null && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit) && target.IsValidTarget(Player.Instance.GetAutoAttackRange() + SpellManager.Q.Range) && Orbwalker.LastTarget != target)
            {
                var enemy = ObjectManager.GetUnitByNetworkId<Obj_AI_Base>((uint)target.NetworkId);
                actionManager.EnqueueAction(
                    lastHitQueue,
                    () => CanCastQ(Orbwalker.ActiveModes.LastHit),
                    () => SpellManager.Q.Cast(ExtensionsHelpers.BestTumblePost(enemy)),
                    () => !CanCastQ(Orbwalker.ActiveModes.LastHit));
            }
            else if (target != null && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) && target.IsValidTarget(Player.Instance.GetAutoAttackRange() + SpellManager.Q.Range))
            {
                var enemy = ObjectManager.GetUnitByNetworkId<Obj_AI_Base>((uint)target.NetworkId);
                actionManager.EnqueueAction(
                    laneQueue,
                    () => CanCastQ(Orbwalker.ActiveModes.LaneClear),
                    () => SpellManager.Q.Cast(ExtensionsHelpers.BestTumblePost(enemy)),
                    () => !CanCastQ(Orbwalker.ActiveModes.LaneClear));
            }
            else if (target != null && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear) && target.IsValidTarget(Player.Instance.GetAutoAttackRange() + SpellManager.Q.Range))
            {
                var enemy = ObjectManager.GetUnitByNetworkId<Obj_AI_Base>((uint)target.NetworkId);
                actionManager.EnqueueAction(
                    jungleQueue,
                    () => CanCastQ(Orbwalker.ActiveModes.JungleClear),
                    () => SpellManager.Q.Cast(ExtensionsHelpers.BestTumblePost(enemy)),
                    () => !CanCastQ(Orbwalker.ActiveModes.JungleClear));
            }
        }

        private void PermaActive()
        {
            if (MenuManager.Modes.Misc.Evade && !EvadeHelper.Evading)
                EvadeHelper.OnLoad();

            if (!MenuManager.Modes.Misc.Evade && EvadeHelper.Evading)
                EvadeHelper.UnLoad();

            activeMode = Orbwalker.ActiveModesFlags;

            if (activeMode.HasFlag(Orbwalker.ActiveModes.Combo)) OnCombo();
            if (activeMode.HasFlag(Orbwalker.ActiveModes.LastHit)) OnLastHit();
            if (activeMode.HasFlag(Orbwalker.ActiveModes.Harass)) OnHarass();
            if (activeMode.HasFlag(Orbwalker.ActiveModes.LaneClear)) OnLaneClear();
            if (activeMode.HasFlag(Orbwalker.ActiveModes.JungleClear)) OnJungleClear();
            /*        
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee)) OnFlee();
            */
        }

        private void OnLaneClear()
        {
            if (actionManager.ExecuteNextAction(laneQueue))
            {
                return;
            }

            if (!MenuManager.Modes.Misc.WaitAAForQ)
            {
                var target = EntityManager.MinionsAndMonsters.EnemyMinions.FirstOrDefault(m => m.IsValidTarget(Player.Instance.AttackRange));

                if (target != null)
                {
                    actionManager.EnqueueAction(
                    laneQueue,
                    () => CanCastQ(Orbwalker.ActiveModes.LaneClear),
                    () => SpellManager.Q.Cast(ExtensionsHelpers.BestTumblePost(target)),
                    () => !CanCastQ(Orbwalker.ActiveModes.LaneClear));
                }
            }
        }

        private void OnJungleClear()
        {
            if (actionManager.ExecuteNextAction(jungleQueue))
            {
                return;
            }

            if (!MenuManager.Modes.Misc.WaitAAForQ)
            {
                var target = EntityManager.MinionsAndMonsters.Monsters.FirstOrDefault(m => m.IsValidTarget(Player.Instance.AttackRange));

                if (target != null)
                {
                    actionManager.EnqueueAction(
                    jungleQueue,
                    () => CanCastQ(Orbwalker.ActiveModes.JungleClear),
                    () => SpellManager.Q.Cast(ExtensionsHelpers.BestTumblePost(target)),
                    () => !CanCastQ(Orbwalker.ActiveModes.JungleClear));
                }
            }
        }

        private void OnHarass()
        {
            if (actionManager.ExecuteNextAction(harassQueue))
            {
                return;
            }

            if (!MenuManager.Modes.Misc.WaitAAForQ)
            {
                var target = TargetSelector.GetTarget(Player.Instance.GetAutoAttackRange() + SpellManager.Q.Range, DamageType.Physical);
                actionManager.EnqueueAction(
                    harassQueue,
                    () => CanCastQ(Orbwalker.ActiveModes.Harass),
                    () => SpellManager.Q.Cast(ExtensionsHelpers.BestTumblePost(target)),
                    () => !CanCastQ(Orbwalker.ActiveModes.Harass));
            }

            CanCastE(Orbwalker.ActiveModes.Harass);
        }

        private void OnLastHit()
        {
            if (actionManager.ExecuteNextAction(lastHitQueue))
            {
                return;
            }

            if (!MenuManager.Modes.Misc.WaitAAForQ && !Orbwalker.CanAutoAttack && MenuManager.Modes.LastHit.Mana)
            {
                var targets = EntityManager.MinionsAndMonsters.EnemyMinions.Where(t => t != null && t.IsValidTarget(SpellManager.Q.Range) && Orbwalker.LastTarget != null && t.NetworkId != Orbwalker.LastTarget.NetworkId && Player.Instance.GetAutoAttackDamage(t, true) > t.Health);

                if (!targets.Any()) return;

                actionManager.EnqueueAction(
                    lastHitQueue,
                    () => CanCastQ(Orbwalker.ActiveModes.Combo),
                    () => SpellManager.Q.Cast(ExtensionsHelpers.BestTumblePost(targets.FirstOrDefault())),
                    () => !CanCastQ(Orbwalker.ActiveModes.Combo));
            }
        }

        private void OnCombo()
        {
            if (actionManager.ExecuteNextAction(comboQueue))
            {
                return;
            }

            actionManager.EnqueueAction(
                    comboQueue,
                    () => CanCastR(Orbwalker.ActiveModes.Combo),
                    () => SpellManager.R.Cast(),
                    () => !CanCastR(Orbwalker.ActiveModes.Combo));

            if (!MenuManager.Modes.Misc.WaitAAForQ)
            {
                var target = TargetSelector.GetTarget(Player.Instance.GetAutoAttackRange() + SpellManager.Q.Range, DamageType.Physical);
                actionManager.EnqueueAction(
                    comboQueue,
                    () => CanCastQ(Orbwalker.ActiveModes.Combo),
                    () => SpellManager.Q.Cast(ExtensionsHelpers.BestTumblePost(target)),
                    () => !CanCastQ(Orbwalker.ActiveModes.Combo));
            }

            CanCastE(Orbwalker.ActiveModes.Combo);
        }

        private static bool CanCastQ(Orbwalker.ActiveModes activeMode)
        {
            switch (activeMode)
            {
                case Orbwalker.ActiveModes.Combo:
                    if (SpellManager.Q.IsReady() && MenuManager.Modes.Combo.UseQ)
                        return true;
                    break;
                case Orbwalker.ActiveModes.Harass:
                    if (SpellManager.Q.IsReady() && MenuManager.Modes.Harass.UseQ && MenuManager.Modes.Harass.Mana)
                        return true;
                    break;
                case Orbwalker.ActiveModes.LastHit:
                    if (SpellManager.Q.IsReady() && MenuManager.Modes.LastHit.UseQ && MenuManager.Modes.LastHit.Mana)
                        return true;
                    break;
                case Orbwalker.ActiveModes.LaneClear:
                    if (SpellManager.Q.IsReady() && MenuManager.Modes.LaneClear.UseQ && MenuManager.Modes.LaneClear.Mana)
                        return true;
                    break;
                case Orbwalker.ActiveModes.JungleClear:
                    if (SpellManager.Q.IsReady() && MenuManager.Modes.JungleClear.UseQ && MenuManager.Modes.JungleClear.Mana)
                        return true;
                    break;
            }

            return false;
        }

        private void CanCastE(Orbwalker.ActiveModes activeMode)
        {
            if (MenuManager.Modes.Condemn.CondemnOnlyTarget)
            {
                var target = TargetSelector.GetTarget(SpellManager.E.Range, DamageType.Physical);

                if (target == null || !target.IsValidTarget()) return;

                switch (activeMode)
                {
                    case Orbwalker.ActiveModes.Combo:
                        if (SpellManager.E.IsReady() && MenuManager.Modes.Combo.UseE)
                            actionManager.EnqueueAction(
                                comboQueue,
                                () => target.Condenable(),
                                () => SpellManager.E.Cast(target),
                                () => !target.Condenable());
                        break;
                    case Orbwalker.ActiveModes.Harass:
                        if (SpellManager.E.IsReady() && MenuManager.Modes.Harass.UseE && MenuManager.Modes.Harass.Mana)
                            actionManager.EnqueueAction(
                                comboQueue,
                                () => target.Condenable(),
                                () => SpellManager.E.Cast(target),
                                () => !target.Condenable());
                        break;
                }
            }
            else
            {
                foreach (var target in EntityManager.Heroes.Enemies.Where(e => e.IsValidTarget(SpellManager.E.Range)))
                {
                    switch (activeMode)
                    {
                        case Orbwalker.ActiveModes.Combo:
                            if (SpellManager.E.IsReady() && MenuManager.Modes.Combo.UseE)
                                actionManager.EnqueueAction(
                                    comboQueue,
                                    () => target.Condenable(),
                                    () => SpellManager.E.Cast(target),
                                    () => !target.Condenable());
                            break;
                        case Orbwalker.ActiveModes.Harass:
                            if (SpellManager.E.IsReady() && MenuManager.Modes.Harass.UseE && MenuManager.Modes.Harass.Mana && target.Condenable())
                                actionManager.EnqueueAction(
                                    comboQueue,
                                    () => target.Condenable(),
                                    () => SpellManager.E.Cast(target),
                                    () => !target.Condenable());
                            break;
                    }
                }
            }
        }

        private bool CanCastR(Orbwalker.ActiveModes activeMode)
        {
            var target = TargetSelector.GetTarget(Player.Instance.GetAutoAttackRange(), DamageType.Physical);

            if (target == null || !target.IsValidTarget()) return false;

            switch (activeMode)
            {
                case Orbwalker.ActiveModes.Combo:
                    if (SpellManager.R.IsReady() && MenuManager.Modes.Combo.UseR && (MenuManager.Modes.Combo.RMin || Player.Instance.HealthPercent < target.HealthPercent))
                        return true;
                    break;
            }

            return false;
        }
    }
}

    

