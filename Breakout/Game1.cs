﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;

namespace Breakout
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private KeyboardState kbstate;
        private GamePadState padstate;
        private Texture2D platform;
        private Texture2D ball;
        private Vector2 platform_pos;
        private Vector2 ball_pos;

      /*private bool doYinv, doXinv; */
        private bool isstuck; // for determening whether or not the ball is stuck to the platform.

        private Brick[] bricks;
        private int brickammount;
        private int rows;
        private int totalbricks;
        private Color[] brickColor;

        private short brickSidezone;

        private float ballangle; // Angle in which the ball is moving. 0 = 90 degree upwards; -1 = 0 Degrees ( completly right ); +1 = 180 degrees.
        private int basespeed;
        private bool yinv;

        /* Debugging Information */
        private SpriteFont font;
        private string debug;

        private bool is_gameover;
        private SpriteFont font_gameover;

        /* Save current time for controller rumble handler */
        static DateTime startRumble;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            // values
            brickammount = 9; // per row
            rows = 5;
            basespeed = 6;
            debug = "debug loading";
            isstuck = true;
            yinv = false;
            is_gameover = false;
            totalbricks = rows * brickammount;

            //Sizes and settings
            platform_pos = new Vector2(graphics.GraphicsDevice.Viewport.Width / 2 - 64, graphics.GraphicsDevice.Viewport.Height - 16);
            ball = new Texture2D(graphics.GraphicsDevice, 20, 20);
            ball_pos = new Vector2(50, 50);
            brickColor = new Color[] { Color.Black, Color.Red, Color.Blue, Color.Green, Color.Orange };

            brickSidezone = 7;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //Load Textures
            platform = this.Content.Load<Texture2D>("platform_128");
            System.Diagnostics.Debug.WriteLine("platform dimensions: " + platform.Width + "," + platform.Height);

            //Load Ball Data
            Color[] balldata = new Color[20 * 20];
            for (int i = 0; i < balldata.Length; i++) balldata[i] = Color.Black;
            ball.SetData(balldata);

            //Load Fonts
            font = Content.Load<SpriteFont>("NewSpriteFont");
            font_gameover = Content.Load<SpriteFont>("font_gameover");

            //Create Bricks
            System.Diagnostics.Debug.WriteLine("creating " + totalbricks + " bricks.");
            bricks = new Brick[totalbricks];

            for (int _rows = 0; _rows < rows; _rows++)
                for (int i = 0; i < brickammount; i++)
                {
                    bricks[i+(_rows*brickammount)] = new Brick(graphics, new Vector2((85 * i) + (5 * i), (20 * _rows)), new Vector2(85, 15), brickColor[_rows]);
                }
        }

        protected override void UnloadContent()
        {
            platform.Dispose();
            ball.Dispose();
            for (int i = 0; i < bricks.Length - 1; i++) bricks[i].Dispose();
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit();

            /* Ball Event Handler */
            bool doXinv, doYinv;
            doXinv = false;
            doYinv = false;

            /* Controlls */
            kbstate = Keyboard.GetState();
            padstate = GamePad.GetState(PlayerIndex.One);

            /* Get Thumbsticks */
            float xfloatright = padstate.ThumbSticks.Right.X * 10;
            int xintright = (int)xfloatright;
            float xfloatleft = padstate.ThumbSticks.Left.X * 10;
            int xintleft = (int)xfloatleft;

            /* Sprinting */
            if (kbstate.IsKeyDown(Keys.LeftShift) | kbstate.IsKeyDown(Keys.RightShift))
            {
                if (platform_pos.X > 0) if (kbstate.IsKeyDown(Keys.A) | kbstate.IsKeyDown(Keys.Left)) platform_pos.X -= 10;
                if (platform_pos.X + platform.Width < graphics.GraphicsDevice.Viewport.Width) if (kbstate.IsKeyDown(Keys.D) | kbstate.IsKeyDown(Keys.Right)) platform_pos.X += 10;
            }
            else {
                if (platform_pos.X > 0) if (kbstate.IsKeyDown(Keys.A) | kbstate.IsKeyDown(Keys.Left)) platform_pos.X -= 5;
                if (platform_pos.X + platform.Width < graphics.GraphicsDevice.Viewport.Width) if (kbstate.IsKeyDown(Keys.D) | kbstate.IsKeyDown(Keys.Right)) platform_pos.X += 5;
            }


            if (padstate.DPad.Left == ButtonState.Pressed | padstate.DPad.Right == ButtonState.Pressed | kbstate.IsKeyDown(Keys.A) | kbstate.IsKeyDown(Keys.D))
            {
                if (platform_pos.X > 0) if (padstate.DPad.Left == ButtonState.Pressed) platform_pos.X -= 10;
                if (platform_pos.X + platform.Width < graphics.GraphicsDevice.Viewport.Width) if (padstate.DPad.Right == ButtonState.Pressed) platform_pos.X += 10;
            }
            else
            {
                if (GamePad.GetCapabilities(PlayerIndex.One).HasRightXThumbStick)
                {
                    if (xintleft == 0)
                    {
                        if (xintright < 0) if (platform_pos.X > 0) platform_pos.X += xintright;
                        if (xintright > 0) if (platform_pos.X + platform.Width < graphics.GraphicsDevice.Viewport.Width) platform_pos.X += xintright;
                    }
                }
                if (GamePad.GetCapabilities(PlayerIndex.One).HasLeftXThumbStick)
                {
                    if (xintright == 0)
                    {
                        if (xintleft < 0) if (platform_pos.X > 0) platform_pos.X += xintleft;
                        if (xintleft > 0) if (platform_pos.X + platform.Width < graphics.GraphicsDevice.Viewport.Width) platform_pos.X += xintleft;
                    }
                }
            }

            if (padstate.DPad.Left == ButtonState.Released && padstate.DPad.Right == ButtonState.Released && kbstate.IsKeyUp(Keys.A) && kbstate.IsKeyUp(Keys.D) && xintleft == 0 && xintright == 0)
            {
                if (platform_pos.X > 0) if (padstate.Buttons.LeftShoulder == ButtonState.Pressed) platform_pos.X -= 10;
                if (platform_pos.X + platform.Width < graphics.GraphicsDevice.Viewport.Width) if (padstate.Buttons.RightShoulder == ButtonState.Pressed) platform_pos.X += 10;
            }

            //Reset
            if (padstate.Buttons.Start == ButtonState.Pressed || kbstate.IsKeyDown(Keys.Enter))
            {
                isstuck = true;
                yinv = false;
                is_gameover = false;
                basespeed = 6;
            }

            //Check for unstuck
            if (isstuck)
            if (kbstate.IsKeyDown(Keys.Space) | padstate.Buttons.A == ButtonState.Pressed) //Space on keyboard or Button A on the GamePad unstucks ball
            {
                isstuck = false;
                ballangle = 0f; //Ball will initially move straight upwards.
            }

            //Ball Stick thing
            if (isstuck) // Ball Position is being constantly updated and set according to the platform position.
            {
                ball_pos.Y = platform_pos.Y - ball.Height;
                ball_pos.X = platform_pos.X + platform.Width / 2 - ball.Width / 2;
            }
            else
            {
                //Wall collisions
                if(ball_pos.X <= 0 && ballangle < 0)
                {
                    doXinv = true;
                }
                else if((ball_pos.X + ball.Width) >= graphics.GraphicsDevice.Viewport.Width && ballangle > 0)
                {
                    doXinv = true;
                }

                //Ceiling collision
                if (ball_pos.Y <= 0 && !yinv) doYinv = true;

                //Platform collision
                if ((ball_pos.Y + ball.Height) >= platform_pos.Y) //if ball on same y level as platform
                {
                    if ((ball_pos.X + (ball.Width / 2)) >= platform_pos.X && (ball_pos.X + (ball.Width / 2)) <= platform_pos.X + platform.Width) //if bottom center of ball on same X level as platform
                    {
                        yinv = false;
                        int ballpos = (((int)(ball_pos.X + (ball.Width / 2)) - (int)platform_pos.X));
                        double impactscore = ballpos * (200 / (float)platform.Width);
                        impactscore -= 100;
                        ballangle = (float)impactscore / 100;
                        debug = "impactscore = " + impactscore + "; angle = " + ballangle + "; ballpos = " + ballpos;

                        //Controller Rumble
                        if (!is_gameover) startrumble();
                    }
                }

                //Basic Ball movement
                //TODO check for consistent ball speed
                float xmv, ymv;
                xmv = (ballangle * 2) * basespeed;
                if (ballangle < 0) ymv = (-2 - (ballangle * 2)) * basespeed;
                else ymv = (-2 - (ballangle * -2)) * basespeed;
                ball_pos.X += xmv;
                if (yinv) ball_pos.Y += (ymv * -1);
                else ball_pos.Y += ymv;

                //Brick collision
                for (int i = 0; i < bricks.Length; i++)
                {
                    if (bricks[i].active)
                    {
                        if (ball_pos.X < bricks[i].position.X + bricks[i].size.X && ball_pos.X + ball.Width > bricks[i].position.X) // Check for X match
                            if (ball_pos.Y < bricks[i].position.Y + bricks[i].size.Y && ball_pos.Y + ball.Height > bricks[i].position.Y) // Check for Y match
                            {
                                Vector2 relDist;
                                bricks[i].active = false;
                                if (ballangle == 0) /* Ball move Straight up (or down I think) */
                {
                    doYinv = true;
                                }
                                else if (ballangle > 0) /* Ball moves left to right */
                                {
                                    if (!yinv) /* Ball moves upwards */ /* TODO Fix */
                                    {
                                        relDist = calcRelDist(new Vector2((ball_pos.X + ball.Width), ball_pos.Y), new Vector2(bricks[i].position.X, (bricks[i].position.Y + bricks[i].size.Y)));
                                    }
                                    else /* Ball moves downwards */
                                    {
                                        relDist = calcRelDist(new Vector2((ball_pos.X + ball.Width), (ball_pos.Y + ball.Height)), new Vector2(bricks[i].position.X, bricks[i].position.Y));
                                    }


                                    if (relDist.X < relDist.Y)
                                    {
                                        doXinv = true;
                                    }
                                    else
                                    {
                                        doYinv = true;
                                    }


                                }

                                else /* Ball moves right to left */
                                {
                                    if (yinv) /* Ball moves upwards */
                                    {
                                        relDist = calcRelDist(ball_pos, new Vector2((bricks[i].position.X + bricks[i].size.X), (bricks[i].position.Y + bricks[i].size.Y)));
                                    }
                                    else /* Ball moves downwards */
                                    {
                                        relDist = calcRelDist(new Vector2((ball_pos.X), (ball_pos.Y + ball.Width)), new Vector2((bricks[i].position.X + bricks[i].size.X), bricks[i].position.Y));
                                    }

                                    if (relDist.X < relDist.Y)
                                    {
                                        doXinv = true;
                                    }
                                    else
                                    {
                                        doYinv = true;
                                    }

                                }

                            }
                    }
                }


                if (checkGameOver(ball_pos.Y))
                {
                    basespeed = 0;
                    is_gameover = true;
                }

            }

            /* Controller Rumble Check */
            checkrumble();

            /* Execute Ball Movement Changes */
            execBallDirChange(doXinv,doYinv);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
            spriteBatch.Draw(platform, platform_pos);
            spriteBatch.Draw(ball, ball_pos);

            for (int i = 0; i < bricks.Length; i++)
            {
                if (bricks[i].active) spriteBatch.Draw(bricks[i].texture, bricks[i].position);
            }
            if (is_gameover)
            {
                if (padstate.IsConnected)
                {
                    spriteBatch.DrawString(font_gameover, "Press START", new Vector2((graphics.GraphicsDevice.Viewport.Width / 2) - (font_gameover.MeasureString("Press START").X / 2), (graphics.GraphicsDevice.Viewport.Height / 2) - (font_gameover.MeasureString("Press START").Y / 2)), Color.Red);
                }
                else
                {
                    spriteBatch.DrawString(font_gameover, "Press ENTER", new Vector2((graphics.GraphicsDevice.Viewport.Width / 2) - (font_gameover.MeasureString("Press ENTER").X / 2), (graphics.GraphicsDevice.Viewport.Height / 2) - (font_gameover.MeasureString("Press ENTER").Y / 2)), Color.Red);
                }
            }
            else
            {
                spriteBatch.DrawString(font, debug, new Vector2(0, graphics.GraphicsDevice.Viewport.Height - font.MeasureString(debug).Y), Color.Red);
            }

            spriteBatch.End();
            base.Draw(gameTime);
        }

        /* Simple check funciton for better code organizing */
        private bool checkGameOver(float pos)
        {
            if (pos > graphics.GraphicsDevice.Viewport.Height)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void checkrumble()
        {
            if (padstate.IsConnected)
            {
                TimeSpan timePassed = DateTime.Now - startRumble;
                if (timePassed.TotalSeconds >= 0.07)
                {
                    GamePad.SetVibration(PlayerIndex.One, 0f, 0f);

                }
            }
        }


        private void startrumble()
        {
            if (padstate.IsConnected)
            {
                GamePad.SetVibration(PlayerIndex.One, 1f, 0f);
                startRumble = DateTime.Now;
            }
        }

        private void invertX()
        {
            ballangle *= -1;
        }

        private void invertY()
        {
            yinv = !yinv;
        }

        private Vector2 calcRelDist(Vector2 point, Vector2 pointBrick) /* calculate relative distance between two points (for brick collision) */
        {
            float tmpangle;
            Vector2 dist;
            Vector2 relAngle;

            /* set temporary ballangle */
            if (ballangle < 0) tmpangle = ballangle * -1; /* if ball moves Right to Left */
            else tmpangle = ballangle; /* if ball moves Left to Right */

            /* calculate relative angle */

            relAngle.X = (1 - tmpangle); /* % of movement speed vertical */
            relAngle.Y = tmpangle; /* % of movement speed horizontal */

            /* left to right */
            if(ballangle>0) dist.X = (point.X - pointBrick.X) * relAngle.X;
            else dist.X = (pointBrick.X - point.X) * relAngle.X;

            /* down to up */
            /* TODO Fix it better. */
            if(!yinv) dist.Y = (pointBrick.Y - point.Y) * relAngle.Y;
            else dist.Y = (point.Y - pointBrick.Y) * relAngle.Y;

            return dist;
        }

        private void execBallDirChange(bool doXinv, bool doYinv)
        {
            if (doXinv)
            {
                invertX();
            }

            if (doYinv)
            {
                invertY();
            }
        }

    }
}
