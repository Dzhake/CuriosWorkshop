using System.Collections;
using System.Linq;
using RogueLibsCore;
using UnityEngine;

namespace CuriosWorkshop
{
    public static class HomeBasePatches
    {
        private static RoguePatcher Patcher => CuriosPlugin.Patcher;
        public static GameController gc => GameController.gameController;

        public static ExtraUnlock ImpostorEndingUnlock = null!;

        [RLSetup]
        public static void Setup()
        {
            ImpostorEndingUnlock = (ExtraUnlock)RogueLibs.CreateCustomUnlock(new ExtraUnlock("ImpostorEnding")).Unlock;

            RogueLibs.CreateCustomName("ImpostorEnding_Achievement_1", NameTypes.Interface, new CustomNameInfo
            {
                English = "You destroyed",
                Russian = @"Вы уничтожили",
            });
            RogueLibs.CreateCustomName("ImpostorEnding_Achievement_2", NameTypes.Interface, new CustomNameInfo
            {
                English = "The Resistance!",
                Russian = @"Сопротивление!",
            });

            RogueLibs.CreateCustomName("BeginImpostorScenario", NameTypes.Interface, new CustomNameInfo
            {
                English = "",
                Russian = @"",
            });

            RogueLibs.CreateCustomName("ImpostorEnding_Mayor_1", NameTypes.Dialogue, new CustomNameInfo
            {
                English = "Wo-a-ah! You actually did it!!! I can't believe someone like you managed to destroy the entire Resistance!",
                Russian = @"Во-о-оу! Ты реально это сделал!!! Я не могу поверить, что кто-то как ты смог уничтожить всё Сопротивление!",
            });
            RogueLibs.CreateCustomName("ImpostorEnding_Mayor_2", NameTypes.Dialogue, new CustomNameInfo
            {
                English = "These mind-controlling drugs really ARE useful!",
                Russian = @"Эта наркота для контроля разума реально полезная!",
            });
            RogueLibs.CreateCustomName("ImpostorEnding_Mayor_3", NameTypes.Dialogue, new CustomNameInfo
            {
                English = "Oh, and we put a nuclear bomb into you as well. Just to be safe.",
                Russian = @"Ах да, мы ещё засунули в тебя ядерную бомбу. Чтоб уж наверняка.",
            });
            RogueLibs.CreateCustomName("ImpostorEnding_Mayor_4", NameTypes.Dialogue, new CustomNameInfo
            {
                English = "So, uh... Good luck being disintegrated. I'm off.",
                Russian = @"Ну, ээ... Удачи разрываться на кусочки. Я побежал.",
            });

            RogueLibs.CreateCustomName("ImpostorEnding_Epilogue_1", NameTypes.Dialogue, new CustomNameInfo
            {
                English = "Huh... It seems that the Mayor finally got what he wanted, and now he will be the sole ruler of this land forever, with no one to oppose him...",
                Russian = @"Хм... Похоже Мэр всё-таки сделал что хотел, и теперь он будет единственным правителем этих земель навсегда, и никто его не остановит...",
            });
            RogueLibs.CreateCustomName("ImpostorEnding_Epilogue_2", NameTypes.Dialogue, new CustomNameInfo
            {
                English = "... Just kidding. A new Resistance Base was set up a couple of days later and immediately started planning on taking down the Mayor.",
                Russian = @"... Шучу. Новая База Сопротивления была возведена пару дней спустя и они немедля начали строить планы по свержению Мэра.",
            });
            RogueLibs.CreateCustomName("ImpostorEnding_Epilogue_3", NameTypes.Dialogue, new CustomNameInfo
            {
                English = "Even after all that, nothing's changed. \"The circle of life\" can never be broken... At least, not in this way.",
                Russian = @"И даже после всего этого, ничего не изменилось. ""Круг жизни"" не сломать... По крайней мере, не таким образом.",
            });

            RogueInteractions.CreateProvider<Agent>(static h =>
            {
                if (h.gc.levelType == "HomeBase" && h.Object.agentName == VanillaAgents.Werewolf)
                {
                    h.AddButton("BeginImpostorScenario", static m =>
                    {
                        gc.StartCoroutine(ImpostorScenario());
                        m.StopInteraction();
                    });
                }
            });

        }

        public static void Apply()
        {
            Patcher.TypeWithPatches = typeof(HomeBasePatches);

            Patcher.Postfix(typeof(LoadLevel), nameof(LoadLevel.HomeBaseAgentSpawns));

            Patcher.Prefix(typeof(MainGUI), nameof(MainGUI.ShowStatsScreen), nameof(DisableDuringScenario));
            Patcher.Prefix(typeof(ObjectReal), nameof(ObjectReal.SwitchLinkOperate), nameof(DisableDuringScenario));

            Patcher.Prefix(typeof(Agent), nameof(Agent.CanTeleport));




        }

        public static Agent mayor = null!;
        public const string HomeBaseScenario = "HomeBaseScenario";

        public static void LoadLevel_HomeBaseAgentSpawns()
        {
            if (ImpostorEndingUnlock.IsUnlocked)
            {
                mayor = gc.spawnerMain.SpawnAgent(new Vector3(60, 95) * 0.64f, gc.playerAgent, "Mayor");
                InvItem hat = new InvItem { invItemName = "MayorHat", invItemCount = 100 };
                hat.ItemSetup(false);
                mayor.inventory.justPlayedPickup = true;
                hat = mayor.inventory.AddItem(hat);
                mayor.inventory.EquipArmorHead(hat, false);

                mayor.SetDefaultGoal("Guard");
                mayor.startingAngle = 270;
            }
        }

        public static bool DisableDuringScenario()
            => gc.levelType != HomeBaseScenario;

        public static bool Agent_CanTeleport(ref string __result)
        {
            if (gc.levelType == HomeBaseScenario)
            {
                __result = "CantTeleportIndoors";
                return false;
            }
            return true;
        }

        public static IEnumerator ImpostorScenario()
        {
            gc.levelType = HomeBaseScenario;
            gc.challenges.Clear();

            // ##### Show and hide GUI

            gc.nonClickableGUI.ShowGUI();
            gc.nonClickableGUI.GetComponent<CanvasGroup>().alpha = 1f;
            if (gc.gameEventsStarted)
                gc.mainGUI.toolbar.GetComponent<CanvasGroup>().alpha = 1f;

            gc.worldSpaceGUI.questMarkerTextNest.SetActive(false);

            // ##### Set up combatants

            foreach (Agent agent in gc.agentList)
            {
                agent.relationships.RevertAllVars();
                agent.relationships.SetupRelationshipList();
            }
            foreach (Agent agent in gc.agentList)
            {
                agent.wontFlee = true;
                if (agent.isPlayer == 0)
                {
                    // TODO: increase heath and healthMax
                }
                foreach (Agent other in gc.agentList)
                {
                    if (agent == other) continue;
                    agent.relationships.SetRel(other, other.isPlayer > 0 ? "Hateful" : "Aligned");
                    agent.relationships.SetRelHate(other, other.isPlayer > 0 ? 5 : 0);
                }
            }

            // ##### Make the Mayor disappear

            foreach (Agent m in gc.agentList.Where(static a => a.agentName == "Mayor"))
                m.statusEffects.Disappear(true);

            // ##### Set the mood

            gc.audioHandler.MusicStop();
            gc.musicPlayer.clip = gc.sessionDataBig.musicTrackDic["Track_Hype_v4"];
            gc.audioHandler.MusicPlay();

            // ##### Main loop
            bool succeeded = true;

            while (true)
            {
                if (gc.levelType != HomeBaseScenario || gc.playerAgent.dead)
                {
                    // The scenario ended or the player died - Failed!
                    succeeded = false;
                    break;
                }
                if (gc.agentList.Exists(static a => a.isPlayer == 0 && !a.mechEmpty && !a.dead && !a.disappeared))
                {
                    // Everyone else is dead - Success!
                    break;
                }
                // Make sure that everyone's attracted to the player
                gc.spawnerMain.SpawnNoise(gc.playerAgent.curPosition, 4f, gc.playerAgent, "Attract").noiseDistance = 9999f;
                yield return new WaitForSeconds(0.2f);
            }

            if (!succeeded)
            {
                gc.worldSpaceGUI.questMarkerTextNest.SetActive(true);
                yield break;
            }

            // ##### Fade out the music

            float curVolume = gc.musicPlayer.volume;
            while (curVolume > 0f)
            {
                curVolume -= 0.5f * Time.deltaTime;
                gc.musicPlayer.volume = curVolume;
                yield return null;
            }
            gc.audioHandler.MusicStop();

            yield return ImpostorEnding();
        }

        public static IEnumerator ImpostorEnding()
        {
            yield break;
        }


    }
}
