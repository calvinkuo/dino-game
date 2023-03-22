using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using static System.Formats.Asn1.AsnWriter;
using static System.Net.WebRequestMethods;

namespace mg1_calvinkuo
{
    abstract class SpriteBox
    {
        public Vector2 pos;
        public abstract Texture2D SpriteSheet { get; set; }
        public abstract Rectangle SpriteSheetCoords { get; }
        public float X { get { return pos.X; } }
        public float Y { get { return pos.Y; } }
        public float Width { get { return SpriteSheetCoords.Width; } }
        public float Height { get { return SpriteSheetCoords.Height; } }
        public Rectangle Rectangle { get { return new Rectangle((int)pos.X, (int)pos.Y, (int)(Width * Game1.scale.X), (int)(Height * Game1.scale.Y)); } }

        public SpriteBox(Vector2 pos)
        {
            this.pos = pos;
        }
        
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(SpriteSheet, pos, SpriteSheetCoords, Color.White, 0f, new Vector2(0, 0), Game1.scale, SpriteEffects.None, 0f);
        }

        public abstract Point HandleCollision(Rectangle other, ref Vector2 veloc, ref bool jumping);
        public abstract void Update();
    }

    abstract class Platform : SpriteBox
    {
        protected Platform(Vector2 pos) : base(pos)
        {
        }

        public override void Update()
        {
        }
    }

    class WoodenPlatform : Platform
    {
        public WoodenPlatform(Vector2 pos) : base(pos)
        {
        }

        public override Point HandleCollision(Rectangle other, ref Vector2 veloc, ref bool jumping)
        {
            var rect = this.Rectangle;
            if (other.Intersects(rect))
            {
                var gapRight = other.Left - rect.Right;
                var gapLeft = rect.Left - other.Right;
                var gapTop = rect.Top - other.Bottom;
                var gapBottom = other.Top - rect.Bottom;
                // System.Diagnostics.Debug.WriteLine($"{gapRight}, {gapLeft}, {gapTop}, {gapBottom}");

                var minMovement = gapTop;
                foreach (int i in new int[] { gapRight, gapLeft, gapTop, gapBottom })
                {
                    if (i < 0 && i > minMovement)
                    {
                        minMovement = i;
                    }
                }
                if (minMovement == gapRight)
                {
                    other.X = rect.Right;
                }
                if (minMovement == gapLeft)
                {
                    other.X = rect.Left - other.Width;
                }
                if (minMovement == gapTop)
                {
                    other.Y = rect.Top - other.Height;
                    veloc.Y = 0f;
                    jumping = false;
                }
                if (minMovement == gapBottom)
                {
                    other.Y = rect.Bottom;
                    veloc.Y = 0f;
                }
            }
            return other.Location;
        }

        private static Texture2D spriteSheet;
        public override Texture2D SpriteSheet {
            get { return spriteSheet; }
            set { spriteSheet = value; }
        }
        private static Rectangle spriteLoc = new Rectangle(272, 16, 48, 5);

        public override Rectangle SpriteSheetCoords {
            get { return spriteLoc; }
        }
    }

    class StonePlatform : Platform
    {
        public StonePlatform(Vector2 loc) : base(loc)
        {
        }


        public override Point HandleCollision(Rectangle other, ref Vector2 veloc, ref bool jumping)
        {
            var rect = this.Rectangle;
            if (other.Intersects(rect))
            {
                var gapTop = rect.Top - other.Bottom;
                var gapBottom = other.Top - rect.Bottom;
                // System.Diagnostics.Debug.WriteLine($"{gapTop}, {gapBottom}");

                var minMovement = gapTop;
                foreach (int i in new int[] { gapTop, gapBottom })
                {
                    if (i < 0 && i > minMovement)
                    {
                        minMovement = i;
                    }
                }
                if (minMovement == gapTop && veloc.Y > 0)
                {
                    other.Y = rect.Top - other.Height;
                    veloc.Y = 0f;
                    jumping = false;
                }
            }
            return other.Location;
        }

        private static Texture2D spriteSheet;
        public override Texture2D SpriteSheet
        {
            get { return spriteSheet; }
            set { spriteSheet = value; }
        }
        private static Rectangle spriteLoc = new Rectangle(272, 32, 48, 5);
        public override Rectangle SpriteSheetCoords
        {
            get { return spriteLoc; }
        }
    }

    class Box : SpriteBox
    {
        public Box(Vector2 pos) : base(pos)
        {
        }

        private static Texture2D spriteSheet;
        public override Texture2D SpriteSheet
        {
            get { return spriteSheet; }
            set { spriteSheet = value; }
        }
        private static Rectangle spriteLoc = new Rectangle(192, 144, 16, 16);
        public override Rectangle SpriteSheetCoords
        {
            get { return spriteLoc; }
        }

        public Vector2 veloc = new Vector2(0f, 0f);
        private Vector2 accel = new Vector2(0f, 1f);
        public bool jumping = false;
        public bool hitTop = false;

        public override Point HandleCollision(Rectangle other, ref Vector2 veloc, ref bool jumping)
        {
            var rect = this.Rectangle;
            if (other.Intersects(rect))
            {
                var gapRight = other.Left - rect.Right;
                var gapLeft = rect.Left - other.Right;
                var gapTop = rect.Top - other.Bottom;
                var gapBottom = other.Top - rect.Bottom;
                // System.Diagnostics.Debug.WriteLine($"{gapRight}, {gapLeft}, {gapTop}, {gapBottom}");

                var minMovement = gapRight;
                foreach (int i in new int[] { gapRight, gapLeft, gapTop, gapBottom })
                {
                    if (i < 0 && i > minMovement)
                    {
                        minMovement = i;
                    }
                }
                if (minMovement == gapRight)
                {
                    this.pos.X = other.Left - this.Rectangle.Width;
                    this.veloc.X = 0f;
                }
                if (minMovement == gapLeft)
                {
                    this.pos.X = other.Right;
                    this.veloc.X = 0f;
                }
                if (minMovement == gapTop)
                {
                    other.Y = rect.Top - other.Height;
                    veloc.Y = 0f;
                    jumping = false;
                }
                if (minMovement == gapBottom)
                {
                    if (veloc.Y >= 0)
                    {
                        hitTop = true;
                        System.Diagnostics.Debug.WriteLine($"hitTop!");
                    }
                    else
                    {
                        other.Y = rect.Bottom;
                        veloc.Y = 1f;
                    }
                }
            }
            return other.Location;
        }
        public Point HandleCollision2(Rectangle other, ref Vector2 veloc, ref bool jumping)
        {
            var rect = this.Rectangle;
            if (other.Intersects(rect))
            {
                var gapRight = other.Left - rect.Right;
                var gapLeft = rect.Left - other.Right;
                var gapTop = rect.Top - other.Bottom;
                var gapBottom = other.Top - rect.Bottom;
                // System.Diagnostics.Debug.WriteLine($"{gapRight}, {gapLeft}, {gapTop}, {gapBottom}");

                var minMovement = gapTop;
                foreach (int i in new int[] { gapRight, gapLeft, gapTop, gapBottom })
                {
                    if (i < 0 && i > minMovement)
                    {
                        minMovement = i;
                    }
                }
                if (minMovement == gapRight)
                {
                    other.X = rect.Right;
                }
                if (minMovement == gapLeft)
                {
                    other.X = rect.Left - other.Width;
                }
                if (minMovement == gapTop)
                {
                    other.Y = rect.Top - other.Height;
                    veloc.Y = 0f;
                    jumping = false;
                }
                if (minMovement == gapBottom)
                {
                    other.Y = rect.Bottom;
                    veloc.Y = 0f;
                }
            }
            return other.Location;
        }

        public override void Update()
        {
            pos += veloc;
            veloc += accel;
            if (pos.Y > Game1.GameBounds.Y - (Height * Game1.scale.Y))
            {
                pos.Y = Game1.GameBounds.Y - (Height * Game1.scale.Y);
                veloc.Y = 0f;
                jumping = false;
            }

            // respawn if it goes off-screen
            var rect = Rectangle;
            if (pos.X < -rect.Width * 0.75f || rect.Right > Game1.GameBounds.X + rect.Width * 0.75f)
            {
                pos = new Vector2(350, -100);
                veloc.X = veloc.Y = 0f;
            }
        }
    }

    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        public static Point GameBounds = new Point(1280, 720); //window resolution

        private float timer = 0f;
        private int threshold = (int)(150f / speed);
        private Vector2 pos = new Vector2(0f, 0f);
        private Vector2 veloc = new Vector2(0f, 0f);
        private Vector2 accel = new Vector2(0f, 1f);
        private bool canJump = true;
        private bool flip = false;
        private bool jumping = false;
        private bool kicking = false;
        public static Vector2 scale = new Vector2(4, 4);
        private const float speed = 1.5f;
        private Random rand = new Random();

        private SpriteBox[] gameObjects;
        private Texture2D dinoSheet;
        private Texture2D terrainSheet;
        private Animation currentAnimation = Animation.Idle;
        private Animation? nextAnimation = null;
        private int currentAnimationIndex = 0;
        private Rectangle[][] animation = new Rectangle[][] {
            new Rectangle[4],
            new Rectangle[6],
            new Rectangle[4],
            new Rectangle[3],
            new Rectangle[1],
            new Rectangle[6],
        };

        private enum Animation
        {
            Idle,
            Walk,
            Kick,
            Hurt,
            DashStart,
            Dash,
        }

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = GameBounds.X;
            _graphics.PreferredBackBufferHeight = GameBounds.Y;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
            pos.X = GameBounds.X / 2 - (12 * scale.X);
            pos.Y = GameBounds.Y - (21 * scale.Y);
            gameObjects = new SpriteBox[] {
                new Box(new Vector2(350, 200)),
                new StonePlatform(new Vector2(300, 250)),
                new StonePlatform(new Vector2(900, 250)),
                new WoodenPlatform(new Vector2(600, 300)),
                new WoodenPlatform(new Vector2(100, 450)),
                new StonePlatform(new Vector2(500, 500)),
                new WoodenPlatform(new Vector2(950, 450)),
                new WoodenPlatform(new Vector2(200, 675)),
                new StonePlatform(new Vector2(750, 675)),
            };
            foreach (SpriteBox spriteBox in gameObjects)
            {
                spriteBox.SpriteSheet = terrainSheet;
            }
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            
            dinoSheet = Content.Load<Texture2D>("Images/vita");
            terrainSheet = Content.Load<Texture2D>("Images/terrain");
            int i = 0, j;
            foreach (Rectangle[] anim in animation)
            {
                j = 0;
                for (; j < anim.Length; i++, j++)
                {
                    anim[j] = new Rectangle(24 * i, 0, 24, 24);
                }
            }
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            if (Keyboard.GetState().IsKeyUp(Keys.W))
            {
                canJump = true;
            }

            if (!jumping && !kicking && currentAnimation != Animation.Kick && Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                kicking = true;
                currentAnimation = Animation.Kick;
                nextAnimation = Animation.Idle;
                currentAnimationIndex = 0;
            }
            if (kicking && Keyboard.GetState().IsKeyUp(Keys.Space))
            {
                kicking = false;
            }
            if (currentAnimation != Animation.Kick && currentAnimation != Animation.Hurt)
            {
                if (Keyboard.GetState().IsKeyUp(Keys.LeftShift) && Keyboard.GetState().IsKeyDown(Keys.A))
                {
                    if (!jumping)
                        currentAnimation = Animation.Walk;
                    flip = true;
                    pos.X -= (float)gameTime.ElapsedGameTime.TotalMilliseconds * 0.15f * speed;
                    veloc.X = 0f;
                    nextAnimation = null;
                }
                if (Keyboard.GetState().IsKeyDown(Keys.LeftShift) && Keyboard.GetState().IsKeyDown(Keys.A))
                {
                    if (!jumping)
                        currentAnimation = Animation.Dash;
                    flip = true;
                    pos.X -= (float)gameTime.ElapsedGameTime.TotalMilliseconds * 0.45f * speed;
                    if (veloc.X > 0f)
                        veloc.X = 0f;
                    veloc.X -= (float)gameTime.ElapsedGameTime.TotalMilliseconds * 0.01f * speed;
                    nextAnimation = null;
                }
                if (Keyboard.GetState().IsKeyUp(Keys.LeftShift) && Keyboard.GetState().IsKeyDown(Keys.D))
                {
                    if (!jumping)
                        currentAnimation = Animation.Walk;
                    flip = false;
                    pos.X += (float)gameTime.ElapsedGameTime.TotalMilliseconds * 0.15f * speed;
                    veloc.X = 0f;
                    nextAnimation = null;
                }
                if (Keyboard.GetState().IsKeyDown(Keys.LeftShift) && Keyboard.GetState().IsKeyDown(Keys.D))
                {
                    if (!jumping)
                        currentAnimation = Animation.Dash;
                    flip = false;
                    pos.X += (float)gameTime.ElapsedGameTime.TotalMilliseconds * 0.45f * speed;
                    if (veloc.X < 0f)
                        veloc.X = 0f;
                    veloc.X += (float)gameTime.ElapsedGameTime.TotalMilliseconds * 0.01f * speed;
                    nextAnimation = null;
                }
                if (!jumping && !kicking && (currentAnimation == Animation.Walk || currentAnimation == Animation.Dash) && Keyboard.GetState().IsKeyUp(Keys.D) && Keyboard.GetState().IsKeyUp(Keys.A))
                {
                    currentAnimation = Animation.Idle;
                    nextAnimation = null;
                    veloc.X = 0f;
                }
                if (canJump && !jumping && !kicking && Keyboard.GetState().IsKeyDown(Keys.W))
                {
                    canJump = false;
                    jumping = true;
                    currentAnimation = Animation.Dash;
                    nextAnimation = null;
                    veloc.Y = -25f;
                }
            }

            // Bounds checking
            pos.X += veloc.X;
            veloc.X += accel.X;
            pos.Y += veloc.Y;
            veloc.Y += accel.Y;
            if (pos.X < (-5 * scale.X))
            {
                pos.X = (-5 * scale.X);
                if (currentAnimation == Animation.Dash || currentAnimation == Animation.Walk)
                {
                    currentAnimation = Animation.Hurt;
                    nextAnimation = Animation.Idle;
                    currentAnimationIndex = 0;
                }
            }
            if (pos.X > GameBounds.X - ((24 - 5) * scale.X))
            {
                pos.X = GameBounds.X - ((24 - 5) * scale.X);
                if (currentAnimation == Animation.Dash || currentAnimation == Animation.Walk)
                {
                    currentAnimation = Animation.Hurt;
                    nextAnimation = Animation.Idle;
                    currentAnimationIndex = 0;
                }
            }
            if (pos.Y > GameBounds.Y - ((24 - 3) * scale.Y))
            {
                pos.Y = GameBounds.Y - ((24 - 3) * scale.Y);
                veloc.Y = 0f;
                jumping = false;
            }

            if (timer > threshold)
            {
                currentAnimationIndex += 1;
                timer = 0f;
            }
            if (currentAnimationIndex >= animation[(int)currentAnimation].Length)
            {
                if (nextAnimation != null)
                {
                    currentAnimation = (Animation)nextAnimation;
                    nextAnimation = null;
                }
                currentAnimationIndex = 0;
            }

            timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            // player collision
            foreach (var gameObject in gameObjects)
            {
                gameObject.Update();
                var newPos = gameObject.HandleCollision(new Rectangle((int)(pos.X + 8 * scale.X), (int)(pos.Y + 6 * scale.X), (int)(8 * scale.X), (int)(15 * scale.Y)), ref veloc, ref jumping);
                pos.X = newPos.X - 8 * scale.X;
                pos.Y = newPos.Y - 6 * scale.X;
            }

            // box landing on player
            Box box = (Box)gameObjects[0];
            if (box.hitTop)
            {
                currentAnimation = Animation.Hurt;
                nextAnimation = Animation.Idle;
                currentAnimationIndex = 0;
            }

            // kick the box
            if (kicking && !flip && Math.Abs(box.Rectangle.Left - (pos.X + 16 * scale.X)) < 1 && Math.Abs(box.Rectangle.Bottom - (pos.Y + 21 * scale.Y)) < 3)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.LeftShift))
                {
                    box.veloc.X += 8f;
                }
                else
                {
                    box.veloc.X += 4f;
                }
            }
            if (kicking && flip && Math.Abs(box.Rectangle.Right - (pos.X + 8 * scale.X)) < 1 && Math.Abs(box.Rectangle.Bottom - (pos.Y + 21 * scale.Y)) < 3)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.LeftShift))
                {
                    box.veloc.X -= 8f;
                }
                else
                {
                    box.veloc.X -= 4f;
                }
            }

            // box collision
            foreach (var gameObject in gameObjects[1..])
            {
                var newPos = gameObject.HandleCollision(box.Rectangle, ref box.veloc, ref box.jumping);
                box.pos.X = newPos.X;
                box.pos.Y = newPos.Y;
            }
            // ensure player does not clip box
            if (!box.hitTop) {
                var newPos = box.HandleCollision2(new Rectangle((int)(pos.X + 8 * scale.X), (int)(pos.Y + 6 * scale.X), (int)(8 * scale.X), (int)(15 * scale.Y)), ref veloc, ref jumping);
                pos.X = newPos.X - 8 * scale.X;
                pos.Y = newPos.Y - 6 * scale.X;
            }
            box.hitTop = false;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            foreach (var gameObject in gameObjects[1..])
            {
                gameObject.Draw(_spriteBatch);
            }
            foreach (var gameObject in gameObjects[..1]) // draw box last
            {
                gameObject.Draw(_spriteBatch);
            }
            _spriteBatch.Draw(dinoSheet, pos, animation[(int)currentAnimation][currentAnimationIndex], Color.White, 0f, new Vector2(0, 0), scale,
                flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}