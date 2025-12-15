using RMCLinkNET;
using System;
using System.Threading;
using System.Windows.Forms;

namespace kyDemo
{
    public class PLCConnectionManager
    {
        private static PLCConnectionManager _instance;
        private RMCLink rmc;
        public string serverIp { get; set; }
        private const int DataSize = 100;        //实时读或者写的数据长度
        private const int chunkSize = 100;      //长曲线数据分段时设置的每一段数据长度
        private const int variableSpace = 200;  //长曲线数据的空间是从第200开始往后，前200个是通讯及控制器编程用到的变量空间
        //private const float xInterval = 0.01;                     //时间间隔，0.001代表1ms，以此类推0.1代表100ms，1代表1s
        private float[] readData = new float[DataSize];         //循环读写，长度为50的数组，用以接收从RMC读取的一些状态值，如液压缸位移等。
        private float[] writeData = new float[DataSize];        //这里暂时没确定写什么所以没写
        private float[][] FourTestCurveData = new float[4][];       //保存四个轴轨迹规划曲线的原始数据
        private System.Threading.Timer readTimer;
        private const int ReadInterval = 200; // 读取间隔200毫秒
        private PLCConnectionManager() { }
        public static PLCConnectionManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new PLCConnectionManager();
                }
                return _instance;
            }
        }
        public void SetIP(string ip)
        {
            serverIp = ip;
        }
        public void Connect()
        {
            if (rmc == null)
                rmc = RMCLink.CreateEthernetLink(DeviceType.RMC150, serverIp);
            try
            {
                if (rmc != null && !(rmc.IsConnected(PingType.Ping)))
                {
                    rmc.Connect();
                    StartReadingData();
                }
            }
            catch (ConnectionNotMadeException ex)
            {
                MessageBox.Show("Unable to connect. " + ex.Message);
            }
        }
        public void DisConnect()
        {
            if (rmc != null && rmc.IsConnected(PingType.Ping))
            {
                rmc.Disconnect();
                StopReadingData();
            }
        }
        public bool GetConnectState()
        {
            //return rmc != null ? rmc.IsConnected(PingType.DoNotPing):false;
            return rmc != null ? rmc.IsConnected(PingType.Ping) : false;        //陈修改：我改成了先ping一下再确定
        }

        //*****************读取***********************
        private void ReadDataFromRMC()
        {
            if (rmc == null)
                return;
            // TODO: 实现从下位机读取数据的逻辑，将数据填充到传入的数组中  
            try
            {
                //读RMC150indirect data，需要下位机将需要读取的寄存器放在indirect data map中0号寄存器开始。indirect data map最大到255
                rmc.ReadFFile((int)FileNumber150.fn150IndDataValues, 0, readData, 0, DataSize);
            }
            catch (ReadWriteFailedException ex)
            {
                System.Windows.Forms.MessageBox.Show("Unable to read data. " + ex.Message);
            }
        }
        private void StartReadingData()
        {
            if (readTimer == null)
            {
                readTimer = new System.Threading.Timer(ReadDataCallback, null, 0, ReadInterval);
            }
            else
            {
                readTimer.Change(0, ReadInterval);
            }
        }

        private void StopReadingData()
        {
            if (readTimer != null)
            {
                readTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }

        private void ReadDataCallback(object state)
        {
            ReadDataFromRMC();
        }

        //*****************写入***********************
        public void WriteAllDataToRMC(float[] data)    //将50位的数组一次性写入
        {
            // TODO: 实现将数据写入下位机的逻辑，发送传入的数组中的数据到下位机  
            if (rmc == null)
                return;
            // TODO: 实现从下位机读取数据的逻辑，将数据填充到传入的数组中  
            try
            {
                //写RMC150indirect data，写到在indirect data map中55寄存器开始。indirect data map最大到255
                rmc.WriteFFile((int)FileNumber150.fn150IndDataValues, 55, data, 0, data.Length);
            }
            catch (ReadWriteFailedException ex)
            {
                MessageBox.Show("Unable to read data. " + ex.Message);
            }
        }
        public void WriteOneDataToRMCRegister(float data, int offset)    //只写入一个指定的寄存器
        {
            // TODO: 实现将数据写入下位机的逻辑，发送传入的数组中的数据到下位机  
            if (rmc == null)
                return;
            // TODO: 实现从下位机读取数据的逻辑，将数据填充到传入的数组中  
            try
            {
                float[] dataVector = { data };
                //写RMC150indirect data，写到在indirect data map中55寄存器开始。indirect data map最大到255
                rmc.WriteFFile((int)FileNumber150.fn150IndDataValues, 55+offset, dataVector, 0, 1);
            }
            catch (ReadWriteFailedException ex)
            {
                MessageBox.Show("Unable to read data. " + ex.Message);
            }
        }
        public void WriteOneDataToRMCRegisterAbsolute(float data, int address)    //只写入一个指定的寄存器
        {
            // TODO: 实现将数据写入下位机的逻辑，发送传入的数组中的数据到下位机  
            if (rmc == null)
                return;
            // TODO: 实现从下位机读取数据的逻辑，将数据填充到传入的数组中  
            try
            {
                float[] dataVector = { data };
                //写RMC150indirect data，写到在indirect data map中55寄存器开始。indirect data map最大到255
                rmc.WriteFFile((int)FileNumber150.fn150IndDataValues, address, dataVector, 0, 1);
            }
            catch (ReadWriteFailedException ex)
            {
                System.Windows.Forms.MessageBox.Show("Unable to read data. " + ex.Message);
            }
        }
        public void PrepareWriteToRMC(float[] data)
        {
            // TODO: 根据需要准备要发送到下位机的数据，填充到传入的数组中  
        }

        //*****************基本的外部方法***********************
        public double[] GetCurrentLength()  //获取传感器值
        {
            double RT = readData[5] + ParamModel.Instance.L2_zero;
            double valueInsideArccos = (1640025 - Math.Pow(RT, 2)) / 747000;
            double WP = 0;
            if (valueInsideArccos < -1 || valueInsideArccos > 1)
            {
                Console.WriteLine("超出arccos的定义域");
            }
            else
            {
                double cosValue = Math.Cos(3.47 - Math.Acos(valueInsideArccos));
                double value = 1640025 - 747000 * cosValue;
                if (value < 0)
                {
                    Console.WriteLine("无法对负数开平方根");
                }
                else
                {
                    WP = Math.Sqrt(value);
                }
            }
            double[] len = { WP, RT, readData[7] + ParamModel.Instance.L3_zero, readData[8] + ParamModel.Instance.L4_zero , readData[9] + ParamModel.Instance.L5_zero };
            //len[0] = 938.5484;
            //len[1] = 725.427;
            //len[2] = 1117.48;
            //len[3] = 1327.41;
            //len[4] = 1388.787;
            return len;
        }
        public float GetL1()  //获取回转1拉线传感器值
        {
            return readData[5];
        }

        public float GetL3()  //获取大臂拉线传感器值
        {
            return readData[7];
        }

        public float GetL4()  //获取二臂拉线传感器值
        {
            return readData[8];
        }

        public float GetL5()  //获取小臂拉线传感器值
        {
            return readData[9];
        }

        public float GetMotorVoltage()  //获取电机电压
        {
            return readData[12];
        }

        public float GetMotorCurrent()  //获取电机电流
        {
            return readData[13];
        }

        public float GetOilLevel()  //获取液压油位值
        {
            return readData[14];
        }

        public float GetOilTemperature()  //获取液压油油温
        {
            return readData[15];
        }
        public float GetOilPress()  //获取液压油油压
        {
            return readData[16];
        }
        public float GetMotorStart()  //获取电机启停
        {
            return readData[88];
        }
        public float GetEmergencyStop()  //获取液压油油压
        {
            return readData[89];
        }
        public float GetPoSui()  //获取破碎
        {
            return readData[90];
        }
        public float GetLaBa()  //获取喇叭
        {
            return readData[91];
        }

        public float GetHuiZhuan()  //获取回转
        {
            return readData[80];
        }
        public float GetDaBi()  //获取大臂控制值
        {
            return readData[81];
        }
        public float GeErBi()  //获取二臂控制值
        {
            return readData[82];
        }
        public float GetSanBi()  //获取三臂控制值
        {
            return readData[83];
        }
        public float GetHuiZhuanVoltageOutput()  //获取回转输出电压
        {
            return readData[21];
        }
        public float GetDaBiVoltageOutput()  //获取大臂输出电压
        {
            return readData[22];
        }
        public float GetErBiVoltageOutput()  //获取二臂输出电压
        {
            return readData[23];
        }
        public float GetSanBiVoltageOutput()  //获取三臂输出电压
        {
            return readData[24];
        }
        public void EmergencyStop(int enable)
        {
            WriteOneDataToRMCRegister(enable, 6);  //急停
            //writeData[6] = enable;
            //WriteAllDataToRMC(writeData);
            Thread.Sleep(1000);
        }
        public void StartMove()  //启动运动
        {
            WriteOneDataToRMCRegister(1, 15);
            //writeData[15] = 1;
            //WriteAllDataToRMC(writeData);
            Thread.Sleep(1000);
            Console.WriteLine("启动运动阻塞：" + GetMoveState());
            while (GetMoveState() == 1)
            {
                Thread.Sleep(500);
            }
            Console.WriteLine("结束运动阻塞：" + GetMoveState());
        }
        public void StopMove()  //停止运动
        {
            WriteOneDataToRMCRegister(1, 16);
            //writeData[16] = 1;
            //WriteAllDataToRMC(writeData);
            Thread.Sleep(1000);
        }
        public int GetMoveState()  //获取回零状态
        {
            Console.WriteLine("MoveState:" + readData[42]);
            return (int)readData[42];
        }
        public float GetOilPressure()  //获取液压油压力
        {
            return readData[16];
        }
        public void GoHome()
        {
            WriteOneDataToRMCRegister(1, 17);  //下发回零
            //writeData[17] = 1;
            //WriteAllDataToRMC(writeData);
            Thread.Sleep(1000);
            Console.WriteLine("回零阻塞开始: " + GetHomeState());
            while (GetHomeState() == 1)
            {
                Thread.Sleep(500);
            }
            Console.WriteLine("回零阻塞结束: " + GetHomeState());
        }

        public int GetHomeState()  //获取回零状态
        {
            Console.WriteLine("HomeState:" + readData[44]);
            return (int)readData[44];
        }
        public void GoReady()
        {
            WriteOneDataToRMCRegister(1, 18);  //下发回就绪
            Thread.Sleep(1000);
            Console.WriteLine("回就绪点阻塞开始: " + GetReadyState());
            while (GetReadyState() == 1)
            {
                Thread.Sleep(500);
            }
            Console.WriteLine("回就绪点阻塞结束: " + GetReadyState());
        }
        public int GetReadyState()  //获取回就绪点状态
        {
            return (int)readData[45];
        }
        //按照按照选择的dataFormat将原始数组进行分段加前缀处理，返回一个二维交错数组
        //参数1：原始数组；参数2：每个点之间的时间间隔单位s（加f）
        // pos  0: 70,80,90,60,50
        // pos  1: 70.2,80.5,90.6,60.8,50.9
        //axis1[0] = {70,70.2}
        //axis2[1] = {80,80.5}
        //axis3[2] = {90,90.6}
        //axis4[3] = {60,60.8}
        //axis5[4] = {50,50.9}
        //DownloadAndAddCurve(SplitAndAddHeaders(axis2,0.01),0);   生成曲线
        //DownloadAndAddCurve(SplitAndAddHeaders(axis3,0.01),1);   生成曲线
        //DownloadAndAddCurve(SplitAndAddHeaders(axis4,0.01),2);   生成曲线
        //DownloadAndAddCurve(SplitAndAddHeaders(axis5,0.01),3);   生成曲线
        public float[][] SplitAndAddHeaders(float[] originalArray, float xInterval)
        {
            //下面四个数值是使用 Evenly-Spaced Points(20)数据格式第一层headers的值，需要和原本的的数据拼接起来。
            int pointCount = originalArray.Length;      //原始数据长度
            int interpolationOptions = 8;               //interpolation options暂定0+8     注：methods和options是分开的
            float X_0 = 0;                              //时间为横轴就取0
            xInterval = ((float)ParamModel.Instance.step) / 1000;                     //时间间隔，0.001代表1ms，以此类推0.1代表100ms，1代表1s
            float[] firstHeadersArray = { pointCount, interpolationOptions, X_0, xInterval };   //第一层headers，有4个元素的数组

            //下面是第二层分段加headers的相关数值
            int statusInitial = 0;          //每一段第一位的status的初值都为0
            int Format = 20;                //数据格式选择 Evenly-Spaced Points(20)，没有放到外面定义，考虑到如果方式修改，第一层headers的值也要跟着修改
            int totalLength = firstHeadersArray.Length + pointCount;    //加上第一层头之后的总长
            //int chunkSize = 100;                    //每一段数据长度，放到类里面方法外面定义了，便于修改和其它方法使用
            int numOfChunks = (int)System.Math.Ceiling(totalLength / (double)chunkSize);    //计算有多少段，向上取整

            //给原始数据加上第一层headers，只加一次，
            float[] totalArray = new float[totalLength];    //不算第二层headers的总数组
            Array.Copy(firstHeadersArray, 0, totalArray, 0, firstHeadersArray.Length);
            Array.Copy(originalArray, 0, totalArray, firstHeadersArray.Length, pointCount);

            //第二步，分段加第二个headers，每段都加
            float[][] resultArrays = new float[numOfChunks][];

            for (int i = 0; i < numOfChunks; i++)
            {
                int chunkSizeForThisIteration = Math.Min(chunkSize, totalLength - i * chunkSize);   //本段的数据长度
                float[] metadataAndChunk = new float[chunkSizeForThisIteration + 5];    //5是第二层headers的长度，和数据格式无关，是固定值

                // Add metadata
                metadataAndChunk[0] = statusInitial;                // status初值
                metadataAndChunk[1] = Format;                       // 数据格式
                metadataAndChunk[2] = i * chunkSize;                // 该段首在第一层处理后的长数组的偏移量
                metadataAndChunk[3] = chunkSizeForThisIteration;    // 该段的长度
                metadataAndChunk[4] = totalLength;                  // 加上第一层头之后的总长

                // Add the chunk data
                Array.Copy(totalArray, i * chunkSize, metadataAndChunk, 5, chunkSizeForThisIteration);

                resultArrays[i] = metadataAndChunk;
            }

            return resultArrays;
        }

        //将SplitAndAddHeaders()分段处理好的数据逐段下发到RMC150，同时生成曲线
        //参数1：分段处理好的二维交错数组；参数2：该组点位数据对应的轴序号0~3（同时也是生成的曲线ID，一个轴对应一条曲线）
        public void DownloadAndAddCurve(float[][] array2D, int axisNumber)
        {
            //注意！！！使用时一定要确定RMC中已经建立了相应数量的Axis

            float status;       //判断每段生成曲线的状态：0没开始，1进行中，2部分完成，3全部完成

            //Variables中的下载地址，按照曲线序号axisNumber和每段数据长度chunkSize来计算，从200开始
            int varElement = 200 + (chunkSize + 10) * axisNumber;     //10是间隔5加第二层headers的5

            //curveAdd指令的地址公式是file*4096+element，variables里面包含了四个file，分别为56、57、58、59，每个256个寄存器
            int varAddress = (varElement / 256 + 56) * 4096 + varElement % 256;

            float[] CurveAddcmd = new float[5];     //curveAdd指令参数有四个，再加上第一位写指令序号(82)

            CurveAddcmd[0] = 82;
            CurveAddcmd[1] = axisNumber;
            CurveAddcmd[2] = varAddress;
            CurveAddcmd[3] = 2;                     //Interpolation Methods：Cubic(2)、Linear(1)、Constant(0) 注：methods和options是分开的
            CurveAddcmd[4] = 0;                     //这个是循环次数，就一次所以循环0次

            foreach (var item in array2D)
            {
                try
                {
                    //向RMC的variables寄存器写一段数据
                    rmc.WriteFFile((int)FileNumber150.fn150VarCurValues, varElement, item, 0, item.Length);
                    //给对应的轴下发生成曲线指令
                    rmc.WriteFFile((int)FileNumber150.fn150CommandArea, axisNumber * 10, CurveAddcmd, 0, 5);

                    //检查下载地址首的状态，该状态代表曲线生成情况。
                    //当为2或3时代表部分或全部曲线生成结束，可以进行下一段数据的下载和生成曲线。0代表生成曲线进程未开始，1代表曲线生成中
                    do
                    {
                        // 获取当前状态位  
                        float[] temp = new float[1];
                        rmc.ReadFFile((int)FileNumber150.fn150VarCurValues, varElement, temp, 0, 1);
                        status = temp[0];
                        // 如果状态不是2或3，可以稍微等待一下再次检查  
                        // Thread.Sleep(100); // 可选的等待时间，以便给状态位变化提供时间  
                    } while (status != 2 && status != 3);
                }
                catch (ReadWriteFailedException ex)
                {
                    System.Windows.Forms.MessageBox.Show("Unable to write curve data. " + ex.Message);
                }
            }
        }
    }
}
