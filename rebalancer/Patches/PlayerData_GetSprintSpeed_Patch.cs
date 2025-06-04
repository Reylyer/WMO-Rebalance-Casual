using HarmonyLib;
using UnityEngine;  // the namespace where the real game’s PlayerController lives

namespace rebalancer.Patches
{
    // Tell Harmony: “We want to patch PlayerController.TakeDamage(int)”
    [HarmonyPatch(typeof(PlayerData), "GetSprintSpeed")]
    public static class PlayerController_TakeDamage_Patch
    {

    }
}
