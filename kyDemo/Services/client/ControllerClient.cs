using System;
using System.Collections.Generic;
using System.Linq;

namespace kyDemo
{
    public class ControllerClient
    {
        private static ControllerClient _instance;
        public string ip_ { set; get; }
        public string port_ { set; get; }
        public int FD {set;get;}
        private ControllerClient() { }

        public static ControllerClient Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ControllerClient();
                }
                return _instance;
            }
        }

        public bool Connect(string ip, string port)
        {
            ip_ = ip;
            port_ = port;
            int temp = nrc_interface.connect_robot(ip, port);
            if(temp > 0)
            {
                FD = temp;
                return true;
            }else
            {
                return false;
            }
        }

        public void DisConnect()
        {
            nrc_interface.disconnect_robot(FD);
        }
        public bool GetConnectState()
        {
            return nrc_interface.get_connection_status(FD) == 1;
        }
        public double[] GetPositionByLength(double[] length,int coord)
        {
            DoubleVector temp_length = new DoubleVector(length);
            DoubleVector temp_postion = new DoubleVector(7);
            int res = nrc_interface.get_current_position_hydraulic(FD, temp_length, coord, temp_postion, 1);
            return temp_postion.ToArray().Length > 0 ? temp_postion.ToArray() : new double[7];
        }
        public double[] GetLengthByPosition(double[] position, int coord)
        {
            DoubleVector temp_length = new DoubleVector(5);
            DoubleVector temp_postion = new DoubleVector(position);
            nrc_interface.get_current_position_hydraulic(FD, temp_length, coord, temp_postion, 2);
            return temp_length.ToArray();
        }
        public int RobotMoveLinear(double[] currentLength, double[] targetPos,int coord, int vel, int acc, int dec)
        {
            if (currentLength.Count() < 5 || targetPos.Count() < 6)
                return -1;
            DoubleVector curLength = new DoubleVector(currentLength);
            DoubleVector pos = new DoubleVector(targetPos);

            int result = nrc_interface.robot_move_hydraulic(FD,1, curLength, pos, vel, coord, acc, dec);
            return result;
        }

        public List<double[]> GetPlanningList()
        {
            int result = 0;
            List<double[]> posList = new List<double[]>();
            //posList.Capacity = 10000;
            int length = 0;
            DoubleVector lastPos = null;
            int i = 0;
            int step = ParamModel.Instance.step;
            if (step <= 0)
            {
                step = 10;
            }
            VectorVectorDouble temp_pos = new VectorVectorDouble();
            result = nrc_interface.get_planning_position_hydraulic(FD, temp_pos);
            if (result <= 0) {
                return posList;
            }
            
            for (i = 0; i < temp_pos.Count; i++)
            {
                //Console.WriteLine($"未处理的点 {i}: {string.Join(", ", temp_pos[i])}");
                lastPos = temp_pos[i];

                if (length % step == 0) // 每10个添加一次
                {
                    posList.Add(temp_pos[i].ToArray());
                }
                length++;
            }
            // 如果最后一个不满10个，则添加最后一个
            if (length % step != 0 && lastPos != null)
            {
                posList.Add(lastPos.ToArray());
            }

            return posList;
        }
    }
}
