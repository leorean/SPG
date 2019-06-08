using Microsoft.Xna.Framework;
using Leore.Main;
using System.Collections.Generic;

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

        public static readonly List<EnemyStats> EnemyVoidling = new List<EnemyStats>
        {
             new EnemyStats { HP = 16, EXP = 10, Damage = 3 },
             new EnemyStats { HP = 20, EXP = 15, Damage = 3 },
             new EnemyStats { HP = 30, EXP = 25, Damage = 3 }
        };

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
                    {SpellLevel.THREE, 1 }
                }
            },
            {
                SpellType.CRIMSON_ARC,
                new Dictionary<SpellLevel, float> {
                    {SpellLevel.ONE, .1f },
                    {SpellLevel.TWO, .1f },
                    {SpellLevel.THREE, .05f }
                }
            },
        };

        // colors

        public static readonly Color OxygenColor1 = new Color(3, 243, 243);
        public static readonly Color OxygenColor2 = new Color(79, 3, 243);
        
        // items


    }
}
