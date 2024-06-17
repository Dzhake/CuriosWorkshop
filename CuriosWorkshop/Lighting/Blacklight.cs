using System.Collections;
using RogueLibsCore;
using UnityEngine;

namespace CuriosWorkshop
{
    [ItemCategories(RogueCategories.Technology, RogueCategories.NonViolent, RogueCategories.NonStandardWeapons2, RogueCategories.NotRealWeapons, RogueCategories.Weird)]
    public class Blacklight : CustomItem, IFlashlight
    {
        [RLSetup]
        public static void Setup()
        {
            RogueLibs.CreateCustomItem<Blacklight>()
                     .WithName(new CustomNameInfo
                     {
                         English = "[CW] UV Flashlight",
                         Russian = @"[CW] Ультрафиолетовый фонарик",
                     })
                     .WithDescription(new CustomNameInfo
                     {
                         English = "Works like a Flashlight, but also reveals footprints and traps. Burns Vampires. Can also be used to open Safes through a mini-game (there will be a 9-key keypad with fingerprints). 100% charge = 2 minutes.",
                     })
                     .WithSprite(Properties.Resources.Blacklight)
                     .WithUnlock(new ItemUnlock
                     {
                         UnlockCost = 5,
                         CharacterCreationCost = 2,
                         LoadoutCost = 2,
                     });
        }

        public override void SetupDetails()
        {
            Item.itemType = ItemTypes.WeaponProjectile;
            Item.weaponCode = weaponType.WeaponProjectile;
            Item.initCount = 60 * 100;
            Item.rewardCount = 60 * 100;
            Item.itemValue = 50;
            Item.stackable = true;
            Item.hasCharges = true;

            Item.isWeapon = true;
            Item.rapidFire = true;
            Item.dontSelectNPC = true;
            Item.dontAutomaticallySelect = true;
            Item.doesNoDamage = true;
            Item.gunKnockback = 0;
        }
        public override CustomTooltip GetCountString()
            => $"{Mathf.CeilToInt(100f * Count / Item.initCount)}%";

        private float lastDamageTick;

        public void TurnOn(Gun gun)
        {
            Owner!.weaponCooldown = 0.01f;
            gun.SubtractBullets(1, Item);
            AimLight(gun);
        }
        public void AimLight(Gun gun)
        {
            MoveableLightSource lightSource = MoveableLightSource.Get(gun);
            lightSource.gameObject.layer = LightingPatches.LightSourceLayer;
            lightSource.TurnOn(() => gc.audioHandler.Play(Owner!, "FlashlightOn"));
            lightSource.UpdateLight(new Color32(130, 0, 255, 255), 4f);

            float delta = Time.time - lastDamageTick;
            bool isDamageTick = delta >= 0.25f;
            if (!isDamageTick) return;
            lastDamageTick = Time.time;

            foreach (Agent agent in gc.agentList)
            {
                if (agent == Owner || agent.dead) continue;
                if (!LightingPatches.DeadlyUltraViolet && agent.specialAbility != VanillaAbilities.Bite && !agent.zombified) continue;
                if (Vector2.Distance(agent.tr.position, gun.tr.position) > 5 * 0.64f) continue;
                if (!agent.movement.HasLOSPosition(gun.tr.position, "360")) continue;

                Vector2 toAgent = agent.tr.position - gun.tr.position;
                float angle = Vector2.Angle(toAgent, gun.tr.right);
                if (Mathf.Abs(angle) > 30f) continue;

                agent.StartCoroutine(DoBurnDamage());
                IEnumerator DoBurnDamage()
                {
                    GameObject pfx = gc.spawnerMain.SpawnParticleEffect("Flamethrower", agent.tr.position, 0f, agent.tr);
                    pfx.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                    for (int i = 0; i < 8 && !agent.dead; i++)
                    {
                        if (i == 4)
                        {
                            Object.Destroy(pfx);
                            pfx = gc.spawnerMain.SpawnParticleEffect("Smoke", agent.tr.position, 0f, agent.tr);
                            pfx.transform.localScale = new Vector3(2f, 2f, 2f);
                        }
                        if (i % 2 != 0) continue;
                        gc.audioHandler.Play(agent, "FireHit");

                        agent.deathMethodItem = Item.invItemName;
                        agent.deathMethod = "Flamethrower";
                        agent.deathMethodObject = Item.invItemName;
                        agent.deathKiller = Owner!.agentName;
                        agent.justHitByAgent = Owner;
                        agent.lastHitByAgent = Owner;

                        agent.ChangeHealth(i < 4 ? -2f : -1f, Owner);
                        agent.relationships.SetRel(agent.justHitByAgent, "Hateful");
                        agent.relationships.SetRelHate(agent.justHitByAgent, 5);
                        yield return new WaitForSeconds(1f);
                    }
                    Object.Destroy(pfx);
                }
            }

        }
        public void TurnOff(Gun gun)
        {
            MoveableLightSource lightSource = MoveableLightSource.Get(gun);
            lightSource.gameObject.layer = LightingPatches.LightSourceLayer;
            lightSource.TurnOff(() => gc.audioHandler.Play(Owner!, "FlashlightOff"));
        }

    }
}
