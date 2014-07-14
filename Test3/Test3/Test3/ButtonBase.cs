using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace UIComponents
{
    abstract class ButtonBase:DrawableGameComponent
    {
        #region 列挙
        enum EventType
        {
            Pressed,Selected
        }
        #endregion
        #region フィールド
        Rectangle area;
        Color color;
        //MouseState currentMouseState, lastMouseState;
        KeyboardState currentKeyState, lastKeyState;
        SpriteBatch spriteBatch;
        Texture2D image;
        SpriteFont font;
        string title;
        EventType eventType;
        int eventTimer, eventTime;
        Keys[] shortCutKeys;
        Pointer pointer;
        #endregion

        #region プロパティ
        public int Left { get { return area.X; } set { area.X = value; } }
        public int Right { get { return area.X + area.Width; } set { area.X = value - area.Width; } }
        public int Top { get { return area.Y; } set { area.Y = value; } }
        public int Bottom { get { return area.Y + area.Height; } set { area.Y = value - area.Height; } }
        public Vector2 Center { get { return new Vector2(Center.X, Center.Y); } set { area = new Rectangle((int)value.X - area.Width / 2, (int)value.Y - area.Height / 2, area.Width, area.Height); } }
        public int Center_X { get { return (int)Center.X; } set { area.X = value - area.Width / 2; } }
        public int Center_Y {get {return (int)Center.Y;}set{area.Y = value - area.Height/2;}}
        public int Width{get{return area.Width;}set{area.Width = value;}}
        public int Height{get{return area.Height;}set{area.Height = value;}}

        public bool MouseEnabled { get; set; }
        public bool IsPressed { get { return MouseEnabled && pointer.Accepted && pointer.HotSpot.IsOn(area); } }
        //public bool IsReleased { get { return MouseEnabled && !pointer.Accepted; } }
        public abstract bool IsSelected { get; }
        //public abstract bool IsDeselected { get; }

        #endregion

        #region イベント
        public event Action Pressed;
        //public event Action Released;
        public event Action Selected;
        //public event Action Deselected;
        #endregion

        #region コンストラクタ
        public ButtonBase(Game game,Rectangle rect,string title,Texture2D tex,SpriteFont fonts,Keys[] shortCuts,Pointer p)
            : base(game)
        {
            spriteBatch = new SpriteBatch(game.GraphicsDevice);
            area = rect;
            color = Color.White;
            this.title = title;
            image = tex;
            this.font = fonts;
            shortCutKeys = new Keys[shortCuts.Length];
            shortCuts.CopyTo(shortCutKeys,0);

            Pressed += () => { eventType = EventType.Pressed; eventTime = eventTimer; };
            Selected += () => { eventType = EventType.Selected; eventTime = eventTimer; };

            currentKeyState = new KeyboardState();
            lastKeyState = new KeyboardState();

            pointer = p;
        }
        #endregion
        
        #region 操作
        public bool IsInArea(Vector2 point)
        {
            return 
                point.X > area.X && point.X < area.X + area.Width &&
                point.Y > area.Y && point.Y < area.Y + area.Height;
        }

        #endregion

        #region 更新・描画
        public override void Update(GameTime gameTime)
        {
            lastKeyState = currentKeyState;
            //lastMouseState = currentMouseState;
            currentKeyState = Keyboard.GetState();
            //currentMouseState = Mouse.GetState();

            if ((IsPressed || shortCutsPressed()) && Pressed != null )
            {
                Pressed();
            }
            //if (IsReleased && Released != null)
            //{
            //    Released();
            //    eventTime = eventTimer;
            //}
            if (IsSelected && Selected != null)
            {
                Selected();
            }
            //if (IsDeselected && Deselected != null)
            //{
            //    Deselected();
            //    eventTime = eventTimer;
            //}
            eventTimer++;
            base.Update(gameTime);
        }
        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();
            if (eventType == EventType.Pressed)
            {
                PressedAnimation(eventTimer - eventTime);
            }
            else if(eventType == EventType.Selected)
            {
                SelectedAnimation(eventTimer - eventTime);
            }
            spriteBatch.Draw(image, area, color);
            spriteBatch.End();
            base.Draw(gameTime);
        }
        protected virtual void PressedAnimation(int time)
        {
            
        }
        protected virtual void SelectedAnimation(int time)
        {

        }
        #endregion

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


    }
    static class Extensions
    {
        public static Rectangle Scaling(this Rectangle sourceRect,float scale)
        {
            if (scale <= 0)
                throw new InvalidOperationException();

            return new Rectangle(
                (int)(sourceRect.X - (sourceRect.Width * scale - sourceRect.Width) / 2),
                (int)(sourceRect.Y - (sourceRect.Height * scale - sourceRect.Height) / 2),
                (int)(sourceRect.Width * scale),
                (int)(sourceRect.Height * scale)
            );
        }
        public static bool IsOn(this Vector2 point,Rectangle area)
        {
            return
                point.X > area.X && point.X < area.X + area.Width &&
                point.Y > area.Y && point.Y < area.Y + area.Height;
        }
    }
}
