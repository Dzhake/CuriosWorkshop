using System;
using System.Collections.Generic;
using System.Linq;
using RogueLibsCore;
using UnityEngine;

namespace CuriosWorkshop
{
    [ItemCategories(RogueCategories.Usable, RogueCategories.Technology, RogueCategories.Weird, RogueCategories.Weapons, RogueCategories.NonStandardWeapons2)]
    public class SoulStealer : BasicCamera
    {
        [RLSetup]
        public static void Setup()
        {
            RogueLibs.CreateCustomItem<SoulStealer>()
                     .WithName(new CustomNameInfo
                     {
                         English = "[CW] Soul Stealer",
                         Russian = @"[CW] Похититель душ",
                     })
                     .WithDescription(new CustomNameInfo
                     {
                         English = "A camera that steals souls! All NPCs with souls in the photographed area will turn into mindless angry walking corpses.",
                     })
                     .WithSprite(Properties.Resources.SoulStealer)
                     .WithUnlock(new ItemUnlock
                     {
                         UnlockCost = 10,
                         CharacterCreationCost = 3,
                         LoadoutCost = 3,
                         Prerequisites = { nameof(PhotoCamera), VanillaItems.BooUrn },
                     });

            CuriosPlugin.CreateOctoSprite("BlankFaceHead", SpriteScope.Agents, Properties.Resources.BlankFaceHead, 64f);
        }

        public override void SetupDetails()
        {
            Item.itemType = ItemTypes.Tool;
            Item.initCount = 1;
            Item.rewardCount = 2;
            Item.itemValue = 80;
            Item.stackable = true;
            Item.hasCharges = true;
            Item.goesInToolbar = true;
        }

        public override CameraOverlayType Type => CameraOverlayType.SoulStealer;

        private static Func<Agent, bool> GetTargetPredicate(Rect area)
        {
            return a =>
            {
                if (a.dead || a.disappeared) return false;
                if (a.inhuman || a.electronic || a.zombified) return false;
                if (a.mechEmpty || a.mechFilled) return false;
                return a.isActiveAndEnabled && area.Contains((Vector2)a.tr.position);
            };
        }

        public override bool OnOverlay(Rect area, Vector2Int size)
        {
            CameraOverlay overlay = Owner!.mainGUI.Get<CameraOverlay>();
            overlay.Set(Type, area, size);

            Vector2 padding = new Vector2(0.32f, 0.32f);
            area = new Rect(area.min + padding, area.size - 2 * padding);

            Func<Agent, bool> predicate = GetTargetPredicate(area);
            return gc.agentList.Any(predicate);
        }
        public override bool TakePhoto(Rect area, Vector2Int size)
        {
            gc.audioHandler.Play(Owner, "TakePhoto");
            CameraOverlay overlay = Owner!.mainGUI.Get<CameraOverlay>();

            Texture2D screenshot = overlay.Capture(takeScreenshot =>
            {
                Func<Agent, bool> predicate = GetTargetPredicate(area);
                List<Agent> targets = gc.agentList.FindAll(a => predicate(a));

                foreach (Agent agent in targets)
                {
                    AgentHitbox? ahb = agent.agentHitboxScript;
                    if (ahb is null) continue;
                    ahb.eyes?.SetSprite("Clear");
                    ahb.eyesH?.SetSprite("Clear");
                    ahb.eyesWB?.SetSprite("Clear");
                    ahb.eyesWBH?.SetSprite("Clear");
                    ahb.head?.SetSprite("BlankFaceHead" + agent.playerDir);
                    ahb.headH?.SetSprite("BlankFaceHead" + agent.playerDir);
                    ahb.headWB?.SetSprite("BlankFaceHead" + agent.playerDir);
                    ahb.headWBH?.SetSprite("BlankFaceHead" + agent.playerDir);
                }

                takeScreenshot();

                foreach (Agent agent in targets)
                {
                    agent.agentHitboxScript?.MustRefresh();
                    gc.spawnerMain.TransformAgent(agent, "Zombie");
                }
            });

            Photo photo = Inventory!.AddItem<Photo>(1)!;
            photo.genTexture = screenshot;
            photo.capturedFeatures = PhotoUtils.GetFeatures(area);

            return true;
        }

    }
}
