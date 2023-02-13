using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace mg1_calvinkuo
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Point GameBounds = new Point(1280, 720); //window resolution

        private float timer = 0f;
        private int threshold = (int)(150f / speed);
        private Vector2 pos = new Vector2(0f, 0f);
        private Vector2 veloc = new Vector2(0f, 0f);
        private Vector2 accel = new Vector2(0f, 1f);
        private bool flip = false;
        private bool jumping = false;
        private bool kicking = false;
        private Vector2 scale = new Vector2(4, 4);
        private const float speed = 1.5f;
        private Random rand = new Random();

        private Texture2D dinoSheet;
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
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            
            dinoSheet = Content.Load<Texture2D>("Images/vita");
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
                if (!jumping && !kicking && Keyboard.GetState().IsKeyDown(Keys.W))
                {
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

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            _spriteBatch.Draw(dinoSheet, pos, animation[(int)currentAnimation][currentAnimationIndex], Color.White, 0f, new Vector2(0, 0), scale,
                flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}