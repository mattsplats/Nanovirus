using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace XNA2DGame
{
    class BlobMob : Monster
    {
        // Input-related fields (for testing only)
        //KeyboardState oldKeyboardState;
        //GamePadState oldGamepadState;
        
        // Movement-related fields
        const float xMaxVelocity = 1.5f;                    // X max speed
        const float xAcceleration = 0.5f;                   // How fast xMaxVelocity is reached
        const float xDeceleration = 0.3f;                   // When no accel is given, how fast velocity.X returns to 0
        const float jumpInitialVelocity = -3.2f;            // Initial -y speed on jump - currently does not accelerate

        // Animation-related fields
        public static Texture2D[] blobMobTextures = new Texture2D[4];
        const int thisMillisecondsPerFrame = 50;            // General animation speed

        // Collision-related fields
        Point collisionOffset = new Point(5, 3);
        const int bottomResetPoint = 23;

        // Status-related fields
        const int maxHP = 1;

        // AI-related fields

        // Constructors
        public BlobMob(Vector2 position)
            : base(blobMobTextures[2], position, new Point(36, 24), new Point(6, 4),
            thisMillisecondsPerFrame, Vector2.Zero, Color.White, bottomResetPoint, maxHP)
        {
            CurrentAnimation = 2;
            IsOnTheGround = true;
            CollisionOffset = collisionOffset;
        }
        
        // Properties
        public override Rectangle BoundingBox
        {
            get { return new Rectangle((int)position.X + 5, (int)position.Y + 3, frameSize.X - 10, frameSize.Y - 3); }
        }

        public override Rectangle TextureBox
        {
            get { return new Rectangle(((currentFrame.X - 1) * frameSize.X) + 5, ((currentFrame.Y - 1) * frameSize.Y) + 3, frameSize.X - 10, frameSize.Y - 3); }
        }

        // BlobMob's terrain collision boxes
        public override Rectangle FrontBox
        {
            get
            {
                if (IsFacingLeft)
                    return new Rectangle((int)position.X + 6, (int)position.Y + 5, 2, bottomResetPoint - 5);
                else
                    return new Rectangle((int)position.X + 28, (int)position.Y + 5, 2, bottomResetPoint - 5);
            }
        }

        public override Rectangle BackBox
        {
            get
            {
                if (IsFacingLeft)
                    return new Rectangle((int)position.X + 28, (int)position.Y + 5, 2, bottomResetPoint - 5);
                else
                    return new Rectangle((int)position.X + 6, (int)position.Y + 5, 2, bottomResetPoint - 5);
            }
        }

        public override Rectangle BottomBox
        {
            get
            {
                if (IsFacingLeft)
                    return new Rectangle((int)position.X + 6, (int)position.Y + bottomResetPoint - 2, 24, 3);
                else
                    return new Rectangle((int)position.X + 6, (int)position.Y + bottomResetPoint - 2, 24, 3);
            }
        }

        public override Rectangle TopBox
        {
            get
            {
                if (IsFacingLeft)
                    return new Rectangle((int)position.X + 6, (int)position.Y + 4, 24, 3);
                else
                    return new Rectangle((int)position.X + 6, (int)position.Y + 4, 24, 3);
            }
        }

        public override int CurrentAnimation
        {
            get { return animationState; }
            set
            {
                if (value != animationState)
                {
                    // Setup state variables for changed animation
                    if (value == 0 || value == 1)
                        sheetSize = new Point(6, 4);
                    else
                        sheetSize = new Point(6, 3);

                    currentFrame = new Point(1, 1);

                    // Update the animationState and current texture
                    animationState = value;
                    textureImage = blobMobTextures[value];
                }
            }
        }

        // Unique methods
        protected override void ChangeTexture()
        {
            if (XVel < 0)
                IsFacingLeft = true;
            if (XVel > 0)
                IsFacingLeft = false;

            if (YVel == 1)
            {
                if (IsFacingLeft)
                    CurrentAnimation = 0;
                else
                    CurrentAnimation = 1;
            }
            else
            {
                if (IsFacingLeft)
                    CurrentAnimation = 2;
                else
                    CurrentAnimation = 3;
            }
        }

        protected override void RunAIandUpdateVelocity()
        {
            // Getting new input states (for testing only)
            //GamePadState newGamepadState = GamePad.GetState(PlayerIndex.One);
            //KeyboardState newKeyboardState = Keyboard.GetState();
            
            //float inputVel = -xMaxVelocity;

            // AI portion
            
            // Handling X velocity
            //if (inputVel == 0)
            //{
            //    if (XVel > 0)
            //    {
            //        XVel -= xDeceleration;
            //        if (XVel < 0)
            //            XVel = 0;
            //    }
            //    if (XVel < 0)
            //    {
            //        XVel += xDeceleration;
            //        if (XVel > 0)
            //            XVel = 0;
            //    }
            //    if (Math.Abs(XVel) < 0.08f)                     // Round small fractions to 0
            //        XVel = 0;
            //}
            //else
            //    XVel += inputVel;

            XVel = MathHelper.Clamp(XVel, -xMaxVelocity, xMaxVelocity);

            // Handling Y velocity
            /*if ((newKeyboardState.IsKeyDown(Keys.RightShift) && oldKeyboardState.IsKeyUp(Keys.RightShift)) ||
                (newGamepadState.Buttos.Y == ButtonState.Pressed && oldGamepadState.Buttons.Y == ButtonState.Released))*/
            if (isLeftOfPlayer)
            {
                isLeftOfPlayer = false;
                if (IsOnTheGround)
                {
                    YVel = jumpInitialVelocity;
                    XVel = -1.0f;
                }
            }

            if (isRightOfPlayer)
            {
                isRightOfPlayer = false;
                if (IsOnTheGround)
                {
                    YVel = jumpInitialVelocity;
                    XVel = 1.0f;
                }
            }
            
            if (YVel != 0)
                IsOnTheGround = false;

            if (IsOnTheGround)                                  // If ground collision last update
            {
                YVel = 1;                                       // Start falling again to detect walking off ledge or slope
                XVel = 0;
            }
            else
                YVel += gravity;

            if (YVel > terminalVelocity)
                YVel = terminalVelocity;
        }
    }
}
