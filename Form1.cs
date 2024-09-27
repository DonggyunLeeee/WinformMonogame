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
        private int checkBoxCnt = 1;

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
                    float Z;
                    if (layer_points.point_index[i].polarity == 'P')
                    {
                        gray = Microsoft.Xna.Framework.Color.White;
                        Z = 0.0f;
                    }
                    else
                    {
                        gray = Microsoft.Xna.Framework.Color.Red;
                        Z = 0.0f;
                    }

                    foreach (var triangle in polyline.Triangles)
                    {
                        layer.triangleVertices.Add(new VertexPositionColor(new Vector3((float)triangle.Points[2].X, (float)-triangle.Points[2].Y, Z), gray));
                        layer.triangleVertices.Add(new VertexPositionColor(new Vector3((float)triangle.Points[1].X, (float)-triangle.Points[1].Y, Z), gray));
                        layer.triangleVertices.Add(new VertexPositionColor(new Vector3((float)triangle.Points[0].X, (float)-triangle.Points[0].Y, Z), gray));
                    }
                }

                gameObject.layers.Add(layer);

                CheckBox chkBox = new CheckBox
                {
                    Text = $"Layer {checkBoxCnt}",
                    AutoSize = true,
                    Location = new System.Drawing.Point(20, (checkBoxCnt * 30) + 20)
                };

                checkBoxCnt++;
                chkBox.CheckedChanged += new EventHandler(CheckBox_CheckedChanged);
                this.Controls.Add(chkBox);
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
                    gameObject.needUpdate = true;
                }
                else
                {
                    gameObject.layers[num - 1].isDraw = false;
                }
            }
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
