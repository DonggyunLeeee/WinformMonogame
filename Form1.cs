using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Test_Layer_Points;
using Poly2Tri;
using Poly2Tri.Triangulation.Polygon;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using SkiaSharp;

namespace WinformMonoGame
{
    public partial class Form1 : Form
    {
        private int layerCnt = 1;
        private Random random = new Random();
        private DrawShapeType drawType = DrawShapeType.LIMIT;
        private bool isDrawing = false;
        private int curThickness = 1;
        private Microsoft.Xna.Framework.Color curColor = new Microsoft.Xna.Framework.Color(255, 255, 255, 255);

        private Game1 gameObject;
        public Form1()
        {
            InitializeComponent();
        }

        public IntPtr GetDrawSurface()
        {
            return pictureBox1.Handle;
        }

        public void SetGameObject(Game1 game)
        {
            gameObject = game;
            gameObject.graphics.PreferredBackBufferWidth = pictureBox1.Width;
            gameObject.graphics.PreferredBackBufferHeight = pictureBox1.Height;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            //gameObject.BtnClicked();
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

            // Temp
            folderBrowserDialog.SelectedPath = "C:\\Repo\\Test_Layer_Points\\layer_points";
            //

            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                string selectedFolder = folderBrowserDialog.SelectedPath;
                Layer_Points layer_points = new Layer_Points();
                if (!layer_points.Read(selectedFolder))
                {
                    return;
                }
                LayerInfo layer = new LayerInfo();
                Microsoft.Xna.Framework.Color foreColor = new Microsoft.Xna.Framework.Color(
                    random.Next(1, 256),
                    random.Next(1, 256),
                    random.Next(1, 256)
                );

                layer.foreColor = foreColor;
                layer.texture = new Texture2D(gameObject.GraphicsDevice, gameObject.GraphicsDevice.Viewport.Width, gameObject.GraphicsDevice.Viewport.Height);

                for (int i = 0; i < layer_points.point_index.Count; i++)
                {
                    int start_pos = layer_points.point_index[i].pos_start;
                    int count = layer_points.point_index[i].count;

                    if (count < 3)
                    {
                        continue;
                    }

                    float[] arr_X = new float[count];
                    float[] arr_Y = new float[count];

                    Array.Copy(layer_points.point_X, start_pos, arr_X, 0, count);
                    Array.Copy(layer_points.point_Y, start_pos, arr_Y, 0, count);

                    var points = new List<PolygonPoint>();
                    for (int j = 0; j < count; j++)
                    {
                        points.Add(new PolygonPoint((double)arr_X[j], gameObject.GraphicsDevice.Viewport.Height - (double)arr_Y[j]));
                    }

                    var polyline = new Polygon(points);
                    P2T.Triangulate(polyline);
                    Microsoft.Xna.Framework.Color gray;
                    
                    if (layer_points.point_index[i].polarity == 'P')
                    {
                        gray = foreColor;
                    }
                    else
                    {
                        gray = new Microsoft.Xna.Framework.Color(0, 0, 0);
                    }

                    foreach (var triangle in polyline.Triangles)
                    {
                        layer.triangleVertices.Add(new VertexPositionColor(new Vector3((float)triangle.Points[2].X, (float)-triangle.Points[2].Y, 0), gray));
                        layer.triangleVertices.Add(new VertexPositionColor(new Vector3((float)triangle.Points[1].X, (float)-triangle.Points[1].Y, 0), gray));
                        layer.triangleVertices.Add(new VertexPositionColor(new Vector3((float)triangle.Points[0].X, (float)-triangle.Points[0].Y, 0), gray));
                    }
                }

                gameObject.layers.Add(layer);

                CheckBox chkBox = new CheckBox
                {
                    Text = $"Layer {layerCnt}",
                    AutoSize = true,
                    Location = new System.Drawing.Point(20, this.button1.Location.Y + ((layerCnt-1) * 40) + 50)
                };
                chkBox.CheckedChanged += new EventHandler(CheckBox_CheckedChanged);
                this.Controls.Add(chkBox);

                TrackBar layerTrackBar = new TrackBar();
                layerTrackBar.Text = $"Layer {layerCnt}";
                layerTrackBar.Location = new System.Drawing.Point(100, chkBox.Location.Y);
                layerTrackBar.Minimum = 0;
                layerTrackBar.Maximum = 255;
                layerTrackBar.Value = 255;
                layerTrackBar.TickFrequency = 10;
                layerTrackBar.Scroll += new EventHandler(LayerTrackBar_Scroll);
                this.Controls.Add(layerTrackBar);

                layerCnt++;
            }
        }

        private void CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chkBox = sender as CheckBox;
            
            if (chkBox != null)
            {
                int num = int.Parse(chkBox.Text.Split(' ')[1]);
                if (chkBox.Checked)
                {
                    gameObject.layers[num].isDraw = true;
                }
                else
                {
                    gameObject.layers[num].isDraw = false;
                }
            }
        }

        private void LayerTrackBar_Scroll(object sender, EventArgs e)
        {
            TrackBar trackBar = sender as TrackBar;

            if (trackBar != null)
            {
                int num = int.Parse(trackBar.Text.Split(' ')[1]);
                gameObject.layers[num].transparency = trackBar.Value;
            }
        }

        private void PictureBox1_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            gameObject.mouse_delta = e.Delta;
            gameObject.mouse_position = new Vector2(e.Location.X, e.Location.Y);
        }

        private void PictureBox1_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            gameObject.mouse_position = new Vector2(e.Location.X, e.Location.Y);
            
            if (e.Button == MouseButtons.Left)
            {
                if (drawType != DrawShapeType.LIMIT)
                {
                    switch (drawType)
                    {
                        case DrawShapeType.RECTANGLE:
                            if (!isDrawing)
                            {
                                ShapeInfo<object> shape = new ShapeInfo<object>();
                                shape.color = new Microsoft.Xna.Framework.Color(curColor.R, curColor.G, curColor.B, (byte)50);
                                shape.thickness = curThickness;
                                shape.drawType = drawType;
                                shape.shape = new ShapeSquare(new Vector2(e.Location.X, e.Location.Y));
                                gameObject.layers[0].shapes.Add(shape);
                                isDrawing = true;
                            }
                            else
                            {
                                ShapeSquare shape = (ShapeSquare)gameObject.layers[0].GetLastShape().shape;
                                Vector2 start = shape.leftTop;
                                Vector2 end = new Vector2(e.Location.X, e.Location.Y);

                                shape.leftTop.X = start.X < end.X ? start.X : end.X;
                                shape.leftTop.Y = start.Y < end.Y ? start.Y : end.Y;
                                shape.width = Math.Abs(start.X - end.X);
                                shape.height = Math.Abs(start.Y - end.Y);
                                gameObject.layers[0].GetLastShape().color = curColor;
                                isDrawing = false;
                            }
                            break;
                        case DrawShapeType.CIRCLE:
                            if (!isDrawing)
                            {
                                ShapeInfo<object> shape = new ShapeInfo<object>();
                                shape.color = new Microsoft.Xna.Framework.Color(curColor.R, curColor.G, curColor.B, (byte)50);
                                shape.thickness = curThickness;
                                shape.drawType = drawType;
                                shape.shape = new ShapeCircle(new Vector2(e.Location.X, e.Location.Y));
                                gameObject.layers[0].shapes.Add(shape);
                                isDrawing = true;
                            }
                            else
                            {
                                ShapeCircle shape = (ShapeCircle)gameObject.layers[0].GetLastShape().shape;
                                Vector2 start = shape.leftTop;
                                Vector2 end = new Vector2(e.Location.X, e.Location.Y);

                                shape.center = (end + start) / 2;
                                shape.radius = ((end - start).Length()) / 2;
                                gameObject.layers[0].GetLastShape().color = curColor;
                                isDrawing = false;
                            }
                            break;
                        case DrawShapeType.POLYGON:
                            {
                                if (!isDrawing)
                                {
                                    ShapeInfo<object> shape = new ShapeInfo<object>();
                                    shape.color = new Microsoft.Xna.Framework.Color(curColor.R, curColor.G, curColor.B, (byte)50);
                                    shape.thickness = curThickness;
                                    shape.drawType = drawType;
                                    shape.shape = new ShapePolygon();
                                    gameObject.layers[0].shapes.Add(shape);

                                    ShapePolygon tmp = (ShapePolygon)gameObject.layers[0].GetLastShape().shape;
                                    tmp.points.Add(new Vector2(e.Location.X, e.Location.Y));
                                    tmp.points.Add(new Vector2(e.Location.X, e.Location.Y));
                                    isDrawing = true;
                                }
                                ShapePolygon polygon = (ShapePolygon)gameObject.layers[0].GetLastShape().shape;
                                polygon.points[polygon.points.Count - 1] = new Vector2(e.Location.X, e.Location.Y);
                                polygon.points.Add(new Vector2(e.Location.X, e.Location.Y));
                            }
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    gameObject.isDragging = true;
                    gameObject.prevmousePosition = new Vector2(e.Location.X, e.Location.Y);
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                if (drawType != DrawShapeType.LIMIT)
                {
                    switch (drawType)
                    {
                        case DrawShapeType.RECTANGLE:
                            break;
                        case DrawShapeType.CIRCLE:
                            break;
                        case DrawShapeType.POLYGON:
                            ShapePolygon polygon = (ShapePolygon)gameObject.layers[0].GetLastShape().shape;
                            gameObject.layers[0].GetLastShape().color = curColor;
                            if (isDrawing && polygon.points.Count > 1)
                            {
                                polygon.points[polygon.points.Count - 1] = new Vector2(polygon.points[0].X, polygon.points[0].Y);
                                isDrawing = false;
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private void PictureBox1_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            gameObject.mouse_position = new Vector2(e.Location.X, e.Location.Y);

            if (e.Button == MouseButtons.Left)
            {
                if (drawType != DrawShapeType.LIMIT)
                {
                    switch (drawType)
                    {
                        case DrawShapeType.RECTANGLE:
                            break;
                        case DrawShapeType.CIRCLE:
                            break;
                        case DrawShapeType.POLYGON:
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    gameObject.isDragging = false;
                }
            }
        }

        private void PictureBox1_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            gameObject.mouse_position = new Vector2(e.Location.X, e.Location.Y);

            this.label1.Text = $"X: {e.X}, Y: {e.Y}";

            if (drawType != DrawShapeType.LIMIT && isDrawing)
            {
                switch (drawType)
                {
                    case DrawShapeType.RECTANGLE:
                        {
                            ShapeSquare shape = (ShapeSquare)gameObject.layers[0].GetLastShape().shape;
                            Vector2 start = shape.leftTop;
                            Vector2 end = new Vector2(e.Location.X, e.Location.Y);

                            shape.width = (end.X - start.X);
                            shape.height = (end.Y - start.Y);
                        }
                        break;
                    case DrawShapeType.CIRCLE:
                        {
                            ShapeCircle shape = (ShapeCircle)gameObject.layers[0].GetLastShape().shape;
                            Vector2 start = shape.leftTop;
                            Vector2 end = new Vector2(e.Location.X, e.Location.Y);

                            shape.center = (end + start) / 2;
                            shape.radius = ((end - start).Length()) / 2;
                        }
                        break;
                    case DrawShapeType.POLYGON:
                        {
                            ShapePolygon polygon = (ShapePolygon)gameObject.layers[0].GetLastShape().shape;
                            polygon.points[polygon.points.Count - 1] = new Vector2(e.Location.X, e.Location.Y);
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private void toolStripButton_Click(object sender, EventArgs e, int index)
        {
            const int buttonNum = 3;
            ToolStripButton toolStripButton = sender as ToolStripButton;

            if (toolStripButton.Checked)
            {
                toolStripButton.Checked = !toolStripButton.Checked;
                drawType = DrawShapeType.LIMIT;
            }
            else
            {
                for (int i = 0; i < buttonNum; i++) 
                {
                    if(index == i + 1)
                    {
                        toolStripButton.Checked = true;
                    }
                    else
                    {
                        ToolStripButton tmp = (ToolStripButton)this.toolStrip1.Items[i];
                        tmp.Checked = false;
                    }
                }
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            drawType = DrawShapeType.RECTANGLE;
            toolStripButton_Click(sender, e, 1);
            isDrawing = false;
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            drawType = DrawShapeType.CIRCLE;
            toolStripButton_Click(sender, e, 2);
            isDrawing = false;
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            drawType = DrawShapeType.POLYGON;
            toolStripButton_Click(sender, e, 3);
            isDrawing = false;
        }

        private void ToolStripComboBox1_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            string number = new string(this.toolStripComboBox1.SelectedItem.ToString().Where(char.IsDigit).ToArray());
            curThickness = int.Parse(number);
        }

        private void ToolStripComboBox2_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            string str = this.toolStripComboBox2.SelectedItem.ToString();
            switch (str)
            {
                case "White":
                    curColor = Microsoft.Xna.Framework.Color.White;
                    break;
                case "Red":
                    curColor = Microsoft.Xna.Framework.Color.Red;
                    break;
                case "Blue":
                    curColor = Microsoft.Xna.Framework.Color.Blue;
                    break;
                case "Green":
                    curColor = Microsoft.Xna.Framework.Color.Green;
                    break;
                case "Yellow":
                    curColor = Microsoft.Xna.Framework.Color.Yellow;
                    break;
                case "User Defined":
                    ColorDialog colorDialog = new ColorDialog();
                    if (colorDialog.ShowDialog() == DialogResult.OK)
                    {
                        curColor = new Microsoft.Xna.Framework.Color(colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B, colorDialog.Color.A);
                    }
                    break;
            }
        }

        private void pictureBox1_SizeChanged(object sender, EventArgs e)
        {
            gameObject.graphics.PreferredBackBufferWidth = pictureBox1.Width;
            gameObject.graphics.PreferredBackBufferHeight = pictureBox1.Height;
            gameObject.graphics.ApplyChanges();
            gameObject.renderTarget = new RenderTarget2D(gameObject.GraphicsDevice, pictureBox1.Width, pictureBox1.Height);
            gameObject.skSurface = SKSurface.Create(new SKImageInfo(pictureBox1.Width, pictureBox1.Height));
        }
    }
}