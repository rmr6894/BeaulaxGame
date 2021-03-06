﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace Beaulax.Classes
{
    class Player : Characters
    {
        // saving attributes
        private bool hasFlashlight;
        private bool hasJumppack;
        private bool hasSpacesuit;
        private int accessLevel;
        private Texture2D sprite;
        private Vector2 velocity;
        private bool hasJumped;
        private bool hasDoubleJumped;
        private Vector2 position;
        private float jumpHeight;
        private bool takingDamage = false;
        private Texture2D laser;
        private Rectangle attackBox;
        private float laserRotation = 0f;
        private bool firingLaser = false;
        private List<int> counterWhileDamaging = new List<int>();
        private Rectangle onLine;
        private int attackLineX = 0;
        private int attackLineY = 0;
        private Vector2 initialPos;

        //Player movement enum (FaceRight first so player initially starts off idly facing to the right)
        enum PlayerState { FaceRight, WalkRight, FaceLeft, WalkLeft, Jumping };
        PlayerState pState;

        //Player movement attributes
        private double timePerFrame = 100; //ms
        private int totalFrames = 8; //Just for the animation, frame 0 is the idle sprite; 9 TOTAL on spriteWalkingSheet
        private const int PLAYER_HEIGHT = 128;
        private const int PLAYER_WIDTH = 64;
        private int currentFramePlayer;
        private int framesElapsed;

        //Laser attributes
        private int totalLaserFrames = 5;
        private const int LASER_HEIGHT = 64;
        private const int LASER_WIDTH = 800;
        private int currentFrameLaser;
        private int cyclesWhileFiring = 0;
        private int cyclesToOverheat = 90;
        private int cyclesWhileCooling = 75;
        private int cyclesToCooldown = 75;
        private bool onCooldown = false;

        // defining states
        KeyboardState state; // gives the current state of pressed keys
        KeyboardState prevState;  // will give the previous state of pressed keys

        // constructors
        public Player()
        {
            hasFlashlight = false;
            hasJumppack = false;
            hasSpacesuit = false;
            accessLevel = 0;
            location = new Vector2(0, 0);
            hasJumped = true;
            hasDoubleJumped = true;
            speed = 3f;
            jumpHeight = 10f;
            hitBox = new Rectangle((int)location.X, (int)location.Y, width, height);
            health = 100;
            damage = 10;
        }

        public Player(Texture2D sprite, Texture2D laserBeam, bool ihasFlashlight, bool ihasJumpsuit, bool ihasSpacesuit, int iaccessLevel, float ispeed, float ijumpHeight, Vector2 ilocation, int iWidth, int iHeight, int pHlth, int pDmg)
        {
            hasFlashlight = ihasFlashlight;
            hasJumppack = ihasJumpsuit;
            hasSpacesuit = ihasSpacesuit;
            accessLevel = iaccessLevel;
            location = ilocation;
            position = new Vector2(ilocation.X, 1000f);
            initialPos = new Vector2(ilocation.X, 1000f);
            hasJumped = true;
            hasDoubleJumped = true;
            this.sprite = sprite;
            speed = ispeed;
            jumpHeight = ijumpHeight;
            width = iWidth;
            height = iHeight;
            hitBox = new Rectangle((int)location.X, (int)location.Y, width, height);
            laser = laserBeam;
            health = pHlth;
            damage = pDmg;
        }

        // properties
        public bool HasFlashlight { get { return hasFlashlight; } set { hasFlashlight = value; } }
        public bool HasJumppack { get { return hasJumppack; } set { hasJumppack = value; } }
        public bool HasSpacesuit { get { return hasSpacesuit; } set { hasSpacesuit = value; } }
        public int AccessLevel { get { return accessLevel; } set { accessLevel = value; } }
        public Texture2D Sprite { get { return sprite; } set { sprite = value; } }
        public Vector2 Velocity { get { return velocity; } set { velocity = value; } }
        public bool HasJumped { get { return hasJumped; } set { hasJumped = value; } }
        public bool HasDoubleJumped { get { return hasDoubleJumped; } set { hasDoubleJumped = value; } }
        public float Speed { get { return speed; } set { speed = value; } }
        public float JumpHeight { get { return jumpHeight; } set { jumpHeight = value; } }
        public Vector2 Position { get { return position; } set { position = value; } }
        public Vector2 InitialPos { get { return initialPos; } set { initialPos = value; } }
        public Rectangle AttackBox { get { return attackBox; } set { attackBox = value; } }
        public int Damage { get { return damage; } set { damage = value; } }
        

        // method
        public override string ToString()
        {
            return "Flashlight: " + hasFlashlight + "\nJumppack: " + hasJumppack + "\nSpacesuit: " + hasSpacesuit + "\nAccess Level: " + accessLevel + "\nLocation: " + location;
        }

        /// <summary>
        /// Allows player input to be read so the sprite can move and jump
        /// </summary>
        public void Movement()
        {
            state = Keyboard.GetState(); // moved location change to update

            if (state.IsKeyDown(Keys.D) && state.IsKeyUp(Keys.A)) // move player right
            {
                velocity.X = speed;
                pState = PlayerState.WalkRight;
            }

            else if (state.IsKeyDown(Keys.A) && state.IsKeyUp(Keys.D)) // move player left
            {
                velocity.X = -speed;
                pState = PlayerState.WalkLeft;
            }

            else // makes sure player does not move if there is no input
            {
                velocity.X = 0f;
                //changing pState for drawing purposes
                if (prevState.IsKeyDown(Keys.A)) // not moving, facing left
                {
                    pState = PlayerState.FaceLeft;
                }
                else if (prevState.IsKeyDown(Keys.D))// not moving, facing right
                {
                    pState = PlayerState.FaceRight;
                }
            }

            if (state.IsKeyDown(Keys.W) && hasJumped == false) // initiates jump if the player is on the ground and W is pressed
            {
                /*location.Y -= jumpHeight;
                velocity.Y = -5f; */
                location.Y -= 1;
                velocity.Y = -jumpHeight;
                hasJumped = true;
            }
            else if (state.IsKeyDown(Keys.W) && prevState.IsKeyUp(Keys.W) && hasJumped == true && hasJumppack == true && hasDoubleJumped == false) // double jump: checks if the player presses w a second time, ensures they have the jumppack and haven't already double-jumped
            {
                location.Y -= 10f;
                velocity.Y = -9f;
                hasDoubleJumped = true;
            }

            if (hasJumped == true)
            {
                this.Fall();
            }

            if (location.Y >= position.Y) // sets boolean back to false and the landing Y coordinate to initial Y position
            {
                hasJumped = false;
                hasDoubleJumped = false;
                location.Y = position.Y;
            }

            if (hasJumped == false) // makes sure the sprite does not move past the floor
            {
                velocity.Y = 0f;
            }

            prevState = state;
        }

        public void Fall()
        {
            float i = 1;
            velocity.Y += 0.20f * i; // acceleration
        }

        /// <summary>
        /// Fire a laser that deals damage to enemies.
        /// </summary>
        /// <param name="enemy"></param>
        public void Attack(List<Enemy> enemies)
        {
            state = Keyboard.GetState();
            bool[] isDamaging = new bool[enemies.Count];
            if (counterWhileDamaging.Count < 1)
            {
                for (int i = 0; i < isDamaging.Length; i++)
                {
                    counterWhileDamaging.Add(0);
                }
            }
            
            attackBox = new Rectangle(hitBox.X + (hitBox.Width / 4), hitBox.Y + (hitBox.Height / 2), 800, 64);

            // shoot diagonally up right
            if (state.IsKeyDown(Keys.Right) && state.IsKeyDown(Keys.Up))
            {
                laserRotation = (7 * (float)Math.PI) / 4;
                if (onCooldown == false)
                {
                    firingLaser = true;
                }

                // check if the laser is hitting the enemy
                while (attackLineX <= attackBox.Width)
                {
                    attackLineY = -attackLineX - 21;
                    onLine = new Rectangle(hitBox.X + (hitBox.Width / 2) + attackLineX, hitBox.Y + (hitBox.Height / 2) + attackLineY, 21, 21);
                    for (int i = 0; i < enemies.Count; i++)
                    {
                        if (onLine.Intersects(enemies[i].HitBox))
                        {
                            isDamaging[i] = true;
                        }
                    }
                    attackLineX++;
                }
            }
            // shoot diagonally down right
            else if (state.IsKeyDown(Keys.Right) && state.IsKeyDown(Keys.Down))
            {
                laserRotation = (float)Math.PI / 4;
                if (onCooldown == false)
                {
                    firingLaser = true;
                }

                // check if the laser is hitting the enemy
                while (attackLineX <= attackBox.Width)
                {
                    attackLineY = attackLineX;
                    onLine = new Rectangle(hitBox.X + (hitBox.Width / 2) + attackLineX, hitBox.Y + (hitBox.Height / 2) + attackLineY, 21, 21);
                    for (int i = 0; i < enemies.Count; i++)
                    {
                        if (onLine.Intersects(enemies[i].HitBox))
                        {
                            isDamaging[i] = true;
                        }
                    }
                    attackLineX++;
                }
            }
            // shoot diagonally up left
            else if (state.IsKeyDown(Keys.Left) && state.IsKeyDown(Keys.Up))
            {
                laserRotation = (5 * (float)Math.PI) / 4;
                if (onCooldown == false)
                {
                    firingLaser = true;
                }

                // check if the laser is hitting the enemy
                while (attackLineX >= -attackBox.Width)
                {
                    attackLineY = attackLineX;
                    onLine = new Rectangle(hitBox.X + (hitBox.Width / 2) + attackLineX, hitBox.Y + (hitBox.Height / 2) + attackLineY, 21, 21);
                    for (int i = 0; i < enemies.Count; i++)
                    {
                        if (onLine.Intersects(enemies[i].HitBox))
                        {
                            isDamaging[i] = true;
                        }
                    }
                    attackLineX--;
                }
            }
            // shoot diagonally down left
            else if (state.IsKeyDown(Keys.Left) && state.IsKeyDown(Keys.Down))
            {
                laserRotation = (3 * (float)Math.PI) / 4;
                if (onCooldown == false)
                {
                    firingLaser = true;
                }

                // check if the laser is hitting the enemy
                while (attackLineX >= -attackBox.Width)
                {
                    attackLineY = -attackLineX - 21;
                    onLine = new Rectangle(hitBox.X + (hitBox.Width / 2) + attackLineX, hitBox.Y + (hitBox.Height / 2) + attackLineY, 21, 21);
                    for (int i = 0; i < enemies.Count; i++)
                    {
                        if (onLine.Intersects(enemies[i].HitBox))
                        {
                            isDamaging[i] = true;
                        }
                    }
                    attackLineX--;
                }
            }
            // shoot right
            else if (state.IsKeyDown(Keys.Right) && state.IsKeyUp(Keys.Left))
            {
                laserRotation = 0f;
                if (onCooldown == false)
                {
                    firingLaser = true;
                }

                // check if the laser is hitting the enemy
                onLine = new Rectangle(hitBox.X + (hitBox.Width / 2), hitBox.Y + (hitBox.Height / 2) - 15, attackBox.Width, attackBox.Height);
                for (int i = 0; i < enemies.Count; i++)
                {
                    if (onLine.Intersects(enemies[i].HitBox))
                    {
                        isDamaging[i] = true;
                    }
                }
            }
            // shoot left
            else if (state.IsKeyDown(Keys.Left) && state.IsKeyUp(Keys.Right))
            {
                laserRotation = (float)Math.PI;
                if (onCooldown == false)
                {
                    firingLaser = true;
                }

                // check if the laser is hitting the enemy
                onLine = new Rectangle(hitBox.X + (hitBox.Width / 2) - attackBox.Width, hitBox.Y + (hitBox.Height / 2) - 15, attackBox.Width, attackBox.Height);
                for (int i = 0; i < enemies.Count; i++)
                {
                    if (onLine.Intersects(enemies[i].HitBox))
                    {
                        isDamaging[i] = true;
                    }
                }
            }
            // shoot up
            else if (state.IsKeyDown(Keys.Up) && state.IsKeyUp(Keys.Down))
            {
                laserRotation = (3 * (float)Math.PI) / 2;
                if (onCooldown == false)
                {
                    firingLaser = true;
                }

                // check if the laser is hitting the enemy
                onLine = new Rectangle(hitBox.X + (hitBox.Width / 2) - 15, hitBox.Y + (hitBox.Height / 2) - attackBox.Width, attackBox.Height, attackBox.Width);
                for (int i = 0; i < enemies.Count; i++)
                {
                    if (onLine.Intersects(enemies[i].HitBox))
                    {
                        isDamaging[i] = true;
                    }
                }
            }
            // shoot down
            else if (state.IsKeyDown(Keys.Down) && state.IsKeyUp(Keys.Up))
            {
                laserRotation = (float)Math.PI / 2;
                if (onCooldown == false)
                {
                    firingLaser = true;
                }

                // check if the laser is hitting the enemy
                onLine = new Rectangle(hitBox.X + (hitBox.Width / 2) - 15, hitBox.Y + (hitBox.Height / 2), attackBox.Height, attackBox.Width);
                for (int i = 0; i < enemies.Count; i++)
                {
                    if (onLine.Intersects(enemies[i].HitBox))
                    {
                        isDamaging[i] = true;
                    }
                }
            }
            // if nothing is pressed then the player is not firing the laser
            else
            {
                firingLaser = false;
            }

            // if the laser is being fired and is not overheated
            if (firingLaser == true && cyclesWhileFiring <= cyclesToOverheat)
            {
                // if the player is hitting the enemy then deal damage to the enemy
                for (int i = 0; i < enemies.Count; i++)
                {
                    if (isDamaging[i])
                    {
                        if (counterWhileDamaging[i] >= 20)
                        {
                            counterWhileDamaging[i] = 0;
                            enemies[i].TakeDamage(damage);
                        }
                        else
                        {
                            counterWhileDamaging[i]++;
                        }
                    }
                }
                cyclesWhileFiring++;
                cyclesWhileCooling = (int)((1f - (float)cyclesWhileFiring / (float)cyclesToOverheat) * (float)cyclesToCooldown);
            }
            else
            {
                // if the charge bar is empty then trigger the cooldown where you cannot shoot the laser while it regenerates
                if (cyclesWhileCooling == 0)
                {
                    onCooldown = true;
                }

                // if the laser is on cooldown
                if (cyclesWhileCooling <= cyclesToCooldown)
                {
                    if (onCooldown)
                    {
                        cyclesWhileCooling++;
                        firingLaser = false;
                    }
                    else
                    {
                        cyclesWhileCooling++;
                        firingLaser = false;
                        cyclesWhileFiring = (int)((1f - (float)cyclesWhileCooling / (float)cyclesToCooldown) * (float)cyclesToOverheat);
                    }
                }
                else
                {
                    onCooldown = false;
                    cyclesWhileFiring = 0;
                }
            }

            // reset attack variables
            attackLineX = 0;
            attackLineY = 0;
            for (int i = 0; i < isDamaging.Length; i++)
            {
                isDamaging[i] = false;
            }
        }

        /// <summary>
        /// Dameges the player by a certain number.
        /// </summary>
        /// <param name="damage">amount of damage that the player takes</param>
        public void TakeDamage(int damage)
        {
            if (health > 0)
            {
                health -= damage;
            }
            else if (health < 0)
            {
                health = 0;
            }
            takingDamage = true;
        }

        public void Update(GameTime gameTime)
        {
            location += velocity;
            hitBox = new Rectangle((int)location.X, (int)location.Y, width, height);

            //Animation
            framesElapsed = (int)(gameTime.TotalGameTime.TotalMilliseconds / timePerFrame);
            currentFramePlayer = framesElapsed % totalFrames + 1;
            currentFrameLaser = framesElapsed % totalLaserFrames;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // if the player is shooting then draw the laser
            if (firingLaser)
            {
                switch (pState.ToString())
                {
                    case "WalkLeft":
                    case "FaceLeft":
                        spriteBatch.Draw(laser, attackBox, new Rectangle(0, LASER_HEIGHT * currentFrameLaser, LASER_WIDTH, LASER_HEIGHT), Color.White, laserRotation, new Vector2(0, LASER_HEIGHT / 2), SpriteEffects.None, 0);
                        break;
                    case "WalkRight":
                    case "FaceRight":
                        spriteBatch.Draw(laser, attackBox, new Rectangle(0, LASER_HEIGHT * currentFrameLaser, LASER_WIDTH, LASER_HEIGHT), Color.White, laserRotation, new Vector2(0, LASER_HEIGHT / 2), SpriteEffects.FlipHorizontally, 0);
                        break;
                }
            }

            // if the player is taking damage then they turn red
            if (takingDamage)
            {
                switch (pState.ToString())
                {
                    case "WalkLeft":
                        spriteBatch.Draw(sprite, new Vector2((int)location.X, (int)location.Y), new Rectangle(PLAYER_WIDTH * currentFramePlayer, 0, PLAYER_WIDTH, PLAYER_HEIGHT), Color.Red, 0, Vector2.Zero, (float)0.5, SpriteEffects.None, 0);
                        break;
                    case "WalkRight":
                        spriteBatch.Draw(sprite, new Vector2((int)location.X, (int)location.Y), new Rectangle(PLAYER_WIDTH * currentFramePlayer, 0, PLAYER_WIDTH, PLAYER_HEIGHT), Color.Red, 0, Vector2.Zero, (float)0.5, SpriteEffects.FlipHorizontally, 0);
                        break;
                    case "FaceLeft":
                        spriteBatch.Draw(sprite, new Vector2((int)location.X, (int)location.Y), new Rectangle(0, 0, PLAYER_WIDTH, PLAYER_HEIGHT), Color.Red, 0, Vector2.Zero, (float)0.5, SpriteEffects.None, 0);
                        break;
                    case "FaceRight":
                        spriteBatch.Draw(sprite, new Vector2((int)location.X, (int)location.Y), new Rectangle(0, 0, PLAYER_WIDTH, PLAYER_HEIGHT), Color.Red, 0, Vector2.Zero, (float)0.5, SpriteEffects.FlipHorizontally, 0);
                        break;
                }
                takingDamage = false;
            }
            // if the player is not taking damage then draw them normally
            else
            {
                switch (pState.ToString())
                {
                    case "WalkLeft":
                        spriteBatch.Draw(sprite, new Vector2((int)location.X, (int)location.Y), new Rectangle(PLAYER_WIDTH * currentFramePlayer, 0, PLAYER_WIDTH, PLAYER_HEIGHT), Color.White, 0, Vector2.Zero, (float)0.5, SpriteEffects.None, 0);
                        break;
                    case "WalkRight":
                        spriteBatch.Draw(sprite, new Vector2((int)location.X, (int)location.Y), new Rectangle(PLAYER_WIDTH * currentFramePlayer, 0, PLAYER_WIDTH, PLAYER_HEIGHT), Color.White, 0, Vector2.Zero, (float)0.5, SpriteEffects.FlipHorizontally, 0);
                        break;
                    case "FaceLeft":
                        spriteBatch.Draw(sprite, new Vector2((int)location.X, (int)location.Y), new Rectangle(0, 0, PLAYER_WIDTH, PLAYER_HEIGHT), Color.White, 0, Vector2.Zero, (float)0.5, SpriteEffects.None, 0);
                        break;
                    case "FaceRight":
                        spriteBatch.Draw(sprite, new Vector2((int)location.X, (int)location.Y), new Rectangle(0, 0, PLAYER_WIDTH, PLAYER_HEIGHT), Color.White, 0, Vector2.Zero, (float)0.5, SpriteEffects.FlipHorizontally, 0);
                        break;
                }
            }

            //spriteBatch.Draw(sprite, hitBox, Color.Red); // we'll deal with this later
        }

        /// <summary>
        /// Draws the health bar, the weapon charge bar, and the UI for it
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="UI"></param>
        /// <param name="healthBar"></param>
        /// <param name="chargeBar"></param>
        /// <param name="currentHealth"></param>
        /// <param name="maxHealth"></param>
        public void DrawHUD(SpriteBatch spriteBatch, Texture2D UI, Texture2D healthBar, Texture2D chargeBar, int currentHealth, int maxHealth)
        {
            // calculate width of health bar based on player health
            float healthPercent = 1f - (float)currentHealth / (float)maxHealth;
            int healthWidth = 343 - (int)(healthPercent * 248f);
            int healthWidthS = healthBar.Width - (int)(healthPercent * 1260f);

            // calculate the width of the weapin charge bar
            float chargePercent;

            // on cooldown the bar is recharging
            if (onCooldown == true)
            {
                chargePercent = 1f - (float)cyclesWhileCooling / (float)cyclesToCooldown;
            }
            //not on cooldown the bar is draining
            else
            {
                chargePercent = (float)cyclesWhileFiring / (float)cyclesToOverheat;
            }
            int chargeWidth = 277 - (int)(chargePercent * 180f);
            int chargeWidthS = chargeBar.Width - (int)(chargePercent * 918f);

            // draw bar holder
            Rectangle barHolderRectangle = new Rectangle(50, 50, 345, 100);
            spriteBatch.Draw(UI, barHolderRectangle, Color.White);

            // draw health bar
            spriteBatch.Draw(healthBar, new Rectangle(barHolderRectangle.X, barHolderRectangle.Y, 343, 34), Color.Gray);
            spriteBatch.Draw(healthBar, new Rectangle(barHolderRectangle.X, barHolderRectangle.Y, healthWidth, 34), new Rectangle(0, 0, healthWidthS, healthBar.Height), Color.White);

            // draw weapon charge bar
            spriteBatch.Draw(chargeBar, new Rectangle(barHolderRectangle.X, barHolderRectangle.Y, 277, 73), Color.Gray);
            spriteBatch.Draw(chargeBar, new Rectangle(barHolderRectangle.X, barHolderRectangle.Y, chargeWidth, 73), new Rectangle(0, 0, chargeWidthS, chargeBar.Height), Color.White);
        }
    }
}

