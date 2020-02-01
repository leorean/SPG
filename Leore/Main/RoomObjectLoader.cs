using Leore.Objects.Enemies;
using Leore.Objects.Items;
using Leore.Objects.Level;
using SPG.Objects;
using SPG.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SPG.Map;
using Leore.Objects.Effects.Emitters;
using Microsoft.Xna.Framework;
using static Leore.Objects.Items.StatUpItem;
using Leore.Objects;
using Leore.Objects.Level.Blocks;
using Leore.Objects.Level.Switches;
using Leore.Objects.Effects.Weather;
using Leore.Objects.Level.Obstacles;
using Leore.Resources;
using Leore.Objects.Effects.Ambience;
using Leore.Objects.Obstacles;
using static Leore.Objects.Effects.Transition;
using SPG.Exceptions;

namespace Leore.Main
{
    public static class RoomObjectLoader
    {

        /// <summary>
        /// loads objects from a given room. 
        /// </summary>
        public static void CreateRoomObjects(Room room)
        {
            if (room == null || GameManager.Current.LoadedRooms.Contains(room))
                return;

            var x = MathUtil.Div(room.X, RoomCamera.Current.ViewWidth) * 16;
            var y = MathUtil.Div(room.Y, RoomCamera.Current.ViewHeight) * 9;
            var w = MathUtil.Div(room.BoundingBox.Width, RoomCamera.Current.ViewWidth) * 16;
            var h = MathUtil.Div(room.BoundingBox.Height, RoomCamera.Current.ViewHeight) * 9;

            // load objects from FG tile data
            {
                var index = GameManager.Current.Map.LayerDepth.ToList().IndexOf(GameManager.Current.Map.LayerDepth.First(o => o.Key.ToLower() == "fg"));
                var data = GameManager.Current.Map.LayerData.ElementAt(index);

                x = (int)((float)x).Clamp(0, data.Width);
                y = (int)((float)y).Clamp(0, data.Height);
                
                for (int i = x; i < x + w; i++)
                {
                    for (int j = y; j < y + h; j++)
                    {
                        var t = data.Get(i, j);

                        if (t == null || t.ID == -1)
                            continue;

                        t.TileOptions.Visible = false;
                        t.TileOptions.Solid = false;

                        switch (t.ID)
                        {
                            case 0: // platforms
                            case 12:
                            case 34:
                            case 304:
                            case 535:
                            case 612:
                            case 646:
                            case 789:
                            case 794:
                            case 1041:
                            case 1219:
                                var platform = new Platform(i * Globals.T, j * Globals.T, room);
                                t.TileOptions.Visible = true;
                                if (t.ID == 646) t.TileOptions.Visible = false; // <- invisible platform
                                break;
                            case 387: // mushrooms
                                var mushroom = new Mushroom(i * Globals.T, j * Globals.T, room)
                                {
                                    Texture = GameManager.Current.Map.TileSet[t.ID]
                                };
                                break;
                            case 367: // lava
                                new Lava(i * Globals.T, j * Globals.T, room);
                                t.TileOptions.Solid = true;
                                t.TileOptions.Visible = true;
                                break;
                            case 576: // save-statues
                                new SaveStatue(i * Globals.T, j * Globals.T, room);
                                break;
                            case 512: // spikes (bottom)
                            case 1105:
                                new SpikeBottom(i * Globals.T, j * Globals.T, room);
                                t.TileOptions.Visible = true;
                                break;
                            case 513: // spikes (top)
                            case 977:
                                new SpikeTop(i * Globals.T, j * Globals.T, room);
                                t.TileOptions.Visible = true;
                                break;
                            case 514: // spikes (right)
                            case 1040:
                                new SpikeRight(i * Globals.T, j * Globals.T, room);
                                t.TileOptions.Visible = true;
                                break;
                            case 515: // spikes (left)
                            case 1042:
                                new SpikeLeft(i * Globals.T, j * Globals.T, room);
                                t.TileOptions.Visible = true;
                                break;
                            case 966:
                                new HiddenPlatform(i * Globals.T, j * Globals.T, room) { Texture = GameManager.Current.Map.TileSet[t.ID] };
                                break;
                            case 967:
                                new RollBouncer(i * Globals.T, j * Globals.T, room) { Texture = GameManager.Current.Map.TileSet[t.ID], Direction = Direction.LEFT };
                                break;
                            case 968:
                                new RollBouncer(i * Globals.T, j * Globals.T, room) { Texture = GameManager.Current.Map.TileSet[t.ID], Direction = Direction.RIGHT };
                                break;
                            case 969:
                                new RollBouncer(i * Globals.T, j * Globals.T, room) { Texture = GameManager.Current.Map.TileSet[t.ID], Direction = Direction.LEFT, VerticalDirection = Direction.DOWN };
                                break;
                            case 970:
                                new RollBouncer(i * Globals.T, j * Globals.T, room) { Texture = GameManager.Current.Map.TileSet[t.ID], Direction = Direction.RIGHT, VerticalDirection = Direction.DOWN };
                                break;
                            // spikes (corner, inside)
                            case 976:
                            case 978:
                            case 1104:
                            case 1106:
                                new SpikeCorner(i * Globals.T, j * Globals.T, room);
                                t.TileOptions.Visible = true;
                                break;
                            case 844: // trap spike (default)
                                new TrapSpike(i * Globals.T, j * Globals.T, room, false, 120) { Texture = GameManager.Current.Map.TileSet[844] };
                                break;
                            case 845: // trap spike (inverted)
                                new TrapSpike(i * Globals.T, j * Globals.T, room, true, 120) { Texture = GameManager.Current.Map.TileSet[844] };
                                break;
                            case 846: // trap spike (speed ++)
                                new TrapSpike(i * Globals.T, j * Globals.T, room, false, 60) { Texture = GameManager.Current.Map.TileSet[844] };
                                break;
                            // non-blocking fg tiles
                            case 1099:
                            case 1100:
                            case 1163:
                            case 1164:
                            case 1102:
                            case 1103:
                            case 1166:
                            case 1167:
                            case 1168:
                            case 1230:
                            case 1231:
                            case 1232:
                                t.TileOptions.Visible = true;
                                break;
                            case 577: // BIG spikes (deadly)
                                var bigSpike = new BigSpike(i * Globals.T, j * Globals.T, room);
                                t.TileOptions.Visible = true;
                                break;
                            case 578:
                            case 641:
                            case 642:
                                t.TileOptions.Visible = true;
                                break;
                            case 599: // chimney smoke 
                                new Smoke(i * Globals.T + 8, j * Globals.T + 8, room);
                                break;
                            case 640: // push-blocks
                                var pushBlock = new PushBlock(i * Globals.T, j * Globals.T, room);
                                pushBlock.Texture = GameManager.Current.Map.TileSet[t.ID];
                                break;
                            case 643: // switches (ground)
                                new GroundSwitch(i * Globals.T, j * Globals.T, false, room) { Texture = GameManager.Current.Map.TileSet[t.ID] };
                                break;
                            case 644: // switches (ground) - activate once
                                new GroundSwitch(i * Globals.T, j * Globals.T, true, room) { Texture = GameManager.Current.Map.TileSet[t.ID] };
                                break;
                            case 648: // waterfall (bright)
                            case 649: // waterfall (cave)
                            case 650: // waterfall (mountain)
                                new WaterFall(i * Globals.T, j * Globals.T, room, (t.ID - 648));
                                break;
                            case 579: // hp potion
                                new Potion(i * Globals.T + 8, j * Globals.T + 8, room, PotionType.HP) { Texture = GameManager.Current.Map.TileSet[t.ID] };
                                break;
                            case 580: // mp potion
                                new Potion(i * Globals.T + 8, j * Globals.T + 8, room, PotionType.MP) { Texture = GameManager.Current.Map.TileSet[t.ID] };
                                break;
                            case 581: // key
                                var key = new Key(i * Globals.T + 8, j * Globals.T + 8, room);
                                break;
                            case 582: // keyblock
                                new KeyBlock(i * Globals.T, j * Globals.T, room) { Texture = GameManager.Current.Map.TileSet[t.ID] };
                                break;
                            case 704:
                            case 705:
                            case 706:
                            case 707:
                            case 708:
                            case 709:
                            case 710:
                                var coin = new Coin(i * Globals.T + 8, j * Globals.T + 8, room, (t.ID - 704).TileIDToCoinValue());
                                break;
                            case 711: // max HP
                                new StatUpItem(i * Globals.T + 8, j * Globals.T + 8, room, StatType.HP);
                                break;
                            case 712: // max MP
                                new StatUpItem(i * Globals.T + 8, j * Globals.T + 8, room, StatType.MP);
                                break;
                            case 713: // MP Regen +
                                new StatUpItem(i * Globals.T + 8, j * Globals.T + 8, room, StatType.Regen);
                                break;
                            case 714: // destroy blocks
                            case 715:
                                new DestroyBlock(i * Globals.T, j * Globals.T, room, t.ID - 714 + 1);
                                break;
                            case 971:
                                new RollDestroyBlock(i * Globals.T, j * Globals.T, room) { Texture = GameManager.Current.Map.TileSet[t.ID] };
                                break;
                            case 964: // ice block
                                new IceBlock(i * Globals.T, j * Globals.T, room) { Texture = GameManager.Current.Map.TileSet[t.ID] };
                                break;
                            case 965: // fire block
                                new FireBlock(i * Globals.T, j * Globals.T, room) { Texture = GameManager.Current.Map.TileSet[t.ID] };
                                break;
                            case 716: // enemy block
                                new EnemyBlock(i * Globals.T, j * Globals.T, room);
                                break;
                            case 717: // orb block
                                new OrbBlock(i * Globals.T, j * Globals.T, room);
                                break;
                            case 719: // switch block (default: on)
                                new SwitchBlock(i * Globals.T, j * Globals.T, room, 1);
                                break;
                            case 720: // switch block (default: off)
                                new SwitchBlock(i * Globals.T, j * Globals.T, room, 0);
                                break;
                            case 721: // pots
                                var pot = new Pot(i * Globals.T, j * Globals.T, room);
                                pot.Texture = GameManager.Current.Map.TileSet[t.ID];
                                break;
                            case 722: // bushes
                                var bush = new Bush(i * Globals.T, j * Globals.T, room);
                                bush.Texture = GameManager.Current.Map.TileSet[t.ID];
                                break;
                            case 723:
                                new AirBubbleSpawner(i * Globals.T, j * Globals.T, room) { Texture = GameManager.Current.Map.TileSet[t.ID] };
                                break;
                            case 724: // door disabler (switch)
                                new DoorDisabler(i * Globals.T, j * Globals.T, room, DoorDisabler.TriggerType.Switch) { Texture = GameManager.Current.Map.TileSet[t.ID] };
                                break;
                            case 725: // door disabler (enemy)
                                new DoorDisabler(i * Globals.T, j * Globals.T, room, DoorDisabler.TriggerType.Enemy) { Texture = GameManager.Current.Map.TileSet[t.ID] };
                                break;
                            case 726: // door disabler (key)
                                new DoorDisabler(i * Globals.T, j * Globals.T, room, DoorDisabler.TriggerType.Key) { Texture = GameManager.Current.Map.TileSet[t.ID] };
                                break;
                            case 727: // door disabler (switch - reversed)
                                new DoorDisabler(i * Globals.T, j * Globals.T, room, DoorDisabler.TriggerType.SwitchReversed) { Texture = GameManager.Current.Map.TileSet[t.ID] };
                                break;
                            case 768: // enemy Bat
                                new EnemyBat(i * Globals.T + 8, j * Globals.T + 8, room);
                                break;
                            case 769: // enemy Grassy
                                new EnemyGrassy(i * Globals.T + 8, j * Globals.T + 8, room);
                                break;
                            case 770: // enemy Voidling (without shield)
                                new EnemyVoidling(i * Globals.T + 8, j * Globals.T + 8, room, 0);
                                break;
                            case 771: // enemy Voidling (with shield)
                                new EnemyVoidling(i * Globals.T + 8, j * Globals.T + 8, room, 1);
                                break;
                            case 773: // lava slime
                                {
                                    var slime = new EnemySlime(i * Globals.T + 8, j * Globals.T + 16, room, 0);
                                    slime.OverrideHP(4);
                                    slime.MergeTimer = 0;
                                }
                                break;
                            case 774: // ice slime
                                {
                                    var slime = new EnemySlime(i * Globals.T + 8, j * Globals.T + 16, room, 1);
                                    slime.OverrideHP(2);
                                    slime.MergeTimer = 0;
                                }
                                break;
                            case 775: // dark slime
                                {
                                    var slime = new EnemySlime(i * Globals.T + 8, j * Globals.T + 16, room, 2);
                                    slime.OverrideHP(4);
                                    slime.MergeTimer = 0;
                                }
                                break;
                            case 776:
                                new EnemySlurp(i * Globals.T + 8, j * Globals.T + 8, room);
                                break;
                            case 832: // teleporters
                                new Teleporter(i * Globals.T + 8, j * Globals.T + 8, room);
                                break;
                            case 833: // toggle switch (on)
                                new ToggleSwitch(i * Globals.T + 8, j * Globals.T + 8, room, true);
                                break;
                            case 834: // toggle switch (off)
                                new ToggleSwitch(i * Globals.T + 8, j * Globals.T + 8, room, false);
                                break;
                            case 835:
                                new FallingPlatform(i * Globals.T, j * Globals.T, room) { Texture = GameManager.Current.Map.TileSet[t.ID] };
                                break;
                            // torches
                            case 836:
                                new Torch(i * Globals.T, j * Globals.T, room, false, LightSource.LightState.Bright, false);
                                break;
                            case 837:
                                new Torch(i * Globals.T, j * Globals.T, room, true, LightSource.LightState.Default, false);
                                break;
                            case 838:
                                new Torch(i * Globals.T, j * Globals.T, room, false, LightSource.LightState.Bright, false);
                                break;
                            case 839:
                                new Torch(i * Globals.T, j * Globals.T, room, true, LightSource.LightState.Bright, false);
                                break;
                            case 840:
                                new Torch(i * Globals.T, j * Globals.T, room, false, LightSource.LightState.Default, true);
                                break;
                            case 841:
                                new AmbientLightSource(i * Globals.T, j * Globals.T, room);
                                break;
                            case 842:
                                new LightObject(i * Globals.T, j * Globals.T, room);
                                break;
                            case 843:
                                new FallOutOfScreenObject(i * Globals.T, j * Globals.T, room) { Texture = GameManager.Current.Map.TileSet[t.ID] };
                                break;
                            case 960: // timed switch (1s)
                                new TimeSwitch(i * Globals.T, j * Globals.T, 1 * 60, room);
                                break;
                            case 961: // timed switch (3s)
                                new TimeSwitch(i * Globals.T, j * Globals.T, 3 * 60, room);
                                break;
                            case 962: // timed switch (5s)
                                new TimeSwitch(i * Globals.T, j * Globals.T, 5 * 60, room);
                                break;
                            case 963: // timed switch (10s)
                                new TimeSwitch(i * Globals.T, j * Globals.T, 10 * 60, room);
                                break;
                            // flow objects:
                            case 896:
                            case 897:
                            case 898:
                            case 899:
                                if (t.ID == 896)
                                    new Flow(i * Globals.T, j * Globals.T, room, Direction.UP);
                                if (t.ID == 897)
                                    new Flow(i * Globals.T, j * Globals.T, room, Direction.DOWN);
                                if (t.ID == 898)
                                    new Flow(i * Globals.T, j * Globals.T, room, Direction.LEFT);
                                if (t.ID == 899)
                                    new Flow(i * Globals.T, j * Globals.T, room, Direction.RIGHT);
                                break;
                            // flow objects (activatable)
                            case 900:
                            case 901:
                            case 902:
                            case 903:
                                if (t.ID == 900)
                                    new Flow(i * Globals.T, j * Globals.T, room, Direction.UP).Activatable = true;
                                if (t.ID == 901)
                                    new Flow(i * Globals.T, j * Globals.T, room, Direction.DOWN).Activatable = true;
                                if (t.ID == 902)
                                    new Flow(i * Globals.T, j * Globals.T, room, Direction.LEFT).Activatable = true;
                                if (t.ID == 903)
                                    new Flow(i * Globals.T, j * Globals.T, room, Direction.RIGHT).Activatable = true;
                                break;
                            // flow objects (activatable, default: on)
                            case 904:
                            case 905:
                            case 906:
                            case 907:
                                if (t.ID == 904)
                                    new Flow(i * Globals.T, j * Globals.T, room, Direction.UP) { Activatable = true, ActiveByDefault = true };
                                if (t.ID == 905)
                                    new Flow(i * Globals.T, j * Globals.T, room, Direction.DOWN) { Activatable = true, ActiveByDefault = true };
                                if (t.ID == 906)
                                    new Flow(i * Globals.T, j * Globals.T, room, Direction.LEFT) { Activatable = true, ActiveByDefault = true };
                                if (t.ID == 907)
                                    new Flow(i * Globals.T, j * Globals.T, room, Direction.RIGHT) { Activatable = true, ActiveByDefault = true };
                                break;
                            case 908: // laser (vertical)
                                new Laser(i * Globals.T, j * Globals.T, room, Laser.Orientation.Vertical);
                                break;
                            case 909: // laser (horizontal)
                                new Laser(i * Globals.T, j * Globals.T, room, Laser.Orientation.Horizontal);
                                break;
                            case 910: // toggle laser (vertical)
                                new Laser(i * Globals.T, j * Globals.T, room, Laser.Orientation.Vertical, true);
                                break;
                            case 911: // toggle laser (horizontal)
                                new Laser(i * Globals.T, j * Globals.T, room, Laser.Orientation.Horizontal, true);
                                break;
                            case 912: // toggle laser (vertical)
                                new Laser(i * Globals.T, j * Globals.T, room, Laser.Orientation.Vertical, true, defaultValue: false);
                                break;
                            case 913: // toggle laser (horizontal)
                                new Laser(i * Globals.T, j * Globals.T, room, Laser.Orientation.Horizontal, true, defaultValue: false);
                                break;
                            case 1024: // AMBIENCE (firefly)
                                new EmitterSpawner<FireFlyEmitter>(i * Globals.T, j * Globals.T, room);
                                break;
                            case 1025: // AMBIENCE (small bird)
                                new LittleBirb(i * Globals.T + 8, j * Globals.T, room);
                                break;
                            default:
                                var solid = new Solid(i * Globals.T, j * Globals.T, room);
                                t.TileOptions.Visible = true;
                                t.TileOptions.Solid = true;
                                if (t.ID == 645) t.TileOptions.Visible = false;  // <- invisible blocks
                                break;
                        }
                    }
                }
            }

            // water animated tiles
            {
                var index = GameManager.Current.Map.LayerDepth.ToList().IndexOf(GameManager.Current.Map.LayerDepth.First(o => o.Key.ToLower() == "water"));
                var data = GameManager.Current.Map.LayerData.ElementAt(index);

                x = (int)((float)x).Clamp(0, data.Width);
                y = (int)((float)y).Clamp(0, data.Height);

                // load objects from water tile data

                for (int i = x; i < x + w; i++)
                {
                    for (int j = y; j < y + h; j++)
                    {
                        var t = data.Get(i, j);

                        if (t == null || t.ID == -1)
                            continue;

                        switch (t.ID)
                        {
                            case 199: // default water
                                t.TileOptions.Visible = false;
                                new AnimatedWaterSurface(i * Globals.T, j * Globals.T, room, 0);
                                break;
                            case 140: // cave water
                                t.TileOptions.Visible = false;
                                new AnimatedWaterSurface(i * Globals.T, j * Globals.T, room, 1);
                                break;
                            case 934: // deep cave water
                                t.TileOptions.Visible = false;
                                new AnimatedWaterSurface(i * Globals.T, j * Globals.T, room, 2);
                                break;
                        }                        
                    }
                }
            }

            // doesn't matter which weather: always have bubbles in water
            new EmitterSpawner<GlobalWaterBubbleEmitter>(room.X, room.Y, room);

            //Debug.WriteLine("Created " + solidCount + " solid objects.");
            GameManager.Current.LoadedRooms.Add(room);
        }

        public static void CreateRoomObjectsFromData(List<Dictionary<string, object>> objectData, Room room)
        {
            var camera = RoomCamera.Current;
            
            try
            {
                foreach (var data in objectData)
                {
                    var x = (int)data["x"];
                    var y = (int)data["y"];
                    var width = (int)data["width"];
                    var height = (int)data["height"];
                    
                    if (!((float)x).In(room.X, room.X + room.BoundingBox.Width) || !((float)y).In(room.Y, room.Y + room.BoundingBox.Height))
                    {
                        continue;
                    }

                    var type = data["name"].ToString();

                    if (type == "item")
                    {
                        var itemType = data.ContainsKey("itemType") ? (int)data["itemType"] : -1;
                        var itemName = data.ContainsKey("itemName") ? data["itemName"].ToString() : "-unknown-";
                        var itemText = data.ContainsKey("text") ? data["text"].ToString() : "-unknown-";
                        var itemSetCondition = data.ContainsKey("setCondition") ? data["setCondition"].ToString() : null;
                        var itemAppearCondition = data.ContainsKey("appearCondition") ? data["appearCondition"].ToString() : null;

                        // warp after obtain (story item only)
                        var tx = data.ContainsKey("tx") ? (int)data["tx"] : -1;
                        var ty = data.ContainsKey("ty") ? (int)data["ty"] : -1;

                        AbilityItem item = null;

                        switch (itemType)
                        {
                            case 0: // ability: push
                                item = new AbilityItem(x + 8, y + 8, room, itemName, setCondition: itemSetCondition, appearCondition: itemAppearCondition);
                                item.Texture = AssetManager.Items[0];
                                item.Text = itemText;
                                item.OnObtain = () => { GameManager.Current.Player.Stats.Abilities |= PlayerAbility.PUSH; };
                                break;
                            case 1: // ability: orb
                                item = new AbilityItem(x + 8, y + 8, room, itemName, setCondition: itemSetCondition, appearCondition: itemAppearCondition);
                                item.Texture = AssetManager.Orbs[0];
                                item.DrawOffset = new Vector2(8);
                                item.OnObtain = () => { GameManager.Current.Player.Stats.Abilities |= PlayerAbility.ORB; };
                                item.Text = itemText;
                                break;
                            case 2: // spell: shooting star
                                item = new AbilityItem(x + 8, y + 8, room, itemName, setCondition: itemSetCondition, appearCondition: itemAppearCondition);
                                item.Texture = AssetManager.Items[4];
                                item.OnObtain = () =>
                                {
                                    GameManager.Current.AddSpell(SpellType.STAR);
                                };
                                item.Text = itemText;
                                break;
                            case 3: // spell: snatch
                                item = new AbilityItem(x + 8, y + 8, room, itemName, setCondition: itemSetCondition, appearCondition: itemAppearCondition);
                                item.Texture = AssetManager.Items[7];
                                item.OnObtain = () =>
                                {
                                    GameManager.Current.AddSpell(SpellType.SNATCH_KEYS);
                                };
                                item.Text = itemText;
                                break;
                            case 4: // ability: double jump
                                item = new AbilityItem(x + 8, y + 8, room, itemName, setCondition: itemSetCondition, appearCondition: itemAppearCondition);
                                item.Texture = AssetManager.Items[8];
                                item.OnObtain = () => { GameManager.Current.Player.Stats.Abilities |= PlayerAbility.DOUBLE_JUMP; };
                                item.Text = itemText;
                                break;
                            case 5: // spell: fire
                                item = new AbilityItem(x + 8, y + 8, room, itemName, setCondition: itemSetCondition, appearCondition: itemAppearCondition);
                                item.Texture = AssetManager.Items[9];
                                item.OnObtain = () => { GameManager.Current.AddSpell(SpellType.FIRE); };
                                item.Text = itemText;                                
                                break;
                            case 6: // ability: climb wall
                                item = new AbilityItem(x + 8, y + 8, room, itemName, setCondition: itemSetCondition, appearCondition: itemAppearCondition);
                                item.Texture = AssetManager.Items[10];
                                item.OnObtain = () => { GameManager.Current.Player.Stats.Abilities |= PlayerAbility.CLIMB_WALL; };
                                item.Text = itemText;
                                break;
                            case 7: // ability: climb ceil
                                item = new AbilityItem(x + 8, y + 8, room, itemName, setCondition: itemSetCondition, appearCondition: itemAppearCondition);
                                item.Texture = AssetManager.Items[11];
                                item.OnObtain = () => { GameManager.Current.Player.Stats.Abilities |= PlayerAbility.CLIMB_CEIL; };
                                item.Text = itemText;
                                break;
                            case 8: // ability: roll
                                item = new AbilityItem(x + 8, y + 8, room, itemName, setCondition: itemSetCondition, appearCondition: itemAppearCondition);
                                item.Texture = AssetManager.Items[12];
                                item.OnObtain = () => { GameManager.Current.Player.Stats.Abilities |= PlayerAbility.ROLL; };
                                item.Text = itemText;
                                break;
                            case 9: // ability: levitate
                                item = new AbilityItem(x + 8, y + 8, room, itemName, setCondition: itemSetCondition, appearCondition: itemAppearCondition);
                                item.Texture = AssetManager.Items[13];
                                item.OnObtain = () => { GameManager.Current.Player.Stats.Abilities |= PlayerAbility.LEVITATE; };
                                item.Text = itemText;
                                break;
                            case 10: // ability: breathe underwater
                                item = new AbilityItem(x + 8, y + 8, room, itemName, setCondition: itemSetCondition, appearCondition: itemAppearCondition);
                                item.Texture = AssetManager.Items[14];
                                item.OnObtain = () => { GameManager.Current.Player.Stats.Abilities |= PlayerAbility.BREATHE_UNDERWATER; };
                                item.Text = itemText;
                                break;
                            // TODO: add other item types, collectables etc.
                            default:
                                throw new NotImplementedException("Item type not implemented!");
                        }
                    }
                    if (type == "door")
                    {
                        var tx = data.ContainsKey("tx") ? (int)data["tx"] : 0;
                        var ty = data.ContainsKey("ty") ? (int)data["ty"] : 0;

                        var fadeType = data.ContainsKey("fadeType") ? (int)data["fadeType"] : 0;
                        var direction = data.ContainsKey("direction") ? (Direction)(int)data["direction"] : Direction.NONE;
                        var levelName = data.ContainsKey("levelName") ? data["levelName"].ToString() : null;

                        var door = new Door(x, y, room, tx, ty, levelName);
                        door.TransitionType = (TransitionType)fadeType;
                        door.Direction = direction;
                    }                    
                    if (type == "npc")
                    {
                        var npcText = data.ContainsKey("text") ? data["text"].ToString() : "-unknown-";
                        var npcYesText = data.ContainsKey("yesText") ? data["yesText"].ToString() : null;
                        var npcNoText = data.ContainsKey("noText") ? data["noText"].ToString() : null;
                        var npcType = data.ContainsKey("npcType") ? (int)data["npcType"] : -1;
                        var npcDirection = data.ContainsKey("direction") ? (Direction)(int)data["direction"] : Direction.NONE;
                        var npcCenterText = data.ContainsKey("centerText") ? (bool)data["centerText"] : false;

                        var npcAppearCondition = data.ContainsKey("appearCondition") ? data["appearCondition"].ToString() : null;
                        var npcSetCondition = data.ContainsKey("setCondition") ? data["setCondition"].ToString() : null;
                        var npcDisappearCondition = data.ContainsKey("disappearCondition") ? data["disappearCondition"].ToString() : null;

                        var npc = new NPC(x + 8, y + 8, room, npcType, npcText, npcAppearCondition, npcSetCondition, npcDisappearCondition, npcCenterText, npcDirection, yesText:npcYesText, noText:npcNoText);                        
                    }
                    if (type == "coinStatue")
                    {
                        var amount = data.ContainsKey("amount") ? (int)data["amount"] : 0;
                        var coinStatue = new CoinStatue(x + 8, y + 8, room, amount);
                    }
                    if (type == "shopItem")
                    {
                        var shopItemDisplayText = data.ContainsKey("displayText") ? data["displayText"].ToString() : "-unknown-";
                        var shopItemAcquireText = data.ContainsKey("acquireText") ? data["acquireText"].ToString() : "-unknown-";
                        var shopItemName = data.ContainsKey("itemName") ? data["itemName"].ToString() : "-unknown-";
                        var shopItemType = data.ContainsKey("itemType") ? (int)data["itemType"] : 0;
                        var shopItemRespawn = data.ContainsKey("respawn") ? (bool)data["respawn"] : false;
                        var shopItemPrice = data.ContainsKey("price") ? (int)data["price"] : 0;
                        var shopItemAppearCondition = data.ContainsKey("appearCondition") ? data["appearCondition"].ToString() : null;

                        var shopItem = new ShopItem(x + 8, y + 8, room, shopItemName, shopItemType, shopItemPrice, shopItemDisplayText, shopItemAcquireText, shopItemRespawn, shopItemAppearCondition);
                    }
                    if (type == "chest")
                    {
                        var chestValue = data.ContainsKey("value") ? (float)data["value"] : 0.0f;
                        var chest = new Chest(x, y, room, chestValue);
                    }
                    if (type == "movingPlatform")
                    {
                        var movXvel = data.ContainsKey("xVel") ? (float)data["xVel"] : 0f;
                        var movYvel = data.ContainsKey("yVel") ? (float)data["yVel"] : 0f;
                        // in tile units
                        var movXrange = data.ContainsKey("xRange") ? (int)data["xRange"] : 0;
                        var movYrange = data.ContainsKey("yRange") ? (int)data["yRange"] : 0;

                        var moveTimeout = data.ContainsKey("timeOut") ? (int)data["timeOut"] : -1;
                        var activatable = data.ContainsKey("activatable") ? (bool)data["activatable"] : false;

                        var movingPlatform = new LinearMovingPlatform(x, y, movXvel, movYvel, movXrange, movYrange, activatable, moveTimeout, room);
                    }
                    if (type == "waterMill")
                    {
                        new WaterMill(x + 8, y + 8, room);
                    }
                    if (type == "boss")
                    {
                        var bossAppearCondition = data.ContainsKey("appearCondition") ? data["appearCondition"].ToString() : null;
                        var bossSetCondition = data.ContainsKey("setCondition") ? data["setCondition"].ToString() : null;

                        var bossType = data.ContainsKey("bossType") ? (int)data["bossType"] : 0;
                        var bossSpawn = new BossSpawn(x, y, room, bossType, bossAppearCondition, bossSetCondition);
                        bossSpawn.BoundingBox = new RectF(0, 0, width, height);
                    }
                    if (type == "storyTrigger")
                    {
                        var storyTriggerSetCondition = data.ContainsKey("setCondition") ? data["setCondition"].ToString() : null;
                        var storyTrigger = new StoryTrigger(x, y, room, storyTriggerSetCondition);
                        storyTrigger.BoundingBox = new RectF(0, 0, width, height);
                    }
                    if (type == "storyWarp")
                    {
                        var tx = data.ContainsKey("tx") ? (int)data["tx"] : 0;
                        var ty = data.ContainsKey("ty") ? (int)data["ty"] : 0;
                        var setCondition = data.ContainsKey("setCondition") ? data["setCondition"].ToString() : null;
                        var text = data.ContainsKey("text") ? data["text"].ToString() : null;
                        var levelName = data.ContainsKey("levelName") ? data["levelName"].ToString() : null;
                        var direction = data.ContainsKey("direction") ? (Direction)(int)data["direction"] : Direction.NONE;

                        var warp = new StoryWarp(x + 16, y + 8, room, setCondition, tx, ty, text, direction, levelName);
                    }
                    if (type == "jumpDisabler")
                    {
                        var jumpDisablerDisappearCondition = data.ContainsKey("disappearCondition") ? data["disappearCondition"].ToString() : null;
                        var jumpDisabler = new JumpControlDisabler(x, y, room, jumpDisablerDisappearCondition);
                        jumpDisabler.BoundingBox = new RectF(0, 0, width, height);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Unable to initialize objects: " + e.Message);
                throw;
            }
        }
        
        /// <summary>
        /// Creates a room object and adds it to the camera room list.
        /// </summary>
        /// <param name="roomData"></param>
        public static void CreateAllRooms(List<Dictionary<string, object>> roomData, int mapIndex)
        {
            var camera = RoomCamera.Current;

            try
            {
                foreach (var data in roomData)
                {
                    var x = (int)data["x"];
                    var y = (int)data["y"];

                    var w = data.ContainsKey("width") ? (int)data["width"] : camera.ViewWidth;
                    var h = data.ContainsKey("height") ? (int)data["height"] : camera.ViewHeight;
                    var bg = data.ContainsKey("bg") ? (int)data["bg"] : -1;
                    var weather = data.ContainsKey("weather") ? (int)data["weather"] : -1;
                    var isDark = data.ContainsKey("dark") ? Convert.ToBoolean(data["dark"]) : false;

                    int remX, remY, remW, remH;
                    Math.DivRem(x, camera.ViewWidth, out remX);
                    Math.DivRem(y, camera.ViewHeight, out remY);
                    Math.DivRem(w, camera.ViewWidth, out remW);
                    Math.DivRem(h, camera.ViewHeight, out remH);

                    if (remX != 0 || remY != 0 || remW != 0 || remH != 0)
                        throw new ArgumentException($"The room at ({x},{y}) has an incorrect size or position!");

                    var room = new Room(x, y, w, h, mapIndex);
                    
                    room.Background = bg;
                    room.Weather = weather;
                    room.IsDark = isDark;
                }

                // load rooms of standard size when there is none
                for (var i = 0; i < GameManager.Current.Map.Width * Globals.T; i += camera.ViewWidth)
                {
                    for (var j = 0; j < GameManager.Current.Map.Height * Globals.T; j += camera.ViewHeight)
                    {
                        var c = ObjectManager.CollisionPoints<Room>(i + Globals.T, j + Globals.T).Count;
                        if (c == 0)
                        {
                            var room = new Room(i, j, camera.ViewWidth, camera.ViewHeight, mapIndex);
                        }
                    }
                }
            }
            catch (ObjectException e)
            {
                Debug.WriteLine("Unable to initialize room from data: " + e.Message);
                throw;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Unable to initialize room from data: " + e.Message);
                throw;
            }
        }

        /// <summary>
        /// After rooms + neighbours are created, call this method to clean all room objects except for the active room
        /// </summary>
        /// <param name="room"></param>
        [Obsolete("TODO: Find a better solution to the inefficient object loading problem!")]
        public static void CleanObjectsExceptRoom(Room room)
        {
            var toDelete = ObjectManager.Objects.Where(
                o => o is RoomObject                
                && !(o is Collider) 
                && (o as RoomObject).Room != room).ToList();

            var arr = new GameObject[toDelete.Count];
            toDelete.CopyTo(arr);

            foreach (var del in arr)
                del.Destroy();            
        }
    }
}

