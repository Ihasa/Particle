using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace VisualEffects
{
    public class VisualEffect
    {
        public static Game Game { get; private set; }
        public static void Init(Game game)
        {
            Game = game;
        }
    }
}
