using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNA2DGame
{
    class Sprite
    {
        // Base fields
        protected Texture2D textureImage;
        protected Vector2 position;
        protected float layerDepth;

        // Basic overloads
        protected Vector2 velocity;
        protected Color tint;

        // Global movement-related fields
        protected const float terminalVelocity = 5f;                  // Y max speed
        protected const float gravity = 0.12f;                        // Y acceleration

        // For bounding box drawing
        static public Texture2D boxTexture;
        static public bool ShowBoundingBoxes = false;
        static public bool ShowTerrainBoxes = false;

        // Constructors
        public Sprite (Texture2D textureImage, Vector2 position, float layerDepth, Vector2 velocity, Color tint)
        {
            this.textureImage = textureImage;
            this.position = position;
            this.layerDepth = layerDepth;
            this.velocity = velocity;
            this.tint = tint;
        }

        // Base Sprite methods
        public virtual void Update(GameTime gameTime)
        {
            if (velocity != Vector2.Zero)
                position += velocity;
        }

        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(textureImage, position, null, tint, 0, Vector2.Zero, 1f, SpriteEffects.None, layerDepth);

            if (ShowBoundingBoxes)
                DrawBoundingBox(spriteBatch);

            if (ShowTerrainBoxes)
                DrawTerrainBoxes(spriteBatch);
        }

        // Properties
        public virtual Rectangle BoundingBox
        {
            get { return new Rectangle((int)position.X, (int)position.Y, textureImage.Width, textureImage.Height); }
        }
        
        public Texture2D Texture
        {
            get { return textureImage; }
            set { textureImage = value; }
        }

        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }

        public float XPos
        {
            get { return position.X; }
            set { position.X = value; }
        }

        public float YPos
        {
            get { return position.Y; }
            set { position.Y = value; }
        }

        public Vector2 Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }

        public float XVel
        {
            get { return velocity.X; }
            set { velocity.X = value; }
        }

        public float YVel
        {
            get { return velocity.Y; }
            set { velocity.Y = value; }
        }

        public Color Tint
        {
            get { return tint; }
            set { tint = value; }
        }

        // Bounding box draw method
        protected void DrawBoundingBox(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(boxTexture, BoundingBox, null, Color.Red, 0, Vector2.Zero, SpriteEffects.None, 0.3f);
        }

        protected virtual void DrawTerrainBoxes(SpriteBatch spriteBatch)
        {
        }
    }
}
