using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNA2DGame
{
    class ReverseMob : Monster
    {
        // Movement-related fields
        const float xMaxVelocity = 0.9f;                    // X max speed
        const float xAcceleration = 0.2f;                   // How fast xMaxVelocity is reached
        const float xDeceleration = 0.3f;                   // When no accel is given, how fast velocity.X returns to 0
        const float jumpInitialVelocity = -3.5f;            // Initial -y speed on jump - currently does not accelerate

        // Input-related fields
        //KeyboardState oldKeyboardState;
        //GamePadState oldGamepadState;

        // Animation-related fields
        public static Texture2D[] reverseMobTextures = new Texture2D[4];
        const int thisMillisecondsPerFrame = 50;            // General animation speed
        
        // Collision-related fields
        Point collisionOffset = new Point(7, 4);
        const int bottomResetPoint = 47;

        // Status-related fields
        const int maxHP = 3;

        // AI-related fields

        // Constructors
        public ReverseMob(Vector2 position)
            : base(reverseMobTextures[2], position, new Point(30, 48), new Point(6, 4),
            thisMillisecondsPerFrame, Vector2.Zero, Color.White, bottomResetPoint, maxHP)
        {
            CurrentAnimation = 2;
            IsOnTheGround = true;
            XVel = -xMaxVelocity;
            CollisionOffset = collisionOffset;
        }
        
        // Properties
        public override Rectangle BoundingBox
        {
            get { return new Rectangle((int)position.X + 7, (int)position.Y + 4, frameSize.X - 13, frameSize.Y - 4); }
        }

        public override Rectangle TextureBox
        {
            get { return new Rectangle(((currentFrame.X - 1) * frameSize.X) + 7, ((currentFrame.Y - 1) * frameSize.Y) + 4, frameSize.X - 13, frameSize.Y - 4); }
        }

        // ReverseMob's terrain collision boxes
        public override Rectangle FrontBox
        {
            get
            {
                if (IsFacingLeft)
                    return new Rectangle((int)position.X + 8, (int)position.Y + 2, 2, bottomResetPoint - 2);
                else
                    return new Rectangle((int)position.X + 21, (int)position.Y + 2, 2, bottomResetPoint - 2);
            }
        }

        public override Rectangle BackBox
        {
            get
            {
                if (IsFacingLeft)
                    return new Rectangle((int)position.X + 22, (int)position.Y + 2, 2, bottomResetPoint - 2);
                else
                    return new Rectangle((int)position.X + 7, (int)position.Y + 2, 2, bottomResetPoint - 2);
            }
        }

        public override Rectangle BottomBox
        {
            get
            {
                if (IsFacingLeft)
                    return new Rectangle((int)position.X + 8, (int)position.Y + bottomResetPoint - 2, 16, 3);
                else
                    return new Rectangle((int)position.X + 7, (int)position.Y + bottomResetPoint - 2, 16, 3);
            }
        }

        public override Rectangle TopBox
        {
            get
            {
                if (IsFacingLeft)
                    return new Rectangle((int)position.X + 8, (int)position.Y + 1, 16, 2);
                else
                    return new Rectangle((int)position.X + 7, (int)position.Y + 1, 16, 2);
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
                    {
                        animatedTexture = false;
                        currentFrame = new Point(1, 1);
                    }
                    else
                        animatedTexture = true;

                    // Update the animationState and current texture
                    animationState = value;
                    textureImage = reverseMobTextures[value];
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

            if (XVel == 0)
            {
                if (IsFacingLeft)
                    CurrentAnimation = 0;
                else
                    CurrentAnimation = 1;
            }
            else if (XVel < 0)
                CurrentAnimation = 2;
            else if (XVel > 0)
                CurrentAnimation = 3;
        }

        protected override void RunAIandUpdateVelocity()
        {
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
            if (YVel != 0)
                IsOnTheGround = false;
            
            if (IsOnTheGround)                                  // If ground collision last update
                YVel = 1;                                       // Start falling again to detect walking off ledge or slope
            else
                YVel += gravity;

            if (YVel > terminalVelocity)
                YVel = terminalVelocity;
        }
    }
}
