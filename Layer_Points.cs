using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;
using HalconDotNet;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Reflection;

namespace Test_Layer_Points
{
    public enum ViewSymbol
    {
        v_POLYGON,
        v_PAD,
        v_LINE,
        v_ARC,
    };

    [Serializable()]
    public class Layer_Points_Index
    {
        // Старт/стоп позиции примитива
        public int pos_start { get; set; }
        public int count { get; set; }
        // Тип символа
        public ViewSymbol symbol_type { get; set; }
        // P или N
        public char polarity { get; set; }
        // Тип, в основном для группировки падов по типам
        public uint apf_def { get; set; }


        public Layer_Points_Index()
        {
        }
    }


    public class Layer_Points
    {
        private JsonSerializerOptions json_options = new JsonSerializerOptions { WriteIndented = true };
        private const int MAX_COUNT_POINTS = 15000000;

        // Координаты примитивов
        public float[] point_X;
        public float[] point_Y;

        // Сегментация точек
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



        public void Add(ViewSymbol symbol_type, char polarity, uint apf_def, HTuple row, HTuple col)
        {
            float[] arr_X = col.ToFArr();
            float[] arr_Y = row.ToFArr();

            int start_pos = (point_index.Count > 0) ? point_index.Last().pos_start + point_index.Last().count : 0;
            int count = arr_X.Length;
            Array.Copy(arr_X, 0, point_X, start_pos, count);
            Array.Copy(arr_Y, 0, point_Y, start_pos, count);

            Layer_Points_Index idx = new Layer_Points_Index();
            idx.pos_start = start_pos;
            idx.count = count;
            idx.symbol_type = symbol_type;
            idx.polarity = polarity;
            idx.apf_def = apf_def;
            point_index.Add(idx);

        }


        public void Write(string folder_name)
        {
            int length = (point_index.Last().pos_start + point_index.Last().count) * sizeof(float);
            byte[] buff = new byte[length];

            Buffer.BlockCopy(point_X, 0, buff, 0, buff.Length);
            File.WriteAllBytes(folder_name + "\\point_X.dat", buff);

            Buffer.BlockCopy(point_Y, 0, buff, 0, buff.Length);
            File.WriteAllBytes(folder_name + "\\point_Y.dat", buff);

            var stream = File.Create(folder_name + "\\point_index.json");
            JsonSerializer.Serialize(stream, point_index, json_options);
            stream.Dispose();
        }

        public void Read(string folder_name)
        {
            byte[] buff;

            buff = File.ReadAllBytes(folder_name + "\\point_X.dat");
            Buffer.BlockCopy(buff, 0, point_X, 0, buff.Length);

            buff = File.ReadAllBytes(folder_name + "\\point_Y.dat");
            Buffer.BlockCopy(buff, 0, point_Y, 0, buff.Length);

            string str_json = File.ReadAllText(folder_name + "\\point_index.json");
            point_index = JsonSerializer.Deserialize<List<Layer_Points_Index>>(str_json);

        }

    }

    [Serializable]
    public class Layer_Sub
    {
        public List<Contur_Sub> m_List_Contour;
        public double profile_size_x;
        public double profile_size_y;
        public double ratio;

        public Layer_Sub()
        {
            m_List_Contour = new List<Contur_Sub>();
        }

        public void Clear()
        {
            m_List_Contour.Clear();
        }

        /// TODO:
        /// Добавить методы формирования изображения и субпиксельных регионов
        /// Добавить методы формирования базы данных и индексации

    }

    [Serializable]
    public class Contur_Sub
    {
        public List<HObject> ho_Conturs;
        //public HObject ho_Contur = null;  // Все контуры здесь
        public HTuple hv_Polarity;        // Тип дрка или нет
        public HTuple hv_Count;

        public ViewSymbol hv_Type;        // Тип контура - пад, линия, полигон...
        public HTuple hv_Feature_Tupe;    // Тип символа

        public Contur_Sub(ViewSymbol type, uint feature_tupe)
        {
            ho_Conturs = new List<HObject>();
            Init(type, feature_tupe);
        }
        public void Init(ViewSymbol type, uint feature_tupe)
        {
            //HOperatorSet.GenEmptyObj(out ho_Contur);
            ho_Conturs.Clear();
            hv_Polarity = new HTuple();
            hv_Count = 0;

            hv_Type = type;
            hv_Feature_Tupe = feature_tupe;
        }

        public void Add(HObject ho_Contur, char polarity)
        {
            if (ho_Contur != null)
            {
                HOperatorSet.CountObj(ho_Contur, out HTuple hv_num);
                if (hv_num > 0)
                {
                    ho_Conturs.Add(ho_Contur);
                    //HOperatorSet.ConcatObj(this.ho_Contur, ho_Contur, out this.ho_Contur);
                    hv_Polarity[hv_Count] = polarity;
                    hv_Count += 1;
                }
            }
        }

    }


    public sealed class CustomizedBinder : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            //Type returntype = null;
            //string sharedAssemblyName = "ODB_Creator, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
            //assemblyName = Assembly.GetExecutingAssembly().FullName;
            //typeName = typeName.Replace(sharedAssemblyName, assemblyName);
            //returntype =
            //        Type.GetType(String.Format("{0}, {1}",
            //        typeName, assemblyName));

            //return returntype;

            Type typeToDeserialize = null;

            String currentAssembly = Assembly.GetExecutingAssembly().FullName;

            // In this case we are always using the current assembly
            assemblyName = currentAssembly;

            // Get the type using the typeName and assemblyName
            typeToDeserialize = Type.GetType(String.Format("{0}, {1}",
                typeName, assemblyName));

            return typeToDeserialize;
        }

        public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            base.BindToName(serializedType, out assemblyName, out typeName);
            assemblyName = "SharedAssembly, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        }
    }

}
