using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Test3;
namespace UIComponents
{
    class UpdownButton:DrawableGameComponent
    {
        //現在の値
        decimal value;
        decimal? max, min;
        //一度のボタンクリックで増減する量
        decimal dis;
        Texture2D textureUp, textureDown;
        SpriteFont font;
        Vector2 position;
        int width, height;
        Rectangle area;
        string name;
        SpriteBatch spriteBatch;
        MouseState currentState,lastState;
        KeyboardState currentKey, lastKey;
        int pressedTimer = 0;
        bool pressed = false;
        Vector2 stringDrawPosition;
        bool inputMode;
        string input;
        public bool ShowMaxMin { get; set; }
        public UpdownButton(Game game,decimal iniValue,decimal distance,Texture2D texUp, Texture2D texDown,SpriteFont font, Vector2 position,int width,int height, string name,decimal? max = null,decimal? min = null):base(game)
        {
            if ((max != null&&min!=null)&&(max < min || value > max || value < min))
                throw new ArgumentException();
            DrawOrder = 2;
            value = iniValue;
            dis = distance;
            textureUp = texUp;
            textureDown = texDown;
            this.font = font;
            this.position = position;
            this.width = width;
            this.height = height;
            this.area = new Rectangle((int)position.X, (int)position.Y, width, height);
            this.name = name;
            spriteBatch = new SpriteBatch(game.GraphicsDevice);

            this.max = max;
            this.min = min;

            inputMode = false;
            input = "";
            ShowMaxMin = true;

            currentKey = new KeyboardState();
            lastKey = new KeyboardState();
            currentState = new MouseState();
            lastState = new MouseState();
        }
        public override void Update(GameTime gameTime)
        {
            lastState = currentState;
            currentState = Mouse.GetState();
            lastKey = currentKey;
            currentKey = Keyboard.GetState();

            if (inputMode)
            {
                inputString();
                if (currentState.LeftButton == ButtonState.Pressed && lastState.LeftButton == ButtonState.Released)
                {
                    endInput();
                }
            }
            else
            {
                if (currentState.LeftButton == ButtonState.Pressed)
                {
                    if (lastState.LeftButton == ButtonState.Released || pressedTimer > 20)
                    {
                        //文字エリアが押された場合
                        if (inputAreaClicked())
                        {
                            inputMode = true;
                            //input = value.ToString();
                        }
                        //上下ボタンが押された場合
                        if (currentState.X > area.X && currentState.X < area.X + area.Width)
                        {
                            if (currentState.Y > area.Y && currentState.Y < area.Y + area.Height / 2)
                            {
                                pressed = true;
                                value += dis;
                                if (max != null && value >= max)
                                    value = (decimal)max;
                            }
                            else if (currentState.Y > area.Y + area.Height / 2 && currentState.Y < area.Y + area.Height)
                            {
                                pressed = true;
                                value -= dis;
                                if (min != null && value <= min)
                                    value = (decimal)min;
                            }
                        }
                        else
                        {
                            pressed = false;
                        }
                    }
                    if (pressed) pressedTimer++;
                }
                else { pressedTimer = 0; pressed = false; }
            }
            base.Update(gameTime);
        }
        bool inputAreaClicked()
        {
            int width = (int)font.MeasureString(value.ToString()).X;
            if (width < area.Width*2) width = area.Width*2;
            if (currentState.X > area.X + area.Width && currentState.X < area.X + area.Width + width &&
                            currentState.Y > area.Y && currentState.Y < area.Y + area.Height)
                return true;
            else
                return false;
        }
        void inputString()
        {
            Keys[] keys = currentKey.GetPressedKeys();
            Keys? key=null;
            foreach (Keys k in keys)
            {
                if (lastKey.IsKeyUp(k))
                {
                    key = k;
                    break;
                }
            }
            if (key != null)
            {
                if (key == Keys.Enter)
                {
                    try
                    {
                        decimal newValue = decimal.Parse(input);
                        if (
                            (min == null || (min != null && newValue >= min)) &&
                            (max == null || (max != null && newValue <= max))
                        )
                        {
                            this.value = newValue;
                        }
                        else
                        {
                            System.Windows.Forms.MessageBox.Show("無効な数値です");
                        }
                        endInput();
                    }
                    catch (Exception)
                    {
                        endInput();
                    }
                }
                else if (key >= Keys.D0 && key <= Keys.D9)
                {
                    input += "" + (int)(key-Keys.D0);
                }
                else if (key >= Keys.NumPad0 && key <= Keys.NumPad9)
                {
                    input += "" + (int)(key - Keys.NumPad0);
                }
                else if (key == Keys.OemPeriod)
                {
                    if(input != "" && !input.Contains('.'))
                    {
                        input += '.';
                    }
                }
                else if (key == Keys.OemMinus)
                {
                    if (input == "")
                        input += "-";
                }
                else if (key == Keys.Back)
                {
                    if (input != "")
                        input = input.Substring(0, input.Length - 1);
                }
                else if (key == Keys.Delete)
                {
                    input = "";
                }
            }
        }
        void endInput()
        {
            inputMode = false;
            input = "";
        }
        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();
            //ボタンの描画
            Color color;
            //上
            if (currentState.X > area.X && currentState.X < area.X + area.Width &&
                currentState.Y > area.Y && currentState.Y < area.Y + area.Height / 2)
            {
                if (pressed)
                    color = new Color(Vector3.One * 0.4f);
                else color = Color.White;

            }
            else
            {
                color = new Color(Vector3.One * 0.7f);
            }
            if (max != null && value == max)
                color = new Color(Vector3.One * 0.25f);
            spriteBatch.Draw(textureUp, new Rectangle(area.X, area.Y, area.Width, area.Height / 2), color);

            //下
            if (currentState.X > position.X && currentState.X < position.X + this.width &&
                currentState.Y > position.Y + height / 2 && currentState.Y < position.Y + height)
            {
                if (pressed)
                    color = new Color(Vector3.One * 0.4f);
                else
                    color = new Color(Vector3.One);
            }
            else
            {
                color = new Color(Vector3.One * 0.7f);
            }
            if (min != null && value == min)
                color = new Color(Vector3.One * 0.25f);

            spriteBatch.Draw(textureDown, new Rectangle(area.X,area.Y + area.Height/2,area.Width,area.Height/2), color);
            
            //値描画
            stringDrawPosition = new Vector2(area.X,area.Y) + new Vector2(width, height/2) - new Vector2(0,font.MeasureString(""+value).Y/2);
            if (inputMode)
            {
                int wid = (int)font.MeasureString("" + input).X;
                if (wid <= area.Width * 2) wid = area.Width * 2;
                spriteBatch.Draw(Game1.pixel, new Rectangle((int)stringDrawPosition.X,(int)stringDrawPosition.Y,wid,area.Height), Color.White);
                spriteBatch.DrawString(font, input, stringDrawPosition, Color.Black);
            }
            else
            {
                int n = dis.ToString().Length - dis.ToString().IndexOf('.') - 1;
                string format = "";
                for (int i = 0; i < n; i++)
                {
                    format += '#';
                }
                string valueStr = value.ToString("0." + format);
                int wid = (int)font.MeasureString(valueStr).X;
                if (wid <= area.Width * 2) wid = area.Width * 2;
                spriteBatch.Draw(Game1.pixel, new Rectangle((int)stringDrawPosition.X, (int)stringDrawPosition.Y, wid, area.Height), new Color(0.2f, 0.2f, 0.2f, 0.1f));
                spriteBatch.DrawString(font, valueStr, stringDrawPosition, Color.Black);
            }
            //値の範囲描画
            if (ShowMaxMin&&(max != null || min != null))
            {
                spriteBatch.DrawString(font, name, new Vector2(area.X,area.Y) + new Vector2(0, height / 2) - font.MeasureString(name), Color.Black);

                StringBuilder s = new StringBuilder();
                s.Append('(');
                if (min != null)
                {
                    s.Append(min.ToString());
                }
                s.Append(" ~ ");
                if (max != null)
                    s.Append(max.ToString());
                s.Append(')');
                string str = s.ToString();
                Vector2 measure = font.MeasureString(str);
                spriteBatch.DrawString(font, str, position + new Vector2(0, height / 2) - new Vector2(measure.X, 0), Color.Black);
            }
            else
            {
                spriteBatch.DrawString(font, name, position + new Vector2(0, height / 2) - new Vector2(font.MeasureString(name).X,font.MeasureString(name).Y/2), Color.Black);

            }
            spriteBatch.End();
            base.Draw(gameTime);
        }

        public decimal GetValue()
        {
            return value;
        }
        public void SetValue(decimal val)
        {
            value = val;
        }
        public Vector2 GetStringDrawPosition()
        {
            return stringDrawPosition;
        }
    }
}
