using BepInEx;
using System;
using BepInEx;
using UnityEngine;
using SlugBase.Features;
using static SlugBase.Features.FeatureTypes;
using System.Runtime.CompilerServices;
using SlugBase.DataTypes;
using RWCustom;


namespace Stormcat
{
    partial class Stormcat : BaseUnityPlugin
    {
        private void PebblesConversation_AddEvents(On.SSOracleBehavior.PebblesConversation.orig_AddEvents orig, SSOracleBehavior.PebblesConversation self)
        {
            var data = Data(self.owner.player);

            self.events.Add(new SSOracleBehavior.PebblesConversation.PauseAndWaitForStillEvent(self, self.convBehav, 10));
            self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("OwO I catboy Pebbles hewwo Stormy."), 0));
            self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("Bibye!!!! Have fun out there. ^w^"), 0));

            if (self.owner.player.slugcatStats.name != Stormchaser)
            { orig(self); return; }
        }
    }
}