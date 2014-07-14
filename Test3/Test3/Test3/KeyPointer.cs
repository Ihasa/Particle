using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
namespace UIComponents
{
    class KeyPointer:Pointer
    {
        KeyboardState current, last;
        public KeyPointer(Game game, Texture2D texture, Vector2 position, Vector2 hotSpot)
            : base(game, texture, position, hotSpot)
        {
            current = new KeyboardState();
            last = new KeyboardState();
        }
        protected override PointerState GetState()
        {
            PointerState state = new PointerState();
            float offsetY = 0;
            float offsetX = 0;
            if (current.IsKeyDown(Keys.Up))
                offsetY = -5;
            else if (current.IsKeyDown(Keys.Down))
                offsetY = 5;
            if (current.IsKeyDown(Keys.Left))
                offsetX = -5;
            else if (current.IsKeyDown(Keys.Right))
                offsetX = 5;
            state.Offset = new Vector2(offsetX, offsetY);
            state.Accepted = current.IsKeyDown(Keys.Enter) && last.IsKeyUp(Keys.Enter);
            return state;
        }
        public override void Update(GameTime gameTime)
        {
            last = current;
            current = Keyboard.GetState();
            base.Update(gameTime);
        }
    }
    class XBoxPointer : Pointer
    {
        GamePadState current, last;
        public XBoxPointer(Game game, Texture2D texture, Vector2 position, Vector2 hotSpot)
            : base(game, texture, position, hotSpot)
        {
            current = new GamePadState();
            last = new GamePadState();
        }
        protected override PointerState GetState()
        {
            PointerState state = new PointerState();
            

            //if (current.IsKeyDown(Keys.Up))
            //    offsetY = -5;
            //else if (current.IsKeyDown(Keys.Down))
            //    offsetY = 5;
            //if (current.IsKeyDown(Keys.Left))
            //    offsetX = -5;
            //else if (current.IsKeyDown(Keys.Right))
            //    offsetX = 5;
            Vector2 direction = current.ThumbSticks.Left;
            direction.Y = -direction.Y;
            state.Offset = direction * 10;
            //System.Windows.Forms.MessageBox.Show("" + state.Offset);
            state.Accepted = current.IsButtonDown(Buttons.A) && last.IsButtonUp(Buttons.A);
            return state;
        }
        public override void Update(GameTime gameTime)
        {
            last = current;
            current = GamePad.GetState(PlayerIndex.One);
            base.Update(gameTime);
        }
    }

}
