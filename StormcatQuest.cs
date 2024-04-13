using BepInEx;
using UnityEngine;


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


        private void MoonConversation_AddEvents(On.SLOracleBehaviorHasMark.MoonConversation.orig_AddEvents orig, SLOracleBehaviorHasMark.MoonConversation self)
        {
            Debug.Log(self.id.ToString() + " " + self.myBehavior.oracle.room.game.StoryCharacter.value);
            if (self.id == Conversation.ID.MoonFirstPostMarkConversation && self.myBehavior.oracle.room.game.StoryCharacter.value == "Stormchaser")
            {
                
                self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("Hello <PlayerName>."), 0));
                self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("My memory still fails me, but I recognize your species even after a while; <LINE>However none came with adaptations as impressive as yours, it looks like you can glide!"), 0));
                self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("You can understand me; who gave you that mark? I doubt it could be Five Pebbles, storms this violent and frequent are an anomaly in this region, even during the usual monsoons, so I'd wager his systems are failing due to his sickness... I'm not sure how many of us are still around after all."), 0));
                self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("I do wonder who did you meet before coming here though, it wouldn't surprise me if you and your family could glide to and from the remnants of the structures poking above the clouds. Have you seen others from my local group? I no longer am able to know if they're still around or if they too are succumbing to time or their experiments, but perhaps you're the proof I needed to know some may still be here. "), 0));
                self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("Regardless... I'm afraid you will need to at least return to the skies; this facility is becoming more hazardous to survive in by the day, and it might be difficult even for a hardy little creature like you to live in here."), 0));
                self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("I do appreciate the company a lot though, it's not often that someone willing to listen finds me."), 0));
                // self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("Or did I say that already?"), 5));
            }
            else
                orig(self);
        }
		
		
    }
}