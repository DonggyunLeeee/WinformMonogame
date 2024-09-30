using System;
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

namespace WinformMonoGame
{
    public partial class Form1 : Form
    {
        private int layerCnt = 1;
        private Random random = new Random();

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
                layer_points.Read(selectedFolder);
                LayerInfo layer = new LayerInfo();
                Microsoft.Xna.Framework.Color foreColor = new Microsoft.Xna.Framework.Color(
                    random.Next(1, 256),
                    random.Next(1, 256),
                    random.Next(1, 256)
                );

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
                    layer.foreColor = foreColor;
                    layer.texture = new Texture2D(gameObject.GraphicsDevice, gameObject.GraphicsDevice.Viewport.Width, gameObject.GraphicsDevice.Viewport.Height);

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
                    gameObject.layers[num - 1].isDraw = true;
                }
                else
                {
                    gameObject.layers[num - 1].isDraw = false;
                }
            }
        }

        private void LayerTrackBar_Scroll(object sender, EventArgs e)
        {
            TrackBar trackBar = sender as TrackBar;

            if (trackBar != null)
            {
                int num = int.Parse(trackBar.Text.Split(' ')[1]);
                gameObject.layers[num - 1].transparency = trackBar.Value;
            }

            //int layerIndex = layerTrackBars.IndexOf(trackBar);
            //float transparency = trackBar.Value / 100f;
            //game.SetLayerTransparency(layerIndex, transparency);
        }

        private void PictureBox1_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            gameObject.mouse_delta = e.Delta;
            gameObject.mouse_X = e.X;
            gameObject.mouse_Y = e.Y;
        }

        private void PictureBox1_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            gameObject.mouse_X = e.X;
            gameObject.mouse_Y = e.Y;
            
            if (e.Button == MouseButtons.Left)
            {
                gameObject.mouse_left_clicked = true;
            }
        }

        private void PictureBox1_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            gameObject.mouse_X = e.X;
            gameObject.mouse_Y = e.Y;

            if (e.Button == MouseButtons.Left)
            {
                gameObject.mouse_left_clicked = false;
                gameObject.mouse_move = false;
            }
        }

        private void PictureBox1_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            gameObject.mouse_X = e.X;
            gameObject.mouse_Y = e.Y;

            if (e.Button == MouseButtons.Left)
            {
                gameObject.mouse_move = true;
            }
        }
    }
}
