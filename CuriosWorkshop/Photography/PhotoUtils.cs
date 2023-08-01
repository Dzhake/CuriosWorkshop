using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RogueLibsCore;
using UnityEngine;

namespace CuriosWorkshop
{
    public static class PhotoUtils
    {
        public static GameController gc => GameController.gameController;

        public static void WithPhotoVision([InstantHandle] Action action)
        {
            static void SetInterfaceVisible(bool value)
            {
                gc.nonClickableGUI.go.SetActive(value);
                gc.mainGUI.gameObject.SetActive(value);
                gc.questMarkerList.ForEach(q => q.go.SetActive(value));
                gc.questMarkerSmallList.ForEach(q => q.gameObject.SetActive(value));
            }
            List<Agent> disabledAgents = new();
            try
            {
                PhotographyPatches.preventQuestMarkerDestruction = true;
                SetInterfaceVisible(false);

                foreach (Agent agent in gc.agentList)
                {
                    if (agent.ghost || agent.HasTrait(VanillaTraits.CameraShy))
                        disabledAgents.Add(agent);
                }
                foreach (PlayfieldObject obj in gc.agentList.Concat<PlayfieldObject>(gc.objectRealList))
                {
                    ObjectSprite? spr = obj.objectSprite;
                    if (spr is not null && (spr.player1Highlight || spr.player2Highlight || spr.player3Highlight || spr.player4Highlight))
                        spr.SetHighlight("Off");
                }

                disabledAgents.ForEach(static a => a.gameObject.SetActive(false));
                action();
            }
            finally
            {
                SetInterfaceVisible(true);
                PhotographyPatches.preventQuestMarkerDestruction = false;
                disabledAgents.ForEach(static a => a.gameObject.SetActive(true));
            }
        }

        public static PhotoFeature[] GetFeatures(Rect area)
        {
            Vector2 padding = new Vector2(0.32f, 0.32f);
            area = new Rect(area.min + padding, area.size - 2 * padding);

            List<PhotoFeature> list = new();

            foreach (Agent agent in gc.agentList.Where(a => area.Contains((Vector2)a.tr.position)))
            {
                if (agent.agentRealName?.StartsWith("E_") is not false) continue;
                list.Add(new PhotoFeature("Agent", agent.agentRealName, 5f));
            }
            foreach (ObjectReal obj in gc.objectRealList.Where(o => area.Contains((Vector2)o.tr.position)))
            {
                if (obj.objectRealRealName?.StartsWith("E_") is not false) continue;
                list.Add(new PhotoFeature("Object", obj.objectRealRealName, 5f));
            }

            return list.ToArray();
        }

    }
}
