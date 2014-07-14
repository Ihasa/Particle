using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
namespace UIComponents
{
    //Pointerのそのフレームにおける動きを決定する
    struct PointerState
    {
        /// <summary>
        /// 前フレームからの相対移動量
        /// </summary>
        public Vector2 Offset;
        /// <summary>
        /// 項目を決定するボタンが押されたかどうか
        /// </summary>
        public bool Accepted;
    }
    //ゲーム画面上の一点を指すアイテム
    abstract class Pointer:DrawableGameComponent
    {
        Texture2D image;
        Rectangle bounds;
        Vector2 hotSpotOffset;
        public Texture2D Image { get { return image; } set { image = value; } }
        public Vector2 HotSpot { get { return new Vector2(bounds.X + bounds.Width*hotSpotOffset.X,bounds.Y + bounds.Height*hotSpotOffset.Y); } }
        public Rectangle Bounds { get { return bounds; } set { bounds = value; } }
        SpriteBatch spriteBatch;
        public Pointer(Game game,Texture2D texture,Vector2 position,Vector2 size,Vector2 hotSpotOffset):base(game)
        {
            spriteBatch = new SpriteBatch(game.GraphicsDevice);
            image = texture;
            bounds = new Rectangle((int)position.X, (int)position.Y, (int)size.X, (int)size.Y);
            this.hotSpotOffset = hotSpotOffset;
        }
        public Pointer(Game game, Texture2D texture, Vector2 position, Vector2 hotSpot)
            :this(game,texture,position,new Vector2(texture.Width,texture.Height),hotSpot)
        {
        }
        
        protected abstract PointerState GetState();

        public override void Update(GameTime gameTime)
        {
            control(GetState());
            base.Update(gameTime);
        }
        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(image, bounds, Color.White);
            spriteBatch.End();
            base.Draw(gameTime);
        }
        void control(PointerState state)
        {
            bounds.X += (int)state.Offset.X;
            bounds.Y += (int)state.Offset.Y;
            Accepted = state.Accepted;
        }
        public bool Accepted { get; private set; }
    }
}
