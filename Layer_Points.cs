using System;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;

namespace Test_Layer_Points
{

    [Serializable()]
    public class Layer_Points_Index
    {
        public int pos_start { get; set; }
        public int count { get; set; }
        public char polarity { get; set; }

        public Layer_Points_Index()
        {
        }
    }

    public class Layer_Points
    {
        private JsonSerializerOptions json_options = new JsonSerializerOptions { WriteIndented = true };
        private const int MAX_COUNT_POINTS = 15000000;

        public float[] point_X;
        public float[] point_Y;

        public List<Layer_Points_Index> point_index;

        public Layer_Points()
        {
            Init();
        }

        public void Init()
        {
            point_X = new float[MAX_COUNT_POINTS];
            point_Y = new float[MAX_COUNT_POINTS];
            point_index = new List<Layer_Points_Index>();
        }

        public bool Read(string folder_name)
        {
            byte[] buff;

            if(!File.Exists(folder_name + "\\point_X.dat"))
            {
                System.Windows.Forms.MessageBox.Show("There is no file Point_X.dat");
                return false;
            }
            else if (!File.Exists(folder_name + "\\point_Y.dat"))
            {
                System.Windows.Forms.MessageBox.Show("There is no file point_Y.dat");
                return false;
            }
            else if (!File.Exists(folder_name + "\\point_index.json"))
            {
                System.Windows.Forms.MessageBox.Show("There is no file point_index.json");
                return false;
            }


            buff = File.ReadAllBytes(folder_name + "\\point_X.dat");
            Buffer.BlockCopy(buff, 0, point_X, 0, buff.Length);

            buff = File.ReadAllBytes(folder_name + "\\point_Y.dat");
            Buffer.BlockCopy(buff, 0, point_Y, 0, buff.Length);

            string str_json = File.ReadAllText(folder_name + "\\point_index.json");
            point_index = JsonSerializer.Deserialize<List<Layer_Points_Index>>(str_json);

            return true;
        }
    }
}
