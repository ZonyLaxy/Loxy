using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK.Events;

namespace LoxyVayne
{
    class Program
    {
        public static bool IDDQD { get; set; }

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += OnLoadingComplete;
        }

        private static void OnLoadingComplete(EventArgs args)
        {
            if (Player.Instance.ChampionName != "Vayne") return;

            MenuManager.Initialize();
            Chat.OnInput += ChatInputComing;
            MenuManager.Modes.Gosu.UnSetGod();
            new Vayne().Initialize();
        }
        private static void ChatInputComing(ChatInputEventArgs args)
        {
            if (args.Input == "#IDDQD" && !MenuManager.IDDQD)
            {
                args.Input = "";
                MenuManager.IDDQD = true;
                MenuManager.Modes.Gosu.SetGod();
            }
            else if (args.Input == "#IDDQD" && MenuManager.IDDQD)
            {
                args.Input = "";
            }
        }
    }
}
