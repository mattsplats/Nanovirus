using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNA2DGame
{
    class TestStepBox : Sprite
    {
        // Step box texture
        static public Texture2D StepBoxImage;
        
        // Constructors
        public TestStepBox(Vector2 position)
            : this(position, Color.White)
        { }

        public TestStepBox(Vector2 position, Color tint)
            : base(StepBoxImage, position, 0.3f, Vector2.Zero, tint)
        {
            Foreground.wallBoxList.Add(new Rectangle((int)XPos + 1, (int)YPos + 2, 2, 10));
            Foreground.wallBoxList.Add(new Rectangle((int)XPos + 26, (int)YPos + 2, 2, 10));
            Foreground.floorBoxList.Add(new Rectangle((int)XPos + 1, (int)YPos + 1, 27, 3));
            Foreground.ceilingBoxList.Add(new Rectangle((int)XPos + 1, (int)YPos + 10, 27, 3));
        }

        // Properties
        public override Rectangle BoundingBox { get { return Rectangle.Empty; } }
    }
}
