using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace kyDemo.Services
{
    internal class TreatmentProcess
    {
        private static TreatmentProcess _instance;

        public enum Mode
        {
            auto = 0,
            insert = 1,
        }
        public static Mode mode;

        public static bool running;
        private TreatmentProcess() { }
        public static TreatmentProcess Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TreatmentProcess();
                }
                return _instance;
            }
        }
        /*
         * @brief 插入门型指令
         * @param gate_instruction_height 门型指令高度
         */
        public static void InsertGateInstruction(double cameraX, double cameraY, double cameraZ,double gate_instruction_height)
        {
            //获取当前液压杠长度 调用控制器函数得到当前直角值
            DateTime now = DateTime.Now;
            string formattedDateTime = now.ToString("yyyy-MM-dd HH:mm:ss");
            Console.WriteLine(formattedDateTime + "轨迹从此时开始启动解算...");
            double[] currentLengths = PLCConnectionManager.Instance.GetCurrentLength();
            Console.WriteLine($"第1段液压杆长度: {string.Join(", ", currentLengths)}");

            int coordinate = 1; //直角坐标
            double[] currentPosition = ControllerClient.Instance.GetPositionByLength(currentLengths, coordinate);
           // double[] A_pos_length = ControllerClient.Instance.GetLengthByPosition(currentPosition, coordinate);
            //门型指令第一个点
            double[] posB = { currentPosition[0], currentPosition[1], currentPosition[2] + gate_instruction_height,
                                  currentPosition[3], currentPosition[4], currentPosition[5] };
            Console.WriteLine($"pos1: {string.Join(", ", posB)}");
            //门型指令第二个点
            double[] posC = { cameraX, cameraY, currentPosition[2] + gate_instruction_height,
                                  currentPosition[3], currentPosition[4], currentPosition[5] };
            Console.WriteLine($"pos2: {string.Join(", ", posC)}");
            //门型指令终点，即相机点
            double[] posD = { cameraX, cameraY, cameraZ,
                                  currentPosition[3], currentPosition[4], currentPosition[5] };
            Console.WriteLine($"pos3: {string.Join(", ", posD)}");
            //得到三个点后，开始规划路径
            now = DateTime.Now;
            formattedDateTime = now.ToString("yyyy-MM-dd HH:mm:ss");
            Console.WriteLine(formattedDateTime + "从此时开始解算");
            List<double[]> pos;  //存储规划路径的数组
           
            ControllerClient.Instance.RobotMoveLinear(currentLengths, posB, 1, (int)ParamModel.Instance.line_vel, (int)ParamModel.Instance.line_acc, (int)ParamModel.Instance.line_dec);
            pos = ControllerClient.Instance.GetPlanningList();  ///< 获得插补数组
            now = DateTime.Now;
            formattedDateTime = now.ToString("yyyy-MM-dd HH:mm:ss");
            Console.WriteLine(formattedDateTime + "此时已获取第一个点插补数组");
            //for (int i = 0; i < pos.Count; i++)
            //{
            //    Console.WriteLine($"Position {i}: {string.Join(", ", pos[i])}");
            //}
            //DistributeInterpolationPoints(pos); //下发插补点给plc
            //pos.Clear();
            int size = pos.Count;
            ///< 第二个点的规划
            ///
            now = DateTime.Now;
            formattedDateTime = now.ToString("yyyy-MM-dd HH:mm:ss");
            Console.WriteLine(formattedDateTime + "即将执行ControllerClient.Instance.GetLengthByPosition..");
            double[] tempLen1 = ControllerClient.Instance.GetLengthByPosition(posB, coordinate);   ///< 获得到达第一个点的液压杆的长度
            //double[] tempLen1 = PLCConnectionManager.Instance.GetCurrentLength();
            Console.WriteLine($"第2段液压杆长度: {string.Join(", ", tempLen1)}");
            now = DateTime.Now;
            formattedDateTime = now.ToString("yyyy-MM-dd HH:mm:ss");
            Console.WriteLine(formattedDateTime + "即将执行RobotMoveLinear");
            ControllerClient.Instance.RobotMoveLinear(tempLen1, posC, 1, (int)ParamModel.Instance.line_vel, (int)ParamModel.Instance.line_acc, (int)ParamModel.Instance.line_dec);  ///< 从第一个点到第二个点的规划
            now = DateTime.Now;
            formattedDateTime = now.ToString("yyyy-MM-dd HH:mm:ss");
            Console.WriteLine(formattedDateTime + "即将执行AddRange..GetPlanningList");
            pos.AddRange(ControllerClient.Instance.GetPlanningList());  //获得插补数组
            now = DateTime.Now;
            formattedDateTime = now.ToString("yyyy-MM-dd HH:mm:ss");
            Console.WriteLine(formattedDateTime + "此时已获取第二个点插补数组");
            //for (int i = size; i < pos.Count; i++)
            //{
            //    Console.WriteLine($"Position {i}: {string.Join(", ", pos[i])}");
            //}
            //DistributeInterpolationPoints(pos); //下发插补点给plc
            //pos.Clear();
            size = pos.Count;
            ///< 第三个点的规划
            double[] tempLen2 = ControllerClient.Instance.GetLengthByPosition(posC, coordinate);   ///< 获得到达第二个点的液压杆的长度
            Console.WriteLine($"第3段液压杆长度: {string.Join(", ", tempLen2)}");
            //double[] tempLen2 = PLCConnectionManager.Instance.GetCurrentLength();
            ControllerClient.Instance.RobotMoveLinear(tempLen2, posD, 1, (int)ParamModel.Instance.line_vel, (int)ParamModel.Instance.line_acc, (int)ParamModel.Instance.line_dec);  ///< 从第二个点到第三个点
            pos.AddRange(ControllerClient.Instance.GetPlanningList());  //获得插补数组
            now = DateTime.Now;
            formattedDateTime = now.ToString("yyyy-MM-dd HH:mm:ss");
            Console.WriteLine(formattedDateTime + "此时已获取第3个点插补数组");
            //for (int i = size; i < pos.Count; i++)
            //{
            //    Console.WriteLine($"Position {i}: {string.Join(", ", pos[i])}");
            //}
            now = DateTime.Now;
            formattedDateTime = now.ToString("yyyy-MM-dd HH:mm:ss");
            Console.WriteLine(formattedDateTime + "从此时下发");
            DistributeInterpolationPoints(pos); //下发插补点给plc
            now = DateTime.Now;
            formattedDateTime = now.ToString("yyyy-MM-dd HH:mm:ss");
            Console.WriteLine(formattedDateTime + "下发完成");
        }
        /*
         * @brief 插入推散指令
         * @param shuffle_instruction_length
         * @param shuffle_instruction_width
         * @param shuffle_instruction_segments
         */
        public static void InsertShuffleInstruction(double X, double Y, double Z, double shuffle_instruction_length, double shuffle_instruction_width, double shuffle_instruction_segments)
        {
            List<double[]> pos;  //存储规划路径的数组
            double[] currentLengths = PLCConnectionManager.Instance.GetCurrentLength();
            int coordinate = 1; //直角坐标
            double[] currentPosition = ControllerClient.Instance.GetPositionByLength(currentLengths, coordinate);
            double[] pos0 = { X, Y, Z, currentPosition[3], currentPosition[4], currentPosition[5]};
            ControllerClient.Instance.RobotMoveLinear(currentLengths, pos0, 1, (int)ParamModel.Instance.line_vel, (int)ParamModel.Instance.line_acc, (int)ParamModel.Instance.line_dec);
            pos = ControllerClient.Instance.GetPlanningList();
            
            for (int i = 0; i < shuffle_instruction_segments; i++)
            {
                currentLengths = ControllerClient.Instance.GetLengthByPosition(pos0, coordinate);
                currentPosition = ControllerClient.Instance.GetPositionByLength(currentLengths, coordinate);

                double[] pos1 = { currentPosition[0], currentPosition[1] + shuffle_instruction_length, currentPosition[2],
                                  currentPosition[3], currentPosition[4], currentPosition[5]};
                ControllerClient.Instance.RobotMoveLinear(currentLengths, pos1, 1, (int)ParamModel.Instance.line_vel, (int)ParamModel.Instance.line_acc, (int)ParamModel.Instance.line_dec);
                pos.AddRange(ControllerClient.Instance.GetPlanningList());
                //DistributeInterpolationPoints(pos); //下发插补点给plc
                //pos.Clear();

                //currentLengths = PLCConnectionManager.Instance.GetCurrentLength();
                currentLengths = ControllerClient.Instance.GetLengthByPosition(pos1, coordinate);
                currentPosition = ControllerClient.Instance.GetPositionByLength(currentLengths, coordinate);
                double[] pos2 = { currentPosition[0] + shuffle_instruction_width, currentPosition[1], currentPosition[2],
                                  currentPosition[3], currentPosition[4], currentPosition[5]};
                ControllerClient.Instance.RobotMoveLinear(currentLengths, pos2, 1, (int)ParamModel.Instance.line_vel, (int)ParamModel.Instance.line_acc, (int)ParamModel.Instance.line_dec);
                pos.AddRange(ControllerClient.Instance.GetPlanningList());
                //DistributeInterpolationPoints(pos); //下发插补点给plc
                //pos.Clear();

                //currentLengths = PLCConnectionManager.Instance.GetCurrentLength();
                currentLengths = ControllerClient.Instance.GetLengthByPosition(pos2, coordinate);
                currentPosition = ControllerClient.Instance.GetPositionByLength(currentLengths, coordinate);
                double[] pos3 = { currentPosition[0], currentPosition[1] - shuffle_instruction_length, currentPosition[2],
                                  currentPosition[3], currentPosition[4], currentPosition[5]};
                ControllerClient.Instance.RobotMoveLinear(currentLengths, pos3, 1, (int)ParamModel.Instance.line_vel, (int)ParamModel.Instance.line_acc, (int)ParamModel.Instance.line_dec);
                pos.AddRange(ControllerClient.Instance.GetPlanningList());
                //DistributeInterpolationPoints(pos); //下发插补点给plc
                //pos.Clear();

                //currentLengths = PLCConnectionManager.Instance.GetCurrentLength();
                currentLengths = ControllerClient.Instance.GetLengthByPosition(pos3, coordinate);
                currentPosition = ControllerClient.Instance.GetPositionByLength(currentLengths, coordinate);
                double[] pos4 = { currentPosition[0] + shuffle_instruction_width, currentPosition[1], currentPosition[2],
                                  currentPosition[3], currentPosition[4], currentPosition[5]};
                ControllerClient.Instance.RobotMoveLinear(currentLengths, pos2, 1, (int)ParamModel.Instance.line_vel, (int)ParamModel.Instance.line_acc, (int)ParamModel.Instance.line_dec);
                pos.AddRange(ControllerClient.Instance.GetPlanningList());
                DistributeInterpolationPoints(pos); //下发插补点给plc
            }
        }
        public static void DistributeInterpolationPoints(List<double[]> pos)
        {
            ParamModel.LoadUserData();
            int rows = pos.Count;
            if(rows == 0) { return; }
            int cols = 5;
            float[][] axis = new float[cols][];// 创建五个一维数组,这五个数组分别代表一个轴的液压杆变化
            for (int i = 0; i < cols; i++)
            {
                axis[i] = new float[rows];
            }
            for (int i = 0; i < rows; i++)// 遍历列表并将元素分配到一维数组中
            {
                if (pos[i].Length < cols)
                {
                    throw new IndexOutOfRangeException($"第 {i} 行的列数不足 {cols} 列");
                }
                for (int j = 0; j < cols; j++)
                {
                    axis[j][i] = (float)(pos[i][j] - ParamModel.Instance.ZeroValues[j]);
                }
            }
            // 打印结果
            for (int i = 0; i < axis.Length; i++)
            {
                Console.WriteLine($"Array {i + 1}: {string.Join(", ", axis[i])}");
            }
            //todo 下发 pos 到plc
            PLCConnectionManager.Instance.DownloadAndAddCurve(PLCConnectionManager.Instance.SplitAndAddHeaders(axis[1], (float)0.01), 0);
            PLCConnectionManager.Instance.DownloadAndAddCurve(PLCConnectionManager.Instance.SplitAndAddHeaders(axis[2], (float)0.01), 1);
            PLCConnectionManager.Instance.DownloadAndAddCurve(PLCConnectionManager.Instance.SplitAndAddHeaders(axis[3], (float)0.01), 2);
            PLCConnectionManager.Instance.DownloadAndAddCurve(PLCConnectionManager.Instance.SplitAndAddHeaders(axis[4], (float)0.01), 3);
            //启动运动，并阻塞
            PLCConnectionManager.Instance.StartMove();

        }
        /*
        * @brief 全自动处理
        * @param gate_instruction_height 门型指令高度
        */
        public static async Task<bool> AutomaticProcessing(
            double gate_instruction_height
            , double shuffle_instruction_length
            , double shuffle_instruction_width
            , double shuffle_instruction_segments)
        {
            if (mode != 0)
                return false;
            ParamModel.LoadUserData();
            Task<bool> task = Task.Run(() =>
            {
                //下发回零
                PLCConnectionManager.Instance.GoHome();
                running = true;
                while (running)
                {
                    //回就绪点拍照
                    PLCConnectionManager.Instance.GoReady();
                    //获取相机终点
                    //(bool result, double cameraX, double cameraY, double cameraZ) = CameraConnectionManager.Instance.GetPoints();
                    //如果相机未回复，退出破碎
                    //if (!result)
                    //{
                    //    break;
                    //}
                    //InsertGateInstruction(cameraX, cameraY, cameraZ, gate_instruction_height);
                }

                ///< 开始推散
                /*InsertShuffleInstruction(ParamModel.Instance.startPosition[0],
                    ParamModel.Instance.startPosition[1],
                    ParamModel.Instance.startPosition[2],
                    shuffle_instruction_length,
                    shuffle_instruction_width,
                    shuffle_instruction_segments);*/

                return true;
            });
            bool res = await task;
            return res;
        }

        public static void SwitchMode(Mode mode_)
        {
            if (mode == mode_)
                return;
            mode = mode_;
        }

        public static Mode GetMode()
        {
            return mode;
        }
    }
}
