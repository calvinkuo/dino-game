using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using System.Xml;
using static System.Formats.Asn1.AsnWriter;
using static System.Net.WebRequestMethods;

namespace mg1_calvinkuo
{
    class LevelData
    {
        public LevelDataBox[] boxes { get; set; }
        public LevelDataPlatform[] platforms { get; set; }
        public LevelDataFood[] food { get; set; }
    }

    class LevelDataBox
    {
        public int x { get; set; }
        public int y { get; set; }
        public int respawnX { get; set; }
        public int respawnY { get; set; }
    }

    class LevelDataPlatform
    {
        public string type { get; set; }
        public int x { get; set; }
        public int y { get; set; }
    }

    class LevelDataFood
    {
        public int id { get; set; }
        public int x { get; set; }
        public int y { get; set; }
    }

    abstract class SpriteBox
    {
        public Vector2 pos;
        public abstract Texture2D SpriteSheet { get; set; }
        public abstract Rectangle SpriteSheetCoords { get; }
        public abstract float Mass { get; }
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

        public abstract Point HandleCollision(Rectangle other, ref Vector2 veloc, ref bool jumping, float otherMass);
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

        public override float Mass { get { return (float)double.PositiveInfinity; } }
    }

    class WoodenPlatform : Platform
    {
        public WoodenPlatform(Vector2 pos) : base(pos)
        {
        }

        public override Point HandleCollision(Rectangle other, ref Vector2 veloc, ref bool jumping, float otherMass)
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


        public override Point HandleCollision(Rectangle other, ref Vector2 veloc, ref bool jumping, float otherMass)
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
        public Box(Vector2 pos, Vector2 respawnPos) : base(pos)
        {
            RespawnPos = respawnPos;
        }
        public override float Mass { get { return 1f; } }

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
        public Vector2 RespawnPos { get; set; }

        public override Point HandleCollision(Rectangle other, ref Vector2 veloc, ref bool jumping, float otherMass)
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
                    if (otherMass == this.Mass)
                    {
                        veloc.X = this.veloc.X = (veloc.X + this.veloc.X * this.Mass) / 2;
                        veloc.Y = this.veloc.Y = (veloc.Y + this.veloc.Y * this.Mass) / 2;
                    }
                }
                if (minMovement == gapLeft)
                {
                    this.pos.X = other.Right;
                    if (otherMass == this.Mass)
                    {
                        veloc.X = this.veloc.X = (veloc.X + this.veloc.X * this.Mass) / 2;
                        veloc.Y = this.veloc.Y = (veloc.Y + this.veloc.Y * this.Mass) / 2;
                    }
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
                        // System.Diagnostics.Debug.WriteLine($"hitTop!");
                    }
                    else
                    {
                        other.Y = rect.Bottom;
                        // veloc.Y = 1f;
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
                pos.X = RespawnPos.X;
                pos.Y = RespawnPos.Y;
                veloc.X = veloc.Y = 0f;
                //Game1.score += 1;
                //System.Diagnostics.Debug.WriteLine($"Respawning block at ({pos.X}, {pos.Y})");
            }
        }
    }

    abstract class Food : SpriteBox
    {
        protected static HashSet<int> collected = new HashSet<int>();

        public Food(int id, Vector2 pos) : base(pos)
        {
            this.id = id;
            if (collected.Contains(id))
            {
                this.pos.X = -1000;
            }
        }
        public override float Mass { get { return 0f; } }
        public int id;

        private static Texture2D spriteSheet;
        public override Texture2D SpriteSheet
        {
            get { return spriteSheet; }
            set { spriteSheet = value; }
        }

        public override Point HandleCollision(Rectangle other, ref Vector2 veloc, ref bool jumping, float otherMass)
        {
            var rect = this.Rectangle;
            if (other.Intersects(rect))
            {
                collected.Add(id);
                Game1.score += 1;
                this.pos.X = -1000;
            }
            return other.Location;
        }

        public override void Update()
        {
        }
    }

    class Pudding : Food
    {
        public Pudding(int id, Vector2 pos) : base(id, pos)
        {
        }

        private static Rectangle spriteLoc = new Rectangle(9, 9, 14, 14);
        public override Rectangle SpriteSheetCoords
        {
            get { return spriteLoc; }
        }

        public override Point HandleCollision(Rectangle other, ref Vector2 veloc, ref bool jumping, float otherMass)
        {
            var rect = this.Rectangle;
            if (other.Intersects(rect))
            {
                collected.Add(id);
                Game1.score += 1;
                this.pos.X = -1000;
            }
            return other.Location;
        }
    }
    class PuddingLarge : Food
    {
        private static bool isCollected = false;
        public static bool Collected { get { return isCollected; } }

        public PuddingLarge(int id, Vector2 pos) : base(id, pos)
        {
            if (Game1.score < Game1.maxScore)
            {
                this.pos.X = -1000;
            }
        }

        private static Rectangle spriteLoc = new Rectangle(39, 7, 18, 18);
        public override Rectangle SpriteSheetCoords
        {
            get { return spriteLoc; }
        }

        public override Point HandleCollision(Rectangle other, ref Vector2 veloc, ref bool jumping, float otherMass)
        {
            var rect = this.Rectangle;
            if (other.Intersects(rect))
            {
                collected.Add(id);
                Game1.score += 1;
                this.pos.X = -1000;
                isCollected = true;
            }
            return other.Location;
        }
    }

    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private const int dpi = 1;
        public static Point GameBounds = new Point(1280 * dpi, 720 * dpi); //window resolution

        private float timer = 0f;
        private int threshold = (int)(150f / speed);
        private static Vector2 pos = new Vector2(0f, 0f);
        public static Vector2 veloc = new Vector2(0f, 0f);
        private Vector2 accel = new Vector2(0f, 1f);
        private bool canJump = true;
        private bool flip = false;
        private bool jumping = false;
        private bool kicking = false;
        public static Vector2 scale = new Vector2(4 * dpi, 4 * dpi);
        private const float speed = 1.5f;
        private int currentLevel = 1;
        private const int maxLevel = 5;
        public static int score = 0;
        public static int maxScore = 30;

        private Box[] boxes;
        private Food[] food;
        private SpriteBox[] gameObjects;
        private Texture2D dinoSheet;
        private Texture2D terrainSheet;
        private Texture2D foodSheet;
        private SpriteFont font;
        private Texture2D solid;
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
            _graphics.PreferredBackBufferHeight = GameBounds.Y + 96;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
            ResetPosition();
            LoadLevel(currentLevel);
        }
        
        private void ResetPosition()
        {
            pos.X = GameBounds.X / 2 - (12 * scale.X);
            pos.Y = GameBounds.Y - (21 * scale.Y);
        }

        private void LoadLevel(int levelNum)
        {
            if (levelNum < 1)
                currentLevel = maxLevel;
            else if (levelNum > maxLevel)
                currentLevel = 1;
            else
                currentLevel = levelNum;
            System.Diagnostics.Debug.WriteLine($"Entering level {currentLevel}");

            LevelData levelData = JsonSerializer.Deserialize<LevelData>(System.IO.File.ReadAllText(Content.RootDirectory + $"/levels/level-{currentLevel}.json"));
            boxes = new Box[levelData.boxes.Length];
            gameObjects = new SpriteBox[levelData.platforms.Length];
            food = new Food[levelData.food.Length];
            int i = 0;
            foreach (LevelDataBox levelDataBox in levelData.boxes)
            {
                Box box = new Box(new Vector2(levelDataBox.x, levelDataBox.y), new Vector2(levelDataBox.respawnX, levelDataBox.respawnY));
                boxes[i] = box;
                boxes[i].SpriteSheet = terrainSheet;
                i++;
            }
            i = 0;
            foreach (LevelDataFood levelDataFood in levelData.food)
            {
                if (levelDataFood.id > 0)
                {
                    food[i] = new Pudding(levelDataFood.id, new Vector2(levelDataFood.x, levelDataFood.y));
                }
                else
                {
                    food[i] = new PuddingLarge(levelDataFood.id, new Vector2(levelDataFood.x, levelDataFood.y));
                }
                food[i].SpriteSheet = foodSheet;
                i++;
            }
            i = 0;
            foreach (LevelDataPlatform levelDataPlatform in levelData.platforms)
            {
                if (levelDataPlatform.type == "Stone")
                    gameObjects[i] = new StonePlatform(new Vector2(levelDataPlatform.x, levelDataPlatform.y));
                else if (levelDataPlatform.type == "Wooden")
                    gameObjects[i] = new WoodenPlatform(new Vector2(levelDataPlatform.x, levelDataPlatform.y));
                gameObjects[i].SpriteSheet = terrainSheet;
                i++;
            }
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            
            dinoSheet = Content.Load<Texture2D>("Images/vita");
            terrainSheet = Content.Load<Texture2D>("Images/terrain");
            foodSheet = Content.Load<Texture2D>("Images/pudding");
            font = Content.Load<SpriteFont>("Fonts/Pixellari");
            int i = 0, j;
            foreach (Rectangle[] anim in animation)
            {
                j = 0;
                for (; j < anim.Length; i++, j++)
                {
                    anim[j] = new Rectangle(24 * i, 0, 24, 24);
                }
            }
            solid = new Texture2D(GraphicsDevice, 1, 1);
            solid.SetData(new[] { Color.White });
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if ((Keyboard.GetState().IsKeyDown(Keys.D1) || Keyboard.GetState().IsKeyDown(Keys.NumPad1))&& currentLevel != 1)
            {
                LoadLevel(1);
            }
            else if ((Keyboard.GetState().IsKeyDown(Keys.D2) || Keyboard.GetState().IsKeyDown(Keys.NumPad2)) && currentLevel != 2)
            {
                LoadLevel(2);
            }
            else if ((Keyboard.GetState().IsKeyDown(Keys.D3) || Keyboard.GetState().IsKeyDown(Keys.NumPad3)) && currentLevel != 3)
            {
                LoadLevel(3);
            }
            else if ((Keyboard.GetState().IsKeyDown(Keys.D4) || Keyboard.GetState().IsKeyDown(Keys.NumPad4)) && currentLevel != 4)
            {
                LoadLevel(4);
            }
            else if ((Keyboard.GetState().IsKeyDown(Keys.D5) || Keyboard.GetState().IsKeyDown(Keys.NumPad5)) && currentLevel != 5)
            {
                LoadLevel(5);
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.D1) || Keyboard.GetState().IsKeyDown(Keys.NumPad0))
            {
                ResetPosition();
            }

            //if (Keyboard.GetState().IsKeyDown(Keys.R))
            //{
            //    System.Diagnostics.Debug.WriteLine($"Report for level {currentLevel}");
            //    foreach (var b in food)
            //    {
            //        System.Diagnostics.Debug.WriteLine($"{b.pos.X} {b.pos.Y}");
            //    }
            //}
            //if (Keyboard.GetState().IsKeyDown(Keys.OemPlus))
            //{
            //    score += 1;
            //}

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
            if (currentAnimation != Animation.Kick)
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
            if (pos.X < -((24 - 5) * scale.X)) // walk off left edge
            {   
                LoadLevel(currentLevel - 1);
                pos.X = Game1.GameBounds.X + (-5 * scale.X);
            }
            else if (pos.X > Game1.GameBounds.X + (-5 * scale.X)) // walk off right edge
            {
                LoadLevel(currentLevel + 1);
                pos.X = -((24 - 5) * scale.X);
                //pos = new Vector2(350, -100);
                //veloc.X = veloc.Y = 0f;
                //jumping = true;
            }
            //if (pos.X < (-5 * scale.X))
            //{
            //    pos.X = (-5 * scale.X);
            //    if (currentAnimation == Animation.Dash || currentAnimation == Animation.Walk)
            //    {
            //        currentAnimation = Animation.Hurt;
            //        nextAnimation = Animation.Idle;
            //        currentAnimationIndex = 0;
            //    }
            //}
            //if (pos.X > GameBounds.X - ((24 - 5) * scale.X))
            //{
            //    pos.X = GameBounds.X - ((24 - 5) * scale.X);
            //    if (currentAnimation == Animation.Dash || currentAnimation == Animation.Walk)
            //    {
            //        currentAnimation = Animation.Hurt;
            //        nextAnimation = Animation.Idle;
            //        currentAnimationIndex = 0;
            //    }
            //}
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
                    if (nextAnimation == Animation.Idle)
                        accel.X = veloc.X = veloc.Y = 0f;
                }
                currentAnimationIndex = 0;
            }

            timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            // player collision
            Array.Sort<Box>(boxes, new DistanceComparer()); // need to do collision in order of distance from player to ensure blocks push each other
            foreach (var gameObject in boxes)
            {
                gameObject.Update();
                var newPos = gameObject.HandleCollision(new Rectangle((int)(pos.X + 8 * scale.X), (int)(pos.Y + 6 * scale.X), (int)(8 * scale.X), (int)(15 * scale.Y)), ref veloc, ref jumping, (float)double.PositiveInfinity);
                pos.X = newPos.X - 8 * scale.X;
                pos.Y = newPos.Y - 6 * scale.X;
            }
            foreach (var gameObject in gameObjects)
            {
                gameObject.Update();
                var newPos = gameObject.HandleCollision(new Rectangle((int)(pos.X + 8 * scale.X), (int)(pos.Y + 6 * scale.X), (int)(8 * scale.X), (int)(15 * scale.Y)), ref veloc, ref jumping, (float)double.PositiveInfinity);
                pos.X = newPos.X - 8 * scale.X;
                pos.Y = newPos.Y - 6 * scale.X;
            }
            foreach (var gameObject in food)
            {
                gameObject.Update();
                var newPos = gameObject.HandleCollision(new Rectangle((int)(pos.X + 6 * scale.X), (int)(pos.Y + 4 * scale.X), (int)(12 * scale.X), (int)(19 * scale.Y)), ref veloc, ref jumping, (float)double.PositiveInfinity);
                pos.X = newPos.X - 6 * scale.X;
                pos.Y = newPos.Y - 4 * scale.X;
            }

            foreach (Box box in boxes)
            {
                // box landing on player
                if (currentAnimation != Animation.Hurt && box.hitTop)
                {
                    currentAnimation = Animation.Hurt;
                    nextAnimation = Animation.Idle;
                    currentAnimationIndex = 0;
                    accel.X = veloc.X = 0f;
                    kicking = false;
                }

                // kick the box
                if (kicking && !flip && Math.Abs(box.Rectangle.Left - (pos.X + 16 * scale.X)) < 1 &&
                    (pos.Y + 21 * scale.Y) > box.Rectangle.Top - 1 && (pos.Y + 21 * scale.Y) < box.Rectangle.Bottom + 1)
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
                if (kicking && flip && Math.Abs(box.Rectangle.Right - (pos.X + 8 * scale.X)) < 1 &&
                    (pos.Y + 21 * scale.Y) > box.Rectangle.Top - 1 && (pos.Y + 21 * scale.Y) < box.Rectangle.Bottom + 1)
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

                // box collision with platforms
                foreach (var gameObject in gameObjects)
                {
                    if (box.veloc.X > 0 || box.veloc.Y > 0)
                    {
                        var newPos = gameObject.HandleCollision(box.Rectangle, ref box.veloc, ref box.jumping, box.Mass);
                        box.pos.X = newPos.X;
                        box.pos.Y = newPos.Y;
                    }
                }

                // ensure player does not clip box
                if (!box.hitTop)
                {
                    var newPos = box.HandleCollision2(new Rectangle((int)(pos.X + 8 * scale.X), (int)(pos.Y + 6 * scale.X), (int)(8 * scale.X), (int)(15 * scale.Y)), ref veloc, ref jumping);
                    pos.X = newPos.X - 8 * scale.X;
                    pos.Y = newPos.Y - 6 * scale.X;
                }
                box.hitTop = false;

                // box collisions with each other
                foreach (var gameObject in boxes)
                {
                    if (gameObject != box && (gameObject.veloc.X > 0 || gameObject.veloc.Y > 0 || box.veloc.X > 0 || box.veloc.Y > 0))
                    {
                        var newPos = gameObject.HandleCollision(box.Rectangle, ref box.veloc, ref box.jumping, box.Mass);
                        box.pos.X = newPos.X;
                        box.pos.Y = newPos.Y;
                        box.hitTop = false;
                        gameObject.hitTop = false;
                    }
                }
            }

            base.Update(gameTime);
        }

        public class DistanceComparer : IComparer<Box>
        {
            int IComparer<Box>.Compare(Box a, Box b)
            {
                var distA = Math.Pow(a.X - pos.X, 2) + Math.Pow(a.Y - pos.Y, 2);
                var distB = Math.Pow(b.X - pos.X, 2) + Math.Pow(b.Y - pos.Y, 2);
                return Math.Sign(distA - distB);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            Color bgColor = Color.Lerp(Color.CornflowerBlue, Color.DarkOrange, score / (maxScore + 1f));
            Color hudColor = Color.Lerp(Color.SeaGreen, Color.Sienna, score / (maxScore + 1f));
            GraphicsDevice.Clear(bgColor);

            // TODO: Add your drawing code here
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            foreach (var gameObject in gameObjects)
            {
                gameObject.Draw(_spriteBatch);
            }
            foreach (var gameObject in food)
            {
                gameObject.Draw(_spriteBatch);
            }
            foreach (var gameObject in boxes) // draw box last
            {
                gameObject.Draw(_spriteBatch);
            }
            _spriteBatch.Draw(dinoSheet, pos, animation[(int)currentAnimation][currentAnimationIndex], Color.White, 0f, new Vector2(0, 0), scale,
                flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
            _spriteBatch.Draw(solid, new Rectangle(0, GameBounds.Y, GameBounds.X, 96), hudColor);
            if (score < maxScore)
                _spriteBatch.DrawString(font, $"Lvl. {currentLevel} - Collected: {score}/{maxScore}", new Vector2(24, GameBounds.Y + 28), Color.White);
            else
                _spriteBatch.DrawString(font, $"Lvl. {currentLevel} - Collected: {score}/{maxScore + 1}", new Vector2(24, GameBounds.Y + 28), Color.White);
            _spriteBatch.End();

            if (PuddingLarge.Collected)
            {
                _spriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
                _spriteBatch.Draw(solid, new Rectangle(0, 0, GameBounds.X, GameBounds.Y), Color.Black * 0.75f);
                _spriteBatch.DrawString(font, $"THE END", new Vector2(GameBounds.X / 2 - 93, GameBounds.Y / 2 - 20), Color.White);
                _spriteBatch.DrawString(font, $"Congratulations!", new Vector2(920, GameBounds.Y + 28), Color.White);
                _spriteBatch.End();
            }

            base.Draw(gameTime);
        }
    }
}