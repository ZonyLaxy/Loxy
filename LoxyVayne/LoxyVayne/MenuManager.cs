using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

namespace LoxyVayne
{
    class MenuManager
    {
        private static readonly Menu Menu;
        private static Menu CondemnMenu;
        public static bool IDDQD { get; set; }
        static MenuManager()
        {
            Menu = MainMenu.AddMenu("Vayne", "Vayne");
            Menu.AddGroupLabel("Vayne");

            Modes.Initialize();
        }

        public static void Initialize() { }

        public static class Modes
        {
            private static readonly Menu Menu;

            static Modes()
            {
                Menu = MenuManager.Menu.AddSubMenu("Modes");

                Combo.Initialize();
                Menu.AddSeparator();
                Harass.Initialize();
                Menu.AddSeparator();
                LastHit.Initialize();
                Menu.AddSeparator();
                LaneClear.Initialize();
                Menu.AddSeparator();
                JungleClear.Initialize();
                Menu.AddSeparator();

                Menu = MenuManager.Menu.AddSubMenu("Condemn");
                CondemnMenu = Menu;
                Condemn.Initialize();

                Menu = MenuManager.Menu.AddSubMenu("Misc");
                Misc.Initialize();

                Menu = MenuManager.Menu.AddSubMenu("Activator");
                ItemUsage.Initialize();

                Menu = MenuManager.Menu.AddSubMenu("Gosu");
                Gosu.Initialize();
            }

            public static void Initialize()
            {
            }

            public static class Combo
            {
                private static readonly CheckBox _useQ;
                private static readonly CheckBox _focusW;
                private static readonly CheckBox _useE;
                private static readonly CheckBox _useR;
                private static readonly Slider _minEnemiesforR;

                public static bool UseQ
                {
                    get { return _useQ.CurrentValue; }
                }
                public static bool FocusW
                {
                    get { return _focusW.CurrentValue; }
                }
                public static bool UseE
                {
                    get { return _useE.CurrentValue; }
                }
                public static bool UseR
                {
                    get { return _useR.CurrentValue; }
                }
                public static bool RMin
                {
                    get
                    {
                        return EntityManager.Heroes.Enemies.Count(e => Player.Instance.IsInAutoAttackRange(e) && e.IsValidTarget()) >= _minEnemiesforR.CurrentValue;
                    }
                }

                static Combo()
                {
                    Menu.AddGroupLabel("Combo");
                    _useQ = Menu.Add("combo.q", new CheckBox("Use Q"));
                    _focusW = Menu.Add("combo.w", new CheckBox("Focus enemy with 2 stacks", false));
                    _useE = Menu.Add("combo.e", new CheckBox("Use E"));
                    _useR = Menu.Add("combo.r", new CheckBox("Use R"));
                    _minEnemiesforR = Menu.Add("combo.minEnemiesInRangeForR", new Slider("Min Enemies in Range for R", 2, 1, 5));
                }

                public static void Initialize()
                {
                }
            }

            public static class Draw
            {
                private static readonly CheckBox _draw;
                private static readonly CheckBox _drawQ;
                private static readonly CheckBox _possibleDamage;
                private static readonly Slider _countofAA;
                private static readonly CheckBox _drawCondemnPos;
                private static readonly CheckBox _showCondenableHeros;
                private static readonly CheckBox _showIfCondenableOnFlashE;

                public static bool Drawing
                {
                    get { return _draw.CurrentValue; }
                }
                public static bool DrawQ
                {
                    get { return _drawQ.CurrentValue; }
                }
                public static bool DamageIndicator
                {
                    get { return _possibleDamage.CurrentValue; }
                }
                public static int CountAA
                {
                    get { return _countofAA.CurrentValue; }
                }
                public static bool DrawCondemnPos
                {
                    get { return _drawCondemnPos.CurrentValue; }
                }
                public static bool ShowCondenableHeros
                {
                    get { return _showCondenableHeros.CurrentValue; }
                }
                public static bool ShowCondemnFlashE
                {
                    get { return _showIfCondenableOnFlashE.CurrentValue; }
                }

                static Draw()
                {
                    Menu.AddGroupLabel("Draw");
                    _draw = Menu.Add("draw.showDrawings", new CheckBox("Draw Stuffs", false));
                    _drawQ = Menu.Add("draw.q", new CheckBox("Draw Q Range"));
                    _countofAA = Menu.Add("combo.minEnemiesInRangeForR", new Slider("AA in dmg calc", 3, 1, 12));
                    _drawCondemnPos = Menu.Add("draw.condemnPos", new CheckBox("Draw Condemn Pos"));
                    _showCondenableHeros = Menu.Add("draw.showCondenableHeros", new CheckBox("Show Champions able to condemn"));
                    _showIfCondenableOnFlashE = Menu.Add("draw.flashCondemn", new CheckBox("Show Flash Condemn"));
                }

                public static void Initialize()
                {
                }
            }

            public static class LastHit
            {
                private static readonly CheckBox _useQ;
                private static readonly Slider _manaPercent;

                public static bool UseQ
                {
                    get { return _useQ.CurrentValue; }
                }
                public static bool Mana
                {
                    get { return Player.Instance.ManaPercent > _manaPercent.CurrentValue; }
                }

                static LastHit()
                {
                    Menu.AddGroupLabel("LastHit");
                    _useQ = Menu.Add("lasthit.q", new CheckBox("Use Q"));
                    _manaPercent = Menu.Add("lasthit.mana", new Slider("Min Mana %", 45, 0, 100));
                }

                public static void Initialize()
                {
                }
            }

            public static class Harass
            {
                private static readonly CheckBox _useQ;
                private static readonly CheckBox _focusW;
                private static readonly CheckBox _useE;
                private static readonly Slider _manaPercent;

                public static bool UseQ
                {
                    get { return _useQ.CurrentValue; }
                }
                public static bool FocusW
                {
                    get { return _focusW.CurrentValue; }
                }
                public static bool UseE
                {
                    get { return _useE.CurrentValue; }
                }
                public static bool Mana
                {
                    get { return Player.Instance.ManaPercent > _manaPercent.CurrentValue; }
                }

                static Harass()
                {
                    Menu.AddGroupLabel("Harass");
                    _useQ = Menu.Add("harass.q", new CheckBox("Use Q"));
                    _focusW = Menu.Add("harass.w", new CheckBox("Focus enemy with 2 stacks", false));
                    _useE = Menu.Add("harass.e", new CheckBox("Use E"));
                    _manaPercent = Menu.Add("harass.mana", new Slider("Min Mana %", 70, 0, 100));
                }

                public static void Initialize()
                {
                }
            }

            public static class LaneClear
            {
                private static readonly CheckBox _useQ;
                private static readonly CheckBox _focusW;
                private static readonly Slider _manaPercent;

                public static bool UseQ
                {
                    get { return _useQ.CurrentValue; }
                }
                public static bool FocusW
                {
                    get { return _focusW.CurrentValue; }
                }
                public static bool Mana
                {
                    get { return Player.Instance.ManaPercent > _manaPercent.CurrentValue; }
                }

                static LaneClear()
                {
                    Menu.AddGroupLabel("LaneClear");
                    _useQ = Menu.Add("laneclear.q", new CheckBox("Use Q"));
                    _focusW = Menu.Add("laneclear.w", new CheckBox("Focus enemy with 2 stacks", false));
                    _manaPercent = Menu.Add("laneclear.mana", new Slider("Min Mana %", 65, 0, 100));
                }

                public static void Initialize()
                {
                }
            }

            public static class JungleClear
            {
                private static readonly CheckBox _useQ;
                private static readonly CheckBox _focusW;
                private static readonly Slider _manaPercent;

                public static bool UseQ
                {
                    get { return _useQ.CurrentValue; }
                }
                public static bool FocusW
                {
                    get { return _focusW.CurrentValue; }
                }
                public static bool Mana
                {
                    get { return Player.Instance.ManaPercent > _manaPercent.CurrentValue; }
                }

                static JungleClear()
                {
                    Menu.AddGroupLabel("JungleClear");
                    _useQ = Menu.Add("jungleclear.q", new CheckBox("Use Q"));
                    _focusW = Menu.Add("jungleclear.w", new CheckBox("Focus enemy with 2 stacks", false));
                    _manaPercent = Menu.Add("jungleclear.mana", new Slider("Min Mana %", 65, 0, 100));
                }

                public static void Initialize()
                {
                }
            }

            public static class Condemn
            {
                private static readonly CheckBox _condemnAntiGap;
                private static readonly CheckBox _condemnInterrupt;
                private static readonly CheckBox _condemnOnlyTarget;
                private static readonly CheckBox _tumbleForBestPos;
                private static readonly KeyBind _flashCondemn;
                private static readonly Slider _minChanceCondemn;

                public static bool CondemnAntiGap
                {
                    get { return _condemnAntiGap.CurrentValue; }
                }
                public static bool CondemnInterrupt
                {
                    get { return _condemnInterrupt.CurrentValue; }
                }
                public static bool CondemnOnlyTarget
                {
                    get { return _condemnOnlyTarget.CurrentValue; }
                }
                public static bool TumbleIfCondemn
                {
                    get { return _tumbleForBestPos.CurrentValue; }
                }
                public static bool FlashCondemn
                {
                    get { return _flashCondemn.CurrentValue; }
                }
                public static bool CondemnHero(AIHeroClient enemy)
                {
                    return CondemnMenu["dnCondemn" + enemy.ChampionName.ToLower()].Cast<CheckBox>().CurrentValue;
                }
                public static int MinChanceCondemn
                {
                    get { return _minChanceCondemn.CurrentValue; }
                }

                static Condemn()
                {
                    Menu.AddGroupLabel("Condemn");
                    _condemnAntiGap = Menu.Add("condemn.antiGap", new CheckBox("Condemn Gap Closers on You"));
                    _condemnInterrupt = Menu.Add("condemn.interrupt", new CheckBox("Condemn near possible dangerous to interrupt"));
                    _condemnOnlyTarget = Menu.Add("condemn.onlyTarget", new CheckBox("Only Condemn Current Target"));
                    _tumbleForBestPos = Menu.Add("condemn.q", new CheckBox("Tumble for Best Condemn Pos"));
                    _flashCondemn = Menu.Add("condemn.flashCondemn", new KeyBind("Flash Condemn", false, KeyBind.BindTypes.HoldActive, 'T'));
                    _minChanceCondemn = Menu.Add("condemn.minChanceCondemn", new Slider("Min Chance of Condemn to Cast", 4, 1, 5));
                    Menu.AddLabel("1 20%, 2 40%, 3 60%, 4 80%, 5 100%");

                    Menu.AddGroupLabel("Champions Condennable");
                    foreach (var enemy in EntityManager.Heroes.Enemies)
                    {
                        Menu.Add("dnCondemn" + enemy.ChampionName.ToLower(), new CheckBox("Don't Condemn " + enemy.ChampionName, false));
                    }
                }

                public static void Initialize()
                {
                }
            }

            public static class ItemUsage
            {
                private static readonly CheckBox _cleanser;
                private static readonly CheckBox _btrk;
                private static readonly CheckBox _ghostBlade;

                public static bool Cleanse
                {
                    get { return _cleanser.CurrentValue; }
                }
                public static bool BTRK
                {
                    get { return _btrk.CurrentValue; }
                }
                public static bool Youmu
                {
                    get { return _ghostBlade.CurrentValue; }
                }

                static ItemUsage()
                {
                    Menu.AddGroupLabel("Item Manager - Defensive");
                    _cleanser = Menu.Add("item.cleanse", new CheckBox("Cleanse"));
                    Menu.AddGroupLabel("Item Manager - Offensive");
                    _btrk = Menu.Add("item.btrk", new CheckBox("Bilgewater / BTRK"));
                    _ghostBlade = Menu.Add("item.ghostblade", new CheckBox("Youmu"));
                }

                public static void Initialize()
                {
                }
            }

            public static class Misc
            {
                private static readonly CheckBox _waitAAforQ;
                private static readonly CheckBox _tumblePosition;
                private static readonly CheckBox _showMastery;
                private static readonly CheckBox _laugh;
                private static readonly CheckBox _evadeQ;
                private static readonly KeyBind _tumbleWall;
                private static readonly CheckBox _useGameUpdate;

                public static bool WaitAAForQ
                {
                    get { return _waitAAforQ.CurrentValue; }
                }
                public static bool AutomaticPositionTumble
                {
                    get { return _tumblePosition.CurrentValue; }
                }
                public static bool MasteryBadge
                {
                    get { return _showMastery.CurrentValue; }
                }
                public static bool Laugh
                {
                    get { return _laugh.CurrentValue; }
                }
                public static bool Evade
                {
                    get { return _evadeQ.CurrentValue; }
                }
                public static bool WallTumble
                {
                    get { return _tumbleWall.CurrentValue; }
                }
                public static bool GameUpdate
                {
                    get { return _useGameUpdate.CurrentValue; }
                }

                static Misc()
                {
                    Menu.AddGroupLabel("Tumble Options");
                    _waitAAforQ = Menu.Add("tumble.waitAA", new CheckBox("Wait AA for cast Q"));
                    _tumblePosition = Menu.Add("tumble.automaticPosition", new CheckBox("Automatic Position Tumble"));
                    Menu.AddGroupLabel("Evader");
                    _evadeQ = Menu.Add("evader.evadeQ", new CheckBox("Evade"));
                    Menu.AddGroupLabel("Misc");
                    _showMastery = Menu.Add("misc.masteryBadge", new CheckBox("Show Mastery Badge"));
                    _laugh = Menu.Add("misc.laugh", new CheckBox("Laugh from this dead noob", false));
                    Menu.AddGroupLabel("Etc");
                    _useGameUpdate = Menu.Add("misc.gameUpdate", new CheckBox("Use Game Update", false));
                    Menu.AddLabel("Update = Peformance, Tick = Better FPS");
                    _tumbleWall = Menu.Add("misc.tumbleWall", new KeyBind("Wall Tumble", false, KeyBind.BindTypes.HoldActive, 'K'));
                }

                public static void Initialize()
                {
                }
            }

            public static class Gosu
            {
                private static readonly CheckBox _godMode;

                public static bool IDDQD
                {
                    get { return _godMode.CurrentValue; }
                }

                public static void SetGod()
                {
                    _godMode.CurrentValue = true;
                }

                public static void UnSetGod()
                {
                    _godMode.CurrentValue = false;
                }

                static Gosu()
                {
                    Menu.AddGroupLabel("I choosed to be banned by my own risk.");
                    _godMode = Menu.Add("gosu.enable", new CheckBox("IDDQD", false));
                    _godMode.OnValueChange += delegate
                    {
                        if (!MenuManager.IDDQD && _godMode.CurrentValue)
                        {
                            Core.DelayAction(UnSetGod, 250);
                            Core.DelayAction(() => Chat.Print("Cheating on my watch ? definetely no!"), 250);
                        }
                        else if (MenuManager.IDDQD && !_godMode.CurrentValue)
                        {
                            MenuManager.IDDQD = false;
                            Core.DelayAction(() => Chat.Print("Good choice."), 250);
                        }
                    };
                }

                public static void Initialize()
                {
                }
            }
        }
    }
}