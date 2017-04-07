using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNA2DGame
{
    abstract class Monster : AnimatedSprite
    {
        // Movement-related fields
        //const float terminalVelocity = 5f;                  // Defined in base Sprite class
        //const float gravity = 0.12f;                        // Defined in base Sprite class

        // Animation-related fields
        public bool IsFacingLeft = true;
        protected int animationState = 50;                  // Tied to CurrentAnimation property

        // Collision-related fields/properties
        public bool IsOnTheGround { get; set; }
        public bool IsHit { get; set; }
        public int HitFlashCounter { get; set; }
        public Point CollisionOffset { get; protected set; }
        public int BottomResetPoint { get; protected set; }

        // Status-related fields/properties
        public int MaxHP { get; protected set; }
        public int CurrentHP { get; set; }
        public bool isLeftOfPlayer = false;
        public bool isRightOfPlayer = false;

        // Constructors
        public Monster(Texture2D textureImage, Vector2 position, Point frameSize, Point sheetSize, int millisecondsPerFrame,
            Vector2 velocity, Color tint, int bottomResetPoint, int maxHP)
            : base(textureImage, position, 0.5f, frameSize, sheetSize, millisecondsPerFrame, velocity, tint)
        {
            BottomResetPoint = bottomResetPoint;
            MaxHP = maxHP;
            CurrentHP = maxHP;
            IsHit = false;
        }

        // Override methods
        public override void Update(GameTime gameTime)
        {
            RunAIandUpdateVelocity();
            ChangeTexture();
            FlashIfHit();

            base.Update(gameTime);
        }
        
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            base.Draw(gameTime, spriteBatch);
        }

        // Properties
        public abstract Rectangle FrontBox { get; }
        public abstract Rectangle BackBox { get; }
        public abstract Rectangle BottomBox { get; }
        public abstract Rectangle TopBox { get; }

        // Animation "manager" for the monster
        // Handles animationState, animatedTexture, millisecondsPerFrame, sheetSize, and initial currentFrame for each texture
        // Only runs when animationState (the active animation/texture) changes
        // Specific effects within a given animation should be handled in ChangeTexture
        public abstract int CurrentAnimation { get; set; }

        // Unique methods

        // Changes the active animation (texture) based on input, collision, etc.
        // Also handles animation properties (currentFrame, etc.) while an animation is running
        protected abstract void ChangeTexture();

        // Processes AI and updates velocity accordingly
        protected abstract void RunAIandUpdateVelocity();

        // Flash monster when hit
        void FlashIfHit()
        {
            if (IsHit)
            {
                ++HitFlashCounter;
                if (HitFlashCounter > 0 && HitFlashCounter < 5)
                    Tint = Color.LightGreen;
                else
                    Tint = Color.White;
                if (HitFlashCounter > 7)
                {
                    HitFlashCounter = 0;
                    IsHit = false;
                }
            }
        }

        // For diagnostic use - only called when ShowTerrainBoxes = true
        protected override void DrawTerrainBoxes(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(boxTexture, FrontBox, null, Color.Yellow, 0, Vector2.Zero, SpriteEffects.None, 0.65f);
            spriteBatch.Draw(boxTexture, BackBox, null, Color.Yellow, 0, Vector2.Zero, SpriteEffects.None, 0.65f);
            spriteBatch.Draw(boxTexture, BottomBox, null, Color.Green, 0, Vector2.Zero, SpriteEffects.None, 0.64f);
            spriteBatch.Draw(boxTexture, TopBox, null, Color.Cyan, 0, Vector2.Zero, SpriteEffects.None, 0.63f);
        }


        //protected override void RunAIandUpdateVelocity()
        //{
        //    // Getting new input states
        //    GamePadState newGamepadState = GamePad.GetState(PlayerIndex.Two);
        //    KeyboardState newKeyboardState = Keyboard.GetState();

        //    // Handling X velocity and left/right input
        //    float inputVel = 0;

        //    if (newKeyboardState.IsKeyDown(Keys.Left) || newKeyboardState.IsKeyDown(Keys.A))
        //        inputVel = -xAcceleration;
        //    if (newKeyboardState.IsKeyDown(Keys.Right) || newKeyboardState.IsKeyDown(Keys.D))
        //        inputVel = xAcceleration;

        //    if (newGamepadState.ThumbSticks.Left.X < -0.05)
        //        inputVel = -xAcceleration;
        //    if (newGamepadState.ThumbSticks.Left.X > 0.05)
        //        inputVel = xAcceleration;

        //    if (inputVel == 0)
        //    {
        //        if (XVel > 0)
        //        {
        //            XVel -= xDeceleration;
        //            if (XVel < 0)
        //                XVel = 0;
        //        }
        //        if (XVel < 0)
        //        {
        //            XVel += xDeceleration;
        //            if (XVel > 0)
        //                XVel = 0;
        //        }
        //        if (Math.Abs(XVel) < 0.08f)                     // Round small fractions to 0
        //            velocity.X = 0;
        //    }
        //    else
        //        velocity.X += inputVel;

        //    velocity.X = MathHelper.Clamp(velocity.X, -xMaxVelocity, xMaxVelocity);

        //    // Handling Y velocity and jumping
        //    if ((newKeyboardState.IsKeyDown(Keys.Space) && oldKeyboardState.IsKeyUp(Keys.Space)) ||
        //        (newGamepadState.Buttons.A == ButtonState.Pressed && oldGamepadState.Buttons.A == ButtonState.Released))
        //        if (IsOnTheGround == true)
        //            YVel = jumpInitialVelocity;

        //    if (YVel != 0)
        //        IsOnTheGround = false;

        //    if (IsOnTheGround)                                  // If ground collision last update
        //        YVel = 1;                                       // Start falling again to detect walking off ledge or slope
        //    else
        //        velocity.Y += gravity;

        //    if (velocity.Y > terminalVelocity)
        //        velocity.Y = terminalVelocity;

        //    // Turn on/off diagnostic modes
        //    if ((newKeyboardState.IsKeyDown(Keys.D1) && oldKeyboardState.IsKeyUp(Keys.D1)) ||
        //        (newGamepadState.Buttons.LeftShoulder == ButtonState.Pressed && oldGamepadState.Buttons.LeftShoulder == ButtonState.Released))
        //        ShowBoundingBoxes = !ShowBoundingBoxes;

        //    if ((newKeyboardState.IsKeyDown(Keys.D2) && oldKeyboardState.IsKeyUp(Keys.D2)) ||
        //        (newGamepadState.Buttons.RightShoulder == ButtonState.Pressed && oldGamepadState.Buttons.RightShoulder == ButtonState.Released))
        //        ShowTerrainBoxes = !ShowTerrainBoxes;

        //    // Updating old input states
        //    oldGamepadState = newGamepadState;
        //    oldKeyboardState = newKeyboardState;
        //}
    }
}
