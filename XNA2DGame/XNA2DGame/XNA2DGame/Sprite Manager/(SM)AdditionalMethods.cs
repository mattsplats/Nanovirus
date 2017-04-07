using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace XNA2DGame
{
    public partial class SpriteManager : Microsoft.Xna.Framework.DrawableGameComponent
    {
        // This partial of SpriteManager holds only the helper methods and properties for the Update and Draw methods.

        // Collision detection methods
        // For simple sprites (no sprite sheet)
        Color[,] TextureTo2DArray(Texture2D texture)
        {
            Color[] colors1D = new Color[texture.Width * texture.Height];
            texture.GetData(colors1D);

            Color[,] colors2D = new Color[texture.Width, texture.Height];
            for (int x = 0; x < texture.Width; x++)
                for (int y = 0; y < texture.Height; y++)
                    colors2D[x, y] = colors1D[x + (y * texture.Width)];

            return colors2D;
        }

        // For animated sprites: texArea = currentFrame
        Color[,] TextureTo2DArray(Texture2D texture, Rectangle texArea)
        {
            Color[] colors1D = new Color[texArea.Width * texArea.Height];
            texture.GetData(0, texArea, colors1D, 0, (texArea.Width * texArea.Height));

            Color[,] colors2D = new Color[texArea.Width, texArea.Height];
            for (int x = 0; x < texArea.Width; x++)
                for (int y = 0; y < texArea.Height; y++)
                    colors2D[x, y] = colors1D[x + (y * texArea.Width)];

            return colors2D;
        }

        // Requires color arrays of both colliding textures and their screen space transformation matrices
        bool PerPixelCollision(Color[,] tex1, Matrix mat1, Color[,] tex2, Matrix mat2)
        {
            Matrix mat1to2 = mat1 * Matrix.Invert(mat2);
            int width1 = tex1.GetLength(0);
            int height1 = tex1.GetLength(1);
            int width2 = tex2.GetLength(0);
            int height2 = tex2.GetLength(1);

            for (int x1 = 0; x1 < width1; x1++)
            {
                for (int y1 = 0; y1 < height1; y1++)
                {
                    Vector2 pos1 = new Vector2(x1, y1);
                    Vector2 pos2 = Vector2.Transform(pos1, mat1to2);

                    int x2 = (int)pos2.X;
                    int y2 = (int)pos2.Y;

                    if ((x2 >= 0) && (x2 < width2))
                        if ((y2 >= 0) && (y2 < height2))
                            if (tex1[x1, y1].A > 0)
                                if (tex2[x2, y2].A > 0)
                                    return true;
                }
            }
            return false;
        }

        // Deals with player being hit
        void PlayerIsHit()
        {
            if (!player.IsInvulnerable)
                --player.CurrentHP;
            player.IsInvulnerable = true;
        }

        // Flash player when hit and update player.IsInvulnerable
        void FlashPlayerAndUpdateVulnerability()
        {
            if (player.IsInvulnerable)
            {
                ++player.InvulnCounter;
                if ((player.InvulnCounter > 0 && player.InvulnCounter < 4) || (player.InvulnCounter > 7 && player.InvulnCounter < 11))
                    player.Tint = Color.Red;
                else
                    player.Tint = Color.White;
                if (player.InvulnCounter > 30)
                {
                    player.InvulnCounter = 0;
                    player.IsInvulnerable = false;
                }
            }
        }
        
        // Camera movement methods
        void MoveCamera_X()
        {
            // X position of the last map tile
            const int endOfMap = 640;

            // For the following values, L = when facing left, R = when facing right
            // All values are relative to the current camera position
            // Position the player is fixed at if the player is moving in the facing direction
            const float movingPlayerPosL = 160;
            const float movingPlayerPosR = 120;

            // Position at which the camera will start facing the opposite direction
            const float reverseFacingPosL = 230;
            const float reverseFacingPosR = 50;

            // Position the camera will return to normal after hitting the end of the map
            // WARNING: if these are inside their opposite reverseFacingPos values, wonkiness may result
            const float resumeMovementPosL = 50;
            const float resumeMovementPosR = 230;

            // Speed at which camera will attempt to "catch up" to the player position
            const float catchUpSpeed = 15;

            // Player's position on the screen, or "relative position"
            float playerScreenXPos = player.XPos - cameraXPos;

            // X distance to move camera
            float offset = 0;

            switch (movementStateX)
            {
                case 0:     // Left edge of map
                    if (mustCatchUp)                                                    // Catch up function
                    {
                        offset = catchUpSpeed;
                        if (playerScreenXPos - offset < movingPlayerPosR)
                        {
                            offset = playerScreenXPos - movingPlayerPosR;
                            mustCatchUp = false;                                        // We're caught up
                            movementStateX = 2;                                          // Now moving right
                        }
                    }
                    else if (playerScreenXPos > resumeMovementPosR)
                        mustCatchUp = true;
                    else
                        offset = 0;
                    break;

                case 1:     // Right edge of map
                    if (mustCatchUp)                                                    // Catch up function
                    {
                        offset = -catchUpSpeed;
                        if (playerScreenXPos - offset > movingPlayerPosL)
                        {
                            offset = playerScreenXPos - movingPlayerPosL;
                            mustCatchUp = false;                                        // We're caught up
                            movementStateX = 3;                                          // Now moving left
                        }
                    }
                    else if (playerScreenXPos < resumeMovementPosL)
                        mustCatchUp = true;
                    else
                        offset = 0;
                    break;

                case 2:     // Moving right
                    if (mustCatchUp)                                                    // Catch up function
                    {
                        offset = catchUpSpeed;
                        if (playerScreenXPos - offset < movingPlayerPosR)
                        {
                            offset = playerScreenXPos - movingPlayerPosR;
                            mustCatchUp = false;                                        // We're caught up
                        }
                    }
                    else if (playerScreenXPos > movingPlayerPosR)                       // If moving right
                    {
                        offset = playerScreenXPos - movingPlayerPosR;
                    }
                    else if (playerScreenXPos < reverseFacingPosR)                       // If moving left
                    {
                        offset = 0;
                        mustCatchUp = true;                                             // Face the other way
                        movementStateX = 3;                                              // and catch up
                    }
                    else
                        offset = 0;
                    break;

                case 3:     // Moving left
                    if (mustCatchUp)                                                    // Catch up function
                    {
                        offset = -catchUpSpeed;
                        if (playerScreenXPos - offset > movingPlayerPosL)
                        {
                            offset = playerScreenXPos - movingPlayerPosL;
                            mustCatchUp = false;                                        // We're caught up
                        }
                    }
                    else if (playerScreenXPos < movingPlayerPosL)                      // If moving right
                    {
                        offset = playerScreenXPos - movingPlayerPosL;
                    }
                    else if (playerScreenXPos > reverseFacingPosL)                        // If moving left
                    {
                        offset = 0;
                        mustCatchUp = true;                                             // Face the other way
                        movementStateX = 2;                                              // and catch up
                    }
                    else
                        offset = 0;
                    break;
            }

            cameraXPos += offset;

            if (cameraXPos < 0)                                             // If moved beyond left map edge
            {
                cameraXPos = 0;                                             // Move to end of map
                movementStateX = 0;                                          // We're at left edge
                mustCatchUp = false;
            }
            if (cameraXPos > endOfMap)                                      // If moved beyond right map edge
            {
                cameraXPos = endOfMap;                                      // Move to end of map
                movementStateX = 1;                                          // We're at right edge
                mustCatchUp = false;
            }
        }

        void MoveCamera_Y()
        {
            // True if the current foreground is taller than the screen height (180)
            bool isTallerThanScreen = true;

            // All values are relative to the current camera position
            // Position the player is fixed at if the player is moving up/down
            const float movingPlayerPosUp = 30;
            const float movingPlayerPosDown = 90;

            // Player's position on the screen, or "relative position"
            float playerScreenYPos = player.YPos - cameraYPos;

            // Y distance to move camera
            float offset = 0;

            if (isTallerThanScreen)
            {
                if (playerScreenYPos < movingPlayerPosUp)
                    offset = playerScreenYPos - movingPlayerPosUp;
                if (playerScreenYPos > movingPlayerPosDown)
                    offset = playerScreenYPos - movingPlayerPosDown;
            }

            cameraYPos += offset;

            if (cameraYPos < -180)                                      // If moved beyond top map edge
                cameraYPos = -180;                                      // Move to end of map
            if (cameraYPos > 0)                                         // If moved beyond bottom map edge
                cameraYPos = 0;                                         // Move to end of map
        }

        // Terrain collision methods
        void CheckForTerrainCollisions()
        {
            // For the player
            // Wall collisions
            if (player.XVel != 0)
                foreach (Rectangle r in Foreground.wallBoxList)
                {
                    if (r.Intersects(player.FrontBox))
                    {
                        if (player.IsFacingLeft)
                            player.XPos = r.X + r.Width - 10;
                        else
                            player.XPos = r.X - 21;
                        player.XVel = 0;
                    }
                    if (!player.IsFacingLeft && player.XVel < 0)
                    {
                        if (r.Intersects(player.BackBox))
                        {
                            player.XPos = r.X + r.Width - 9;
                            player.XVel = 0;
                        }
                        if (r.Intersects(player.BackOfHeadBox))
                        {
                            player.XPos = r.X + r.Width - 13;
                            player.XVel = 0;
                        }
                    }
                    if (player.IsFacingLeft && player.XVel > 0)
                    {
                        if (r.Intersects(player.BackBox))
                        {
                            player.XPos = r.X - 22;
                            player.XVel = 0;
                        }
                        if (r.Intersects(player.BackOfHeadBox))
                        {
                            player.XPos = r.X - 17;
                            player.XVel = 0;
                        }
                    }
                }

            // Floor collisions (moving down only)
            if (player.YVel >= 0)
                foreach (Rectangle r in Foreground.floorBoxList)
                    if (r.Intersects(player.FeetBox))
                    {
                        player.YPos = r.Y - 49;
                        player.YVel = 0;
                        player.IsOnTheGround = true;
                    }

            // Ceiling collisions (moving up only)
            if (player.YVel < 0)
                foreach (Rectangle r in Foreground.ceilingBoxList)
                {
                    if (r.Intersects(player.HeadBox))
                    {
                        player.YPos = r.Y - 1;
                        player.YVel = 0;
                    }
                    if (r.Intersects(player.ShouldersBox))
                    {
                        player.YPos = r.Y - 9;
                        player.YVel = 0;
                    }
                }

            // For all monsters
            foreach (Monster m in monsterList)
            {
                // Wall collisions
                if (m.XVel != 0)
                    foreach (Rectangle r in Foreground.wallBoxList)
                    {
                        if (r.Intersects(m.FrontBox))
                        {
                            if (m.IsFacingLeft)
                                m.XPos = r.X + r.Width - 10;
                            else
                                m.XPos = r.X - 21;
                            m.XVel *= -1;
                        }
                        if (!m.IsFacingLeft && m.XVel < 0)
                        {
                            if (r.Intersects(m.BackBox))
                            {
                                m.XPos = r.X + r.Width - 9;
                                m.XVel = 0;
                            }
                        }
                        if (m.IsFacingLeft && m.XVel > 0)
                        {
                            if (r.Intersects(m.BackBox))
                            {
                                m.XPos = r.X - 22;
                                m.XVel = 0;
                            }
                        }
                    }

                // Floor collisions (moving down only)
                if (m.YVel >= 0)
                    foreach (Rectangle r in Foreground.floorBoxList)
                        if (r.Intersects(m.BottomBox))
                        {
                            m.YPos = r.Y - m.BottomResetPoint;
                            m.YVel = 0;
                            m.IsOnTheGround = true;
                        }

                // Ceiling collisions (moving up only)
                if (m.YVel < 0)
                    foreach (Rectangle r in Foreground.ceilingBoxList)
                    {
                        if (r.Intersects(m.TopBox))
                        {
                            m.YPos = r.Y - 1;
                            m.YVel = 0;
                        }
                    }
            }
        }
    }
}
