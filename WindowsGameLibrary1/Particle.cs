using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;

using Extension;
namespace VisualEffects
{
    /// <summary>
    /// Particleの特徴を決めるパラメータコレクション
    /// </summary>
    public struct ParticleParams
    {
        public Texture2D InitialTexture;
        public Texture2D FinalTexture;
        public Vector3 EmitPoint;
        public float InitialVelocity;
        public Vector3 Direction;
        public float Radian;
        public Vector3 Attraction;
        public float Acceleration;
        public float InitialSize;
        public float FinalSize;
        public float InitialAlpha;
        public float FinalAlpha;
        public Vector3 InitialColor;
        public Vector3 FinalColor;
        public int LifeTime;
        public int PartsPerEmit;
        public int RotationPerFrame;
        public float Scale;
        public ParticleParams(
            Texture2D tex1, Texture2D tex2,Vector3 emit, float spd, Vector3 dir, float rad, Vector3 accelDirection,float acceleration,
            float szStart, float szEnd,float alphaStart,float alphaEnd,Vector3 iniColor,Vector3 finColor,
            int life, int num,int rotsPerFrame)
        {
            if (tex1 == null)
                throw new ArgumentNullException();
            InitialTexture = tex1;
            FinalTexture = tex2;
            EmitPoint = emit;
            InitialVelocity = spd;
            Direction = dir;
            Radian = rad;
            Attraction = accelDirection;
            Acceleration = acceleration;
            InitialSize = szStart;
            FinalSize = szEnd;
            InitialColor = iniColor;
            FinalColor = finColor;
            InitialAlpha = alphaStart;
            FinalAlpha = alphaEnd;
            LifeTime = life;
            PartsPerEmit = num;
            RotationPerFrame = rotsPerFrame;
            Scale = 1;
        }

    }
    /// <summary>
    /// パーティクルの粒たちを管理する
    /// </summary>
    public class Particle:DrawableGameComponent
    {
        #region ParticleElementオブジェクトの生成に必要なフィールド
        ParticleParams parameters;
        //Texture2D texture;
        //Vector3 emitPoint;
        //float speed;
        //Vector3 direction;
        //float radian;
        //Vector3 acceleration;
        //float sizeStart;
        //float sizeEnd;
        //int lifeTime;
        //int adds;
        //Camera camera;
        Random rand;
        //static Game game;
        Vector3 cameraDirection;
        Matrix cameraView;
        Matrix cameraProjection;
        #endregion

        #region その他のフィールド
        /// <summary>
        /// 粒子リスト
        /// </summary>
        List<ParticleElement> elements;
        /// <summary>
        /// 粒子同士の相互作用
        /// </summary>
        Action<ParticleElement,ParticleElement> interaction;
        #endregion

        #region コンストラクタ
        
        public Particle(Texture2D texture1, Texture2D texture2,Vector3 emit, float spd, Vector3 dir, float rad, Vector3 attraction,float acceleration,
            float szStart, float szEnd, float alphaStart, float alphaEnd, Vector3 iniColor, Vector3 finColor, int life, int num, int rotsPerFrame,Action<ParticleElement,ParticleElement> interaction = null)
            : this(new ParticleParams(texture1, texture2, emit, spd, dir, rad, attraction, acceleration, szStart, szEnd, alphaStart, alphaEnd, iniColor, finColor, life, num, rotsPerFrame),interaction)
        {
        }
        public Particle(/*Camera camera, */ParticleParams parameters,Action<ParticleElement,ParticleElement> interaction = null)
            : base(VisualEffect.Game)
        {
            //this.camera = camera;
            this.parameters = parameters;
            //texture = tex1;
            //emitPoint = emit;
            //speed = spd;
            //direction = dir;
            //radian = rad;
            //acceleration = accel;
            //sizeStart = szStart;
            //sizeEnd = szEnd;
            //lifeTime = life;
            //adds = num;
            rand = new Random();
            elements = new List<ParticleElement>();
            DrawOrder = WholeDrawOrder;
            Normal = null;
            Interaction = interaction;
            MaxParts = 500;
            base.Initialize();
        }
        #endregion

        #region ファイルIO
        /// <summary>
        /// Particleファイルから読み込んだデータでインスタンスを生成
        /// </summary>
        /// <param name="path">Particleファイルへのパス</param>
        /// <returns>Particleオブジェクト</returns>
        public static Particle CreateFromParFile(string path)
        {
            ParticleParams newParams = new ParticleParams();
            //ストリーム生成
            FileInfo fileInfo = new FileInfo(path);
            if (!fileInfo.Exists)
            {
                throw new FileNotFoundException();
            }
            //try
            //{
                BinaryReader reader = new BinaryReader(fileInfo.OpenRead());
                int textures = reader.ReadByte();

                //テクスチャデータの読み込み
                Texture2D texture1;
                Texture2D texture2 = null;
                texture1 = readTexture(reader);
                //もう一つのテクスチャがあればそれも読む
                if (textures == 1)
                    texture2 = readTexture(reader);
                //各種パラメータの設定
                newParams.InitialTexture = texture1;
                newParams.FinalTexture = texture2;
                newParams.EmitPoint = readVec3(reader);
                newParams.InitialVelocity = reader.ReadSingle();
                newParams.Direction = readVec3(reader);
                newParams.Radian = reader.ReadSingle();
                newParams.Attraction = readVec3(reader);
                newParams.Acceleration = reader.ReadSingle();
                newParams.InitialSize = reader.ReadSingle();
                newParams.FinalSize = reader.ReadSingle();
                newParams.InitialAlpha = reader.ReadSingle();
                newParams.FinalAlpha = reader.ReadSingle();
                newParams.InitialColor = readVec3(reader);
                newParams.FinalColor = readVec3(reader);
                newParams.LifeTime = reader.ReadInt32();
                newParams.PartsPerEmit = reader.ReadInt16();
                newParams.RotationPerFrame = reader.ReadInt16();
                reader.Close();
                return new Particle(newParams);
            //}
            //catch (Exception e)
            //{
            //    throw e;
            //}
        }
        static Texture2D readTexture(BinaryReader reader)
        {
            int width = reader.ReadInt32();
            int height = reader.ReadInt32();
            Texture2D texture = new Texture2D(VisualEffect.Game.GraphicsDevice, width, height);
            Color[] data = new Color[width * height];
            for (int i = 0; i < data.Length; i++)
            {
                byte r = reader.ReadByte();
                byte g = reader.ReadByte();
                byte b = reader.ReadByte();
                byte a = reader.ReadByte();
                data[i].R = r;
                data[i].G = g;
                data[i].B = b;
                data[i].A = a;
            }
            texture.SetData(data);
            return texture;
        }
        static Vector3 readVec3(BinaryReader reader)
        {
            return new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }
        /// <summary>
        /// このインスタンスをParticleファイル(*.par)に保存
        /// </summary>
        /// <param name="path">保存先のファイルパス</param>
        /// <returns>保存が成功したかどうか</returns>
        public bool SaveAsParFile(string path,bool showWarning)
        {
            try
            {
                //Vector4[] data = new Vector4[textureData.Length];
                //for (int i = 0; i < data.Length; i++)
                //{
                //    data[i] = textureData[i].ToVector4();
                //}

                //保存先ファイル生成
                FileInfo fileInfo = new FileInfo(path);
                if (fileInfo.Exists&&showWarning)
                {
                    System.Windows.Forms.DialogResult res = System.Windows.Forms.MessageBox.Show("上書きしてよろしいですか?", "Parファイルの保存", System.Windows.Forms.MessageBoxButtons.YesNo);
                    if (res == System.Windows.Forms.DialogResult.No)
                        return false;
                }
                Stream stream = fileInfo.Open(FileMode.OpenOrCreate);
                if (stream == null)
                    return false;

                BinaryWriter writer = new BinaryWriter(stream);

                if (parameters.FinalTexture == null)
                    writer.Write((byte)0);
                else writer.Write((byte)1);
                //テクスチャデータ1の書き込み
                writeTextureToFile(writer, parameters.InitialTexture);
                //テクスチャデータ2の書き込み
                if (parameters.FinalTexture != null)
                {
                    writeTextureToFile(writer, parameters.FinalTexture);
                }
                //その他パラメータ
                writer.Write(parameters.EmitPoint.X); writer.Write(parameters.EmitPoint.Y); writer.Write(parameters.EmitPoint.Z);
                writer.Write(parameters.InitialVelocity);
                writer.Write(parameters.Direction.X); writer.Write(parameters.Direction.Y); writer.Write(parameters.Direction.Z);
                writer.Write(parameters.Radian);
                writer.Write(parameters.Attraction.X); writer.Write(parameters.Attraction.Y); writer.Write(parameters.Attraction.Z);
                writer.Write(parameters.Acceleration);
                writer.Write(parameters.InitialSize);
                writer.Write(parameters.FinalSize);
                writer.Write(parameters.InitialAlpha);
                writer.Write(parameters.FinalAlpha);
                writer.Write(parameters.InitialColor.X); writer.Write(parameters.InitialColor.Y); writer.Write(parameters.InitialColor.Z);
                writer.Write(parameters.FinalColor.X); writer.Write(parameters.FinalColor.Y); writer.Write(parameters.FinalColor.Z);
                writer.Write(parameters.LifeTime);
                writer.Write((Int16)parameters.PartsPerEmit);
                writer.Write((Int16)parameters.RotationPerFrame);
                writer.Close();
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.ToString());
                return false;
            }
            return true;
        }
        void writeTextureToFile(BinaryWriter writer, Texture2D texture)
        {
            int width = texture.Width;
            int height = texture.Height;
            Color[] textureData = new Color[width * height];
            texture.GetData(textureData);

            //テクスチャデータの横幅と縦幅(4B * 2)
            writer.Write(width);
            writer.Write(height);
            //テクスチャーデータ(width*height*(0.5*4)B)
            foreach (Color c in textureData)
            {
                writer.Write(c.R);//R
                writer.Write(c.G);//G
                writer.Write(c.B);//B
                writer.Write(c.A);//A
            }
        }

        /// <summary>
        /// 既存のファイルからテクスチャを読み込む
        /// </summary>
        /// <param name="path">絶対パスor相対パス</param>
        /// <returns>Texture2D型の画像ファイル</returns>
        static Texture2D loadTexture(string path)
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

            Texture2D texture = new Texture2D(VisualEffect.Game.GraphicsDevice, bitmap.Width, bitmap.Height);
            texture.SetData(pixelData);
            //Texture2D texture = Texture2D.FromStream(GraphicsDevice, openFileDialog.OpenFile());

            System.Windows.Forms.MessageBox.Show(texture.Format.ToString());
            return texture;
        }

        #endregion

        #region プロパティ
        public ParticleParams Parameters { get { return parameters; } set { parameters = value; } }
        public Texture2D InitialTexture { get { return parameters.InitialTexture; } set { parameters.InitialTexture = value; } }
        public Texture2D FinalTexture { get { return parameters.FinalTexture; } set { parameters.FinalTexture = value; } }
        public Vector3 Direction { get { return parameters.Direction; } set { parameters.Direction = value; } }
        public Vector3 EmitPoint { get { return parameters.EmitPoint; } set { parameters.EmitPoint = value; } }
        public float InitialVelocity { get { return parameters.InitialVelocity; } set { parameters.InitialVelocity = value; } }
        public float Radian { get { return parameters.Radian; } set { parameters.Radian = value; } }
        public Vector3 Attraction { get { return parameters.Attraction; } set { parameters.Attraction = value; } }
        public float Acceleration { get { return parameters.Acceleration; } set { parameters.Acceleration = value; } }
        public float InitialSize { get { return parameters.InitialSize; } set { parameters.InitialSize = value; } }
        public float FinalSize { get { return parameters.FinalSize; } set { parameters.FinalSize = value; } }
        public float InitialAlpha { get { return parameters.InitialAlpha; } set { parameters.InitialAlpha = value; } }
        public float FinalAlpha { get { return parameters.FinalAlpha; } set { parameters.FinalAlpha = value; } }
        public Vector3 InitialColor { get { return parameters.InitialColor; } set { parameters.InitialColor = value; } }
        public Vector3 FinalColor { get { return parameters.FinalColor; } set { parameters.FinalColor = value; } }
        public int LifeTime { get { return parameters.LifeTime; } set { parameters.LifeTime = value; } }
        public int PartsPerEmit { get { return parameters.PartsPerEmit; } set { parameters.PartsPerEmit = value; } }
        public int RotationPerFrame { get { return parameters.RotationPerFrame; } set { parameters.RotationPerFrame = value; } }
        public float Scale { get { return parameters.Scale; } set { parameters.Scale = value; } }
        public int Rotation { get; set; }
        public int MaxParts { get; set; }
        public Action<ParticleElement,ParticleElement> Interaction { set { interaction = value;} }
        
        //取得用
        public List<ParticleElement> Elements { get { return elements; } }
        public Vector3? Normal { get; set; }
        //public int ElementsCount { get { return elements.Count; } }
        #endregion

        #region 操作
        /// <summary>
        /// 一回分の粒を噴出する
        /// </summary>
        public void Emit()
        {
            if (parameters.PartsPerEmit <= 0)
                throw new ArgumentException();
            //MaxParts以上は発生させない
            if (elements.Count >= MaxParts)
                return;
            //directionに直交する単位ベクトルをひとつ求める
            Vector3 dir;
            if (parameters.Direction.X != 0)
            {
                dir = new Vector3((-parameters.Direction.Y - parameters.Direction.Z) / parameters.Direction.X, 1, 1);
            }
            else if (parameters.Direction.Y != 0)
            {
                dir = new Vector3(1, (-parameters.Direction.X - parameters.Direction.Z) / parameters.Direction.Y, 1);
            }
            else if (parameters.Direction.Z != 0)
            {
                dir = new Vector3(1, 1, (-parameters.Direction.X - parameters.Direction.Y) / parameters.Direction.Z);
            }
            else throw new ArgumentException("方向ベクトルが無効です");
            //正規化
            dir = Vector3.Normalize(dir);

            //MaxPartsを超えないように追加
            int adds = elements.Count + parameters.PartsPerEmit > MaxParts ? MaxParts - elements.Count : PartsPerEmit;
            for (int i = 0; i < adds; i++)
            {
                //ばらつき角度
                float rad = (float)(parameters.Radian * rand.NextDouble());

                //方向をランダムにする
                Vector3 dir2 = Vector3.TransformNormal((float)Math.Cos(rad) * parameters.Direction + (float)Math.Sin(rad) * dir, Matrix.CreateFromAxisAngle(parameters.Direction, (float)(rand.NextDouble() * MathHelper.TwoPi)));
                //追加

                elements.Add(new ParticleElement(
                    GraphicsDevice,
                    parameters.InitialTexture,
                    parameters.FinalTexture,
                    parameters.LifeTime,
                    parameters.EmitPoint,
                    dir2 * parameters.InitialVelocity*parameters.Scale, //new Vector3(dir2.X * parameters.Speed, dir2.Y * parameters.Speed, dir2.Z * parameters.Speed),
                    dir2*parameters.Acceleration*parameters.Scale,
                    //parameters.InitialSize*parameters.Scale,
                    //parameters.FinalSize*parameters.Scale,
                    parameters.RotationPerFrame,
                    Rotation
                    )
                );
            }

            //Game1.debugStr = "" + elements.Count;

        }
        /// <summary>
        /// ビュー行列の設定
        /// </summary>
        /// <param name="cameraPosition"></param>
        /// <param name="cameraTarget"></param>
        /// <param name="cameraUpVector"></param>
        public void SetView(Vector3 cameraPosition, Vector3 cameraTarget, Vector3 cameraUpVector)
        {
            cameraView = Matrix.CreateLookAt(cameraPosition, cameraTarget, cameraUpVector);
            cameraDirection = cameraTarget - cameraPosition;
        }
        /// <summary>
        /// 射影変換行列の設定
        /// </summary>
        /// <param name="projection"></param>
        public void SetProjection(Matrix projection)
        {
            cameraProjection = projection;
        }
        /// <summary>
        /// すべての粒をクリア
        /// </summary>
        public void Reset()
        {
            elements.Clear();
        }

        //public static void Init(Game g)
        //{
        //    game = g;
        //}
        public static int WholeDrawOrder { get; set; }
        #endregion

        #region 更新と描画
        public override void Update(GameTime gameTime)
        {
            //粒の寿命が終わっていたらリストから消す
            for (int i = 0; i < elements.Count; i++)
            {
                if (elements[i].EndLife)
                {
                    elements.RemoveAt(i);
                }
            }
            //粒の状態更新
            foreach (ParticleElement element in elements)
            {
                if (Normal == null)
                    element.Update(-cameraDirection);
                else
                    element.Update((Vector3)Normal);
                element.Color = Vector3.Lerp(Parameters.InitialColor, Parameters.FinalColor, element.Alpha);
                element.Size = MathHelper.Lerp(Parameters.InitialSize, Parameters.FinalSize, element.Alpha) * Scale;
                //外の世界の引力を働かせる
                element.Attract(parameters.Attraction * parameters.Scale);
                //相互作用
                if (interaction != null)
                {
                    for (int i = elements.IndexOf(element); i < elements.Count; i++)
                    {
                        interaction(element, elements[i]);
                    }
                }
            }
        }
        
        public override void Draw(GameTime gameTime)
        {
            //elementsが空の時は何もしない
            if (elements.Count == 0)
                return;
            //elements内のすべてをまとめて描画
            List<VertexPositionNormalTexture> vertexList = new List<VertexPositionNormalTexture>();
            //elements内のすべての頂点情報を格納
            foreach (ParticleElement element in elements)
            {
                if (element.Vertices != null)
                {
                    foreach (VertexPositionNormalTexture vertex in element.Vertices)
                    {
                        vertexList.Add(vertex);
                    }
                }
                else
                    return;
            }
            //普通の配列に直す
            VertexPositionNormalTexture[] vertices = vertexList.ToArray();

            // TODO: Add your drawing code here
            GraphicsDevice.BlendState = BlendState.AlphaBlend;

            VertexBuffer vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionNormalTexture), vertices.Length, BufferUsage.None);
            vertexBuffer.SetData(vertices);

            IndexBuffer indexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits, 6, BufferUsage.None);
            indexBuffer.SetData(ParticleElement.Indices);

            GraphicsDevice.SetVertexBuffer(vertexBuffer);
            GraphicsDevice.Indices = indexBuffer;
            //深度バッファを変更
            GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
            //プリミティブ描画でTexture2Dを描画
            BasicEffect effect = new BasicEffect(GraphicsDevice);
            effect.TextureEnabled = true;
            effect.View = cameraView;
            effect.Projection = cameraProjection;
            
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                for (int i = elements.Count - 1; i >= 0; i-- )
                {
                    effect.Alpha = MathHelper.Lerp(Parameters.InitialAlpha,Parameters.FinalAlpha, elements[i].Alpha);
                    if(elements[i].FinalTexture != null)
                        effect.Texture = blendTexture(elements[i].InitialTexture,elements[i].FinalTexture,elements[i].Alpha);
                    else 
                        effect.Texture = elements[i].InitialTexture;
                    effect.DiffuseColor = elements[i].Color/255.0f;// Vector3.Lerp(Parameters.InitialColor, Parameters.FinalColor, elements[i].Alpha) / 255.0f;
                    
                    //effect.InitialTexture = elements[i].InitialTexture;
                    pass.Apply();
                    GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, i * 4, 0, vertices.Length, 0, ParticleElement.Indices.Length / 3);
                }
            }
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        }

        /// <summary>
        /// tex1とtex2をamountで線形補間したテクスチャを返す
        /// </summary>
        /// <param name="tex1"></param>
        /// <param name="tex2"></param>
        /// <param name="alpha">合成の際の係数(0～1)</param>
        /// <returns></returns>
        private Texture2D blendTexture(Texture2D tex1, Texture2D tex2,float amount)
        {
            //新しいサイズを決定
            int width = (tex1.Width > tex2.Width) ? tex1.Width : tex2.Width;
            int height = (tex1.Height > tex2.Height) ? tex1.Height : tex2.Height;
            //新しいサイズに合わせてテクスチャを生成
            Texture2D res = new Texture2D(this.GraphicsDevice, width, height);

            //tex1,tex2のカラーデータを取得
            Color[] data1 = new Color[tex1.Width * tex1.Height];
            tex1.GetData(data1);
            Color[] data2 = new Color[tex2.Width * tex2.Height];
            tex2.GetData(data2);

            //新しいサイズに合わせたtex1のカラーデータを生成
            Color[] newData1 = new Color[width * height];
            //全てのピクセルを透明(new Color(0,0,0,0))で初期化
            //for (int i = 0; i < newData1.Length; i++)
            //{
            //    newData1[i] = new Color(0,0,0,0);
            //}
            //貼り付け場所を決めるためのオフセットを求める
            int offsetX1 = (width - tex1.Width)/2;
            int offsetY1 = (height -tex1.Height)/2;
            int count1 = 0;
            //for (int y = 0; y < tex1.Height; y++)
            //{
            //    for (int x = 0; x < tex1.Width; x++)
            //    {
            //        newData1[height * (y+offsetY1) + x + offsetX1] = data1[count1++];
            //    }
            //}

            //同様にtex2
            Color[] newData2 = new Color[width * height];
            //for (int i = 0; i < newData2.Length; i++)
            //{
            //    newData2[i] = new Color(0,0,0,0);
            //}
            int offsetX2 = (width - tex2.Width)/2;
            int offsetY2 = (height -tex2.Height)/2;
            int count2 = 0;
            //for (int y = 0; y < tex2.Height; y++)
            //{
            //    for (int x = 0; x < tex2.Width; x++)
            //    {
            //        newData2[height * (y+offsetY2) + x+offsetX2] = data2[count2++];
            //    }
            //}

            //両方同時に設定
            //for (int y = 0; y < height; y++)
            //{
            //    for (int x = 0; x < width; x++)
            //    {
            //        if (x >= offsetX1 && x < offsetX1 + tex1.Width &&
            //            y >= offsetY1 && y < offsetY1 + tex1.Height)
            //        {
            //            newData1[height * y + x] = data1[count1++];
            //        }
            //        if (x >= offsetX2 && x < offsetX2 + tex2.Width &&
            //            y >= offsetY2 && y < offsetY2 + tex2.Height)
            //        {
            //            newData2[height * y + x] = data2[count2++];
            //        }
            //    }
            //}


            //newData1とnewData2をfactor : 1-factorで合成
            Color[] resultData = new Color[width*height];
            //for (int i = 0; i < width*height; i++)
            //{
            //    //線形補間でOK?
            //    //Color newColor = new Color(
            //    //    (int)MathHelper.SmoothStep(newData1[i].R, newData2[i].R, amount),
            //    //    (int)MathHelper.SmoothStep(newData1[i].G, newData2[i].G, amount),
            //    //    (int)MathHelper.SmoothStep(newData1[i].B, newData2[i].B, amount),
            //    //    (int)MathHelper.SmoothStep(newData1[i].A, newData2[i].A, amount)
            //    //);
            //    Color newColor = Color.Lerp(newData1[i], newData2[i], amount);
            //    resultData[i] = newColor;
            //}
          
            //カラーデータをブレンド
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color c1 = new Color(0, 0, 0, 0);
                    Color c2 = new Color(0, 0, 0, 0);
                    /*if (x >= offsetX1 && x < offsetX1 + tex1.Width &&
                        y >= offsetY1 && y < offsetY1 + tex1.Height&&
                        x >= offsetX2 && x < offsetX2 + tex2.Width &&
                        y >= offsetY2 && y < offsetY2 + tex2.Height)
                    {
                        resultData[height * y + x] = Color.Lerp(data1[count1++], data2[count2++], amount);
                    }
                    else*/
                    if (x >= offsetX1 && x < offsetX1 + tex1.Width &&
                        y >= offsetY1 && y < offsetY1 + tex1.Height)
                    {
                        c1 = data1[count1++];
                        //resultData[height * y + x] = Color.Lerp(data1[count1++],resultData[height * y+x],amount);
                    }
                    if (x >= offsetX2 && x < offsetX2 + tex2.Width &&
                        y >= offsetY2 && y < offsetY2 + tex2.Height)
                    {
                        c2 = data2[count2++];
                        //resultData[height * y + x] = Color.Lerp(resultData[height*y+x],data2[count2++],amount);
                    }
                    resultData[width * y + x] = Color.Lerp(c1, c2, amount);
                }
            }
            res.SetData(resultData);
            return res;
        }
        #endregion
        #region 汎用性の高そうなインスタンスを返す静的メソッド
        //particle = new Particle(GraphicsDevice, particleTex, Vector3.Zero, 0.1f, Vector3.Normalize(Vector3.Up),MathHelper.ToRadians(20),new Vector3(0,0,0), 1f,0f,30,1);//炎
        //particle = new Particle(GraphicsDevice, particleTex, Vector3.Zero, 0.1f, Vector3.Normalize(Vector3.Up), MathHelper.ToRadians(10), new Vector3(0, 0, 0), 0.1f, 3f, 120,1);//普通の煙
        //particle = new Particle(GraphicsDevice, particleTex, Vector3.Zero, 0f, Vector3.Normalize(Vector3.Up), 0, new Vector3(0, 0, 0), 1f, 3f, 20,1);//エフェクト単体
        //particle = new Particle(GraphicsDevice, particleTex, Vector3.Zero, 0.4f, Vector3.Normalize(Vector3.Up),MathHelper.ToRadians(15),new Vector3(0,-0.008f,0), 1f,5f,90,1);//噴水を作りたかった
        //particle = new Particle(GraphicsDevice, particleTex, Vector3.Zero, 0.4f, Vector3.Normalize(Vector3.Up), MathHelper.Pi, new Vector3(0, 0, 0), 1f, 1f, 20,2);//火花
        //particle = new Particle(GraphicsDevice, particleTex, Vector3.Zero, 1f, Vector3.Normalize(Vector3.Up), MathHelper.Pi, new Vector3(0, 0, 0), 1f, 10f, 20,180);//爆発
        //public static ParticleParams GetFireParticle(Texture2D texture,Vector3 position, float size)
        //{
        //    return new ParticleParams(texture, position, 0.1f * size, Vector3.Normalize(Vector3.Up), MathHelper.ToRadians(20), new Vector3(0, 0, 0),0, 1f * size, 0.01f * size,1,0, 30, 1);
        //}
        //public static ParticleParams GetStandardParticle(Texture2D texture, Vector3 position, float size)
        //{
        //    return new ParticleParams(texture, position, 0.1f * size, Vector3.Normalize(Vector3.Up), MathHelper.ToRadians(10), new Vector3(0, 0, 0),0, 0.1f * size, 3f * size,1,0, 120, 1);
        //}
        //public static ParticleParams GetExplosionParticle(Texture2D texture, Vector3 position, float size)
        //{
        //    return new ParticleParams(texture, position, 1f * size, Vector3.Normalize(Vector3.Up), MathHelper.Pi, new Vector3(0, 0, 0),0, 1f * size, 30f * size, 1,0,20, 180);
        //}

        #endregion
    }
}
