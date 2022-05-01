using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace TownOfUs.Patches
{
    [HarmonyPatch(typeof(Constants), nameof(Constants.ShouldHorseAround))]
    class HorseModePatch
    {
        public static bool Prefix(ref bool __result)
        {
            __result = CustomGameOptions.Horse;
            return false;
        }
    }

    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
    class HorseHudPatch
    {
        public static void Prefix(PlayerPhysics __instance)
        {
            if ((CustomGameOptions.Horse && !PlayerControl.LocalPlayer.gameObject.transform.Find("HorseParent/Horse").GetComponent<SpriteRenderer>().enabled) || (!CustomGameOptions.Horse && PlayerControl.LocalPlayer.gameObject.transform.Find("HorseParent/Horse").GetComponent<SpriteRenderer>().enabled))
            {
                Utils.EndGame();
            }
        }
    }

}
