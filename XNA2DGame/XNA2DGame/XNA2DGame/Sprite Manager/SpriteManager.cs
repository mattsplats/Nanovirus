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
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public partial class SpriteManager : Microsoft.Xna.Framework.DrawableGameComponent
    {
        // Setup fields
        SpriteBatch spriteBatch;
        float scalingFactor = 4;
        
        // Sprites
        Player player;
        List<Foreground> foregroundList = new List<Foreground>();
        List<Sprite> foregroundObjectList = new List<Sprite>();
        List<Monster> monsterList = new List<Monster>();
        List<Sprite> projectileList = new List<Sprite>();

        // Fonts
        SpriteFont UIfont;
        
        // Camera fields
        float cameraXPos = 0;
        float cameraYPos = 0;
        int movementStateX = 0;         // Movement state of player vs rest of map (0 = at left edge, 1 = at right edge, 2 = moving right, 3 = moving left)
        bool mustCatchUp = false;       // Used to get player "caught up" to movingPlayerPos if direction was reversed or player was at end of map

        // Constructor
        public SpriteManager(Game game)
            : base(game)
        {
        }

        // Overload methods
        public override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
 	        spriteBatch = new SpriteBatch(Game.GraphicsDevice);

            // Texture for drawing bounding boxes - consists of a single white pixel
            Sprite.boxTexture = new Texture2D(Game.GraphicsDevice, 1, 1);
            Sprite.boxTexture.SetData(new Color[] { Color.White });

            LoadPlayer();
            LoadForegrounds();
            LoadForegroundObjects();
            LoadMonsters();

            LoadFonts();

            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            // Exit if player is dead
            if (player.CurrentHP <= 0)
            {
                System.Threading.Thread.Sleep(1000);
                Game.Exit();
            }
            
            // Update the player
            player.Update(gameTime);

            // Create and update gunshots
            if (player.PulledTheTrigger && (!player.IsInvulnerable || player.InvulnCounter > 15))
            {
                if (player.IsFacingLeft)
                    projectileList.Add(new Sprite(Game.Content.Load<Texture2D>(@"images/projectiles/player/basic_projectile_left"),
                        new Vector2(player.XPos + 4, player.YPos + 18), 0.6f, new Vector2(-8, 0), Color.White));
                else
                    projectileList.Add(new Sprite(Game.Content.Load<Texture2D>(@"images/projectiles/player/basic_projectile_right"),
                        new Vector2(player.XPos + 22, player.YPos + 18), 0.6f, new Vector2(8, 0), Color.White));
            }

            for (int i = 0; i < projectileList.Count; ++i)
            {
                Sprite p = projectileList[i];
                
                p.Update(gameTime);

                // Check for out of bounds
                if (p.XPos - p.Texture.Width + 5 < cameraXPos || p.XPos - 5 > cameraXPos + 320
                    || p.YPos - p.Texture.Height + 5 < cameraYPos || p.YPos - 5 > cameraYPos + 180)
                {
                    projectileList.RemoveAt(i);
                    --i;
                }
            }

            // Update all monsters
            for (int i = 0; i < monsterList.Count; ++i)
            {
                Monster m = monsterList[i];

                // Determine if BlobMob (i.e. has MaxHP == 1) is within jumping dist of player
                if (m.MaxHP == 1)
                    if (Math.Abs(m.YPos - player.YPos) < 35)    
                        if ((m.XPos - player.XPos) < 32)
                            if ((m.XPos - player.XPos) > 0)
                                m.isLeftOfPlayer = true;
                        else if ((player.XPos - m.XPos) < 42)
                            if ((player.XPos - m.XPos) > 10)
                                m.isRightOfPlayer = true;              

                m.Update(gameTime);

                // Number of hits monster took
                int hitCount = 0;

                // Check for gunshot collisions
                for (int j = 0; j < projectileList.Count; ++j)
                {
                    Sprite p = projectileList[j];
                    
                    if (p.BoundingBox.Intersects(m.BoundingBox))
                    {
                        Matrix projectileMatrix = Matrix.CreateTranslation(p.XPos, p.YPos, 0);
                        Matrix monsterMatrix = Matrix.CreateTranslation(m.XPos, m.YPos, 0);

                        if (PerPixelCollision(TextureTo2DArray(p.Texture), projectileMatrix,
                            TextureTo2DArray(m.Texture, m.TextureBox), monsterMatrix))
                        {
                            projectileList.RemoveAt(j);
                            --j;
                            ++hitCount;
                        }
                    }
                }

                // Flash monster and lower monster HP based on # of hits taken, remove dead monsters
                if (hitCount > 0)
                {
                    m.IsHit = true;
                    m.CurrentHP -= hitCount;
                    if (m.CurrentHP <= 0)
                    {
                        monsterList.RemoveAt(i);
                        --i;
                    }
                }

                // Check for collisions w/ player
                else if (m.BoundingBox.Intersects(player.BoundingBox))
                {
                    // Rectangle overlapBox = Rectangle.Intersect(s.BoundingBox, player.BoundingBox);

                    Matrix playerMatrix = Matrix.CreateTranslation(player.XPos + 7, player.YPos + 4, 0);
                    Matrix monsterMatrix = Matrix.CreateTranslation(m.XPos + m.CollisionOffset.X, m.YPos + m.CollisionOffset.Y, 0);

                    if (PerPixelCollision(TextureTo2DArray(player.Texture, player.TextureBox), playerMatrix,
                        TextureTo2DArray(m.Texture, m.TextureBox), monsterMatrix))
                        PlayerIsHit();
                }
            }

            // Flash player when hit and update playerIsInvulnerable
            FlashPlayerAndUpdateVulnerability();

            // Update player and monster positions if they collide with the foreground
            CheckForTerrainCollisions();

            // Move the camera to the new position
            MoveCamera_X();
            MoveCamera_Y();

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            // Draw all game objects and backgrounds
            Matrix globalTransformation = Matrix.CreateTranslation(-cameraXPos, -cameraYPos, 0) * 
                Matrix.CreateScale(new Vector3(scalingFactor, scalingFactor, 1));

            spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp,
                null, null, null, globalTransformation);

            // Draw the player
            player.Draw(gameTime, spriteBatch);
            
            // Draw all projectiles
            foreach (Sprite s in projectileList)
                s.Draw(gameTime, spriteBatch);

            // Draw background tiles
            foreach (Foreground b in foregroundList)
                b.Draw(gameTime, spriteBatch);

            // Draw all monsters
            foreach (Monster m in monsterList)
                m.Draw(gameTime, spriteBatch);

            // Draw all foreground objects
            foreach (Sprite s in foregroundObjectList)
                s.Draw(gameTime, spriteBatch);

            spriteBatch.End();

            // Draw all UI elements
            spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, null, null);

            // Draw player health indicator
            spriteBatch.DrawString(UIfont, "Health: " + player.CurrentHP + "%", new Vector2(1090, 30), Color.DarkBlue,
                0, Vector2.Zero, 1, SpriteEffects.None, 1);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
