using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Extension;
namespace VisualEffects
{
    public class TexturePlane:DrawableGameComponent
    {
        #region フィールド
        Texture2D texture;
        GraphicsDevice graphicsDevice;
        Vector3 position;
        Vector3 normal;
        Vector2 size;
        bool lighting;
        float rotation;
        /// <summary>
        /// 描画に必要な4つの点
        /// </summary>
        public VertexPositionNormalTexture[] Vertices { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public static short[] Indices
        {
            get
            {
                return new short[]{
                       0,1,2,
                       2,1,3
                };
            }
        }
        #endregion

        #region コンストラクタ
        public TexturePlane(Texture2D texture,Vector3 position,Vector3 normal,float rotation,Vector2 size,bool lighting)
            :base(VisualEffect.Game)
        {
            this.graphicsDevice = VisualEffect.Game.GraphicsDevice;
            this.texture = texture;
            this.position = position;
            this.normal = normal;
            this.rotation = rotation;
            this.size = size;

            this.lighting = lighting;
        }
        #endregion

        /// <summary>
        /// この板一枚だけを描画
        /// </summary>
        public virtual void Draw(Matrix view,Matrix projection)
        {
            VertexBuffer vertexBuffer = new VertexBuffer(this.graphicsDevice,typeof(VertexPositionNormalTexture),Vertices.Length,BufferUsage.None);
            vertexBuffer.SetData(Vertices);
            IndexBuffer indexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, Indices.Length, BufferUsage.None);
            indexBuffer.SetData(Indices);
            graphicsDevice.SetVertexBuffer(vertexBuffer);
            graphicsDevice.Indices = indexBuffer;
            BasicEffect effect = new BasicEffect(graphicsDevice);
            effect.View = view;
            effect.Projection = projection;
            if (lighting)
                effect.EnableDefaultLighting();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, Vertices.Length, 0, 2);
            }
        }
        public virtual void Update()
        {
            setVertices();
        }
        #region メソッド(private)
        private void setVertices()
        {
            //float size = MathHelper.Lerp(sizeStart, sizeEnd, Alpha);

            Vector3[] vecs = new Vector3[]{
                new Vector3(-size.X,size.Y,0),
                new Vector3(size.X,size.Y,0),
                new Vector3(-size.X,-size.Y,0),
                new Vector3(size.X,-size.Y,0)
                };

            for (int i = 0; i < vecs.Length; i++)
            {
                vecs[i] = new Vector3(
                    (float)(vecs[i].X * Math.Cos(MathHelper.ToRadians(rotation)) + vecs[i].Y * Math.Sin(MathHelper.ToRadians(rotation))),
                    (float)(vecs[i].X * -Math.Sin(MathHelper.ToRadians(rotation)) + vecs[i].Y * Math.Cos(MathHelper.ToRadians(rotation))),
                    vecs[i].Z
                );
            }
            //normalに応じて回転角度を求める
            float rx, ry;
            turn(out rx, out ry);
            //回転
            for (int i = 0; i < vecs.Length; i++)
            {
                vecs[i] = Vector3.Transform(vecs[i], Matrix.CreateRotationX(rx) * Matrix.CreateRotationY(ry));
            }

            Vertices = new VertexPositionNormalTexture[]{
                    new VertexPositionNormalTexture(position + vecs[0], normal, new Vector2(0, 0)),
                    new VertexPositionNormalTexture(position + vecs[1], normal, new Vector2(1, 0)),
                    new VertexPositionNormalTexture(position + vecs[2], normal, new Vector2(0, 1)),
                    new VertexPositionNormalTexture(position + vecs[3], normal, new Vector2(1, 1))
                };
        }
        private void turn(out float radX, out float radY)
        {
            Vector2 vec = new Vector2(normal.X, normal.Z);
            radY = vec.ToRadians();
            radX = (float)Math.Asin(-normal.Y / normal.Length());
        }

        #endregion




    }
}
