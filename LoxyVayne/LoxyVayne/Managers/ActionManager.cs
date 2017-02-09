using EloBuddy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy.SDK;
using LoxyVayne;

namespace LoxyVay.Managers
{
    class ActionManager
    {
        public void EnqueueAction(ActionQueueList list, Func<bool> preCondition, Action comboAction, Func<bool> conditionToRemove)
        {
            list.Add(new ActionModel()
            {
                Time = Game.Time,
                PreConditionFunc = preCondition,
                ComboAction = comboAction,
                ConditionToRemoveFunc = conditionToRemove
            });
        }

        public bool ExecuteNextAction(ActionQueueList list)
        {
            var rnd = new Random();
            if (list.Count > 0)
            {
                if (Game.Time > list[0].Time + 2F)
                {
                    list.Remove(list[0]);
                    return true;
                }

                if (list[0].PreConditionFunc.Invoke())
                {
                    if (!MenuManager.IDDQD)
                        Core.DelayAction(list[0].ComboAction.Invoke, rnd.Next(50, 125));
                    else
                        list[0].ComboAction.Invoke();

                }

                if (list[0].ConditionToRemoveFunc.Invoke() || Game.Time > list[0].Time + 1.5F)
                {
                    list.Remove(list[0]);
                    if (list.Count > 0)
                    {
                        var nextItem = list[0];
                        nextItem.Time = Game.Time;
                        list[0] = nextItem;
                    }
                }
                return true;
            }
            return false;
        }
    }
}
