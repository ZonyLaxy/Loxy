using System;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Constants;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Notifications;
using EloBuddy.SDK.Rendering;
using EloBuddy.SDK.Spells;
using EloBuddy.SDK.ThirdParty;
using EloBuddy.SDK.ThirdParty.Glide;
using EloBuddy.SDK.Utils;

namespace RyzeClear
{
	class Program
	{
		static Menu pcrmenu;
		static readonly Spell.Skillshot Q = new Spell.Skillshot(SpellSlot.Q, 1000, SkillShotType.Linear, 250, 1700, 60, DamageType.Magical)
		{
			AllowedCollisionCount = 0,
			MinimumHitChance = HitChance.High
		};
		static readonly Spell.Skillshot QQ = new Spell.Skillshot(SpellSlot.Q, 1000, SkillShotType.Linear, 250, 1700, 60, DamageType.Magical)
		{
			AllowedCollisionCount = 1000,
			MinimumHitChance = HitChance.High
		};
		static readonly Spell.Targeted W = new Spell.Targeted(SpellSlot.W, 615, DamageType.Magical);
		static readonly Spell.Targeted E = new Spell.Targeted(SpellSlot.E, 615, DamageType.Magical);
		
		public static void Main(string[] args)
		{
			Loading.OnLoadingComplete += Loading_OnLoadingComplete;
		}

		static void Loading_OnLoadingComplete(EventArgs args)
		{
			pcrmenu = MainMenu.AddMenu("Ryze Alpha", "PCR");
			pcrmenu.AddGroupLabel("RyzeClear [1.2.0.1]");
			Game.OnTick += Game_OnTick;
		}
		
		static void Game_OnTick(EventArgs args)
		{
			if (Orbwalker.ActiveModesFlags.Equals(Orbwalker.ActiveModes.Combo))
			{
				DisableAA();
				CastSpells();
			}
			else
			{
				Orbwalker.DisableAttacking = false;
			}
		}
		
		static void DisableAA()
		{
			if ((TargetIn == null || TargetIn.Distance(Player.Instance) > 575) && (W.IsReady() || E.IsReady()))
			{
				Orbwalker.DisableAttacking = true;
				return;
			}
			Orbwalker.DisableAttacking = false;
		}
		
		static void CastSpells()
		{
			if (TargetIn != null)
			{
				AIHeroClient target = TargetIn;
				
				if (W.IsReady())
				{
					W.Cast(target);
				}
				
				if (E.IsReady())
				{
					E.Cast(target);
				}
				
				if (QQ.IsReady())
				{
					QQ.Cast(target);
				}
			}
			else if (TargetOut != null)
			{
				if (Q.IsReady())
				{
					Q.Cast(TargetOut);
				}
			}
		}
		
		static AIHeroClient TargetIn
		{
			get
			{
				var t = TargetSelector.GetTarget(615, DamageType.Magical);
				if (t.IsValidTarget())
				{
					return t;
				}
				return null;
			}
		}
		
		static AIHeroClient TargetOut
		{
			get
			{
				var t = TargetSelector.GetTarget(1000, DamageType.Magical);
				if (t.IsValidTarget())
				{
					return t;
				}
				return null;
			}
		}
	}
}
