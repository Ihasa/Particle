using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Forms = System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Storage;

using VisualEffects;
using UIComponents;

namespace Test3
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        SpriteFont font;
        Particle particle;
        Vector3 cameraPosition, cameraTarget;
        Matrix projection;
        ParticleParams particleParams = new ParticleParams();
        ParticleParams defaultParams;

        Dictionary<string, UpdownButton> updownButtons;
        Button colorButton1, colorButton2;
        Dictionary<string, decimal> defaultValues;

        string textureDirectory = Environment.CurrentDirectory;
        string texturePath = "";
        string saveDirectory = Environment.CurrentDirectory;
        string openDirectory = Environment.CurrentDirectory;

        int windowHeight;
        Vector2 showTexture;
        int textureSize;
        public static Texture2D pixel;
        bool emitting;
        float distance;
        float disVel;
        float rot;
        float rotVel;
        float rot2;
        float rot2Vel;
        int emitTimer = 0;
        MouseState currentState, lastState;


        Dictionary<string, string> texts = new Dictionary<string, string>();

        string file = "";
        public Game1(string fileName)
        {
            file = fileName;
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            int width = 1080*3/4;
            int height = 800*3/4;
            graphics.PreferredBackBufferWidth = width;
            windowHeight = graphics.PreferredBackBufferHeight = height;
            IsMouseVisible = true;
            Window.Title = "Particle Tester";
            emitting = false;
            texts["F1 ~ F4"] = "Camera";
            texts["F5 ~ F7"] = "Emit Direction";
            //panel = new System.Windows.Forms.Panel();
            //panel.Bounds = new System.Drawing.Rectangle(0, 0, graphics.PreferredBackBufferWidth, windowHeight);
            //panel.AllowDrop = true;
            //panel.DragDrop += (o, e) =>
            //{
            //    System.Windows.Forms.MessageBox.Show("DD");
            //};
            //panel.CreateControl();
            //panel.Show();
        }
        RenderTarget2D ssTarget;
        string dirName;
        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            VisualEffect.Init(this);

            ssTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            dirName = DateTime.Now.ToString("yyyyMMddHHmmss");
            //window.Show();

//            openFileDialog.ShowDialog();
            base.Initialize();
        }
        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            try
            {
                // Create a new SpriteBatch, which can be used to draw textures.
                spriteBatch = new SpriteBatch(GraphicsDevice);
                distance = 30;
                disVel = 0;
                rot = 0;
                rotVel = 0;
                rot2 = 60;
                rot2Vel = 0;
                //cameraPosition = new Vector3(0, 1, distance);
                rot = 45;
                Texture2D texUp = Content.Load<Texture2D>("up");
                Texture2D texDown = Content.Load<Texture2D>("down2");
                Texture2D X = Content.Load<Texture2D>("X");
                font = Content.Load<SpriteFont>("Font");
                pixel = Content.Load<Texture2D>("pix");

                string[] names = new string[]{
                    "InitialVelocity","Angle","Acceleration","InitialSize","FinalSize","InitialAlpha","FinalAlpha","LifeTime","Parts/Emit","Rotation/Frame","Attraction.X","Attraction.Y","Attraction.Z","scaling","speed","...distance"
                };
                
                decimal[] iniValues = new decimal[]{
                    0.12m,10,0,0.3m,3m,1,0,60,1,0,0,0,0,1,1,1
                };

                decimal?[] maxs = new decimal?[]{
                    null,180,null,null,null,1,1,null,null,180,null,null,null,null,null,null
                };
                decimal?[] mins = new decimal?[]{
                    null,0,null,0,0,0,0,1,1,-179,null,null,null,0.1m,0.1m,1
                };
                decimal[] distances = new decimal[]{
                    0.001m,1,0.0001m,0.01m,0.01m,0.01m,0.01m,1,1,1,0.0001m,0.0001m,0.0001m,0.1m,0.1m,1
                };
                
                updownButtons = new Dictionary<string, UpdownButton>();
                defaultValues = new Dictionary<string, decimal>();

                Vector2 udButtonSize = new Vector2(16, windowHeight/30);
                Texture2D buttonTex = Content.Load<Texture2D>("button");
                Vector2 buttonSize = new Vector2(160, windowHeight / 40);
                int buttonsX = graphics.PreferredBackBufferWidth - ((int)buttonSize.X + 16);
                for(int i = 0; i < names.Length; i++)
                {
                    if (names[i] == "...distance")
                        updownButtons.Add(names[i], new UpdownButton(this, iniValues[i], distances[i], texUp, texDown, font, new Vector2(buttonsX + font.MeasureString(names[i]).X, graphics.PreferredBackBufferHeight / 2 + (int)buttonSize.Y - 48 + 8), (int)udButtonSize.X, (int)udButtonSize.Y, names[i], maxs[i], mins[i]) { ShowMaxMin = false });
                    else if (i < 13)
                    {
                        updownButtons.Add(names[i], new UpdownButton(this, iniValues[i], distances[i], texUp, texDown, font, new Vector2(128, (int)(10 + i * udButtonSize.Y * 1.7f)), (int)udButtonSize.X, (int)udButtonSize.Y, names[i], maxs[i], mins[i]) { ShowMaxMin = false });
                    }
                    else
                    {
                        Vector2 posi = new Vector2(128, 10 + i * udButtonSize.Y * 1.5f);
                        if (names[i] == "speed" || names[i] == "scaling")
                        {
                            posi.X = buttonsX;
                        }
                        updownButtons.Add(names[i], new UpdownButton(this, iniValues[i], distances[i], texUp, texDown, font, posi+new Vector2(64,-buttonSize.Y / 8), (int)udButtonSize.X, (int)udButtonSize.Y, "", maxs[i], mins[i]) { ShowMaxMin = false });
                        Button button = new Button(this, buttonTex, font, posi, new Vector2(64, buttonSize.Y), names[i]);
                        button.Pressed += () => { setParams(true); };
                        Components.Add(button);
                    }
                    defaultValues.Add(names[i], iniValues[i]);
                }
                foreach (UpdownButton button in updownButtons.Values)
                {
                    Components.Add(button);
                }
                particleParams.InitialTexture = Content.Load<Texture2D>("particle");
                particleParams.FinalTexture = null;
                particleParams.InitialColor = new Vector3(255,255,255);
                particleParams.FinalColor = new Vector3(255,255,255);
                particleParams.Direction = Vector3.Up;
                //defaultParams = particleParams = new ParticleParams(
                //    Content.Load<Texture2D>("particle"),
                //    Vector3.Zero,
                //    0.6f,
                //    Vector3.Up,
                //    MathHelper.ToRadians(15),
                //    Vector3.Zero,
                //    0,
                //    0.6f,
                //    6.0f,
                //    1,
                //    0.0f,
                //    60,
                //    1
                //);
                particle = new Particle(particleParams);
                cameraTarget = new Vector3(0, 0, 0);
                projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 0.1f, 100);
                particle.SetProjection(projection);
                particle.SetView(cameraPosition, cameraTarget, Vector3.Up);
                this.Components.Add(particle);

                textureSize = windowHeight/10;     
                showTexture = new Vector2(96,(updownButtons.Count-1) * udButtonSize.Y * 1.5f + windowHeight/100 + windowHeight/30);

                Vector2 halfButtonSize = new Vector2(buttonSize.X / 2-8, buttonSize.Y);
                //textureChangeButton
                Button texButton = new Button(this, buttonTex, font, showTexture+new Vector2(-80,-windowHeight/30),halfButtonSize, "Texture1");
                texButton.Pressed += () => { 
                    Texture2D tex = loadTexture();
                    if (tex != null)
                        particleParams.InitialTexture = tex;
                };

                Button texButton2 = new Button(this, buttonTex, font, showTexture + new Vector2(8, -windowHeight / 30), halfButtonSize, "Texture2");
                texButton2.Pressed += () => { 
                    Texture2D tex = loadTexture();
                    if (tex != null)
                        particleParams.FinalTexture = tex;
                };

                Button tex2Cancel = new Button(this, X, font, showTexture + new Vector2(8 + halfButtonSize.X+4, -windowHeight / 30), new Vector2(halfButtonSize.Y, halfButtonSize.Y), "");
                tex2Cancel.Pressed += () => 
                {
                    particleParams.FinalTexture = null;
                };
                colorButton1 = new Button(this, pixel, font, showTexture + new Vector2(-80, textureSize + 8), halfButtonSize, "InitColor") 
                { OverColor = new Color(particleParams.InitialColor),MouseOverColor = new Color(particleParams.InitialColor) };

                colorButton2 = new Button(this, pixel, font, showTexture + new Vector2(8, textureSize + 8), halfButtonSize, "FinColor") 
                { OverColor = new Color(particleParams.FinalColor), MouseOverColor = new Color(particleParams.FinalColor) };
                colorButton1.Pressed += () =>
                {
                    Vector3? res = getColorFromDialog();
                    if (res != null)
                    {
                        particleParams.InitialColor = (Vector3)res;
                        changeButtonColor(colorButton1, new Color(particleParams.InitialColor / 255.0f));
                    }
                };
                colorButton2.Pressed += () =>
                {
                    Vector3? res = getColorFromDialog();
                    if (res != null)
                    {
                        particleParams.FinalColor = (Vector3)res;
                        changeButtonColor(colorButton2,new Color(particleParams.FinalColor/255.0f));
                    }
                };
                setParams(true);
                defaultParams = particleParams;

                Button resetButton = new Button(this, buttonTex, font, showTexture + new Vector2(-80,textureSize+halfButtonSize.Y+16), buttonSize, "All Reset");
                resetButton.Pressed += () =>
                {
                    particle.Reset();
                    for (int i = 0; i < names.Length; i++)
                    {
                        //値のリセット
                        updownButtons[names[i]].SetValue(defaultValues[names[i]]);
                    }
                    //テクスチャのリセット
                    particleParams.InitialTexture = defaultParams.InitialTexture;
                    particleParams.FinalTexture = defaultParams.FinalTexture;

                    //色のリセット
                    particleParams.InitialColor = defaultParams.InitialColor;
                    particleParams.FinalColor = defaultParams.FinalColor;
                    changeButtonColor(colorButton1,new Color(particleParams.InitialColor / 255.0f));
                    changeButtonColor(colorButton2,new Color(particleParams.FinalColor / 255.0f));
                    //colorButton1.OverColor = colorButton2.MouseOverColor = new Color(particleParams.InitialColor / 255.0f);
                    //if ((particleParams.InitialColor / 255).Length() > 0.5f)
                    //    colorButton1.TextColor = Color.Black;
                    //else colorButton1.TextColor = Color.White;

                    //colorButton2.OverColor = colorButton2.MouseOverColor = new Color(particleParams.FinalColor / 255.0f);
                    //if ((particleParams.FinalColor / 255).Length() > 0.5f)
                    //    colorButton2.TextColor = Color.Black;
                    //else colorButton2.TextColor = Color.White;
                };
                //ControlButtons
                Button emit1button = new Button(this, buttonTex, font, new Vector2(buttonsX, graphics.PreferredBackBufferHeight/2 +32), buttonSize, "Emit Once");
                emit1button.Pressed += emit;

                Button emitButton = new Button(this, buttonTex, font, new Vector2(buttonsX, graphics.PreferredBackBufferHeight/2 - 48),buttonSize, "Start Emitting",Keys.E,Keys.LeftControl);
                emitButton.Pressed += () =>
                {
                    if (emitting)
                    {
                        emitting = false;
                        emitButton.Name = "Start Emitting";
                        emit1button.IsEnabled = true;
                        emitTimer = 0;
                    }
                    else
                    {
                        emitting = true;
                        emitButton.Name = "Stop";
                        emit1button.IsEnabled = false;
                        particle.Reset();
                    }
                };
                Button clearButton = new Button(this,buttonTex,font,new Vector2(buttonsX,graphics.PreferredBackBufferHeight/2+64),buttonSize,"Clear",Keys.Delete);
                clearButton.Pressed += () =>
                {
                    particle.Reset();
                    emitting = false;
                    emitButton.Name = "Start Emitting";
                    emit1button.IsEnabled = true;
                    emitTimer = 0;
                };
                //fileButtons
                Button saveButton = new Button(this, buttonTex, font, new Vector2(buttonsX, graphics.PreferredBackBufferHeight - 96), buttonSize, "Save",Keys.LeftControl,Keys.S);
                saveButton.Pressed += saveFile;

                Button openButton = new Button(this, buttonTex, font, new Vector2(buttonsX, graphics.PreferredBackBufferHeight - 64), buttonSize, "Open",Keys.LeftControl,Keys.O);
                openButton.Pressed += openFile;

                Button exitButton = new Button(this, buttonTex, font, new Vector2(buttonsX, graphics.PreferredBackBufferHeight - 32), buttonSize, "Exit",Keys.Escape);
                exitButton.Pressed += () =>
                {
                    Exit();
                };

                Components.Add(openButton);
                Components.Add(saveButton);
                Components.Add(emit1button);
                Components.Add(emitButton);
                Components.Add(clearButton);
                Components.Add(exitButton);
                Components.Add(texButton);
                Components.Add(texButton2);
                Components.Add(tex2Cancel);
                Components.Add(colorButton1);
                Components.Add(colorButton2);
                Components.Add(resetButton);
                //Components.Add(new XBoxPointer(this, texUp, Vector2.Zero, new Vector2(0.5f, 0)));

                particle.UpdateOrder = 2;
                //particle.Normal = Vector3.Normalize(new Vector3(0, 1, 0.01f)) ;
                if (file != "")
                {
                    //System.Windows.Forms.MessageBox.Show(file);
                    openParFile(file);
                }
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.ToString());
            }
            // TODO: use this.Content to load your game content here
        }
        void changeButtonColor(Button b, Color c)
        {
            b.OverColor = b.MouseOverColor = c;//new Color(particleParams.InitialColor / 255.0f);
            if (c.ToVector3().Length() > 0.5f)
                b.TextColor = Color.Black;
            else b.TextColor = Color.White;
        }
        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            try
            {
                GamePadState state = GamePad.GetState(PlayerIndex.One);
                // Allows the game to exit
                if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                    this.Exit();

                if (emitting && particle != null)
                {
                    if (emitTimer++ % updownButtons["...distance"].GetValue() == 0)
                        emit();
                }

                //if (Keyboard.GetState().IsKeyDown(Keys.Space) && !pressed)
                //{
                //    changeTexture();
                //}

                setParams(false);
                particle.Parameters = particleParams;

                controlCamera();

                texts["Elements"] = "" + particle.Elements.Count;

                //スクショ
                if (state.IsButtonDown(Buttons.LeftShoulder) && state.IsButtonDown(Buttons.RightShoulder))
                {
                    screenShot(dirName,gameTime);
                }
                base.Update(gameTime);
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.ToString());
                Exit();
            }
        }

        private void controlCamera()
        {
            //マウスによる視点変更
            lastState = currentState;
            currentState = Mouse.GetState();
            //マウスホイールによる距離変更
            if (currentState.ScrollWheelValue != lastState.ScrollWheelValue)
            {
                if (currentState.ScrollWheelValue - lastState.ScrollWheelValue < 0)
                    disVel += 0.01f * distance;
                else disVel -= 0.01f * distance;
            }
            distance += disVel;
            disVel *= 0.9f;
            if (Math.Abs(disVel) < 0.01f)
            {
                disVel = 0;
            }
            if (distance < 2)
            {
                distance = 2;
                disVel = 0;
            }

            //右クリックによる回転
            float vel = 0.2f;
            if (currentState.RightButton == ButtonState.Pressed && lastState != null && lastState.RightButton == ButtonState.Pressed)
            {
                rotVel = (currentState.X - lastState.X) * vel;
            }
            rot += rotVel;
            rotVel *= 0.9f;
            if (Math.Abs(rotVel) < 0.01f)
            {
                rotVel = 0;
            }

            if (currentState.RightButton == ButtonState.Pressed && lastState != null && lastState.RightButton == ButtonState.Pressed)
            {
                rot2Vel = (lastState.Y - currentState.Y) * vel;// (currentState.Y - lastState.Y) / 64.0f * distance;
            }
            rot2 += rot2Vel;
            if (rot2 < 0.001f)
            {
                rot2 = 0.001f;
                rot2Vel = 0;
            }
            else if (rot2 > 179.999f)
            {
                rot2 = 179.999f;
                rot2Vel = 0;
            }
            rot2Vel *= 0.9f;
            if (Math.Abs(rot2Vel) < 0.01f)
            {
                rot2Vel = 0;
            }

            //キーによる視点変更
            KeyboardState key = Keyboard.GetState();
            if (key.IsKeyDown(Keys.F1))
            {
                rot = 0;
                rot2 = 90;
                rotVel = rot2Vel = 0;
            }
            else if (key.IsKeyDown(Keys.F2))
            {
                rot = 90;
                rot2 = 0.00001f;
                rotVel = rot2Vel = 0;
            }
            else if (key.IsKeyDown(Keys.F3))
            {
                rot = 90;
                rot2 = 90;
                rotVel = rot2Vel = 0;
            }
            else if (key.IsKeyDown(Keys.F4))
            {
                rot = 45;
                rot2 = 60;
                rotVel = rot2Vel = 0;
            }
            else if (key.IsKeyDown(Keys.F5))
            {
                particleParams.Direction = Vector3.UnitZ;
            }
            else if (key.IsKeyDown(Keys.F6))
            {
                particleParams.Direction = Vector3.UnitY;
            }
            else if (key.IsKeyDown(Keys.F7))
            {
                particleParams.Direction = Vector3.UnitX;
            }

            cameraPosition.X = distance * (float)(Math.Cos(MathHelper.ToRadians(rot)) * Math.Sin(MathHelper.ToRadians(rot2)));

            cameraPosition.Z = distance * (float)(Math.Sin(MathHelper.ToRadians(rot)) * Math.Sin(MathHelper.ToRadians(rot2)));

            cameraPosition.Y = distance * (float)Math.Cos(MathHelper.ToRadians(rot2));

            particle.SetView(cameraPosition, cameraTarget, Vector3.Up);
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 0.01f, 100+distance);
            particle.SetProjection(projection);
        }
        void emit()
        {
            particle.Emit();
        }
        void setParams(bool applyOptions)
        {
            decimal speed;
            decimal scale;
            if (applyOptions)
            {
                speed = updownButtons["speed"].GetValue();
                scale = updownButtons["scaling"].GetValue();
                updownButtons["InitialVelocity"].SetValue(updownButtons["InitialVelocity"].GetValue() * scale * speed);
                updownButtons["Attraction.X"].SetValue(updownButtons["Attraction.X"].GetValue() * scale * speed * speed);
                updownButtons["Attraction.Y"].SetValue(updownButtons["Attraction.Y"].GetValue() * scale * speed * speed);
                updownButtons["Attraction.Z"].SetValue(updownButtons["Attraction.Z"].GetValue() * scale * speed * speed);
                updownButtons["Acceleration"].SetValue(updownButtons["Acceleration"].GetValue() * scale * speed * speed);
                updownButtons["InitialSize"].SetValue(updownButtons["InitialSize"].GetValue() * scale);
                updownButtons["FinalSize"].SetValue(updownButtons["FinalSize"].GetValue() * scale);
                updownButtons["LifeTime"].SetValue((int)(updownButtons["LifeTime"].GetValue() / speed));
                updownButtons["speed"].SetValue(1m);
                updownButtons["scaling"].SetValue(1m);
            }
            else
            {
                speed = scale = 1;
            }
            particleParams = new ParticleParams(
                particleParams.InitialTexture,
                particleParams.FinalTexture,
                particleParams.EmitPoint,
                (float)(updownButtons["InitialVelocity"].GetValue() /* scale * speed*/),
                particleParams.Direction,
                MathHelper.ToRadians((float)updownButtons["Angle"].GetValue()),
                new Vector3((float)updownButtons["Attraction.X"].GetValue(), (float)updownButtons["Attraction.Y"].GetValue(), (float)updownButtons["Attraction.Z"].GetValue()) /* (float)(scale * speed * speed)*/,
                (float)(updownButtons["Acceleration"].GetValue() /* scale * speed * speed*/),
                (float)(updownButtons["InitialSize"].GetValue() /* scale*/),
                (float)(updownButtons["FinalSize"].GetValue() /* scale*/),
                (float)updownButtons["InitialAlpha"].GetValue(),
                (float)updownButtons["FinalAlpha"].GetValue(),
                colorButton1.OverColor.ToVector3()*255.0f,//InitialColor
                colorButton2.OverColor.ToVector3()*255.0f,//FinalColor
                (int)(updownButtons["LifeTime"].GetValue() /*/ speed*/),
                (int)updownButtons["Parts/Emit"].GetValue(),
                (int)updownButtons["Rotation/Frame"].GetValue()
            );
        }
        int ssCount = 0;
        void screenShot(string dirName,GameTime gameTime)
        {
            try
            {
                GraphicsDevice.SetRenderTarget(ssTarget);
                GraphicsDevice.Clear(Color.White);
                Draw(gameTime);
                GraphicsDevice.SetRenderTarget(null);
                //string startTime = DateTime.Now.ToString("yyyyMMddHHmmss");
                System.IO.DirectoryInfo directory = new System.IO.DirectoryInfo("../" + dirName);
                if (!directory.Exists)
                {
                    directory.Create();
                }

                using (System.IO.Stream stream = new System.IO.FileInfo("../" + dirName + "/ss" + (ssCount++) + ".png").Open(System.IO.FileMode.OpenOrCreate))
                {
                    ssTarget.SaveAsPng(stream, ssTarget.Width, ssTarget.Height);
                }
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.ToString());
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            try
            {
                GraphicsDevice.Clear(Color.CornflowerBlue);
                //particle.SetView(cameraPosition, cameraTarget, Vector3.Up);

                Color lineColor = Color.Gray;
                int lines = 20;
                List<Vector3> vertices = new List<Vector3>();
                for (int x = 1; x <= lines; x++)
                {
                    vertices.Add(new Vector3(0, x, 0)); vertices.Add(new Vector3(lines, x, 0));//xy...x
                    vertices.Add(new Vector3(x, 0, 0)); vertices.Add(new Vector3(x, lines, 0));//xy...y

                    vertices.Add(new Vector3(0, x, 0)); vertices.Add(new Vector3(0, x, lines));//yz...z
                    vertices.Add(new Vector3(0, 0, x)); vertices.Add(new Vector3(0, lines, x));//yz...x

                    vertices.Add(new Vector3(0, 0, x)); vertices.Add(new Vector3(lines, 0, x));//xz...x
                    vertices.Add(new Vector3(x, 0, 0)); vertices.Add(new Vector3(x, 0, lines));//xz...z
                }
                drawLine3D(lineColor, vertices.ToArray());
                drawLine3D(Vector3.Zero, Vector3.UnitX * lines, Color.Red);
                drawLine3D(Vector3.Zero, Vector3.UnitY * lines, Color.Green);
                drawLine3D(Vector3.Zero, Vector3.UnitZ * lines, Color.Blue);


                spriteBatch.Begin();
                //int textureSize = windowHeight / 6;
                #region
                int showTexX1 = (int)showTexture.X - textureSize / 2 - 8;
                int showTexX2 = (int)showTexture.X + textureSize / 2 + 8;
                if (particleParams.InitialTexture.Width >= textureSize || particleParams.InitialTexture.Height >= textureSize)
                {
                    if (particleParams.InitialTexture.Width >= particleParams.InitialTexture.Height)
                    {
                        spriteBatch.Draw(particleParams.InitialTexture, new Rectangle((int)showTexX1 - textureSize / 2, (int)showTexture.Y, textureSize, (int)(particleParams.InitialTexture.Height * ((float)textureSize / particleParams.InitialTexture.Width))), Color.White);
                        drawRect(pixel, new Vector2(showTexX1, showTexture.Y) - new Vector2(textureSize / 2 + 1, 1), textureSize + 1, (int)(particleParams.InitialTexture.Height * ((float)textureSize / particleParams.InitialTexture.Width) + 1), Color.Red);
                    }
                    else
                    {
                        spriteBatch.Draw(particleParams.InitialTexture, new Rectangle((int)showTexX1 - (int)((float)particleParams.InitialTexture.Width / particleParams.InitialTexture.Height * textureSize) / 2, (int)showTexture.Y, (int)(particleParams.InitialTexture.Width * ((float)textureSize / particleParams.InitialTexture.Height)), textureSize), Color.White);
                        drawRect(
                            pixel,
                            new Vector2(showTexX1, showTexture.Y) - new Vector2((int)((float)particleParams.InitialTexture.Width / particleParams.InitialTexture.Height * textureSize) / 2 + 1, 1),
                            (int)(particleParams.InitialTexture.Width * ((float)textureSize / particleParams.InitialTexture.Height) + 1),
                            textureSize + 1,
                            Color.Red
                        );
                    }
                }
                else
                {
                    spriteBatch.Draw(particleParams.InitialTexture, new Vector2(showTexX1, showTexture.Y) - new Vector2(particleParams.InitialTexture.Width / 2, 0), Color.White);
                    drawRect(pixel, new Vector2(showTexX1, showTexture.Y) - new Vector2(particleParams.InitialTexture.Width / 2 + 1, 1), particleParams.InitialTexture.Width + 1, particleParams.InitialTexture.Height + 1, Color.Red);
                }

                if (particleParams.FinalTexture != null)
                {
                    if (particleParams.FinalTexture.Width >= textureSize || particleParams.FinalTexture.Height >= textureSize)
                    {
                        if (particleParams.FinalTexture.Width >= particleParams.FinalTexture.Height)
                        {
                            spriteBatch.Draw(particleParams.FinalTexture, new Rectangle((int)showTexX2 - textureSize / 2, (int)showTexture.Y, textureSize, (int)(particleParams.FinalTexture.Height * ((float)textureSize / particleParams.FinalTexture.Width))), Color.White);
                            drawRect(pixel, new Vector2(showTexX2, showTexture.Y) - new Vector2(textureSize / 2 + 1, 1), textureSize + 1, (int)(particleParams.FinalTexture.Height * ((float)textureSize / particleParams.FinalTexture.Width) + 1), Color.Red);
                        }
                        else
                        {
                            spriteBatch.Draw(particleParams.FinalTexture, new Rectangle((int)showTexX2 - (int)((float)particleParams.FinalTexture.Width / particleParams.FinalTexture.Height * textureSize) / 2, (int)showTexture.Y, (int)(particleParams.FinalTexture.Width * ((float)textureSize / particleParams.FinalTexture.Height)), textureSize), Color.White);
                            drawRect(
                                pixel,
                                new Vector2(showTexX2, showTexture.Y) - new Vector2((int)((float)particleParams.FinalTexture.Width / particleParams.FinalTexture.Height * textureSize) / 2 + 1, 1),
                                (int)(particleParams.FinalTexture.Width * ((float)textureSize / particleParams.FinalTexture.Height) + 1),
                                textureSize + 1,
                                Color.Red
                            );
                        }
                    }
                    else
                    {
                        spriteBatch.Draw(particleParams.FinalTexture, new Vector2(showTexX2, showTexture.Y) - new Vector2(particleParams.FinalTexture.Width / 2, 0), Color.White);
                        drawRect(pixel, new Vector2(showTexX2, showTexture.Y) - new Vector2(particleParams.FinalTexture.Width / 2 + 1, 1), particleParams.FinalTexture.Width + 1, particleParams.FinalTexture.Height + 1, Color.Red);
                    }
                }
                #endregion
                for (int i = 0; i < texts.Count; i++)
                {
                    string str = texts.Keys.ElementAt(i) + "..." + texts.Values.ElementAt(i);
                    int x = graphics.PreferredBackBufferWidth - (int)font.MeasureString(str).X;
                    spriteBatch.DrawString(font, str, new Vector2(x, i * 15), Color.Black);
                }
                spriteBatch.End();

                base.Draw(gameTime);
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.ToString());
            }
        }

        Texture2D loadTexture()
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.RestoreDirectory = true;
            openFileDialog.FilterIndex = 0;

            openFileDialog.InitialDirectory = textureDirectory;
            openFileDialog.Filter = "Image Files(*.png;*.jpg;*.gif)|*.png;*.jpg;*.gif;"
                                   +"|PNG Files(*.png)|*.png"
                                   +"|JPEG Files(*.jpg)|*.jpg"
                                   +"|GIF Files|*.gif";
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    #region
                    ////OKボタンが押されたときの処理
                    ////BitMapとして読み込み、textureにデータをコピー
                    //System.Drawing.Bitmap bitmap = (System.Drawing.Bitmap)System.Drawing.Bitmap.FromFile(openFileDialog.FileName, false);
                    //System.Drawing.Rectangle bounds = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);
                    //System.Drawing.Imaging.BitmapData bmpData = bitmap.LockBits(bounds, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

                    ////ここで色情報取得
                    //byte[] pixelData = new byte[bitmap.Width * bitmap.Height * 4];
                    //for (int i = 0; i < bitmap.Height; i++)
                    //{
                    //    for (int j = 0; j < bitmap.Width; j++)
                    //    {
                    //        int target = j * 4 + bmpData.Stride * i;
                    //        int index = (i * bitmap.Width + j) * 4;
                    //        for (int k = 0; k < 4; k++)
                    //        {
                    //            pixelData[index + k] = System.Runtime.InteropServices.Marshal.ReadByte(
                    //                bmpData.Scan0,
                    //                target + 4 - ((1 + k) % 4) - 1);
                    //        }
                    //    }
                    //}
                    //bitmap.UnlockBits(bmpData);


                    ////System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(bmpData.Width, bmpData.Height, bmpData.Stride, System.Drawing.Imaging.PixelFormat.Format32bppPArgb, bmpData.Scan0);
                    ////int bitPerPixel = System.Drawing.Bitmap.GetPixelFormatSize(bmp.PixelFormat);
                    ////System.Drawing.Color[] colorData = new System.Drawing.Color[bmp.Width * bmp.Height];
                    ////for (int i = 0; i < bmp.Height; i++)
                    ////{
                    ////    for (int j = 0; j < bmp.Width; j++)
                    ////    {
                    ////        colorData[i * bmp.Width + j] = bmp.GetPixel(j, i);
                    ////    }
                    ////}
                    ////byte[] data = new byte[colorData.Length * 4];
                    ////for (int i = 0; i < colorData.Length; i++)
                    ////{
                    ////    int index = i * 4;
                    ////    data[index + 3] = colorData[i].A;
                    ////    data[index + 2] = colorData[i].B;
                    ////    data[index + 1] = colorData[i].G;
                    ////    data[index] = colorData[i].R;
                    ////}

                    //Texture2D texture = new Texture2D(GraphicsDevice, bitmap.Width, bitmap.Height);
                    //texture.SetData(pixelData);
                    ////Texture2D texture = Texture2D.FromStream(GraphicsDevice, openFileDialog.OpenFile());
                
                    //System.Windows.Forms.MessageBox.Show(texture.Format.ToString());
                    //particleParams.InitialTexture = texture;
                    #endregion

                    textureDirectory = openFileDialog.FileName.Substring(0,openFileDialog.FileName.LastIndexOf('\\'));
                    texturePath = openFileDialog.FileName;
                    return getTextureFromFile(openFileDialog.FileName);
                }
                catch (Exception e)
                {
                    System.Windows.Forms.MessageBox.Show(e.ToString());
                }
            }
            return null;
        }

        Texture2D getTextureFromFile(string path)
        {
            //BitMapとして読み込み、textureにデータをコピー
            System.Drawing.Bitmap bitmap = (System.Drawing.Bitmap)System.Drawing.Bitmap.FromFile(path, false);
            System.Drawing.Rectangle bounds = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);
            System.Drawing.Imaging.BitmapData bmpData = bitmap.LockBits(bounds, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

            //ここで色情報取得
            byte[] pixelData = new byte[bitmap.Width * bitmap.Height * 4];
            for (int i = 0; i < bitmap.Height; i++)
            {
                for (int j = 0; j < bitmap.Width; j++)
                {
                    int target = j * 4 + bmpData.Stride * i;
                    int index = (i * bitmap.Width + j) * 4;
                    for (int k = 0; k < 4; k++)
                    {
                        pixelData[index + k] = System.Runtime.InteropServices.Marshal.ReadByte(
                            bmpData.Scan0,
                            target + 4 - ((1 + k) % 4) - 1);
                    }
                }
            }
            bitmap.UnlockBits(bmpData);

            Texture2D tex = new Texture2D(GraphicsDevice, bitmap.Width, bitmap.Height);
            tex.SetData(pixelData);
            //Texture2D texture = Texture2D.FromStream(GraphicsDevice, openFileDialog.OpenFile());

            //System.Windows.Forms.MessageBox.Show(texture.Format.ToString());
            
            return tex;
        }
        Vector3? getColorFromDialog()
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                return new Vector3(dialog.Color.R, dialog.Color.G, dialog.Color.B);
            }
            return null;
        }
        void saveFile()
        {
            System.Windows.Forms.SaveFileDialog dialog = new System.Windows.Forms.SaveFileDialog();
            dialog.InitialDirectory = this.saveDirectory;
            dialog.OverwritePrompt = false;
            dialog.Filter = "Particle File(*.par)|*.par";
            dialog.FilterIndex = 0;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                saveDirectory = dialog.FileName.Substring(0,dialog.FileName.LastIndexOf('\\'));
                particle.SaveAsParFile(dialog.FileName,true);
                //StreamWriter writer = new StreamWriter(dialog.OpenFile());
                //object[] data = new object[updownButtons.Count + 2];
                //data[0] = "";
                //int i = 1;
                //foreach (UpdownButton b in updownButtons.Values)
                //{
                //    data[i++] = b.GetValue();
                //}
                //if (particle.Parameters.InitialTexture != defaultParams.InitialTexture)
                //{
                //    data[0] = texturePath;
                //}
                //else data[0] = "　";

                //i = 0;
                //foreach (object o in data)
                //{
                //    if (i == 0)
                //        writer.WriteLine(o);
                //    else writer.WriteLine((decimal)o);
                //}
                //writer.Close();
                //changeDefaultValues();
            }
        }
        void openFile()
        {
            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.InitialDirectory = openDirectory;
            dialog.Filter = "Particle Files(*.par)|*.par";
            dialog.FilterIndex = 0;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string fileName = dialog.FileName;
                openParFile(fileName);
                return;
            }
        }

        private void openParFile(string fileName)
        {
            openDirectory = fileName.Substring(0, fileName.LastIndexOf('\\'));
            Particle par;
            try
            {
                par = Particle.CreateFromParFile(fileName);
            }
            catch (Exception)
            {
                System.Windows.Forms.MessageBox.Show("読み込み中にエラーが発生しました");
                return;
            }
            particleParams = par.Parameters;
            //particleParams.InitialTexture = par.Parameters.InitialTexture;
            //particleParams.FinalTexture = par.Parameters.FinalTexture;

            //StreamReader reader = new StreamReader(dialog.OpenFile());
            //string path = reader.ReadLine();
            //if (path != "　")
            //{
            //    loadTexture(path);
            //}
            //foreach (UpdownButton b in updownButtons.Values)
            //{
            //    b.SetValue(decimal.Parse(reader.ReadLine()));
            //}
            //reader.Close();
            setButtonValues(par.Parameters);
            setParams(false);
            changeDefaultValues();
            Window.Title = "Particle Tester" + " ["+fileName+"]";
            return;
        }
        void setButtonValues(ParticleParams p)
        {
            updownButtons["InitialVelocity"].SetValue((decimal)p.InitialVelocity);
            updownButtons["Acceleration"].SetValue((decimal)p.Acceleration);
            updownButtons["Angle"].SetValue((decimal)MathHelper.ToDegrees(p.Radian));
            updownButtons["InitialSize"].SetValue((decimal)p.InitialSize);
            updownButtons["FinalSize"].SetValue((decimal)p.FinalSize);
            updownButtons["InitialAlpha"].SetValue((decimal)p.InitialAlpha);
            updownButtons["FinalAlpha"].SetValue((decimal)p.FinalAlpha);
            updownButtons["Attraction.X"].SetValue((decimal)p.Attraction.X);
            updownButtons["Attraction.Y"].SetValue((decimal)p.Attraction.Y);
            updownButtons["Attraction.Z"].SetValue((decimal)p.Attraction.Z);
            updownButtons["LifeTime"].SetValue((decimal)p.LifeTime);
            updownButtons["Parts/Emit"].SetValue((decimal)p.PartsPerEmit);
            updownButtons["Rotation/Frame"].SetValue((decimal)p.RotationPerFrame);
            changeButtonColor(colorButton1, new Color(p.InitialColor / 255.0f));
            changeButtonColor(colorButton2, new Color(p.FinalColor / 255.0f));
        }
        void changeDefaultValues()
        {
            foreach (KeyValuePair<string, UpdownButton> pair in updownButtons)
            {
                defaultValues[pair.Key] = pair.Value.GetValue();
            }
            defaultParams.InitialTexture = particleParams.InitialTexture;
            defaultParams.FinalTexture = particleParams.FinalTexture;
            
        }
        void drawLine3D(Vector3 iniPosition, Vector3 endPosition, Color color)
        {
            VertexPositionColor vertex1 = new VertexPositionColor(iniPosition,color);
            VertexPositionColor vertex2 = new VertexPositionColor(endPosition,color);

            VertexBuffer vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColor), 2, BufferUsage.None);
            vertexBuffer.SetData(new VertexPositionColor[] { vertex1, vertex2 });

            drawPrimitiveLine(vertexBuffer,PrimitiveType.LineList,1);
        }

        private void drawPrimitiveLine(VertexBuffer vertexBuffer,PrimitiveType type,int primitiveCount)
        {
            BasicEffect effect = new BasicEffect(GraphicsDevice);
            effect.VertexColorEnabled = true;
            effect.View = Matrix.CreateLookAt(cameraPosition, cameraTarget, Vector3.Up);
            effect.Projection = projection;
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.SetVertexBuffer(vertexBuffer);
                GraphicsDevice.DrawPrimitives(type, 0, primitiveCount);
            }
        }
        void drawLine3D(Color color, params Vector3[] positions)
        {
            VertexPositionColor[] vertices = new VertexPositionColor[positions.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = new VertexPositionColor(positions[i], color);               
            }
            VertexBuffer buffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColor), vertices.Length, BufferUsage.None);
            buffer.SetData(vertices);
            drawPrimitiveLine(buffer,PrimitiveType.LineList,vertices.Length/2);            
        }
        void drawLine(Texture2D brush, Vector2 initPoint, Vector2 endPoint, Color color)
        {
            SpriteBatch spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteBatch.Begin();
            int distance = (int)Vector2.Distance(initPoint, endPoint);

            for (int i = 0; i <= distance; i++)
            {
                spriteBatch.Draw(brush, new Vector2((float)MathHelper.Lerp(initPoint.X, endPoint.X, (float)i / distance), (float)MathHelper.Lerp(initPoint.Y, endPoint.Y, (float)i / distance)), color);
            }
            spriteBatch.End();
        }
        void drawRect(Texture2D brush,Vector2 position, int width,int height,Color color)
        {
            drawLine(brush,position, position + new Vector2(width, 0), color);
            drawLine(brush,position, position + new Vector2(0, height), color);
            drawLine(brush,position + new Vector2(width,height),position + new Vector2(width,0), color);
            drawLine(brush,position + new Vector2(width,height), position + new Vector2(0, height), color);
        }
        void fillRect(Texture2D brush, Vector2 position, int width, int height, Color color)
        {
            drawRect(brush, position, width, height, color);
            for (int i = (int)position.Y; i <= position.Y + height; i++)
            {
                drawLine(brush, new Vector2(position.X, i), new Vector2(position.X + width, i), color);
            }
        }
        void drawRect(Texture2D brush, Vector2 minPoint, Vector2 maxPoint, Color color)
        {
            drawRect(brush,minPoint, (int)Math.Abs(maxPoint.X - minPoint.X), (int)Math.Abs(maxPoint.Y - minPoint.Y), color);
        }
        
    }
}
