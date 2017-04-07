
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace XNA2DGame
{
    class Player : AnimatedSprite
    {
        // Input-related fields
        KeyboardState oldKeyboardState;
        GamePadState oldGamepadState;
        
        // Movement-related fields
        const float xMaxVelocity = 2.2f;                    // X max speed
        const float xAcceleration = 0.3f;                   // How fast xMaxVelocity is reached
        const float xDeceleration = 0.3f;                   // When no input is given, how fast velocity.X returns to 0
        const float jumpInitialVelocity = -1.5f;            // Initial -y speed on jump
        const float jumpCurveFactor = 1.5f;                 // Decelerates added jump velocity (higher -> less effect)
        const float minJumpVelFrames = 3;                   // Min/max # of frames additional jump velocity will be applied
        const float maxJumpVelFrames = 15;
        int jumpCounter = 1;                                // Counter for applying additional jump velocity on future frames
        bool isJumping = false;

        // Animation-related fields/properties
        public static Texture2D[] playerTextures = new Texture2D[6];
        public bool IsFacingLeft { get; set; }
        int animationState;                                 // Tied to CurrentAnimation property
        Point hoverFrame = new Point(4, 2);                 // Animation frame player will "hover" on
        const int jumpAnimationSpeed = 66;                  // millisecondsPerFrame for the jump animation
        const float hoverVelocity = 0.30f;                  // Below this velocity, player will stay on "hover frame" mid-jump

        // Collision-related fields/properties
        public bool IsOnTheGround { get; set; }

        // Status-related fields/properties
        public const int MaxHP = 100;
        public int CurrentHP { get; set; }
        public bool IsInvulnerable { get; set; }
        public int InvulnCounter { get; set; }

        // Shooting-related fields/properties
        public bool PulledTheTrigger { get; set; }

        // Constructors
        public Player(Vector2 position, Point frameSize, Point sheetSize)
            : base(playerTextures[1], position, 0.6f, frameSize, sheetSize, 50, Vector2.Zero, Color.White)
        {
            CurrentAnimation = 1;
            IsOnTheGround = true;
            IsFacingLeft = false;
            CurrentHP = MaxHP;
            IsInvulnerable = false;
            PulledTheTrigger = false;
        }

        // Override methods
        public override void Update(GameTime gameTime)
        {
            HandleInputAndUpdateVelocity();
            ChangeTexture();

            base.Update(gameTime);  
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            base.Draw(gameTime, spriteBatch);
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

        // Player's terrain collision boxes
        public Rectangle FrontBox
        {
            get
            {
                if (IsFacingLeft)
                    return new Rectangle((int)position.X + 9, (int)position.Y + 4, 2, 45);
                else
                    return new Rectangle((int)position.X + 20, (int)position.Y + 4, 2, 45);
            }
        }

        public Rectangle BackBox
        {
            get
            {
                if (IsFacingLeft)
                    return new Rectangle((int)position.X + 22, (int)position.Y + 14, 2, 35);
                else
                    return new Rectangle((int)position.X + 7, (int)position.Y + 14, 2, 35);
            }
        }

        public Rectangle BackOfHeadBox
        {
            get
            {
                if (IsFacingLeft)
                    return new Rectangle((int)position.X + 18, (int)position.Y + 4, 2, 9);
                else
                    return new Rectangle((int)position.X + 11, (int)position.Y + 4, 2, 9);
            }
        }
        
        public Rectangle FeetBox
        {
            get
            {
                if (IsFacingLeft)
                    return new Rectangle((int)position.X + 11, (int)position.Y + 47, 12, 3);
                else
                    return new Rectangle((int)position.X + 8, (int)position.Y + 47, 12, 3);
            }
        }

        public Rectangle HeadBox
        {
            get
            {
                if (IsFacingLeft)
                    return new Rectangle((int)position.X + 9, (int)position.Y + 4, 10, 2);
                else
                    return new Rectangle((int)position.X + 12, (int)position.Y + 4, 10, 2);
            }
        }

        public Rectangle ShouldersBox
        {
            get
            {
                if (IsFacingLeft)
                    return new Rectangle((int)position.X + 19, (int)position.Y + 13, 4, 2);
                else
                    return new Rectangle((int)position.X + 8, (int)position.Y + 13, 4, 2);
            }
        }

        // Animation "manager" for the player
        // Handles animationState, animatedTexture, millisecondsPerFrame, sheetSize, and initial currentFrame for each texture
        // Only runs when animationState (the active animation/texture) changes
        // Specific effects within a given animation should be handled in ChangeTexture
        public int CurrentAnimation
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

                    if (value == 2 || value == 3)
                        if (animationState == 4 || animationState == 5)
                            currentFrame = new Point(1, 1);

                    if (value == 4 || value == 5)
                    {
                        sheetSize = new Point(6, 3);
                        millisecondsPerFrame = jumpAnimationSpeed;
                        if (animationState == 2 || animationState == 3)
                            currentFrame = new Point(1, 1);
                    }
                    else
                    {
                        sheetSize = new Point(6, 4);
                        millisecondsPerFrame = 50;
                    }

                    // Update the animationState and current texture
                    animationState = value;
                    textureImage = playerTextures[value];
                }
            }
        }

        // Unique methods

        // Changes the active animation (texture) based on input, collision, etc.
        // Also handles animation properties (currentFrame, etc.) while an animation is running
        void ChangeTexture()
        {
            if (XVel < 0)
                IsFacingLeft = true;
            if (XVel > 0)
                IsFacingLeft = false;

            if (!IsOnTheGround)
            {
                if (IsFacingLeft)
                    CurrentAnimation = 4;
                if (!IsFacingLeft)
                    CurrentAnimation = 5;

                if (Math.Abs(YVel) < hoverVelocity)
                {
                    currentFrame = hoverFrame;
                    animatedTexture = false;
                }
                else if (YVel == 1 + gravity)
                    currentFrame = new Point(1, 3);
                else if (currentFrame == new Point(6, 3))
                    animatedTexture = false;
                else
                    animatedTexture = true;
            }
            else if (XVel == 0)
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

        // Handles all input and updates velocity accordingly
        void HandleInputAndUpdateVelocity()
        {
            // Getting new input states
            GamePadState newGamepadState = GamePad.GetState(PlayerIndex.One);
            KeyboardState newKeyboardState = Keyboard.GetState();

            // Handling X velocity, left/right input, dashing
            float inputVel = 0;

            if (newKeyboardState.IsKeyDown(Keys.Left) || newKeyboardState.IsKeyDown(Keys.A))
                inputVel = -xAcceleration;
            if (newKeyboardState.IsKeyDown(Keys.Right) || newKeyboardState.IsKeyDown(Keys.D))
                inputVel = xAcceleration;

            if (newGamepadState.ThumbSticks.Left.X < -0.05)
                inputVel = -xAcceleration;
            if (newGamepadState.ThumbSticks.Left.X > 0.05)
                inputVel = xAcceleration;

            if (inputVel == 0)                                  // Decelerates player when no left/right input is given-
            {
                if (XVel > 0)
                {
                    XVel -= xDeceleration;
                    if (XVel < 0)
                        XVel = 0;
                }
                if (XVel < 0)
                {
                    XVel += xDeceleration;
                    if (XVel > 0)
                        XVel = 0;
                }
                if (Math.Abs(XVel) < 0.08f)                     // Round small fractions to 0
                    XVel = 0;
            }
            else
                XVel += inputVel;

            if ((newKeyboardState.IsKeyDown(Keys.LeftShift) || newGamepadState.Triggers.Right > 0.05f)  // Dash handling
                && IsOnTheGround && inputVel != 0)
            {
                XVel = MathHelper.Clamp(XVel, 1.75f * -xMaxVelocity, 1.75f * xMaxVelocity);
                millisecondsPerFrame = 30;
            }
            else
            {
                XVel = MathHelper.Clamp(XVel, -xMaxVelocity, xMaxVelocity);
                millisecondsPerFrame = 50;
            }

            // Handling Y velocity and jumping
            if ((newKeyboardState.IsKeyUp(Keys.Space) && newGamepadState.Buttons.A == ButtonState.Released && jumpCounter > minJumpVelFrames)
                || jumpCounter > maxJumpVelFrames)
            {
                isJumping = false;
                jumpCounter = 1;
            }
            else if (isJumping)
            {
                YVel += jumpInitialVelocity / (jumpCurveFactor * jumpCounter);      // Each additional frame jump is held gives less up velocity based on jumpCounter and jumpCurveFactor
                ++jumpCounter;
            }
            
            if ((newKeyboardState.IsKeyDown(Keys.Space) && oldKeyboardState.IsKeyUp(Keys.Space)) ||
                (newGamepadState.Buttons.A == ButtonState.Pressed && oldGamepadState.Buttons.A == ButtonState.Released))
                if (IsOnTheGround)
                {
                    YVel = jumpInitialVelocity;
                    isJumping = true;
                }
            
            if (YVel != 0)                                      // Currently prevents jumping from setting YVel to 1
                IsOnTheGround = false;

            if (IsOnTheGround)                                  // If ground collision last update
                YVel = 1;                                       // Start falling again to detect walking off ledge or slope
            else
                YVel += gravity;

            if (YVel > terminalVelocity)
                YVel = terminalVelocity;

            // Shooting
            if (PulledTheTrigger == true)
                PulledTheTrigger = false;
            if ((newKeyboardState.IsKeyDown(Keys.RightControl) && oldKeyboardState.IsKeyUp(Keys.RightControl)) ||
                (newGamepadState.Buttons.X == ButtonState.Pressed && oldGamepadState.Buttons.X == ButtonState.Released))
                PulledTheTrigger = true;

            // Turn on/off diagnostic modes
            if ((newKeyboardState.IsKeyDown(Keys.D1) && oldKeyboardState.IsKeyUp(Keys.D1)) ||
                (newGamepadState.Buttons.LeftShoulder == ButtonState.Pressed && oldGamepadState.Buttons.LeftShoulder == ButtonState.Released))
                ShowBoundingBoxes = !ShowBoundingBoxes;

            if ((newKeyboardState.IsKeyDown(Keys.D2) && oldKeyboardState.IsKeyUp(Keys.D2)) ||
                (newGamepadState.Buttons.RightShoulder == ButtonState.Pressed && oldGamepadState.Buttons.RightShoulder == ButtonState.Released))
                ShowTerrainBoxes = !ShowTerrainBoxes;

            // Updating old input states
            oldGamepadState = newGamepadState;
            oldKeyboardState = newKeyboardState;
        }

        // For diagnostic use - only called when ShowTerrainBoxes = true
        protected override void DrawTerrainBoxes(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(boxTexture, FrontBox, null, Color.Yellow, 0, Vector2.Zero, SpriteEffects.None, 0.65f);
            spriteBatch.Draw(boxTexture, BackBox, null, Color.Yellow, 0, Vector2.Zero, SpriteEffects.None, 0.65f);
            spriteBatch.Draw(boxTexture, BackOfHeadBox, null, Color.Yellow, 0, Vector2.Zero, SpriteEffects.None, 0.65f);
            spriteBatch.Draw(boxTexture, FeetBox, null, Color.Green, 0, Vector2.Zero, SpriteEffects.None, 0.64f);
            spriteBatch.Draw(boxTexture, HeadBox, null, Color.Cyan, 0, Vector2.Zero, SpriteEffects.None, 0.63f);
            spriteBatch.Draw(boxTexture, ShouldersBox, null, Color.Cyan, 0, Vector2.Zero, SpriteEffects.None, 0.63f);
        }
    }
}
