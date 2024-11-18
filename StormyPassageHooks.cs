using RWCustom;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using SlugBase.SaveData;
using MoreSlugcats;
using System;


namespace Stormcat;

public static class StormyPassageHooks
{
	public static List<DataPearl.AbstractDataPearl.DataPearlType> stormyPearlsPicked = new List<DataPearl.AbstractDataPearl.DataPearlType>();

    public static void Apply()
    {
        On.WinState.CycleCompleted += BP_CycleCompleted;
        On.WinState.CreateAndAddTracker += BP_CreateAndAddTracker;
        On.WinState.PassageDisplayName += WinState_PassageDisplayName;

        On.Menu.MenuScene.BuildScene += BP_BuildScene;
		On.Menu.CustomEndGameScreen.GetDataFromSleepScreen += BP_GetDataFromSleepScreen;

		On.Menu.EndgameMeter.NotchMeter.ctor += NotchMeter_ctor;
        On.Player.SlugcatGrab += Player_SlugcatGrab;
        On.GameSession.ctor += GameSession_ctor;

        On.DreamsState.StaticEndOfCycleProgress += DreamsState_StaticEndOfCycleProgress;
        On.DataPearl.Update += DataPearl_Update;
    }

    //PREVENT VANILLA DREAMS FROM SHOWING UP
    private static void DreamsState_StaticEndOfCycleProgress(On.DreamsState.orig_StaticEndOfCycleProgress orig, SaveState saveState, string currentRegion, string denPosition, ref int cyclesSinceLastDream, ref int cyclesSinceLastFamilyDream, ref int cyclesSinceLastGuideDream, ref int inGWOrSHCounter, ref DreamsState.DreamID upcomingDream, ref DreamsState.DreamID eventDream, ref bool everSleptInSB, ref bool everSleptInSB_S01, ref bool guideHasShownHimselfToPlayer, ref int guideThread, ref bool guideHasShownMoonThisRound, ref int familyThread)
    {
        if (saveState.saveStateNumber == Stormcat.Stormchaser && (eventDream == null || !eventDream.value.Contains("Stormchaser")))
            return;
        orig(saveState, currentRegion, denPosition, ref cyclesSinceLastDream, ref cyclesSinceLastFamilyDream, ref cyclesSinceLastGuideDream, ref inGWOrSHCounter, ref upcomingDream, ref eventDream, ref everSleptInSB, ref everSleptInSB_S01, ref guideHasShownHimselfToPlayer, ref guideThread, ref guideHasShownMoonThisRound, ref familyThread);
    }


    public static void BP_BuildScene(On.Menu.MenuScene.orig_BuildScene orig, Menu.MenuScene self)
    {
        if (self.sceneID == EnumExt_MyScene.Endgame_Storm)
        {
            //FIRST PART ALL OF THEM GET
            if (self is Menu.InteractiveMenuScene)
            {
                (self as Menu.InteractiveMenuScene).idleDepths = new List<float>();
            }
            Vector2 vector = new Vector2(0f, 0f);
            // vector..ctor(0f, 0f);

            //NOW THE CUSTOM PART
            self.sceneFolder = "Scenes" + Path.DirectorySeparatorChar.ToString() + "Endgame - Storm";
            if (self.flatMode)
            {
                self.AddIllustration(new Menu.MenuIllustration(self.menu, self, self.sceneFolder, "Endgame - Storm - Flat", new Vector2(683f, 384f), false, true));
            }
            else
            {
                self.AddIllustration(new Menu.MenuDepthIllustration(self.menu, self, self.sceneFolder, "Storm - 7", new Vector2(71f, 49f), 2.3f, Menu.MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new Menu.MenuDepthIllustration(self.menu, self, self.sceneFolder, "Storm - 6", new Vector2(71f, 49f), 2.2f, Menu.MenuDepthIllustration.MenuShader.Basic)); //Lighten
                self.AddIllustration(new Menu.MenuDepthIllustration(self.menu, self, self.sceneFolder, "Storm - 5", new Vector2(71f, 49f), 1.5f, Menu.MenuDepthIllustration.MenuShader.Basic)); //Normal
                self.AddIllustration(new Menu.MenuDepthIllustration(self.menu, self, self.sceneFolder, "Storm - 4", new Vector2(71f, 49f), 1.7f, Menu.MenuDepthIllustration.MenuShader.Basic));
                //self.depthIllustrations[self.depthIllustrations.Count - 1].setAlpha = new float?(0.5f);
                self.AddIllustration(new Menu.MenuDepthIllustration(self.menu, self, self.sceneFolder, "Storm - 3", new Vector2(71f, 49f), 1.7f, Menu.MenuDepthIllustration.MenuShader.Basic)); //LightEdges
                self.AddIllustration(new Menu.MenuDepthIllustration(self.menu, self, self.sceneFolder, "Storm - 2", new Vector2(71f, 49f), 1.5f, Menu.MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new Menu.MenuDepthIllustration(self.menu, self, self.sceneFolder, "Storm - 1", new Vector2(171f, 49f), 1.3f, Menu.MenuDepthIllustration.MenuShader.Basic)); //LightEdges
                                                                                                                                                                                                  //(self as Menu.InteractiveMenuScene).idleDepths.Add(2.2f);
                (self as Menu.InteractiveMenuScene).idleDepths.Add(2.2f); //HMMM THIS ONE?
                (self as Menu.InteractiveMenuScene).idleDepths.Add(2.2f);
                (self as Menu.InteractiveMenuScene).idleDepths.Add(1.7f);
                (self as Menu.InteractiveMenuScene).idleDepths.Add(1.7f);
                (self as Menu.InteractiveMenuScene).idleDepths.Add(1.5f);
                (self as Menu.InteractiveMenuScene).idleDepths.Add(1.3f);
            }
            self.AddIllustration(new Menu.MenuIllustration(self.menu, self, self.sceneFolder, "Storm - Symbol", new Vector2(683f, 35f), true, false));
            Menu.MenuIllustration MenuIllustration4 = self.flatIllustrations[self.flatIllustrations.Count - 1];
            MenuIllustration4.pos.x = MenuIllustration4.pos.x - (0.01f + self.flatIllustrations[self.flatIllustrations.Count - 1].size.x / 2f);

        }
        else
            orig.Invoke(self);
    }


    private static void BP_GetDataFromSleepScreen(On.Menu.CustomEndGameScreen.orig_GetDataFromSleepScreen orig, Menu.CustomEndGameScreen self, WinState.EndgameID endGameID)
    {
        if (endGameID == EnumExt_MyMod.Storm)
        {
            //GOTTA REPLICATE THE MENU SCREEN
            Menu.MenuScene.SceneID sceneID = Menu.MenuScene.SceneID.Empty;
            sceneID = EnumExt_MyScene.Endgame_Storm;
            self.scene = new Menu.InteractiveMenuScene(self, self.pages[0], sceneID);
            self.pages[0].subObjects.Add(self.scene);
            self.pages[0].Container.AddChild(self.blackSprite);
            if (self.scene.flatIllustrations.Count > 0)
            {
                self.scene.flatIllustrations[0].RemoveSprites();
                self.scene.flatIllustrations[0].Container.AddChild(self.scene.flatIllustrations[0].sprite);
                self.glyphIllustration = self.scene.flatIllustrations[0];
                self.glyphGlowSprite = new FSprite("Futile_White", true);
                self.glyphGlowSprite.shader = self.manager.rainWorld.Shaders["FlatLight"];
                self.pages[0].Container.AddChild(self.glyphGlowSprite);
                self.localBloomSprite = new FSprite("Futile_White", true);
                self.localBloomSprite.shader = self.manager.rainWorld.Shaders["LocalBloom"];
                self.pages[0].Container.AddChild(self.localBloomSprite);
            }
            self.titleLabel = new Menu.MenuLabel(self, self.pages[0], "", new Vector2(583f, 5f), new Vector2(200f, 30f), false, null);
            self.pages[0].subObjects.Add(self.titleLabel);
            self.titleLabel.text = self.Translate(WinState.PassageDisplayName(endGameID));
        }
        else
            orig.Invoke(self, endGameID);
    }



    private static void NotchMeter_ctor(On.Menu.EndgameMeter.NotchMeter.orig_ctor orig, Menu.EndgameMeter.NotchMeter self, Menu.EndgameMeter owner)
    {
		orig(self, owner);
		//JUST COPIED AND PASTED FROM THE SCHOLAR
        if (owner.tracker.ID == EnumExt_MyMod.Storm)
        {
            self.customColors = new Color[self.listTracker.totItemsToWin];
            for (int l = 0; l < self.listTracker.myList.Count; l++)
            {
                string entry = ExtEnum<DataPearl.AbstractDataPearl.DataPearlType>.values.GetEntry(self.listTracker.myList[l]);
                Color color = DataPearl.UniquePearlMainColor(DataPearl.AbstractDataPearl.DataPearlType.Misc);
                Color? color2 = null;
                if (entry != null)
                {
                    DataPearl.AbstractDataPearl.DataPearlType pearlType = new DataPearl.AbstractDataPearl.DataPearlType(entry, false);
                    color = DataPearl.UniquePearlMainColor(pearlType);
                    color2 = DataPearl.UniquePearlHighLightColor(pearlType);
                }
                if (color2 != null)
                {
                    self.customColors[l] = Color.Lerp(color, color2.Value, 0.4f);
                }
                else
                {
                    self.customColors[l] = color;
                }
                self.customColors[l] = Custom.Saturate(self.customColors[l], 0.2f);
            }
        }
    }

    private static void GameSession_ctor(On.GameSession.orig_ctor orig, GameSession self, RainWorldGame game)
    {
		orig(self, game);
		Debug.Log("--NEW GAME SESSION--");
		stormyPearlsPicked.Clear();
    }

    private static void Player_SlugcatGrab(On.Player.orig_SlugcatGrab orig, Player self, PhysicalObject obj, int graspUsed)
    {
		orig(self, obj, graspUsed);

		if (obj is DataPearl pearl)
		{
			if (IsStormchaserPearl(pearl))
			{
				if (!(pearl.abstractPhysicalObject as AbstractConsumable).isConsumed) //!pearl.uniquePearlCountedAsPickedUp ||
                {
                    pearl.uniquePearlCountedAsPickedUp = true;
					stormyPearlsPicked.Add(pearl.AbstractPearl.dataPearlType);
                    pearl.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, 0f, 0.45f, 1f);
                    //this.room.game.GetStorySession.playerSessionRecords[(this.grabbedBy[0].grabber as Player).playerState.playerNumber].pearlsFound.Add(this.AbstractPearl.dataPearlType);

                    //QUEUE THE NEXT DREAM AND SAVE OUR CURRENT DREAM NUMBER
                    if (self.room.game.session is StoryGameSession sesh)
                    {
                        int currentDream = 0;
                        var data = sesh.saveState.miscWorldSaveData.GetSlugBaseData();
                        if (data.TryGet<int>("SSpearlsFound", out int dreamCount))
                        {
                            currentDream = dreamCount;
                        }
                        currentDream += 1;
                        //Debug.Log("CURRENT DREAM: " + currentDream);
                        data.Set<int>("SSpearlsFound", currentDream);

                        if (currentDream == 1)
                            SlugBase.Assets.CustomDreams.QueueDream(sesh, Dreams.Dream_Stormchaser_Wayfarers);
                        else if (currentDream == 2)
                            SlugBase.Assets.CustomDreams.QueueDream(sesh, Dreams.Dream_Stormchaser_Thief);
                        else if (currentDream == 3)
                            SlugBase.Assets.CustomDreams.QueueDream(sesh, Dreams.Dream_Stormchaser_Alone);
                        else if (currentDream == 4)
                            SlugBase.Assets.CustomDreams.QueueDream(sesh, Dreams.Dream_Stormchaser_Beacon);
                        //This should automatically reset if we die or don't complete the cycle so we don't need to worry
                    }
                }

                //AND THEN DESTROY THE PEARL
                self.ReleaseGrasp(graspUsed);
                self.room.PlaySound(SoundID.Snail_Pop, self.mainBodyChunk);
                for (int i = 0; i < 3; i++)
                {
                    self.room.AddObject(new WaterDrip(obj.bodyChunks[0].pos, Custom.DegToVec(UnityEngine.Random.value * 360f) * Mathf.Lerp(4f, 21f, UnityEngine.Random.value), false));
                }
                self.room.AddObject(new ShockWave(obj.bodyChunks[0].pos, 50f, 0.07f, 6, false));
                //UNTRACK PEARLS OR THEY WILL RESPAWN THE NEXT CYCLE
                if (ModManager.MMF && MMF.cfgKeyItemTracking.Value && self.room.game.session is StoryGameSession && AbstractPhysicalObject.UsesAPersistantTracker(obj.abstractPhysicalObject))
                    (self.room.game.session as StoryGameSession).RemovePersistentTracker(obj.abstractPhysicalObject);
                obj.Destroy();
            }
        }
    }


    public static bool IsStormchaserPearl(DataPearl pearl)
    {
        //Debug.Log("DATA PEARL: " + pearl.AbstractPearl.ToString());
        string pearlName = pearl.AbstractPearl.dataPearlType.ToString();

        if (pearlName == "Stormchaser_UW" || pearlName == "Stormchaser_SL" || pearlName == "Stormchaser_HI" || pearlName == "Stormchaser_SI")
            return true;
        else
            return false;
    }

    private static string WinState_PassageDisplayName(On.WinState.orig_PassageDisplayName orig, WinState.EndgameID ID)
	{
		if (ID == EnumExt_MyMod.Storm)
			return "The Storm";
		else
			return orig.Invoke(ID);
	}

	
	public static void BP_CycleCompleted(On.WinState.orig_CycleCompleted orig, WinState self, RainWorldGame game)
	{
        List<DataPearl.AbstractDataPearl.DataPearlType> list = stormyPearlsPicked;

        orig.Invoke(self, game);

        Debug.Log("TOTAL PEARLS LOGGED " + list.Count);
        if (list.Count > 0) //WE DON'T NEED SURVIVOR - && integerTracker.GoalAlreadyFullfilled
		{
            
            WinState.ListTracker listTracker2 = self.GetTracker(EnumExt_MyMod.Storm, true) as WinState.ListTracker;
			foreach (DataPearl.AbstractDataPearl.DataPearlType a in list)
			{
				int item2 = (int)a;
				listTracker2.AddItemToList(item2);
                Debug.Log("ADDING A PEARLS LOGGED ");
            }
		}
	}
	
	
	
	public static WinState.EndgameTracker BP_CreateAndAddTracker(On.WinState.orig_CreateAndAddTracker orig, WinState.EndgameID ID, List<WinState.EndgameTracker> endgameTrackers)
	{
		WinState.EndgameTracker endgameTracker = null;
		
		if (ID == EnumExt_MyMod.Storm)
		{
			endgameTracker = new WinState.ListTracker(ID, 4); //HOW MANY PEARLS? 6?
			Debug.Log("STORMY TRACKER CREATED! ");
		}

        else
            return orig.Invoke(ID, endgameTrackers); //JUST RUN THE ORIGINAL AND NOTHING ELSE BELOW IT



        //AND THEN RUN THE ORIGINAL STUFF THAT WOULD OTHERWISE BE SKIPPED
        if (endgameTracker != null && endgameTrackers != null)
		{
			bool flag = false;
			for (int j = 0; j < endgameTrackers.Count; j++)
			{
				if (endgameTrackers[j].ID == ID)
				{
					flag = true;
					endgameTrackers[j] = endgameTracker;
					break;
				}
			}
			if (!flag)
			{
				endgameTrackers.Add(endgameTracker);
			}
		}
		return endgameTracker;
	}
	
	
	
	public static class EnumExt_MyMod
	{ // You can have multiple EnumExt_ classes in your assembly if you need multiple items with the same name for the different enum
		public static WinState.EndgameID Storm = new WinState.EndgameID("Storm", true);
    }
	
	
	public static class EnumExt_MyScene
	{
		public static Menu.MenuScene.SceneID Endgame_Storm = new Menu.MenuScene.SceneID("Endgame_Storm", true);
	}


    //MAKE THE PEARLS FLOAT
    public static int hoverSinCounter = 0;
    public static int flyingCounter = 0;
    public static float flying = 0f;

    private static void DataPearl_Update(On.DataPearl.orig_Update orig, DataPearl self, bool eu)
    {
        orig(self, eu);
        if (IsStormchaserPearl(self))
        {
            self.firstChunk.vel.y += self.room.gravity * flying;
            bool flag = flyingCounter > 40;
            if (flyingCounter > 40 || flying > 0f)
                HoverBehavior(self, (flyingCounter > 40));
            else if (self.firstChunk.ContactPoint.y < 0)
            { //DON'T ASK ME HOW ALL THIS WORKS, IT'S ALL NONSENSE FROM NSH SWARMER...
                if (HoverTile(self, self.room.GetTilePosition(self.firstChunk.pos)))
                    flyingCounter++;
                else
                    flyingCounter = 0;
            }
            flying = Custom.LerpAndTick(flying, flag ? 1f : 0f, 0.02f, 0.025f);
        }
    }

    public static void HoverBehavior(DataPearl self, bool increaseFly)
    {
        if (!self.room.readyForAI)
        {
            increaseFly = false;
            return;
        }
        hoverSinCounter++;
        self.firstChunk.vel *= 1f - 0.05f * flying;
        if (!self.room.GetTile(self.firstChunk.pos + new Vector2(0f, -10f)).Solid && self.firstChunk.ContactPoint.y < 0)
        {
            self.firstChunk.vel.x = self.firstChunk.vel.x + Mathf.Sign(self.firstChunk.pos.x - self.room.MiddleOfTile(self.firstChunk.pos).x);
        }
        if (HoverTile(self, self.room.GetTilePosition(self.firstChunk.pos)))
        {
            int num = 8;
            for (int i = -1; i <= 1; i++)
            {
                int floorAltitude = self.room.aimap.getAItile(self.firstChunk.pos + new Vector2(20f * i, 0f)).floorAltitude;
                if (floorAltitude < 8 && floorAltitude > 0 && (floorAltitude > num || num == 8))
                {
                    num = floorAltitude;
                }
            }
            num = Math.Min(num, self.room.water ? (self.room.GetTilePosition(self.firstChunk.pos).y - (self.room.defaultWaterLevel + 2)) : 8);
            if (num < 8)
            {
                float num2 = self.room.MiddleOfTile(self.firstChunk.pos + new Vector2(0f, 20f * (2f - num))).y + 5f + Mathf.Sin(hoverSinCounter / 20f) * 6f;
                if (self.room.GetTile(new Vector2(self.firstChunk.pos.x, self.firstChunk.pos.y + 20f)).Solid)
                {
                    num2 = self.room.MiddleOfTile(self.firstChunk.pos).y - 4f + Mathf.Sin(hoverSinCounter / 20f) * 4f;
                }
                else if (self.room.aimap.getAItile(self.firstChunk.pos).narrowSpace)
                {
                    num2 = self.firstChunk.pos.y + 100f;
                }
                self.firstChunk.vel.y = self.firstChunk.vel.y + Custom.LerpMap(self.firstChunk.pos.y, num2 - 30f, num2 + 10f, 0.3f, -0.1f) * flying;
            }
            else
            {
                increaseFly = false;
            }
            if (increaseFly && self.room.GetTile(self.firstChunk.pos + new Vector2(-20f, 0f)).Solid != self.room.GetTile(self.firstChunk.pos + new Vector2(20f, 0f)).Solid)
            {
                self.firstChunk.vel.x = self.firstChunk.vel.x + (self.room.GetTile(self.firstChunk.pos + new Vector2(20f, 0f)).Solid ? -0.1f : 0.1f) * flying;
                return;
            }
        }
    }


    private static bool HoverTile(DataPearl self, IntVector2 tile)
    {
        if (!self.room.readyForAI)
        {
            return false;
        }
        if (self.room.aimap.getAItile(tile).floorAltitude > 8 && tile.y - self.room.defaultWaterLevel > 8)
        {
            return false;
        }
        if (self.room.aimap.getAItile(tile).narrowSpace)
        {
            for (int i = 1; i < 4; i++)
            {
                if (self.room.GetTile(tile.x, tile.y + i).Solid)
                {
                    return false;
                }
                if (!self.room.aimap.getAItile(tile.x, tile.y + i).narrowSpace)
                {
                    return true;
                }
            }
            return false;
        }
        return true;
    }
}