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

        public bool needUpdate;

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
            needUpdate = false;
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
                needUpdate = true;
            }

            //if (mouse_move && mouse_left_clicked)
            //{
            //    posOffset += (mousePosition - prevmousePosition);
            //    prevmousePosition = mousePosition;
            //    needUpdate = true;
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

            drawingVertices.Clear();
            Texture2D texture = new Texture2D(GraphicsDevice, Window.ClientBounds.Width, Window.ClientBounds.Height);

            //Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Start();
            //stopwatch.Stop();
            //System.Windows.Forms.MessageBox.Show($"Elapsed time: {stopwatch.ElapsedMilliseconds} ms");

            bool isDraw = false;
            for(int i = 0; i < layers.Count; i++)
            {
                if (layers[i].isDraw)
                {
                    isDraw = true;
                    break;
                }
            }

            if (isDraw)
            {
                GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Blue, 1.0f, 0);
                GraphicsDevice.SetRenderTarget(renderTarget);

                GraphicsDevice.DepthStencilState = DepthStencilState.Default; // 깊이 버퍼 초기화
                GraphicsDevice.DepthStencilState = new DepthStencilState
                {
                    DepthBufferEnable = true,  // 깊이 버퍼 사용
                    StencilEnable = false
                };

                drawingVertices.Clear();

                for (int i = 0; i < layers.Count; i++)
                {
                    if (layers[i].isDraw)
                    {
                        for(int j = 0; j < layers[i].triangleVertices.Count; j++)
                        {
                            double x = (double)layers[i].triangleVertices[j].Position.X * scale + (double)posOffset.X;
                            double y = (double)layers[i].triangleVertices[j].Position.Y * scale + (double)posOffset.Y;
                            drawingVertices.Add(new VertexPositionColor(new Vector3((float)x, (float)y, layers[i].triangleVertices[j].Position.Z), layers[i].triangleVertices[j].Color));
                        }
                    }
                }

                if (drawingVertices.Count > 0)
                {
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

                    foreach (var pass in basicEffect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        GraphicsDevice.DrawUserPrimitives(Microsoft.Xna.Framework.Graphics.PrimitiveType.TriangleList, drawingVertices.ToArray(), 0, drawingVertices.Count / 3);
                    }
                    spriteBatch.End();

                }

                texture = renderTarget;
                GraphicsDevice.SetRenderTarget(null);
            }

            // 화면을 지우고 렌더 타겟에서 그린 텍스처를 출력합니다.
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Blue, 1.0f, 0);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            spriteBatch.Draw(texture, Vector2.Zero, Color.White);
            //spriteBatch.Draw(polygonTexture, posOffset, null, Color.White, 0f, Vector2.Zero, new Vector2((float)drawScale, (float)drawScale), SpriteEffects.None, 0f);
            spriteBatch.End();

            base.Draw(gameTime);
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

        public void BtnClicked()
        {
            needUpdate = true;
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
