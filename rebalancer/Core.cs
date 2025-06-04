using Fusion;
using HarmonyLib;
using MelonLoader;
using System.Reflection;
using Toked.Weapon;
using Toked.Weapon.Throwable;
using UnityEngine.InputSystem;

[assembly: MelonInfo(typeof(rebalancer.Core), "rebalancer", "1.0.0", "Shr", null)]
[assembly: MelonGame("Toge Productions", "Whisper Mountain Outbreak")]

namespace rebalancer
{
    public class Core : MelonMod
    {
        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("Initialized.");

            // Apply every HarmonyPatch in this assembly
            HarmonyInstance.PatchAll();
            LoggerInstance.Msg("Harmony patches applied.");
        }


        // Patch class
        static readonly FieldInfo SprintSpeedField = AccessTools.Field(typeof(PlayerData), "sprintSpeed");

        [HarmonyPatch(typeof(PlayerData), nameof(PlayerData.GetSprintSpeed))]
        public static class Patch_PlayerData_GetSprintSpeed
        {
            static bool Prefix(PlayerData __instance, ref float __result)
            {
                try
                {
                    float sprintSpeed = (float)SprintSpeedField.GetValue(__instance);
                    float multiplier = __instance.playerController.PlayerMultiplyStatsData.GetMultiplyMovementSpeed();

                    __result = (sprintSpeed + 2.5f) * multiplier;
                }
                catch (Exception e)
                {
                    return true;
                }


                return false; // skip original method
            }
        }

        static FieldInfo MaxHealthField = AccessTools.Field(typeof(PlayerData), "maxHealth");

        [HarmonyPatch(typeof(PlayerData), "InitPlayer", null)]
        public static class Patch_PlayerData_InitPlayer
        {
            static void Postfix(PlayerData __instance)
            {
                try
                {
                    //float maxHealth = (float)MaxHealthField.GetValue(__instance);
                    //MaxHealthField.SetValue(__instance, maxHealth + 20f);
                    
                    __instance.AddSkillPoint(1);
                }
                catch (Exception e)
                {

                }
            }
        }

        //[HarmonyPatch(typeof(PlayerNetwork), "Awake")]
        //public static class Patch_PlayerNetwork_Awake
        //{
        //    static void Postfix(PlayerNetwork __instance)
        //    {
        //        try
        //        {
        //            __instance.SetHealth(__instance.playerController.data.GetMaxHealth());
        //        }
        //        catch (Exception e)
        //        {
        //        }
        //    }
        //}


        static readonly FieldInfo IsBtnDashDownField = AccessTools.Field(typeof(PlayerController), "isBtnDashDown");
        static readonly FieldInfo DirectionDashField = AccessTools.Field(typeof(PlayerController), "directionDash");

        [HarmonyPatch(typeof(PlayerController), nameof(PlayerController.OnInputDash))]
        public static class Patch_PlayerData_OnInputDash
        {
            static bool Prefix(PlayerController __instance, ref InputAction.CallbackContext value)
            {
                try
                {
                    if (__instance.network.GetEnableControl() && value.started && __instance.data.GetStamina() > 0f && __instance.network.GetHealth() > 0)
                    {
                        IsBtnDashDownField.SetValue(__instance, true);
                        DirectionDashField.SetValue(__instance, __instance.network.GetAngledirection());
                    }
                    if (value.canceled)
                    {
                        IsBtnDashDownField.SetValue(__instance, false);
                    }
                }
                catch (Exception e)
                {
                    return true;
                }


                return false; // skip original method
            }
        }


        [HarmonyPatch(typeof(EnemyData), nameof(EnemyData.Init))]
        public static class Patch_EnemyData_Init
        {
            static void Postfix(EnemyData __instance)
            {
                try
                {
                    __instance.damage = 10f;
                }
                catch (Exception e)
                {
                }
            }
        }

        static readonly FieldInfo WeaponDataField = AccessTools.Field(typeof(PlayerController), "_weaponData");


        [HarmonyPatch(typeof(Grenade), "CheckExplosionDamage", null)]
        public static class Patch_Grenade_CheckExplosionDamage
        {

            static bool Prefix(Grenade __instance)
            {
                try
                {
                    WeaponData weapon = (WeaponData)WeaponDataField.GetValue(__instance);
                    weapon.Damage *= 2f;
                }
                catch (Exception e)
                {
                }

                return true;
            }

            static void Postfix(Grenade __instance)
            {
                try
                {
                    WeaponData weapon = (WeaponData)WeaponDataField.GetValue(__instance);
                    weapon.Damage /= 2f;
                }
                catch (Exception e)
                {
                }
            }
        }

        //static readonly FieldInfo MaxStaminaField = AccessTools.Field(typeof(PlayerData), "maxStamina");

        //[HarmonyPatch(typeof(PlayerData), nameof(PlayerData.GetMaxStamina))]
        //public static class Patch_PlayerData_GetMaxStamina
        //{
        //    static bool Prefix(PlayerData __instance, ref float __result)
        //    {
        //        try
        //        {
        //            float maxStamina = (float)MaxStaminaField.GetValue(__instance);

        //            __result = (maxStamina + 20f);
        //        }
        //        catch (Exception e)
        //        {
        //            return true;
        //        }


        //        return false; // skip original method
        //    }
        //}

        [HarmonyPatch(typeof(WeaponController), "Start")]
        public static class Patch_WeaponController_Start
        {
            static void Postfix(WeaponController __instance)
            {
                ModifyXTimer(__instance.chargeTimer, "chargeTimer");
                ModifyXTimer(__instance.halfChargeTimer, "halfChargeTimer");
            }

            static void ModifyXTimer(XTimer timer, string name)
            {
                if (timer == null) return;

                float __factor = 0.5f;

                float originalInit = timer.initInterval;
                float original = timer.interval;

                timer.initInterval = originalInit * __factor;
                timer.interval = original * __factor;

                MelonLogger.Msg($"{name}: initInterval {originalInit} → {timer.initInterval}, interval {original} → {timer.interval}");
            }
        }

        static FieldInfo DelayStaminaRegenField = AccessTools.Field(typeof(PlayerData), "delayStaminaRegen");


        [HarmonyPatch(typeof(PlayerData), "Init")]
        public static class Patch_PlayerData_Init
        {
            static void Postfix(PlayerData __instance)
            {
                float __originalDelay = (float)DelayStaminaRegenField.GetValue(__instance);
                float __factor = 0.6f;

                DelayStaminaRegenField.SetValue(__instance, __originalDelay * __factor);
            }
        }

        [HarmonyPatch(typeof(WaveEnemyManager), "Start")]
        public static class Patch_WaveEnemyManager_Start
        {
            static void Postfix(WaveEnemyManager __instance)
            {
                ModifyXTimer(__instance.hordeTimer, "hordeTimer");
            }

            static void ModifyXTimer(XTimer timer, string name)
            {
                if (timer == null) return;

                float __delay = 60f;

                float originalInit = timer.initInterval;
                float original = timer.interval;

                timer.initInterval = originalInit + __delay;
                timer.interval = original + __delay;

                MelonLogger.Msg($"{name}: initInterval {originalInit} → {timer.initInterval}, interval {original} → {timer.interval}");
            }
        }



        [HarmonyPatch(typeof(PlayerPhotonNetwork), "OnHealthChanged", null)]
        public static class Patch_PlayerPhotonNetwork_OnHealthChanged
        {
            static void Postfix(PlayerPhotonNetwork __instance, ref Changed<PlayerPhotonNetwork> changed)
            {
                PlayerController playerController =  changed.Behaviour.playerNetwork.playerController;

                playerController.canSprint = true;
                playerController.canDash = true;
            }
        }




        //[HarmonyPatch(typeof(EnemyController), "Hurt")]
        //public static class Patch_EnemyController_Hurt
        //{
        //    static void Postfix(EnemyController __instance, float damage, float stuntTime, bool execShakingCam, byte fromPlayer, byte weaponType = 0, bool isGrenade = false, bool isHeadOff = false)
        //    {
        //        try
        //        {
        //            if (__instance.isDead)
        //            {

        //            }
        //        }
        //        catch { }

        //    }
        //}
    }
}