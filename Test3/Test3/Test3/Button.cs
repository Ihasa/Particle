using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
namespace UIComponents
{
    class Button:DrawableGameComponent
    {
        Texture2D texture;
        SpriteFont font;
        Rectangle area;
        string str;
        bool pressed;
        MouseState currentMouseState, lastMouseState;
        KeyboardState currentKeyState, lastKeyState;
        public event Action Pressed;
        SpriteBatch spriteBatch;
        int generalTimer, time;
        public Keys[] shortCutKeys{get;set;}
        public bool IsEnabled { get; set; }
        public string Name { get { return str; } set { str = value; } }
        public Color OverColor { get; set; }
        public Color MouseOverColor { get; set; }
        public Color TextColor { get; set; }

        public Button(Game game, Texture2D tex,SpriteFont font, Vector2 position,Vector2? size, string str,params Keys[] shortCuts)
            : base(game)
        {
            DrawOrder = 2;
            texture = tex;
            this.font = font;
            
            if (size == null)
            {
                int wid = (int)(font.MeasureString(str).X / 0.8f);
                int hei = (int)(font.MeasureString(str).Y / 0.8f);
                area = new Rectangle((int)position.X, (int)position.Y, wid, hei);
            }
            else
            {
                area = new Rectangle((int)position.X, (int)position.Y, (int)(((Vector2)size).X), (int)(((Vector2)size).Y));
            }
            this.str = str;
            pressed = false;
            spriteBatch = new SpriteBatch(game.GraphicsDevice);
            if (font.MeasureString(str).X > area.Width)
            {
                this.area.Width = (int)(font.MeasureString(str).X);
            }
            //if (font.MeasureString(str).Y /** 0.8f*/ > area.Height)
            //{
            //    this.area.Height = (int)(font.MeasureString(str).Y /*/ 0.8f*/);
            //}
            IsEnabled = true;
            generalTimer = 0;
            time = -5;
            if(shortCuts != null && shortCuts.Length != 0)
                shortCutKeys = shortCuts;

            OverColor = Color.White;
            MouseOverColor = Color.Yellow;
            TextColor = Color.Black;
        }

        public override void Draw(GameTime gameTime)
        {
            
            spriteBatch.Begin();
            float rate;
            float rateMin = 0.8f;
            float animationTime = 5;
            Color color;
            if (IsEnabled)
            {
                if (!pressed)
                {
                    if (generalTimer - time > animationTime)
                    {
                        rate = 1;
                        if (currentMouseState.X > area.X && currentMouseState.X < area.X + area.Width &&
                            currentMouseState.Y > area.Y && currentMouseState.Y < area.Y + area.Height)
                        {
                            color = MouseOverColor;
                        }
                        else
                        {
                            color = new Color(OverColor.ToVector3() * MathHelper.Lerp(0.4f, 1, rate));
                        }
                    }
                    else
                    {
                        rate = MathHelper.Lerp(rateMin, 1, (generalTimer - time) / animationTime);
                        color = new Color(OverColor.ToVector3() * MathHelper.Lerp(0.4f, 1, rate));
                    }
                }
                else
                {
                    if (generalTimer - time > animationTime)
                        rate = rateMin;
                    else
                        rate = MathHelper.Lerp(1, rateMin, (generalTimer - time) / animationTime);
                    color = new Color(OverColor.ToVector3() * MathHelper.Lerp(1, 0.4f, rate));
                }
            }
            else
            {
                rate = 1;
                color = new Color(0.3f, 0.3f, 0.3f);
            }
            //spriteBatch.Draw(texture, new Rectangle(area.X + (int)(area.Width * (1 - rate) / 2), area.Y + (int)(area.Height * (1 - rate) / 2), (int)(area.Width * rate), (int)(area.Height * rate)), color);
            spriteBatch.Draw(texture, area.Scaling(rate), color);
            
            spriteBatch.DrawString(font, str, new Vector2(area.X + area.Width / 2, area.Y + area.Height / 2) - font.MeasureString(str) / 2 * rate, TextColor, 0, Vector2.Zero, rate, SpriteEffects.None, 0);
            spriteBatch.End();
            base.Draw(gameTime);
        }
        public override void Update(GameTime gameTime)
        {
            lastMouseState = currentMouseState;
            currentMouseState = Mouse.GetState();
            lastKeyState = currentKeyState;
            currentKeyState = Keyboard.GetState();

            if (IsEnabled)
            {
                if (currentMouseState.LeftButton == ButtonState.Pressed)
                {
                    if (lastMouseState.LeftButton == ButtonState.Released)
                    {
                        if (!pressed &&
                            currentMouseState.X < area.X + area.Width && currentMouseState.X > area.X &&
                            currentMouseState.Y < area.Y + area.Height && currentMouseState.Y > area.Y)
                        {
                            pressed = true;
                            registTime();
                        }
                    }
                }
                else if (currentMouseState.LeftButton == ButtonState.Released)
                {
                    if (clicked() || shortCutsPressed())
                    {
                        onPressed();
                    }
                    if (pressed)
                    {
                        registTime();
                        pressed = false;
                    }
                }
                generalTimer++;
            }
            base.Update(gameTime);
        }
        bool clicked()
        {
            return pressed &&
                        lastMouseState.LeftButton == ButtonState.Pressed &&
                        currentMouseState.X < area.X + area.Width && currentMouseState.X > area.X &&
                        currentMouseState.Y < area.Y + area.Height && currentMouseState.Y > area.Y;
        }
        bool shortCutsPressed()
        {
            if (shortCutKeys != null)
            {

                foreach (Keys key in shortCutKeys)
                {
                    if (currentKeyState.IsKeyUp(key))
                        return false;
                }
                bool allSame = true;
                foreach (Keys key in shortCutKeys)
                {
                    if (lastKeyState[key] != currentKeyState[key])
                        allSame = false;
                }
                if (allSame)
                    return false;

                return true;
            }
            return false;
        }
        void onPressed()
        {
            if (Pressed != null)
                Pressed();
        }
        void registTime()
        {
            time = generalTimer;
        }
    }
}
