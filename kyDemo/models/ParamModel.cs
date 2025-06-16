
using Newtonsoft.Json;
using System.Windows.Forms;
using System;
using System.IO;

namespace kyDemo
{
    public class ParamModel
    {
        [JsonIgnore]
        private static ParamModel _instance;

        private ParamModel() { }

        public static ParamModel Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ParamModel();
                }
                return _instance;
            }
        }

        public double gate_instruction_height { get; set; } = 200;
        public double shuffle_instruction_length { get; set; } = 300;
        public double shuffle_instruction_width { get; set; } = 100;
        public int shuffle_instruction_segments { get; set; } = 3;
        public double line_vel { get; set; } = 500;
        public double line_acc { get; set; } = 50;
        public double line_dec { get; set; } = 50;
        public int line_pl { get; set; } = 5;
        public int step { get; set; } = 10;
        public double[] ZeroValues { get; set; } = new double[5];
        public double[] startPosition { get; set; } = new double[7];
        [JsonIgnore]
        public double L1_zero
        {
            get => ZeroValues[0];
            set => ZeroValues[0] = value;
        }
        [JsonIgnore]
        public double L2_zero
        {
            get => ZeroValues[1];
            set => ZeroValues[1] = value;
        }
        [JsonIgnore]
        public double L3_zero
        {
            get => ZeroValues[2];
            set => ZeroValues[2] = value;
        }
        [JsonIgnore]
        public double L4_zero
        {
            get => ZeroValues[3];
            set => ZeroValues[3] = value;
        }
        [JsonIgnore]
        public double L5_zero
        {
            get => ZeroValues[4];
            set => ZeroValues[4] = value;
        }
        public static void SaveUserData()
        {
            try
            {
                var json = JsonConvert.SerializeObject(_instance, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText("userdata.json", json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save data: {ex.Message}");
            }
        }
        public static void LoadUserData()
        {
            try
            {
                if (File.Exists("userdata.json"))
                {
                    var json = File.ReadAllText("userdata.json");
                    _instance = JsonConvert.DeserializeObject<ParamModel>(json);
                }
                else
                {
                    MessageBox.Show("No saved data found.");
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load data: {ex.Message}");
                return;
            }
        }
    }


}