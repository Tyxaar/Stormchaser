using RWCustom;
using UnityEngine;
using System.Collections.Generic;
using System.IO;


namespace Stormcat;

public static class StormyPassageHooks
{
	public static List<DataPearl.AbstractDataPearl.DataPearlType> stormyPearlsPicked = new List<DataPearl.AbstractDataPearl.DataPearlType>();

    public static void Apply()
    {
        On.WinState.CycleCompleted += BP_CycleCompleted;
        On.WinState.CreateAndAddTracker += BP_CreateAndAddTracker;
        On.WinState.PassageDisplayName += WinState_PassageDisplayName;

        //On.FSprite.ctor_string_bool += FSprite_ctor_string_bool;
        //On.FAtlasManager.GetElementWithName += FAtlasManager_GetElementWithName;

		//TO COME LATER WHEN WE HAVE PASSAGE SCREEN ART
        On.Menu.MenuScene.BuildScene += BP_BuildScene;
		On.Menu.CustomEndGameScreen.GetDataFromSleepScreen += BP_GetDataFromSleepScreen;

		On.Menu.EndgameMeter.NotchMeter.ctor += NotchMeter_ctor;
        On.Player.SlugcatGrab += Player_SlugcatGrab;
        On.GameSession.ctor += GameSession_ctor;
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
			
			Debug.Log("DATA PEARL: " + pearl.AbstractPearl.ToString());
			string pearlName = pearl.AbstractPearl.dataPearlType.ToString();

            if (pearlName == "Stormchaser_UW" || pearlName == "Stormchaser_SL" || pearlName == "Stormchaser_HI" || pearlName == "Stormchaser_SI")
			{
                Debug.Log("THAT'S THE ONE!!!");
				if (!(pearl.abstractPhysicalObject as AbstractConsumable).isConsumed) //!pearl.uniquePearlCountedAsPickedUp ||
                {
                    pearl.uniquePearlCountedAsPickedUp = true;
					stormyPearlsPicked.Add(pearl.AbstractPearl.dataPearlType);
                    pearl.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, 0f, 0.45f, 1f);
                    //this.room.game.GetStorySession.playerSessionRecords[(this.grabbedBy[0].grabber as Player).playerState.playerNumber].pearlsFound.Add(this.AbstractPearl.dataPearlType);
                }
            }
		}
    }

    private static string WinState_PassageDisplayName(On.WinState.orig_PassageDisplayName orig, WinState.EndgameID ID)
	{
		if (ID == EnumExt_MyMod.Storm)
			return "The Storm";
		else
			return orig.Invoke(ID);
	}


	private static FAtlasElement FAtlasManager_GetElementWithName(On.FAtlasManager.orig_GetElementWithName orig, FAtlasManager self, string elementName)
    {
		if (elementName == "StormyA")
			return orig.Invoke(self, "foodSymbol"); //HunterA //smallKarma3
		else if (elementName == "StormyB")
			return orig.Invoke(self, "foodSymbol"); //HunterB
		else
			return orig.Invoke(self, elementName);
	}

    

    private static void FSprite_ctor_string_bool(On.FSprite.orig_ctor_string_bool orig, FSprite self, string elementName, bool quadType)
    {
		if (elementName == "StormyA")
			orig.Invoke(self, "foodSymbol", quadType); //HunterA
		else if (elementName == "StormyB")
			orig.Invoke(self, "foodSymbol", quadType); //HunterB
		else
			orig.Invoke(self, elementName, quadType);
	}
	
	
	
	
	public static void BP_CycleCompleted(On.WinState.orig_CycleCompleted orig, WinState self, RainWorldGame game)
	{



        /*
        // ON CYCLE COMPLETED
        // WinState.IntegerTracker integerTracker = self.GetTracker(WinState.EndgameID.Survivor, true) as WinState.IntegerTracker;
        WinState.IntegerTracker integerTracker5 = self.GetTracker(EnumExt_MyMod.Storm, true) as WinState.IntegerTracker;
		if (integerTracker5 != null && !BellyPlus.VisualsOnly())// && integerTracker.GoalAlreadyFullfilled)
		{
			int gluttonProgress = BellyPlus.bonusFood;
			if (gluttonProgress > 4)
				gluttonProgress = 4 + ((gluttonProgress - 4) / 2);

			if (BPOptions.debugLogs.Value)
				Debug.Log("GLUTTON PROGRESS? " + gluttonProgress + "FILLED? " + integerTracker5.GoalAlreadyFullfilled);

			if (gluttonProgress > 0)
			{
				integerTracker5.SetProgress(integerTracker5.progress + gluttonProgress);
			}
			else if (integerTracker5 != null && !integerTracker5.GoalAlreadyFullfilled)
			{
				integerTracker5.SetProgress(integerTracker5.progress - Mathf.Max(2 - Mathf.CeilToInt(BPOptions.bpDifficulty.Value / 2), 1) );
			}
		}
		*/


        //PEARL CHECK
        /*
		List<DataPearl.AbstractDataPearl.DataPearlType> list = new List<DataPearl.AbstractDataPearl.DataPearlType>();
		for (int i = 0; i < game.GetStorySession.playerSessionRecords.Length; i++)
		{
			if (game.GetStorySession.playerSessionRecords[i] != null)
			{
               
                PlayerSessionRecord playerSessionRecord = game.GetStorySession.playerSessionRecords[i];
                Debug.Log("CHECKING FOUND PEARLS " + playerSessionRecord.pearlsFound.Count);
                for (int l = 0; l < playerSessionRecord.pearlsFound.Count; l++)
				{
					if (!list.Contains(playerSessionRecord.pearlsFound[l]))
					{
						list.Add(playerSessionRecord.pearlsFound[l]);
						Debug.Log("ADDING FOUND PEARL");
					}
				}
			}
		}
		*/

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
	
}