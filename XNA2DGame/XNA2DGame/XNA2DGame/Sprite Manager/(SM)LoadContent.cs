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
        // This partial of SpriteManager holds only the LoadContent methods.

        void LoadPlayer()
        {
            // Player texture array
            // Starting texture is playerTextures[1] - see Player class to modify
            Player.playerTextures[0] = Game.Content.Load<Texture2D>(@"images/player/player_idle_left");
            Player.playerTextures[1] = Game.Content.Load<Texture2D>(@"images/player/player_idle_right");
            Player.playerTextures[2] = Game.Content.Load<Texture2D>(@"images/player/player_walk_left");
            Player.playerTextures[3] = Game.Content.Load<Texture2D>(@"images/player/player_walk_right");
            Player.playerTextures[4] = Game.Content.Load<Texture2D>(@"images/player/player_jump_left");
            Player.playerTextures[5] = Game.Content.Load<Texture2D>(@"images/player/player_jump_right");

            // Player constructor
            player = new Player(new Vector2(20, 119), new Point(30, 50), new Point(6, 4));
        }

        void LoadForegrounds()
        {
            // Foreground textures
            foregroundList.Add(new Foreground(Game.Content.Load<Texture2D>(@"images/backgrounds/reference_level_lower_left"),
                Vector2.Zero));
            foregroundList.Add(new Foreground(Game.Content.Load<Texture2D>(@"images/backgrounds/reference_level_lower_mid"),
                new Vector2(320, 0)));
            foregroundList.Add(new Foreground(Game.Content.Load<Texture2D>(@"images/backgrounds/reference_level_lower_right"),
                new Vector2(640, 0)));
            foregroundList.Add(new Foreground(Game.Content.Load<Texture2D>(@"images/backgrounds/reference_level_upper_left"),
                new Vector2(0, -180)));
            foregroundList.Add(new Foreground(Game.Content.Load<Texture2D>(@"images/backgrounds/reference_level_upper_mid"),
                new Vector2(320, -180)));
            foregroundList.Add(new Foreground(Game.Content.Load<Texture2D>(@"images/backgrounds/reference_level_upper_right"),
                new Vector2(640, -180)));

            // Floor boxes
            Foreground.floorBoxList.Add(new Rectangle(0, 164, 320, 5));
            Foreground.floorBoxList.Add(new Rectangle(320, 164, 320, 5));
            Foreground.floorBoxList.Add(new Rectangle(640, 164, 320, 5));

            Foreground.floorBoxList.Add(new Rectangle(0, -17, 440, 5));
            Foreground.floorBoxList.Add(new Rectangle(513, -17, 445, 5));

            /*Foreground.floorBoxList.Add(new Rectangle(0, 164, 100, 5));
            for (int i = 0; i < 100; i++)
                Foreground.floorBoxList.Add(new Rectangle(100 + 2*i, 163 - i, 2, 5));*/

            // Wall boxes
            Foreground.wallBoxList.Add(new Rectangle(3, -180, 5, 360));
            Foreground.wallBoxList.Add(new Rectangle(951, -180, 5, 360));

            Foreground.wallBoxList.Add(new Rectangle(435, -16, 5, 23));
            Foreground.wallBoxList.Add(new Rectangle(513, -16, 5, 23));

            // Ceiling boxes
            Foreground.ceilingBoxList.Add(new Rectangle(0, -177, 960, 5));

            Foreground.ceilingBoxList.Add(new Rectangle(0, 3, 440, 5));
            Foreground.ceilingBoxList.Add(new Rectangle(513, 3, 445, 5));
        }

        void LoadForegroundObjects()
        {
            TestStepBox.StepBoxImage = Game.Content.Load<Texture2D>(@"images/backgrounds/reference_level_step");

            foregroundObjectList.Add(new TestStepBox(new Vector2(412, 95)));
            foregroundObjectList.Add(new TestStepBox(new Vector2(462, 55)));
            foregroundObjectList.Add(new TestStepBox(new Vector2(462, -80)));
        }

        void LoadMonsters()
        {
            // ReverseMob texture array
            ReverseMob.reverseMobTextures[0] = Game.Content.Load<Texture2D>(@"images/monsters/reverse/idle_left");
            ReverseMob.reverseMobTextures[1] = Game.Content.Load<Texture2D>(@"images/monsters/reverse/idle_right");
            ReverseMob.reverseMobTextures[2] = Game.Content.Load<Texture2D>(@"images/monsters/reverse/walk_left");
            ReverseMob.reverseMobTextures[3] = Game.Content.Load<Texture2D>(@"images/monsters/reverse/walk_right");

            // BlobMob texture array
            BlobMob.blobMobTextures[0] = Game.Content.Load<Texture2D>(@"images/monsters/blob/idle_left");
            BlobMob.blobMobTextures[1] = Game.Content.Load<Texture2D>(@"images/monsters/blob/idle_right");
            BlobMob.blobMobTextures[2] = Game.Content.Load<Texture2D>(@"images/monsters/blob/jump_left");
            BlobMob.blobMobTextures[3] = Game.Content.Load<Texture2D>(@"images/monsters/blob/jump_right");

            // ReverseMobs
            for (int i = 0; i < 4; i++)
            {
                monsterList.Add(new ReverseMob(new Vector2(150 + (40 + Game1.rnd.Next(50)) * i, 100)));
            }
            for (int i = 0; i < 4; i++)
            {
                monsterList.Add(new ReverseMob(new Vector2(22 + (40 + Game1.rnd.Next(50)) * i, -80)));
            }

            // BlobMobs
            monsterList.Add(new BlobMob(new Vector2(150, 130)));
            monsterList.Add(new BlobMob(new Vector2(408, 75)));
        }

        void LoadFonts()
        {
            UIfont = Game.Content.Load<SpriteFont>(@"fonts/UI_font");
        }
    }
}
