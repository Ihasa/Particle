using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
namespace UIComponents
{
    class MousePointer:Pointer
    {
        MouseState current, last;
        public MousePointer(Game game, Texture2D texture, Vector2 iniPosition, Vector2 hotSpot)
            : base(game, texture, iniPosition,new Vector2(32,32), hotSpot)
        {
            current = new MouseState();
            last = new MouseState();
        }
        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            last = current;
            current = Mouse.GetState();
            base.Update(gameTime);
        }
        protected override PointerState GetState()
        {
            PointerState state = new PointerState();
            if (current.LeftButton == ButtonState.Pressed && last.LeftButton == ButtonState.Released)
            {
                state.Accepted = true;
            }
            state.Offset = new Vector2(current.X - last.X, current.Y - last.Y);
            return state;
        }
    }
}
