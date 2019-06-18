﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leore.Main;
using Leore.Resources;
using Leore.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SPG;

namespace Leore.Objects.Enemies
{
    public class Boss : Enemy
    {
        private string setCondition;

        public Boss(float x, float y, Room room, string setCondition) : base(x, y, room)
        {
            this.setCondition = setCondition;
            MainGame.Current.HUD.SetBoss(this);
        }

        public override void Hit(int hitPoints, float degAngle)
        {
            base.Hit(hitPoints, degAngle);
            MainGame.Current.HUD.SetBoss(this);
        }

        public override void OnDeath()
        {
            //base.OnDeath(); // never un-comment this!

            GameManager.Current.AddStoryFlag(setCondition);

            GameManager.Current.Player.Stats.Bosses.Add(ID);
            MainGame.Current.HUD.SetBoss(null);
        }        
    }
}
