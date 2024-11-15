﻿using Leore.Objects.Level;
using SPG.Objects;
using System;
using System.Linq;
using SPG.Map;
using Microsoft.Xna.Framework;
using Leore.Main;

namespace Leore.Objects
{
    public static class CollisionExtensions
    {

        public static bool MoveAdvanced(this IMovable m, bool moveWithPlatforms)
        {
            bool onGround = false;

            m.YVel += m.Gravity;
            m.YVel = Math.Sign(m.YVel) * Math.Min(Math.Abs(m.YVel), 4);

            // moving platform pre-calculations

            var platforms = m.CollisionBounds<Platform>(m.X, m.Y + m.YVel);

            // get off platform when not in X-range
            if (m.MovingPlatform != null)
            {
                if (m.Left > m.MovingPlatform.Right || m.Right < m.MovingPlatform.Left)
                    m.MovingPlatform = null;
            }

            var prev = m.MovingPlatform;

            var movXvel = 0f;
            var movYvel = 0f;

            if (m.MovingPlatform == null)
            {
                movXvel = 0f;
                movYvel = 0f;
            }
            else
            {
                movXvel = m.MovingPlatform.XVel;
                movYvel = m.MovingPlatform.YVel;

                var temp = m.MovingPlatform;

                while ((temp as IMovable)?.MovingPlatform != null)
                {
                    movXvel = (temp as IMovable).MovingPlatform.XVel;
                    movYvel = (temp as IMovable).MovingPlatform.YVel;

                    temp = (temp as IMovable).MovingPlatform;
                }
            }

            var colY = m.CollisionBounds<Collider>(m.X, m.Y + movYvel + m.YVel).Where(o => o is Solid).ToList();

            Platform storedPlatform = null;

            Vector2 moveOffset = Vector2.Zero;

            if (platforms.Count > 0)
            {
                for (var i = 0; i < platforms.Count; i++)
                {
                    if (m.Bottom <= platforms[i].Top - platforms[i].YVel)
                    {
                        if (m.YVel >= 0)
                        {
                            colY.Clear();
                            colY.Add(platforms[i]);

                            if (moveWithPlatforms)
                            {
                                if ((platforms[i] as IMovable)?.MovingPlatform != null) // other objects that have a moving platform
                                {
                                    if (m.MovingPlatform == null)
                                        m.MovingPlatform = platforms[i];
                                }
                                else if (platforms[i] is MovingPlatform) // standard moving platforms
                                {
                                    if (m.MovingPlatform == null)
                                        m.MovingPlatform = platforms[i];
                                }
                                else
                                {
                                    storedPlatform = platforms[i];
                                }
                            }
                        }
                    }
                }

                // prevents standing on a movingplatform that goes down and being then able to get down
                if (m.MovingPlatform != null && m.MovingPlatform.YVel >= 0)
                {
                    var plt = m.CollisionBoundsFirstOrDefault<Platform>(m.X, m.Y + movYvel + m.YVel);
                    
                    if (plt != m.MovingPlatform && plt != null && plt.Center.Y > m.Center.Y)
                        storedPlatform = plt ?? null;
                    if (storedPlatform != null)
                    {
                        m.MovingPlatform = null;
                        colY.Add(plt);
                    }

                    // disables moving platform collision in water
                    var inWater = GameManager.Current.Map.CollisionTile((m as GameObject), m.XVel, m.YVel - 1, GameMap.WATER_INDEX);

                    // special case for player because he is shifted a bit for water collision
                    if (m is Player)
                        inWater = GameManager.Current.Map.CollisionTile(m.X, m.Y + 4, GameMap.WATER_INDEX);
                    
                    if (inWater)
                        m.MovingPlatform = null;
                }
            }

            // get off platform when touching y blocks
            if (m.MovingPlatform != null)
            {
                moveOffset = m.Position - m.MovingPlatform.Position;

                // hitting head against blocks
                if (m.MovingPlatform.YVel < 0)
                {
                    var colYnew = m.CollisionBoundsFirstOrDefault<Solid>(m.X, m.Y + movYvel + m.YVel - 1);
                    if (colYnew != null)
                    {
                        colY.Add(colYnew);
                        m.MovingPlatform = null;
                    }
                }

                if (colY.Where(o => o is Solid).Count() == 0)
                    colY.Add(m.MovingPlatform);
                else
                {
                    colY = m.CollisionBounds<Collider>(m.X, m.Y + movYvel + m.YVel - 1).Where(o => o is Solid).ToList();
                    if (colY.Count > 0)
                        m.MovingPlatform = null;
                }

                // not touching the moving platform any more -> let go
                // + 1 because it wouldn't drive the object upwards otherwise..
                if (!m.CollisionBounds(m.MovingPlatform, m.X, m.Y + movYvel + m.YVel + 1)
                    || m.MovingPlatform.Top < m.Bottom + moveOffset.Y - 1
                    || m.Top + moveOffset.Y > m.MovingPlatform.Bottom - 4)
                {
                    m.MovingPlatform = null;
                }

            }

            // actual movement

            if (m.MovingPlatform != null)
            {
                // trick to lift up M (todo: remove?)
                if (m.Bottom > m.MovingPlatform.Y)
                    m.Move(0, -Math.Abs(m.MovingPlatform.YVel));

                colY.Clear();
                colY.Add(m.MovingPlatform);
            }

            if (colY.Count == 0)
            {
                m.Move(0, m.YVel + movYvel);
            }
            else
            {
                if (m.YVel >= m.Gravity)
                {
                    onGround = true;

                    // trick to "snap" to the bottom:

                    var overlap = m.Bottom - colY.FirstOrDefault().Top + m.Gravity;
                    if (Math.Abs(overlap) <= Math.Abs(m.YVel) + Math.Abs(movYvel) + Math.Abs(colY.First().YVel))
                        m.Move(0, -overlap);

                    //var overlap = m.Bottom - colY.FirstOrDefault().Top;
                    //if (Math.Abs(overlap) <= Math.Abs(m.YVel) + Math.Abs(movYvel) + Math.Abs(colY.First().YVel))
                    //    m.Move(0, -overlap - m.Gravity);

                }
                m.YVel = 0;
            }

            var colX = ObjectManager.CollisionBoundsFirstOrDefault<Solid>(m, m.X + m.XVel + movXvel, m.Y);

            if (colX == null)
            {
                m.Move(m.XVel + movXvel, 0);
            }
            else
            {
                m.XVel = 0;
            }

            return onGround;
        }
    }

    // WORKING VERSION - BUT WITH SOLID GAMEOBJECTS

    //public static bool MoveAdvanced(this IMovable m, bool moveWithPlatforms)
    //{

    //    bool onGround = false;

    //    m.YVel += m.Gravity;
    //    m.YVel = Math.Sign(m.YVel) * Math.Min(Math.Abs(m.YVel), 4);

    //    // moving platform pre-calculations

    //    var platforms = m.CollisionBounds<Platform>(m.X, m.Y + m.YVel);

    //    // get off platform when not in X-range
    //    if (m.MovingPlatform != null)
    //    {
    //        if (m.Left > m.MovingPlatform.Right || m.Right < m.MovingPlatform.Left)
    //            m.MovingPlatform = null;
    //    }

    //    var prev = m.MovingPlatform;

    //    var movXvel = 0f;
    //    var movYvel = 0f;

    //    if (m.MovingPlatform == null)
    //    {
    //        movXvel = 0f;
    //        movYvel = 0f;
    //    }
    //    else
    //    {
    //        movXvel = m.MovingPlatform.XVel;
    //        movYvel = m.MovingPlatform.YVel;

    //        var temp = m.MovingPlatform;

    //        while ((temp as IMovable)?.MovingPlatform != null)
    //        {
    //            movXvel = (temp as IMovable).MovingPlatform.XVel;
    //            movYvel = (temp as IMovable).MovingPlatform.YVel;

    //            temp = (temp as IMovable).MovingPlatform;
    //        }
    //    }

    //    var colY = m.CollisionBounds<Collider>(m.X, m.Y + movYvel + m.YVel).Where(o => o is Solid).ToList();
    //    Platform storedPlatform = null;

    //    Vector2 moveOffset = Vector2.Zero;

    //    if (platforms.Count > 0)
    //    {
    //        for (var i = 0; i < platforms.Count; i++)
    //        {
    //            if (m.Bottom <= platforms[i].Top - platforms[i].YVel)
    //            {
    //                if (m.YVel >= 0)
    //                {
    //                    colY.Clear();
    //                    colY.Add(platforms[i]);

    //                    if (moveWithPlatforms)
    //                    {
    //                        if ((platforms[i] as IMovable)?.MovingPlatform != null) // other objects that have a moving platform
    //                        {
    //                            if (m.MovingPlatform == null)
    //                                m.MovingPlatform = platforms[i];
    //                        }
    //                        else if (platforms[i] is MovingPlatform) // standard moving platforms
    //                        {
    //                            if (m.MovingPlatform == null)
    //                                m.MovingPlatform = platforms[i];
    //                        }
    //                        else
    //                        {
    //                            storedPlatform = platforms[i];
    //                        }
    //                    }
    //                }
    //            }
    //        }

    //        // prevents standing on a movingplatform that goes down and being then able to get down
    //        if (m.MovingPlatform != null && m.MovingPlatform.YVel > 0)
    //        {
    //            if (storedPlatform != null)
    //                m.MovingPlatform = null;
    //        }
    //    }

    //    // get off platform when touching y blocks
    //    if (m.MovingPlatform != null)
    //    {
    //        moveOffset = m.Position - m.MovingPlatform.Position;

    //        // hitting head against blocks
    //        if (m.MovingPlatform.YVel < 0)
    //        {
    //            var colYnew = m.CollisionBoundsFirstOrDefault<Solid>(m.X, m.Y + movYvel + m.YVel - 1);
    //            if (colYnew != null)
    //            {
    //                colY.Add(colYnew);
    //                m.MovingPlatform = null;
    //            }
    //        }

    //        if (colY.Where(o => o is Solid).Count() == 0)
    //            colY.Add(m.MovingPlatform);
    //        else
    //        {
    //            colY = m.CollisionBounds<Collider>(m.X, m.Y + movYvel + m.YVel - 1).Where(o => o is Solid).ToList();
    //            if (colY.Count > 0)
    //                m.MovingPlatform = null;
    //        }

    //        // not touching the moving platform any more -> let go
    //        // + 1 because it wouldn't drive the object upwards otherwise..
    //        if (!m.CollisionBounds(m.MovingPlatform, m.X, m.Y + movYvel + m.YVel + 1)
    //            || m.MovingPlatform.Top < m.Bottom + moveOffset.Y - 1
    //            || m.Top + moveOffset.Y > m.MovingPlatform.Bottom - 4)
    //        {
    //            m.MovingPlatform = null;
    //        }

    //    }

    //    // actual movement

    //    if (m.MovingPlatform != null)
    //    {
    //        // trick to lift up M (todo: remove?)
    //        if (m.Bottom > m.MovingPlatform.Y)
    //            m.Move(0, -Math.Abs(m.MovingPlatform.YVel));

    //        colY.Clear();
    //        colY.Add(m.MovingPlatform);
    //    }

    //    if (colY.Count == 0)
    //    {
    //        m.Move(0, m.YVel + movYvel);
    //    }
    //    else
    //    {
    //        if (m.YVel >= m.Gravity)
    //        {
    //            onGround = true;

    //            // trick to "snap" to the bottom:
    //            var overlap = m.Bottom - colY.FirstOrDefault().Top;
    //            if (Math.Abs(overlap) <= Math.Abs(m.YVel) + Math.Abs(movYvel))
    //                m.Move(0, -overlap - m.Gravity);

    //        }
    //        m.YVel = 0;
    //    }

    //    var colX = ObjectManager.CollisionBoundsFirstOrDefault<Solid>(m, m.X + m.XVel + movXvel, m.Y);

    //    if (colX == null)
    //    {
    //        m.Move(m.XVel + movXvel, 0);
    //    }
    //    else
    //    {
    //        m.XVel = 0;
    //    }

    //    return onGround;
    //}
}
