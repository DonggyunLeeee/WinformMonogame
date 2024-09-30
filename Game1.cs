using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Poly2Tri;
using Poly2Tri.Triangulation.Polygon;
using Test_Layer_Points;
using System.Windows.Forms;
using System.Linq;

namespace WinformMonoGame
{
    public class LayerInfo
    {
        public List<VertexPositionColor> triangleVertices;
        public bool isDraw;
        public Color foreColor;
        public Texture2D texture;
        public int transparency;

        public LayerInfo()
        {
            triangleVertices = new List<VertexPositionColor>();
            isDraw = false;
        }
    }

    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private IntPtr drawSurface;
        private BasicEffect basicEffect;
        private SpriteBatch spriteBatch;
        RenderTarget2D renderTarget;

        private Vector2 posOffset;
        private double scale = 0.1;
        private bool isDragging = false;
        private Vector2 prevmousePosition;

        public List<LayerInfo> layers;

        private List<VertexPositionColor> drawingVertices;

        public int mouse_delta = 0;
        public int mouse_X = 0;
        public int mouse_Y = 0;
        public bool mouse_left_clicked = false;
        public bool mouse_move = false;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        public Game1(IntPtr drawSurface) : this()
        {
            this.drawSurface = drawSurface;
            graphics.PreparingDeviceSettings += new EventHandler<PreparingDeviceSettingsEventArgs>(graphics_PreparingDeviceSettings);
            System.Windows.Forms.Control.FromHandle((this.Window.Handle)).VisibleChanged += new EventHandler(Game1_VisibleChanged);
        }

        protected override void Initialize()
        {
            base.Initialize();

            drawingVertices = new List<VertexPositionColor>();
            posOffset = Vector2.Zero; // 폴리곤의 위치
            layers = new List<LayerInfo>();
        }

        protected override void LoadContent()
        {
            basicEffect = new BasicEffect(GraphicsDevice);
            basicEffect.VertexColorEnabled = true;

            spriteBatch = new SpriteBatch(GraphicsDevice);
            //renderTarget = new RenderTarget2D(GraphicsDevice, Window.ClientBounds.Width, Window.ClientBounds.Height);
            renderTarget = new RenderTarget2D(
                GraphicsDevice,
                Window.ClientBounds.Width, // 원하는 너비
                Window.ClientBounds.Height, // 원하는 높이
                false, // mipMap 사용 여부
                SurfaceFormat.Color,
                DepthFormat.Depth24 // 깊이 버퍼 형식
            );
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
                Exit();

            Vector2 mousePosition = new Vector2(mouse_X, mouse_Y);

            if (mouse_delta != 0)
            {
                double scaleChange = mouse_delta > 0 ? 1.1 : 0.9;
                Vector2 centerToMouse = mousePosition - posOffset;
                scale *= scaleChange;
                posOffset = mousePosition - centerToMouse * (float)scaleChange;
                mouse_delta = 0;
            }

            //if (mouse_move && mouse_left_clicked)
            //{
            //    posOffset += (mousePosition - prevmousePosition);
            //    prevmousePosition = mousePosition;
            //}
            //else if (mouse_left_clicked)
            //{
            //    prevmousePosition = mousePosition;
            //}

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            basicEffect.Projection = Matrix.CreateOrthographicOffCenter
            (
                0, GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height, 0,
                0, 1
            );

            //Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Start();
            //stopwatch.Stop();
            //System.Windows.Forms.MessageBox.Show($"Elapsed time: {stopwatch.ElapsedMilliseconds} ms");

            for (int i = 0; i < layers.Count; i++)
            {
                if (layers[i].isDraw)
                {
                    drawingVertices.Clear();
                    for (int j = 0; j < layers[i].triangleVertices.Count; j++)
                    {
                        double x = (double)layers[i].triangleVertices[j].Position.X * scale + (double)posOffset.X;
                        double y = (double)layers[i].triangleVertices[j].Position.Y * scale + (double)posOffset.Y;
                        drawingVertices.Add(new VertexPositionColor(new Vector3((float)x, (float)y, 0), layers[i].triangleVertices[j].Color));
                    }

                    GraphicsDevice.Clear(Color.Black);
                    GraphicsDevice.SetRenderTarget(renderTarget);

                    spriteBatch.Begin();
                    foreach (var pass in basicEffect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        GraphicsDevice.DrawUserPrimitives(Microsoft.Xna.Framework.Graphics.PrimitiveType.TriangleList, drawingVertices.ToArray(), 0, drawingVertices.Count / 3);
                    }
                    spriteBatch.End();

                    layers[i].texture = renderTarget;
                    layers[i].texture = ApplyTransparency(layers[i].texture, GraphicsDevice, layers[i].foreColor, layers[i].transparency);
                    GraphicsDevice.SetRenderTarget(null);
                }
            }

            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();
            for(int i = 0; i < layers.Count; i++)
            {
                if (layers[i].isDraw)
                {
                    //spriteBatch.Draw(layers[i].texture, Vector2.Zero, Color.White);
                    //spriteBatch.Draw(layers[i].texture, new Vector2(i * 100, i * 100), null, Color.White, 0f, Vector2.Zero, Vector2.One, SpriteEffects.None, (float)(i / layers.Count));
                    spriteBatch.Draw(layers[i].texture, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, Vector2.One, SpriteEffects.None, (float)(i / layers.Count));
                }
            }
            spriteBatch.End();

            base.Draw(gameTime);
        }

        Texture2D ApplyTransparency(Texture2D originalTexture, GraphicsDevice graphicsDevice, Color fore, int transparency)
        {
            Texture2D newTexture = new Texture2D(graphicsDevice, originalTexture.Width, originalTexture.Height);
            Color[] pixelData = new Color[originalTexture.Width * originalTexture.Height];

            originalTexture.GetData(pixelData);

            for (int i = 0; i < pixelData.Length; i++)
            {
                if(pixelData[i].R == fore.R && pixelData[i].G == fore.G && pixelData[i].B == fore.B)
                {
                    pixelData[i] = new Color(fore, transparency);
                }
                else
                {
                    pixelData[i] = new Color(0, 0, 0, 0);
                }
            }

            newTexture.SetData(pixelData);

            return newTexture;
        }

        private List<PolygonPoint> RealToScreen(List<PolygonPoint> points)
        {
            List<PolygonPoint> ret = new List<PolygonPoint>();

            for (int i = 0; i < points.Count; i++)
            {
                double x = (double)points[i].X * scale + (double)posOffset.X;
                double y = (double)points[i].Y * scale + (double)posOffset.Y;
                ret.Add(new PolygonPoint(x, y));
            }

            return ret;
        }

        /// <summary>
        /// Event capturing the construction of a draw surface and makes sure this gets redirected to
        /// a predesignated drawsurface marked by pointer drawSurface
        /// </summary>
        ///<param name="sender"></param>
        ///<param name="e"></param>
        void graphics_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            e.GraphicsDeviceInformation.PresentationParameters.DeviceWindowHandle = drawSurface;
        }

        /// <summary>
        /// Occurs when the original gamewindows' visibility changes and makes sure it stays invisible
        /// </summary>
        ///<param name="sender"></param>
        ///<param name="e"></param>
        private void Game1_VisibleChanged(object sender, EventArgs e)
        {
            if (System.Windows.Forms.Control.FromHandle((this.Window.Handle)).Visible == true)
                System.Windows.Forms.Control.FromHandle((this.Window.Handle)).Visible = false;
        }
    }
}
