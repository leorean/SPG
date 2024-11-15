﻿using Microsoft.Xna.Framework;
using Leore.Main;
using System.Collections.Generic;
using SPG;

namespace Leore.Resources
{
    public struct EnemyStats
    {
        public int HP;
        public int Damage;
        public int EXP;
    }

    public static class GameResources
    {
        // enemy stats

        public static readonly EnemyStats EnemyBat = new EnemyStats { HP = 5, EXP = 3, Damage = 1 };
        public static readonly EnemyStats EnemyGrassy = new EnemyStats { HP = 10, EXP = 15, Damage = 2 };
        public static readonly EnemyStats EnemySlime = new EnemyStats { HP = 8, EXP = 5, Damage = 1 };
        public static readonly EnemyStats EnemySlurp = new EnemyStats { HP = 5, EXP = 10, Damage = 2 };
        public static readonly EnemyStats EnemyDash = new EnemyStats { HP = 10, EXP = 10, Damage = 2 };

        public static readonly List<EnemyStats> EnemyVoidling = new List<EnemyStats>
        {
             new EnemyStats { HP = 16, EXP = 20, Damage = 3 },
             new EnemyStats { HP = 20, EXP = 25, Damage = 3 },
             new EnemyStats { HP = 30, EXP = 30, Damage = 3 }
        };
        
        public static readonly EnemyStats BossMirrorSelf = new EnemyStats { HP = 1, EXP = 0, Damage = 0 }; // kind of an exception

        public static readonly EnemyStats BossShadowLizard = new EnemyStats { HP = 140, EXP = 200, Damage = 1 };

        // orb stats

        public static readonly Dictionary<SpellType, Dictionary<SpellLevel, int>> MaxEXP = new Dictionary<SpellType, Dictionary<SpellLevel, int>>
        {
            {
                SpellType.NONE,
                new Dictionary<SpellLevel, int> {
                    {SpellLevel.ONE, 0 },
                    {SpellLevel.TWO, 0 },
                    {SpellLevel.THREE, 0 }
                }
            },
            {
                SpellType.STAR,
                new Dictionary<SpellLevel, int> {
                    {SpellLevel.ONE, 12 },
                    {SpellLevel.TWO, 40 },
                    {SpellLevel.THREE, 120 }
                }
            },
            {
                SpellType.CRIMSON_ARC,
                new Dictionary<SpellLevel, int> {
                    {SpellLevel.ONE, 45 },
                    {SpellLevel.TWO, 100 },
                    {SpellLevel.THREE, 220 }
                }
            },
            {
                SpellType.VOID,
                new Dictionary<SpellLevel, int> {
                    {SpellLevel.ONE, 100 },
                    {SpellLevel.TWO, 200 },
                    {SpellLevel.THREE, 400 }
                }
            },
            {
                SpellType.SNATCH_KEYS,
                new Dictionary<SpellLevel, int> {
                    {SpellLevel.ONE, 0 },
                    {SpellLevel.TWO, 0 },
                    {SpellLevel.THREE, 0 }
                }
            },
            {
                SpellType.FIRE,
                new Dictionary<SpellLevel, int> {
                    {SpellLevel.ONE, 80 },
                    {SpellLevel.TWO, 120 },
                    {SpellLevel.THREE, 160 }
                }
            },
            {
                SpellType.ICE,
                new Dictionary<SpellLevel, int> {
                    {SpellLevel.ONE, 80 },
                    {SpellLevel.TWO, 100 },
                    {SpellLevel.THREE, 140 }
                }
            },
        };
                
        public static readonly Dictionary<SpellType, Dictionary<SpellLevel, float>> MPCost = new Dictionary<SpellType, Dictionary<SpellLevel, float>>
        {
            {
                SpellType.NONE,
                new Dictionary<SpellLevel, float> {
                    {SpellLevel.ONE, 0 },
                    {SpellLevel.TWO, 0 },
                    {SpellLevel.THREE, 0 }
                }
            },
            {
                SpellType.STAR,
                new Dictionary<SpellLevel, float> {
                    {SpellLevel.ONE, 1 },
                    {SpellLevel.TWO, 2 },
                    {SpellLevel.THREE, .5f }
                }
            },
            {
                SpellType.CRIMSON_ARC,
                new Dictionary<SpellLevel, float> {
                    {SpellLevel.ONE, 3 },
                    {SpellLevel.TWO, 5 },
                    {SpellLevel.THREE, 7 }
                }
            },
            {
                SpellType.VOID,
                new Dictionary<SpellLevel, float> {
                    {SpellLevel.ONE, .15f },
                    {SpellLevel.TWO, .3f },
                    {SpellLevel.THREE, .4f }
                }
            },
            {
                SpellType.SNATCH_KEYS,
                new Dictionary<SpellLevel, float> {
                    {SpellLevel.ONE, 0 },
                    {SpellLevel.TWO, 0 },
                    {SpellLevel.THREE, 0 }
                }
            },
            {
                SpellType.FIRE,
                new Dictionary<SpellLevel, float> {
                    {SpellLevel.ONE, 1 },
                    {SpellLevel.TWO, 4 },
                    {SpellLevel.THREE, .2f }                    
                }
            },
            {
                SpellType.ICE,
                new Dictionary<SpellLevel, float> {
                    {SpellLevel.ONE, 1 },
                    {SpellLevel.TWO, 1 },
                    {SpellLevel.THREE, .75f }
                }
            },
        };

        // colors

        public static readonly Color VoidColor = Colors.FromHex("973bba");

        public static readonly Color SunColor = Colors.FromHex("f6c048");

        public static readonly Color MagicColor = Colors.FromHex("5fcde4");

        public static readonly List<Color> OxygenColors = new List<Color>
        {
            new Color(3, 243, 243),
            new Color(79, 3, 243)
        };

        public static List<Color> CrimsonColors = new List<Color>
        {
            Colors.FromHex("c80e1f"),            
            Colors.FromHex("900717"),
            Colors.FromHex("590210")
        };

        public static readonly List<Color> FireColors = new List<Color>
        {
            Colors.FromHex("ffa100"),
            Colors.FromHex("fc5a00"),
            Colors.FromHex("ffe777"),
        };

        public static readonly List<Color> HpColors = new List<Color>
        {
            Colors.FromHex("ff1734"),
            Colors.FromHex("e20c22"),
            Colors.FromHex("c3000f")
        };

        public static readonly List<Color> MpColors = new List<Color>
        {
            new Color(3, 243, 243),
            new Color(143, 255, 249),
            new Color(95, 205, 208)
        };

        public static readonly List<Color> RegenColors = new List<Color>
        {
            new Color(170, 233, 60),
            new Color(242, 255, 156),
            new Color(104, 197, 100)
        };
        
        // items


    }
}
