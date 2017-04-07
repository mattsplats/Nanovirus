using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNA2DGame
{
    class Foreground : Sprite
    {
        // Bounding box fields
        public static List<Rectangle> floorBoxList = new List<Rectangle>();
        public static List<Rectangle> ceilingBoxList = new List<Rectangle>();
        public static List<Rectangle> wallBoxList = new List<Rectangle>();
        
        // Constructors
        public Foreground(Texture2D textureImage, Vector2 position)
            : this(textureImage, position, Color.White)
        { }

        public Foreground(Texture2D textureImage, Vector2 position, Color tint)
            : base(textureImage, position, 0.2f, Vector2.Zero, tint)
        { }

        // Properties
        public override Rectangle BoundingBox { get { return Rectangle.Empty; } }
        
        // Bounding box draw method for foregrounds - draws all boxes
        protected override void DrawTerrainBoxes(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < wallBoxList.Count; ++i)
            {
                spriteBatch.Draw(boxTexture, wallBoxList[i], null, Color.Yellow, 0, Vector2.Zero, SpriteEffects.None, 0.65f);
            }
            for (int i = 0; i < floorBoxList.Count; ++i)
            {
                spriteBatch.Draw(boxTexture, floorBoxList[i], null, Color.Green, 0, Vector2.Zero, SpriteEffects.None, 0.64f);
            }
            for (int i = 0; i < ceilingBoxList.Count; ++i)
            {
                spriteBatch.Draw(boxTexture, ceilingBoxList[i], null, Color.Cyan, 0, Vector2.Zero, SpriteEffects.None, 0.63f);
            }
        }
    }
}