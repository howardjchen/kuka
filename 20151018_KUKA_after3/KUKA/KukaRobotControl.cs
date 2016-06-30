using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Threading;

namespace KUKA
{
    class KukaRobotControl
    {
        /*
         *      資料成員
         */
        // Vel = X; 代表每12ms跑 X (mm)的距離
        private float vel = 1.5F; 
        private float rotation_Vel = 0.5F;
        private String[] xyzabcCommand = new String[6] { "0", "0", "0", "0", "0", "0" };
        private String[] axisCommand = new String[6] { "0", "0", "0", "0", "0", "0" };
        public float[] xyzabcUpLimited = new float[6] { 750, 300, 750, 300, 300, 300 };
        public float[] xyzabcDownLimited = new float[6] { -100, -300, -150, -300, -300, -300 };
        private float[] axisUpLimited = new float[6] { 170, 45, 165, 190, 120, 358 };
        private float[] axisDownLimited = new float[6] { -170, -190, -119, -190, -120, -358 };


        //Gripper State
        private int Gripper_9_1 = 0;
        private int Gripper_9_2 = 0;
        private int Gripper_10_1 = 0;
        private int Gripper_10_2 = 0;
        private int Gripper_11_1 = 0;
        private int Gripper_11_2 = 0;
        private int Gripper_12_1 = 0;
        private int Gripper_12_2 = 0;

        public float Vel
        {
            get { return vel; }
            set { vel = value; }
        }
        public String[] XyzabcCommand
        {
            get { return xyzabcCommand; }
        }
        public String[] AxisCommand
        {
            get { return axisCommand; }
        }

        //處理Kuka資料數據 - 負責丟傳用
        XmlDocument XmlToRobot = new XmlDocument();
        XmlNode Node;
        XmlElement Element;
        XmlAttribute Attribute;
        ServerCommunication serv = new ServerCommunication(1);
        public ServerCommunication Serv 
        {
            get { return serv; } 
        }


        /*
         *      成員函數
         */
        public void startCom()
        {
            XmlToRobot.PreserveWhitespace = true;
            XmlToRobot.Load("ExternalData.xml"); 
            Serv.SendXml = XmlToRobot;
            Serv.StartSocketThread();
        }

        private void move_KukaArm(String Mode)
        {


            try
            {
               
                if (Mode == "Xyzabc")
                {
                    //RKorr
                    Node = XmlToRobot.SelectSingleNode("//RKorr");
                    Element = (XmlElement)Node;
                    Attribute = Element.GetAttributeNode("X");
                    Attribute.Value = xyzabcCommand[0];
                    Attribute = Element.GetAttributeNode("Y");
                    Attribute.Value = xyzabcCommand[1];
                    Attribute = Element.GetAttributeNode("Z");
                    Attribute.Value = xyzabcCommand[2];
                    Attribute = Element.GetAttributeNode("A");
                    Attribute.Value = xyzabcCommand[3];
                    Attribute = Element.GetAttributeNode("B");
                    Attribute.Value = xyzabcCommand[4];
                    Attribute = Element.GetAttributeNode("C");
                    Attribute.Value = xyzabcCommand[5];
                }
                else if (Mode == "Axis")
                {
                    //AKorr
                    Node = XmlToRobot.SelectSingleNode("//AKorr");
                    Element = (XmlElement)Node;
                    Attribute = Element.GetAttributeNode("A1");
                    Attribute.Value = AxisCommand[0];
                    Attribute = Element.GetAttributeNode("A2");
                    Attribute.Value = AxisCommand[1];
                    Attribute = Element.GetAttributeNode("A3");
                    Attribute.Value = AxisCommand[2];
                    Attribute = Element.GetAttributeNode("A4");
                    Attribute.Value = AxisCommand[3];
                    Attribute = Element.GetAttributeNode("A5");
                    Attribute.Value = AxisCommand[4];
                    Attribute = Element.GetAttributeNode("A6");
                    Attribute.Value = AxisCommand[5];
                }
                else if (Mode == "Grasp")
                {
                    Node = XmlToRobot.SelectSingleNode("//Out");
                    Element = (XmlElement)Node;
                    Attribute = Element.GetAttributeNode("o1");
                    Attribute.Value = Gripper_9_1.ToString();
                    Attribute = Element.GetAttributeNode("o2");
                    Attribute.Value = Gripper_9_2.ToString();
                    Attribute = Element.GetAttributeNode("o3");
                    Attribute.Value = Gripper_11_1.ToString();
                    Attribute = Element.GetAttributeNode("o4");
                    Attribute.Value = Gripper_11_2.ToString();
                    Attribute = Element.GetAttributeNode("o5");
                    Attribute.Value = Gripper_10_1.ToString();
                    Attribute = Element.GetAttributeNode("o6");
                    Attribute.Value = Gripper_10_2.ToString();
                    Attribute = Element.GetAttributeNode("o7");
                    Attribute.Value = Gripper_12_1.ToString();
                    Attribute = Element.GetAttributeNode("o8");
                    Attribute.Value = Gripper_12_2.ToString();
                }
            }
            catch (NullReferenceException e) { }

            //***************************************
            //讓Server 傳送改過的XML給Kuka Controller
            Serv.SendXml_Mutex.WaitOne();

            Serv.SendXml = XmlToRobot;

            Serv.SendXml_Mutex.ReleaseMutex();

        }

        public void GraspControll(int GraspState)
        {

            if (GraspState == 1)
            {
                //關
                Thread.Sleep(50);
                Gripper_9_1 = 0;
                Gripper_9_2 = 1;
                Gripper_11_1 = 1;
                Gripper_11_2 = 0;
                Gripper_10_1 = 1;
                Gripper_10_2 = 0;
                Gripper_12_1 = 0;
                Gripper_12_2 = 1;
                move_KukaArm("Grasp");
                Thread.Sleep(50);
            }
            else if (GraspState == -1)
            {
                //:開
                Thread.Sleep(50);
                Gripper_9_1 = 1;
                Gripper_9_2 = 0;
                Gripper_11_1 = 0;
                Gripper_11_2 = 1;
                Gripper_10_1 = 0;
                Gripper_10_2 = 1;
                Gripper_12_1 = 1;
                Gripper_12_2 = 0;
                move_KukaArm("Grasp");
                Thread.Sleep(50);
            }
            else if (GraspState == 0)
            {
                Thread.Sleep(50);
                Gripper_9_1 = 0;
                Gripper_9_2 = 1;
                Gripper_11_1 = 1;
                Gripper_11_2 = 0;
                Gripper_10_1 = 0;
                Gripper_10_2 = 1;
                Gripper_12_1 = 1;
                Gripper_12_2 = 0;
                move_KukaArm("Grasp");
                Thread.Sleep(50);
            }
        }

        public void move_XYZABC(Coordinate_XYZABC newXyzabc)
        {

            xyzabcCommand[0] = newXyzabc.X.ToString();
            xyzabcCommand[1] = newXyzabc.Y.ToString();
            xyzabcCommand[2] = newXyzabc.Z.ToString();
            xyzabcCommand[3] = newXyzabc.A.ToString();
            xyzabcCommand[4] = newXyzabc.B.ToString();
            xyzabcCommand[5] = newXyzabc.C.ToString();
            move_KukaArm("Xyzabc");
            //(12ms =  一單位)
            Thread.Sleep(12);
            xyzabcCommand[0] = "0";
            xyzabcCommand[1] = "0";
            xyzabcCommand[2] = "0";
            xyzabcCommand[3] = "0";
            xyzabcCommand[4] = "0";
            xyzabcCommand[5] = "0";
            move_KukaArm("Xyzabc");
          
        }

        public Boolean move_XYZABC_SingalMode(Coordinate_XYZABC newXyzabc, Coordinate_XYZABC oldXyzabc)
        {
            if (newXyzabc.X < xyzabcUpLimited[0] && newXyzabc.X > xyzabcDownLimited[0])
            {
                if (newXyzabc.Y < xyzabcUpLimited[1] && newXyzabc.Y > xyzabcDownLimited[1])
                {
                    if (newXyzabc.Z < xyzabcUpLimited[2] && newXyzabc.Z > xyzabcDownLimited[2])
                    {
                        if (newXyzabc.A < xyzabcUpLimited[3] && newXyzabc.A > xyzabcDownLimited[3])
                        {
                            if (newXyzabc.B < xyzabcUpLimited[4] && newXyzabc.B > xyzabcDownLimited[4])
                            {
                                if (newXyzabc.C < xyzabcUpLimited[5] && newXyzabc.C > xyzabcDownLimited[5])
                                {
                                    Double[] XyzabcWaitTime = new Double[6];

                                    XyzabcWaitTime[0] = ((newXyzabc.X - oldXyzabc.X) / Vel) * 12.0;
                                    XyzabcWaitTime[1] = ((newXyzabc.Y - oldXyzabc.Y) / Vel) * 12.0;
                                    XyzabcWaitTime[2] = ((newXyzabc.Z - oldXyzabc.Z) / Vel) * 12.0;

                                    xyzabcCommand[0] = (Math.Sign(XyzabcWaitTime[0]) * Vel).ToString();
                                    xyzabcCommand[1] = "0";
                                    xyzabcCommand[2] = "0";
                                    xyzabcCommand[3] = "0";
                                    xyzabcCommand[4] = "0";
                                    xyzabcCommand[5] = "0";
                                    move_KukaArm("Xyzabc");

                                    Thread.Sleep( (int)Math.Round(Math.Abs(XyzabcWaitTime[0])));
                                    XyzabcCommand[0] = "0";
                                    XyzabcCommand[1] = (Math.Sign(XyzabcWaitTime[1]) * Vel).ToString();
                                    XyzabcCommand[2] = "0";
                                    xyzabcCommand[3] = "0";
                                    xyzabcCommand[4] = "0";
                                    xyzabcCommand[5] = "0";
                                    move_KukaArm("Xyzabc");

                                    Thread.Sleep((int)Math.Round(Math.Abs(XyzabcWaitTime[1])));
                                    xyzabcCommand[0] = "0";
                                    xyzabcCommand[1] = "0";
                                    xyzabcCommand[2] = (Math.Sign(XyzabcWaitTime[2]) * Vel).ToString();
                                    xyzabcCommand[3] = "0";
                                    xyzabcCommand[4] = "0";
                                    xyzabcCommand[5] = "0";
                                    move_KukaArm("Xyzabc");

                                    Thread.Sleep((int)Math.Round(Math.Abs(XyzabcWaitTime[2])));
                                    xyzabcCommand[0] = "0";
                                    xyzabcCommand[1] = "0";
                                    xyzabcCommand[2] = "0";
                                    xyzabcCommand[3] = "0";
                                    xyzabcCommand[4] = "0";
                                    xyzabcCommand[5] = "0";
                                    move_KukaArm("Xyzabc");

                                }
                            }
                        }
                    }
                }
                return true;
            }

            else
            {
                return false;
            }
        }

        public Boolean move_XYZABC_RotationMode(Coordinate_XYZABC newXyzabc, Coordinate_XYZABC oldXyzabc)
        {
            if (newXyzabc.X < xyzabcUpLimited[0] && newXyzabc.X > xyzabcDownLimited[0])
            {
                if (newXyzabc.Y < xyzabcUpLimited[1] && newXyzabc.Y > xyzabcDownLimited[1])
                {
                    if (newXyzabc.Z < xyzabcUpLimited[2] && newXyzabc.Z > xyzabcDownLimited[2])
                    {
                        if (newXyzabc.A < xyzabcUpLimited[3] && newXyzabc.A > xyzabcDownLimited[3])
                        {
                            if (newXyzabc.B < xyzabcUpLimited[4] && newXyzabc.B > xyzabcDownLimited[4])
                            {
                                if (newXyzabc.C < xyzabcUpLimited[5] && newXyzabc.C > xyzabcDownLimited[5])
                                {
                                    float[] SleepTime = new float[3];

                                    SleepTime[0] = newXyzabc.A - oldXyzabc.A;
                                    SleepTime[1] = newXyzabc.B - oldXyzabc.B;
                                    SleepTime[2] = newXyzabc.C - oldXyzabc.C;   


                                    xyzabcCommand[3] = (Math.Sign(SleepTime[0]) * rotation_Vel).ToString();
                                    move_KukaArm("Xyzabc");

                                    Thread.Sleep((int)Math.Round(((Math.Abs(SleepTime[0]) * 12.0)) / rotation_Vel));
                                    xyzabcCommand[3] = "0";
                                    xyzabcCommand[4] = (-Math.Sign(SleepTime[1]) * rotation_Vel).ToString();
                                    move_KukaArm("Xyzabc");

                                    Thread.Sleep((int)Math.Round(((Math.Abs(SleepTime[1]) * 12.0)) / rotation_Vel));
                                    xyzabcCommand[4] = "0";
                                    move_KukaArm("Xyzabc");

                                }
                            }
                        }
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public Boolean move_XYZABC_TogetherMode(Coordinate_XYZABC newXyzabc, Coordinate_XYZABC oldXyzabc)
        {
            if (newXyzabc.X < xyzabcUpLimited[0] && newXyzabc.X > xyzabcDownLimited[0])
            {
                if (newXyzabc.Y < xyzabcUpLimited[1] && newXyzabc.Y > xyzabcDownLimited[1])
                {
                    if (newXyzabc.Z < xyzabcUpLimited[2] && newXyzabc.Z > xyzabcDownLimited[2])
                    {
                        if (newXyzabc.A < xyzabcUpLimited[3] && newXyzabc.A > xyzabcDownLimited[3])
                        {
                            if (newXyzabc.B < xyzabcUpLimited[4] && newXyzabc.B > xyzabcDownLimited[4])
                            {
                                if (newXyzabc.C < xyzabcUpLimited[5] && newXyzabc.C > xyzabcDownLimited[5])
                                {

                                    float[] XyzabcWaitTime = new float[3];
                                    float[] TempWaitTime = new float[3];
                                    int[] Order = new int[3] { -1, -1, -1 };
                                    int SwitchMode;
                                    TempWaitTime[0] = Math.Abs(((newXyzabc.X - oldXyzabc.X) / Vel) * 12);
                                    TempWaitTime[1] = Math.Abs(((newXyzabc.Y - oldXyzabc.Y) / Vel) * 12);
                                    TempWaitTime[2] = Math.Abs(((newXyzabc.Z - oldXyzabc.Z) / Vel) * 12);
                                    XyzabcWaitTime[0] = ((newXyzabc.X - oldXyzabc.X) / Vel) * 12;
                                    XyzabcWaitTime[1] = ((newXyzabc.Y - oldXyzabc.Y) / Vel) * 12;
                                    XyzabcWaitTime[2] = ((newXyzabc.Z - oldXyzabc.Z) / Vel) * 12;
                                    Array.Sort(TempWaitTime);

                                    for (int i = 0; i < 3; i++)
                                    {
                                        if ((Order[0] == -1) & Math.Abs(TempWaitTime[0] - Math.Abs(XyzabcWaitTime[i])) < 0.01)
                                        {
                                            Order[0] = i;
                                        }
                                        else if ((Order[1] == -1) & Math.Abs(TempWaitTime[1] - Math.Abs(XyzabcWaitTime[i])) < 0.01)
                                        {
                                            Order[1] = i;
                                        }
                                        else if ((Order[2] == -1) & Math.Abs(TempWaitTime[2] - Math.Abs(XyzabcWaitTime[i])) < 0.01)
                                        {
                                            Order[2] = i;
                                        }

                                    }

                                    if (((int)(TempWaitTime[1] - TempWaitTime[0]) == 0) & ((int)(TempWaitTime[2] - TempWaitTime[1]) == 0))
                                    {
                                        SwitchMode = 3;
                                    }
                                    else if (((int)(TempWaitTime[1] - TempWaitTime[0]) == 0))
                                    {
                                        SwitchMode = 1;
                                    }
                                    else if (((int)(TempWaitTime[2] - TempWaitTime[1]) == 0))
                                    {
                                        SwitchMode = 2;
                                    }
                                    else
                                    {
                                        SwitchMode = 0;
                                    }


                                    

                                    switch (SwitchMode)
                                    {
                                        case 0:
                                            xyzabcCommand[0] = (Math.Sign(XyzabcWaitTime[0]) * Vel).ToString();
                                            xyzabcCommand[1] = (Math.Sign(XyzabcWaitTime[1]) * Vel).ToString();
                                            xyzabcCommand[2] = (Math.Sign(XyzabcWaitTime[2]) * Vel).ToString();
                                            move_KukaArm("Xyzabc");

                                            Thread.Sleep((int)Math.Round(TempWaitTime[0]));
                                            xyzabcCommand[Order[0]] = "0";
                                            move_KukaArm("Xyzabc");

                                            Thread.Sleep((int)Math.Round((TempWaitTime[1] - TempWaitTime[0])));
                                            xyzabcCommand[Order[1]] = "0";
                                            move_KukaArm("Xyzabc");

                                            Thread.Sleep((int)Math.Round((TempWaitTime[2] - TempWaitTime[1])));
                                            xyzabcCommand[Order[2]] = "0";
                                            move_KukaArm("Xyzabc");
                                            break;

                                        case 1:
                                            xyzabcCommand[0] = (Math.Sign(XyzabcWaitTime[0]) * Vel).ToString();
                                            xyzabcCommand[1] = (Math.Sign(XyzabcWaitTime[1]) * Vel).ToString();
                                            xyzabcCommand[2] = (Math.Sign(XyzabcWaitTime[2]) * Vel).ToString();
                                            move_KukaArm("Xyzabc");

                                            Thread.Sleep((int)Math.Round(TempWaitTime[0]));
                                            xyzabcCommand[Order[0]] = "0";
                                            xyzabcCommand[Order[1]] = "0";
                                            move_KukaArm("Xyzabc");

                                            Thread.Sleep((int)Math.Round((TempWaitTime[2] - TempWaitTime[1])));
                                            xyzabcCommand[Order[2]] = "0";
                                            move_KukaArm("Xyzabc");
                                            break;

                                        case 2:
                                            xyzabcCommand[0] = (Math.Sign(XyzabcWaitTime[0]) * Vel).ToString();
                                            xyzabcCommand[1] = (Math.Sign(XyzabcWaitTime[1]) * Vel).ToString();
                                            xyzabcCommand[2] = (Math.Sign(XyzabcWaitTime[2]) * Vel).ToString();
                                            move_KukaArm("Xyzabc");

                                            Thread.Sleep((int)Math.Round(TempWaitTime[0]));
                                            xyzabcCommand[Order[0]] = "0";
                                            move_KukaArm("Xyzabc");

                                            Thread.Sleep((int)Math.Round((TempWaitTime[1] - TempWaitTime[0])));
                                            xyzabcCommand[Order[1]] = "0";
                                            xyzabcCommand[Order[2]] = "0";
                                            move_KukaArm("Xyzabc");
                                            break;

                                        case 3:
                                            xyzabcCommand[0] = (Math.Sign(XyzabcWaitTime[0]) * Vel).ToString();
                                            xyzabcCommand[1] = (Math.Sign(XyzabcWaitTime[1]) * Vel).ToString();
                                            xyzabcCommand[2] = (Math.Sign(XyzabcWaitTime[2]) * Vel).ToString();
                                            move_KukaArm("Xyzabc");

                                            Thread.Sleep((int)Math.Round(TempWaitTime[0]));
                                            xyzabcCommand[Order[0]] = "0";
                                            xyzabcCommand[Order[1]] = "0";
                                            xyzabcCommand[Order[2]] = "0";
                                            move_KukaArm("Xyzabc");
                                            break;

                                        default:
                                            break;
                                    }                                  

                                }
                            }
                        }
                    }
                }
                return true;
            }

            else
            {
                return false;
            }
        }

        public Boolean move_SingleAxis(Coordinate_Axis newAxis, Coordinate_Axis oldAxis)
        {


            if (newAxis.A1 < axisUpLimited[0] && newAxis.A1 > axisDownLimited[0])
            {
                if (newAxis.A2 < axisUpLimited[1] && newAxis.A2 > axisDownLimited[1])
                {
                    if (newAxis.A3 < axisUpLimited[2] && newAxis.A3 > axisDownLimited[2])
                    {
                        if (newAxis.A4 < axisUpLimited[3] && newAxis.A4 > axisDownLimited[3])
                        {
                            if (newAxis.A5 < axisUpLimited[4] && newAxis.A5 > axisDownLimited[4])
                            {
                                if (newAxis.A6 < axisUpLimited[5] && newAxis.A6 > axisDownLimited[5])
                                {
                                    Coordinate_Axis Axis = new Coordinate_Axis();
                                    float[] AxisWaitTime = new float[6];

                                    AxisWaitTime[0] = (newAxis.A1 - oldAxis.A1) / Vel;
                                    AxisWaitTime[1] = (newAxis.A2 - oldAxis.A2) / Vel;
                                    AxisWaitTime[2] = (newAxis.A3 - oldAxis.A3) / Vel;
                                    AxisWaitTime[3] = (newAxis.A4 - oldAxis.A4) / Vel;
                                    AxisWaitTime[4] = (newAxis.A5 - oldAxis.A5) / Vel;
                                    AxisWaitTime[5] = (newAxis.A6 - oldAxis.A6) / Vel;
                                    //Axis.A1 = newAxis.A1 - oldAxis.A1;
                                    //Axis.A2 = newAxis.A2 - oldAxis.A2;
                                    //Axis.A3 = newAxis.A3 - oldAxis.A3;
                                    //Axis.A4 = newAxis.A4 - oldAxis.A4;
                                    //Axis.A5 = newAxis.A5 - oldAxis.A5;
                                    //Axis.A6 = newAxis.A6 - oldAxis.A6;


                                    AxisCommand[0] = (Math.Sign(AxisWaitTime[0]) * Vel).ToString();
                                    AxisCommand[1] = "0";
                                    AxisCommand[2] = "0";
                                    AxisCommand[3] = "0";
                                    AxisCommand[4] = "0";
                                    AxisCommand[5] = "0";
                                    move_KukaArm("Axis");

                                    Thread.Sleep(Math.Abs((int)AxisWaitTime[0]) * 12);
                                    AxisCommand[0] = "0";
                                    AxisCommand[1] = (Math.Sign(AxisWaitTime[1]) * Vel).ToString();
                                    AxisCommand[2] = "0";
                                    AxisCommand[3] = "0";
                                    AxisCommand[4] = "0";
                                    AxisCommand[5] = "0";
                                    move_KukaArm("Axis");

                                    Thread.Sleep(Math.Abs((int)AxisWaitTime[1]) * 12);
                                    AxisCommand[0] = "0";
                                    AxisCommand[1] = "0";
                                    AxisCommand[2] = (Math.Sign(AxisWaitTime[2]) * Vel).ToString();
                                    AxisCommand[3] = "0";
                                    AxisCommand[4] = "0";
                                    AxisCommand[5] = "0";
                                    move_KukaArm("Axis");

                                    Thread.Sleep(Math.Abs((int)AxisWaitTime[2]) * 12);
                                    AxisCommand[0] = "0";
                                    AxisCommand[1] = "0";
                                    AxisCommand[2] = "0";
                                    AxisCommand[3] = (Math.Sign(AxisWaitTime[3]) * Vel).ToString(); ;
                                    AxisCommand[4] = "0";
                                    AxisCommand[5] = "0";
                                    move_KukaArm("Axis");

                                    Thread.Sleep(Math.Abs((int)AxisWaitTime[3]) * 12);
                                    AxisCommand[0] = "0";
                                    AxisCommand[1] = "0";
                                    AxisCommand[2] = "0";
                                    AxisCommand[3] = "0";
                                    AxisCommand[4] = (Math.Sign(AxisWaitTime[4]) * Vel).ToString();
                                    AxisCommand[5] = "0";
                                    move_KukaArm("Axis");

                                    Thread.Sleep(Math.Abs((int)AxisWaitTime[4]) * 12);
                                    AxisCommand[0] = "0";
                                    AxisCommand[1] = "0";
                                    AxisCommand[2] = "0";
                                    AxisCommand[3] = "0";
                                    AxisCommand[4] = "0";
                                    AxisCommand[5] = (Math.Sign(AxisWaitTime[5]) * Vel).ToString();
                                    move_KukaArm("Axis");

                                    Thread.Sleep(Math.Abs((int)AxisWaitTime[5]) * 12);
                                    AxisCommand[0] = "0";
                                    AxisCommand[1] = "0";
                                    AxisCommand[2] = "0";
                                    AxisCommand[3] = "0";
                                    AxisCommand[4] = "0";
                                    AxisCommand[5] = "0";
                                    move_KukaArm("Axis");

                                }
                            }
                        }
                    }
                }
                return true;
            }

            else
            {
                return false;
            }

        }

        public void move_Axis(Coordinate_Axis newAxis)
        {

            AxisCommand[0] = newAxis.A1.ToString();
            AxisCommand[1] = newAxis.A2.ToString();
            AxisCommand[2] = newAxis.A3.ToString();
            AxisCommand[3] = newAxis.A4.ToString();
            AxisCommand[4] = newAxis.A5.ToString();
            AxisCommand[5] = newAxis.A6.ToString();
            move_KukaArm("Axis");
            //(12ms =  一單位)
            Thread.Sleep(12);
            AxisCommand[0] = "0";
            AxisCommand[1] = "0";
            AxisCommand[2] = "0";
            AxisCommand[3] = "0";
            AxisCommand[4] = "0";
            AxisCommand[5] = "0";
            move_KukaArm("Axis");

        }


        
    }
}
