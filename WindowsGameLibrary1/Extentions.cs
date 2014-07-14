using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
namespace Extension
{
    /// <summary>
    /// 拡張メソッドを定義するクラス
    /// </summary>
    static class Extentions
    {
        /// <summary>
        /// 2次元ベクトルを角度(度数)に変換
        /// </summary>
        /// <param name="vec">ベクトル</param>
        /// <returns>角度(度数)</returns>
        public static float ToDegrees(this Vector2 vec)
        {
            vec = Vector2.Normalize(vec);
            float deg = MathHelper.ToDegrees((float)Math.Acos(vec.Y / vec.Length()));
            if (vec.X < 0)
                deg = -deg;
            return deg;
        }
        /// <summary>
        /// 2次元ベクトルを角度(ラジアン)に変換
        /// </summary>
        /// <param name="vec">ベクトル</param>
        /// <returns>角度(ラジアン)</returns>
        public static float ToRadians(this Vector2 vec)
        {
            vec = Vector2.Normalize(vec);
            float rad = (float)(Math.Acos(vec.Y / vec.Length()));
            if (vec.X < 0)
                rad = -rad;
            return rad;
        }


        public static Vector2 DegToVector2(this float degree)
        {
            return new Vector2((float)Math.Cos(MathHelper.ToRadians(degree)), (float)Math.Sin(MathHelper.ToRadians(degree)));
        }
        public static Vector2 RadToVector2(this float radian)
        {
            return new Vector2((float)Math.Cos(radian), (float)Math.Sin(radian));
        }
    }
}
