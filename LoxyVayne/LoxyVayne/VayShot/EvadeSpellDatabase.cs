using EloBuddy;
using System.Collections.Generic;
using System.Linq;

namespace VayShot
{
    public class EvadeSpellDatabase
    {
        public static List<EvadeSpellData> Spells = new List<EvadeSpellData>();

        static EvadeSpellDatabase()
        {
            EvadeSpellData spell;

            #region Champion SpellShields

            #region Vayne

            if (ObjectManager.Player.ChampionName == "Vayne")
            {
                spell = new DashData("Vayne Q", SpellSlot.Q, 300, true, 100, 910, 2);
                Spells.Add(spell);
            }

            #endregion

            #endregion

            /*Flash
            if (ObjectManager.Player.GetSpellSlot("SummonerFlash") != SpellSlot.Unknown)
            {
                spell = new BlinkData("Flash", ObjectManager.Player.GetSpellSlot("SummonerFlash"), 400, 100, 5, true);
                Spells.Add(spell);
            }*/
        }

        public static EvadeSpellData GetByName(string name)
        {
            name = name.ToLower();
            return Spells.FirstOrDefault(evadeSpellData => evadeSpellData.Name.ToLower() == name);
        }
    }
}