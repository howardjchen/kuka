using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using OpenCvSharp;



namespace KUKA
{
    

    public partial class Form1 : Form
    {
        private KukaRobotControl KukaRobotControlObject = new KukaRobotControl();
        private TxtFile TxtFileObject;
        private Coordinate_XYZABC newXyzabc;
        private Coordinate_XYZABC oldXyzabc;
        private Coordinate_Axis newAxis;
        private Coordinate_Axis oldAxis;
        private String XyzabcMode;
        private String MoveType;
        private int GraspState;
        private int Path_count = 0;
        private String NowTCPPoint = null;
        private float Start_A = 0;
        private Boolean IsArmCanMove = true;
        int GraspObjectType = 0;
        private double[,] Path = new double[20, 6]
                                                {
                                                {27.6185,	-76.8665,	92.0407,	-18.3444,	-3.9479,	-190.7248},
                                                {20.0468,	-86.7375,	106.7662,	-13.5082,	-14.5481,	-188.3828},
                                                {6.5363,	-74.769,	90.9064,	-19.8265,	-13.4989,	-170.8233},
                                                {-9.3461,	-77.3692,	91.0778,	-20.0912,	-11.3522,	-157.4801},
                                                {-26.7664,	-91.8606,	107.8088,	-19.0355,	-18.0934,	-146.3203},
                                                {-29.3269,	-88.6553,	116.6056,	-17.3771,	-36.2137,	-145.2596},
                                                {-17.0804,	-93.7824,	118.9127,	-19.3282,	-29.6642,	-155.9985},
                                                {4.662, 	-86.6109,	111.9172,	-16.2279,	-24.8192,	-175.8832},
                                                {20.9465,	-79.3077,	105.9192,	-16.2279,	-21.6255,	-191.9555},
                                                {28.4595,	-70.2347,	90.2741,	-20.0039,	-9.0681,	-191.0926},
                                                {34.9898,	-51.3426,	74.7548,	 69.6209,	-13.0611,	-281.9815},
                                                {21.882,	-75.6519,	105.6528,	 3.3376,	-21.6826,	-206.4877},
                                                {4.6577,	-93.9722,	127.5326,	-18.5795,	-38.8345,	-179.39},
                                                {-15.6627,	-79.8882,	110.6351,	-43.8333,	-43.7031,	-142.6743},
                                                {-30.2932,	-60.7359,	86.3893,	-62.9088,	-50.9475,	-112.2282},
                                                {-31.706,	-51.162,	77.9897,	-64.8199,	-49.9455,	-105.1093},
                                                {-3.1102,	-77.7806,	111.8073,	-13.8345,	-32.8343,	-170.361},
                                                {24.1765,	-79.9365,	118.2964,	 34.0966,	-44.9176,	-223.9141},
                                                {29.8279,	-80.9112,	123.243,	 43.5355,	-60.4385,	-230.7346},
                                                {39.1503,	-54.602,	88.8541,	 60.7247,	-58.4522,	-255.7336}
                                                };

        //private double[,] Path = new double[12, 6] { {6.11 ,-86.22 ,97.54 ,25.28 ,-10.38 ,-213.78 }, {40.79 ,-81.58 ,100.26 ,66.56 	,-47.88 ,-269.64 }, {-22.99 ,-58.77 ,61.42 ,-94.54 ,-20.68 	,-78.30 },
        //                                            {-23.02 ,-74.23 , 79.02, -76.33 ,-23.31 ,-95.85 }, {2.65 ,-101.73 ,109.86 ,7.08 ,-14.11 ,-194.01 }, {44.90 ,-89.12 ,103.32 ,63.81 ,-44.91 ,-266.40 },
        //                                             {38.27 ,-82.98 ,89.31 ,74.72 ,-43.07 ,-276.37 }, {12.35 ,-92.02 ,95.20 ,71.19 ,-26.30 ,-258.38 }, {-29.57 ,-69.73 ,67.59 ,-90.32 ,-30.03 ,-79.75 },
        //                                             {-27.10 ,-52.09 ,34.24 ,-120.80 ,-36.10 ,-40.75 }, {2.15 ,-82.63 ,76.71 ,-187.39 ,-6.23 ,-1.43 }, {40.61 ,-73.12 ,70.47 ,87.69 ,-46.37 ,-290.97 }};
        public Form1()
        {
            InitializeComponent();
        }

        private delegate void myUICallBack(string myStr, ListBox ctl);
        private void myListBox(string myStr, ListBox ctl)
        {
            if (this.InvokeRequired)
            {
                myUICallBack myUpdate = new myUICallBack(myListBox);
                this.Invoke(myUpdate, myStr, ctl);
            }
            else
            {
                ctl.Items.Add(myStr);
            }
        }

        private delegate void myTextCallBack(string myStr, Color color, Control ctl);
        private void myText(string myStr, Color color, Control ctl)
        {
            if (this.InvokeRequired)
            {
                myTextCallBack myUpdate = new myTextCallBack(myText);
                this.Invoke(myUpdate, myStr, color, ctl);
            }
            else
            {
                ctl.Text = myStr;
                ctl.BackColor = color;
            }
        }


        private void CreatServerThread()
        {
            string kukaStr = "Waiting for KukaController Connect....";
            string poseRecognitionSystemStr = "Waiting for PoseRecognitionSystem Connect....";

            Color color = Color.Red;
            myText(kukaStr, color, KukaClient);
            myText(poseRecognitionSystemStr, color, PoseRecognitionSystemClient);

            bool temp_KukaConnect = false;
            bool temp_PoseRecognitionSystemConnect = false;

            while (!temp_KukaConnect || !temp_PoseRecognitionSystemConnect)
            {
                if (KukaRobotControlObject.Serv.IsConnectKuka == true && temp_KukaConnect == false)
                {
                    temp_KukaConnect = true;
                    kukaStr = KukaRobotControlObject.Serv.Clinet_KukaIP;
                    color = Color.Green;
                    myText(kukaStr, color, KukaClient);
                    Thread UpdateData = new Thread(new ThreadStart(UpdateDataThread));
                    UpdateData.Start();

                    Thread.Sleep(3000);
                    Start_A = float.Parse(A_Value.Text);
                }
                if (KukaRobotControlObject.Serv.IsConnectPoseRecognitionSystem == true && temp_PoseRecognitionSystemConnect == false)
                {
                    temp_PoseRecognitionSystemConnect = true;
                    poseRecognitionSystemStr = KukaRobotControlObject.Serv.ClientPoseRecognitionSystemIP;
                    color = Color.Green;
                    myText(poseRecognitionSystemStr, color, PoseRecognitionSystemClient);

                }
            }
            
            
        }

        private void CreateServerBtn_Click(object sender, EventArgs e)
        {
            if (KukaRobotControlObject.Serv.ServerSocket == null)
            {
                KukaRobotControlObject.startCom();
                Thread CreatServer = new Thread(new ThreadStart(CreatServerThread));
                CreatServer.Start();
            }
            else
            {

            }
        }

        private void UpdateDataThread()
        {
            while (KukaRobotControlObject.Serv.IsConnectKuka)
            {
                if (KukaRobotControlObject.Serv.IsConnectKuka)
                {
                    Thread.Sleep(100);
                    myText(KukaRobotControlObject.Serv.XyzabcData[0], Color.Green, X_Value);
                    myText(KukaRobotControlObject.Serv.XyzabcData[1], Color.Green, Y_Value);
                    myText(KukaRobotControlObject.Serv.XyzabcData[2], Color.Green, Z_Value);
                    myText(KukaRobotControlObject.Serv.XyzabcData[3], Color.Green, A_Value);
                    myText(KukaRobotControlObject.Serv.XyzabcData[4], Color.Green, B_Value);
                    myText(KukaRobotControlObject.Serv.XyzabcData[5], Color.Green, C_Value);
                    myText(KukaRobotControlObject.Serv.AxisData[0], Color.Green, A1_Value);
                    myText(KukaRobotControlObject.Serv.AxisData[1], Color.Green, A2_Value);
                    myText(KukaRobotControlObject.Serv.AxisData[2], Color.Green, A3_Value);
                    myText(KukaRobotControlObject.Serv.AxisData[3], Color.Green, A4_Value);
                    myText(KukaRobotControlObject.Serv.AxisData[4], Color.Green, A5_Value);
                    myText(KukaRobotControlObject.Serv.AxisData[5], Color.Green, A6_Value);

                }
                else
                {
                    continue;
                }
            }
        }

        private void KukaMoveXyzabcModeThread()
        {
            switch (XyzabcMode)
            {
                case "X":
                {
                    if (MoveType == "0")
                    {
                        newXyzabc = new Coordinate_XYZABC();
                        oldXyzabc = new Coordinate_XYZABC();
                        newXyzabc.X = float.Parse(X_Command.Text);
                        newXyzabc.Y = float.Parse(Y_Value.Text);
                        newXyzabc.Z = float.Parse(Z_Value.Text);
                        newXyzabc.A = float.Parse(A_Value.Text);
                        newXyzabc.B = float.Parse(B_Value.Text);
                        newXyzabc.C = float.Parse(C_Value.Text);
                        oldXyzabc.X = float.Parse(X_Value.Text);
                        oldXyzabc.Y = float.Parse(Y_Value.Text);
                        oldXyzabc.Z = float.Parse(Z_Value.Text);
                        oldXyzabc.A = float.Parse(A_Value.Text);
                        oldXyzabc.B = float.Parse(B_Value.Text);
                        oldXyzabc.C = float.Parse(C_Value.Text);
                        if (!KukaRobotControlObject.move_XYZABC_SingalMode(newXyzabc, oldXyzabc))
                            myListBox("Error : Xyzabc Out of limited!!", ListMsg);
                        oldXyzabc = null;
                        newXyzabc = null;
                    }
                    else if (MoveType == "1")
                    {
                        newXyzabc = new Coordinate_XYZABC();
                        newXyzabc.X = 1;
                        newXyzabc.Y = 0;
                        newXyzabc.Z = 0;
                        newXyzabc.A = 0;
                        newXyzabc.B = 0;
                        newXyzabc.C = 0;
                        KukaRobotControlObject.move_XYZABC(newXyzabc);
                        newXyzabc = null;
                    }
                    else if (MoveType == "-1")
                    {
                        newXyzabc = new Coordinate_XYZABC();
                        newXyzabc.X = -1;
                        newXyzabc.Y = 0;
                        newXyzabc.Z = 0;
                        newXyzabc.A = 0;
                        newXyzabc.B = 0;
                        newXyzabc.C = 0;
                        KukaRobotControlObject.move_XYZABC(newXyzabc);
                        newXyzabc = null;
                    }
                    break;
                }
                case "Y":
                {
                    if (MoveType == "0")
                    {
                        newXyzabc = new Coordinate_XYZABC();
                        oldXyzabc = new Coordinate_XYZABC();
                        newXyzabc.X = float.Parse(X_Value.Text);
                        newXyzabc.Y = float.Parse(Y_Command.Text);
                        newXyzabc.Z = float.Parse(Z_Value.Text);
                        newXyzabc.A = float.Parse(A_Value.Text);
                        newXyzabc.B = float.Parse(B_Value.Text);
                        newXyzabc.C = float.Parse(C_Value.Text);
                        oldXyzabc.X = float.Parse(X_Value.Text);
                        oldXyzabc.Y = float.Parse(Y_Value.Text);
                        oldXyzabc.Z = float.Parse(Z_Value.Text);
                        oldXyzabc.A = float.Parse(A_Value.Text);
                        oldXyzabc.B = float.Parse(B_Value.Text);
                        oldXyzabc.C = float.Parse(C_Value.Text);
                        if (!KukaRobotControlObject.move_XYZABC_SingalMode(newXyzabc, oldXyzabc))
                            myListBox("Error : Xyzabc Out of limited!!", ListMsg);
                        oldXyzabc = null;
                        newXyzabc = null;
                    }
                    else if (MoveType == "1")
                    {
                        newXyzabc = new Coordinate_XYZABC();
                        newXyzabc.X = 0;
                        newXyzabc.Y = 1;
                        newXyzabc.Z = 0;
                        newXyzabc.A = 0;
                        newXyzabc.B = 0;
                        newXyzabc.C = 0;
                        KukaRobotControlObject.move_XYZABC(newXyzabc);
                        newXyzabc = null;
                    }
                    else if (MoveType == "-1")
                    {
                        newXyzabc = new Coordinate_XYZABC();
                        newXyzabc.X = 0;
                        newXyzabc.Y = -1;
                        newXyzabc.Z = 0;
                        newXyzabc.A = 0;
                        newXyzabc.B = 0;
                        newXyzabc.C = 0;
                        KukaRobotControlObject.move_XYZABC(newXyzabc);
                        newXyzabc = null;
                    }
                    break;
                }
                case "Z":
                {
                    if (MoveType == "0")
                    {
                        newXyzabc = new Coordinate_XYZABC();
                        oldXyzabc = new Coordinate_XYZABC();
                        newXyzabc.X = float.Parse(X_Value.Text);
                        newXyzabc.Y = float.Parse(Y_Value.Text);
                        newXyzabc.Z = float.Parse(Z_Command.Text);
                        newXyzabc.A = float.Parse(A_Value.Text);
                        newXyzabc.B = float.Parse(B_Value.Text);
                        newXyzabc.C = float.Parse(C_Value.Text);
                        oldXyzabc.X = float.Parse(X_Value.Text);
                        oldXyzabc.Y = float.Parse(Y_Value.Text);
                        oldXyzabc.Z = float.Parse(Z_Value.Text);
                        oldXyzabc.A = float.Parse(A_Value.Text);
                        oldXyzabc.B = float.Parse(B_Value.Text);
                        oldXyzabc.C = float.Parse(C_Value.Text);
                        if (!KukaRobotControlObject.move_XYZABC_SingalMode(newXyzabc, oldXyzabc))
                            myListBox("Error : Xyzabc Out of limited!!", ListMsg);
                        oldXyzabc = null;
                        newXyzabc = null;
                    }
                    else if (MoveType == "1")
                    {
                        newXyzabc = new Coordinate_XYZABC();
                        newXyzabc.X = 0;
                        newXyzabc.Y = 0;
                        newXyzabc.Z = 1;
                        newXyzabc.A = 0;
                        newXyzabc.B = 0;
                        newXyzabc.C = 0;
                        KukaRobotControlObject.move_XYZABC(newXyzabc);
                        newXyzabc = null;
                    }
                    else if (MoveType == "-1")
                    {
                        newXyzabc = new Coordinate_XYZABC();
                        newXyzabc.X = 0;
                        newXyzabc.Y = 0;
                        newXyzabc.Z = -1;
                        newXyzabc.A = 0;
                        newXyzabc.B = 0;
                        newXyzabc.C = 0;
                        KukaRobotControlObject.move_XYZABC(newXyzabc);
                        newXyzabc = null;
                    }
                    break;
                }
                case "A":
                {
                    if (MoveType == "0")
                    {
                        newXyzabc = new Coordinate_XYZABC();
                        oldXyzabc = new Coordinate_XYZABC();
                        newXyzabc.X = float.Parse(X_Value.Text);
                        newXyzabc.Y = float.Parse(Y_Value.Text);
                        newXyzabc.Z = float.Parse(Z_Value.Text);
                        newXyzabc.A = float.Parse(A_Command.Text);
                        newXyzabc.B = float.Parse(B_Value.Text);
                        newXyzabc.C = float.Parse(C_Value.Text);
                        oldXyzabc.X = float.Parse(X_Value.Text);
                        oldXyzabc.Y = float.Parse(Y_Value.Text);
                        oldXyzabc.Z = float.Parse(Z_Value.Text);
                        oldXyzabc.A = float.Parse(A_Value.Text);
                        oldXyzabc.B = float.Parse(B_Value.Text);
                        oldXyzabc.C = float.Parse(C_Value.Text);
                        if (!KukaRobotControlObject.move_XYZABC_RotationMode(newXyzabc, oldXyzabc))
                            myListBox("Error : Xyzabc Out of limited!!", ListMsg);
                        oldXyzabc = null;
                        newXyzabc = null;
                    }
                    else if (MoveType == "1")
                    {
                        newXyzabc = new Coordinate_XYZABC();
                        newXyzabc.X = 0;
                        newXyzabc.Y = 0;
                        newXyzabc.Z = 0;
                        newXyzabc.A = 1;
                        newXyzabc.B = 0;
                        newXyzabc.C = 0;
                        KukaRobotControlObject.move_XYZABC(newXyzabc);
                        newXyzabc = null;
                    }
                    else if (MoveType == "-1")
                    {
                        newXyzabc = new Coordinate_XYZABC();
                        newXyzabc.X = 0;
                        newXyzabc.Y = 0;
                        newXyzabc.Z = 0;
                        newXyzabc.A = -1;
                        newXyzabc.B = 0;
                        newXyzabc.C = 0;
                        KukaRobotControlObject.move_XYZABC(newXyzabc);
                        newXyzabc = null;
                    }
                    break;
                }
                case "B":
                {
                    if (MoveType == "0")
                    {
                        newXyzabc = new Coordinate_XYZABC();
                        oldXyzabc = new Coordinate_XYZABC();
                        newXyzabc.X = float.Parse(X_Value.Text);
                        newXyzabc.Y = float.Parse(Y_Value.Text);
                        newXyzabc.Z = float.Parse(Z_Value.Text);
                        newXyzabc.A = float.Parse(A_Value.Text);
                        newXyzabc.B = float.Parse(B_Command.Text);
                        newXyzabc.C = float.Parse(C_Value.Text);
                        oldXyzabc.X = float.Parse(X_Value.Text);
                        oldXyzabc.Y = float.Parse(Y_Value.Text);
                        oldXyzabc.Z = float.Parse(Z_Value.Text);
                        oldXyzabc.A = float.Parse(A_Value.Text);
                        oldXyzabc.B = float.Parse(B_Value.Text);
                        oldXyzabc.C = float.Parse(C_Value.Text);
                        if (!KukaRobotControlObject.move_XYZABC_RotationMode(newXyzabc, oldXyzabc))
                            myListBox("Error : Xyzabc Out of limited!!", ListMsg);
                        oldXyzabc = null;
                        newXyzabc = null;
                    }
                    else if (MoveType == "1")
                    {
                        newXyzabc = new Coordinate_XYZABC();
                        newXyzabc.X = 0;
                        newXyzabc.Y = 0;
                        newXyzabc.Z = 0;
                        newXyzabc.A = 0;
                        newXyzabc.B = 1;
                        newXyzabc.C = 0;
                        KukaRobotControlObject.move_XYZABC(newXyzabc);
                        newXyzabc = null;
                    }
                    else if (MoveType == "-1")
                    {
                        newXyzabc = new Coordinate_XYZABC();
                        newXyzabc.X = 0;
                        newXyzabc.Y = 0;
                        newXyzabc.Z = 0;
                        newXyzabc.A = 0;
                        newXyzabc.B = -1;
                        newXyzabc.C = 0;
                        KukaRobotControlObject.move_XYZABC(newXyzabc);
                        newXyzabc = null;
                    }
                    break;
                }
                case "C":
                {
                    if (MoveType == "0")
                    {
                        newXyzabc = new Coordinate_XYZABC();
                        oldXyzabc = new Coordinate_XYZABC();
                        newXyzabc.X = float.Parse(X_Value.Text);
                        newXyzabc.Y = float.Parse(Y_Value.Text);
                        newXyzabc.Z = float.Parse(Z_Value.Text);
                        newXyzabc.A = float.Parse(A_Value.Text);
                        newXyzabc.B = float.Parse(B_Value.Text);
                        newXyzabc.C = float.Parse(C_Command.Text);
                        oldXyzabc.X = float.Parse(X_Value.Text);
                        oldXyzabc.Y = float.Parse(Y_Value.Text);
                        oldXyzabc.Z = float.Parse(Z_Value.Text);
                        oldXyzabc.A = float.Parse(A_Value.Text);
                        oldXyzabc.B = float.Parse(B_Value.Text);
                        oldXyzabc.C = float.Parse(C_Value.Text);
                        if (!KukaRobotControlObject.move_XYZABC_RotationMode(newXyzabc, oldXyzabc))
                            myListBox("Error : Xyzabc Out of limited!!", ListMsg);
                        oldXyzabc = null;
                        newXyzabc = null;
                    }
                    else if (MoveType == "1")
                    {
                        newXyzabc = new Coordinate_XYZABC();
                        newXyzabc.X = 0;
                        newXyzabc.Y = 0;
                        newXyzabc.Z = 0;
                        newXyzabc.A = 0;
                        newXyzabc.B = 0;
                        newXyzabc.C = 1;
                        KukaRobotControlObject.move_XYZABC(newXyzabc);
                        newXyzabc = null;
                    }
                    else if (MoveType == "-1")
                    {
                        newXyzabc = new Coordinate_XYZABC();
                        newXyzabc.X = 0;
                        newXyzabc.Y = 0;
                        newXyzabc.Z = 0;
                        newXyzabc.A = 0;
                        newXyzabc.B = 0;
                        newXyzabc.C = -1;
                        KukaRobotControlObject.move_XYZABC(newXyzabc);
                        newXyzabc = null;
                    }
                    break;
                }

                case "A1" :
                {
                    if (MoveType == "0")
                    {

                    }
                    else if (MoveType == "1")
                    {
                        newAxis = new Coordinate_Axis();
                        newAxis.A1 = 1;
                        newAxis.A2 = 0;
                        newAxis.A3 = 0;
                        newAxis.A4 = 0;
                        newAxis.A5 = 0;
                        newAxis.A6 = 0;
                        KukaRobotControlObject.move_Axis(newAxis);
                        newAxis = null;
                    }
                    else if (MoveType == "-1")
                    {
                        newAxis = new Coordinate_Axis();
                        newAxis.A1 = -1;
                        newAxis.A2 = 0;
                        newAxis.A3 = 0;
                        newAxis.A4 = 0;
                        newAxis.A5 = 0;
                        newAxis.A6 = 0;
                        KukaRobotControlObject.move_Axis(newAxis);
                        newAxis = null;
                    }
                    break;
                }
                case "A2":
                {
                    if (MoveType == "0")
                    {

                    }
                    else if (MoveType == "1")
                    {
                        newAxis = new Coordinate_Axis();
                        newAxis.A1 = 0;
                        newAxis.A2 = 1;
                        newAxis.A3 = 0;
                        newAxis.A4 = 0;
                        newAxis.A5 = 0;
                        newAxis.A6 = 0;
                        KukaRobotControlObject.move_Axis(newAxis);
                        newAxis = null;
                    }
                    else if (MoveType == "-1")
                    {
                        newAxis = new Coordinate_Axis();
                        newAxis.A1 = 0;
                        newAxis.A2 = -1;
                        newAxis.A3 = 0;
                        newAxis.A4 = 0;
                        newAxis.A5 = 0;
                        newAxis.A6 = 0;
                        KukaRobotControlObject.move_Axis(newAxis);
                        newAxis = null;
                    }
                    break;
                }
                case "A3":
                {
                    if (MoveType == "0")
                    {

                    }
                    else if (MoveType == "1")
                    {
                        newAxis = new Coordinate_Axis();
                        newAxis.A1 = 0;
                        newAxis.A2 = 0;
                        newAxis.A3 = 1;
                        newAxis.A4 = 0;
                        newAxis.A5 = 0;
                        newAxis.A6 = 0;
                        KukaRobotControlObject.move_Axis(newAxis);
                        newAxis = null;
                    }
                    else if (MoveType == "-1")
                    {
                        newAxis = new Coordinate_Axis();
                        newAxis.A1 = 0;
                        newAxis.A2 = 0;
                        newAxis.A3 = -1;
                        newAxis.A4 = 0;
                        newAxis.A5 = 0;
                        newAxis.A6 = 0;
                        KukaRobotControlObject.move_Axis(newAxis);
                        newAxis = null;
                    }
                    break;
                }
                case "A4":
                {
                    if (MoveType == "0")
                    {

                    }
                    else if (MoveType == "1")
                    {
                        newAxis = new Coordinate_Axis();
                        newAxis.A1 = 0;
                        newAxis.A2 = 0;
                        newAxis.A3 = 0;
                        newAxis.A4 = 1;
                        newAxis.A5 = 0;
                        newAxis.A6 = 0;
                        KukaRobotControlObject.move_Axis(newAxis);
                        newAxis = null;
                    }
                    else if (MoveType == "-1")
                    {
                        newAxis = new Coordinate_Axis();
                        newAxis.A1 = 0;
                        newAxis.A2 = 0;
                        newAxis.A3 = 0;
                        newAxis.A4 = -1;
                        newAxis.A5 = 0;
                        newAxis.A6 = 0;
                        KukaRobotControlObject.move_Axis(newAxis);
                        newAxis = null;
                    }
                    break;
                }
                case "A5":
                {
                    if (MoveType == "0")
                    {

                    }
                    else if (MoveType == "1")
                    {
                        newAxis = new Coordinate_Axis();
                        newAxis.A1 = 0;
                        newAxis.A2 = 0;
                        newAxis.A3 = 0;
                        newAxis.A4 = 0;
                        newAxis.A5 = 1;
                        newAxis.A6 = 0;
                        KukaRobotControlObject.move_Axis(newAxis);
                        newAxis = null;
                    }
                    else if (MoveType == "-1")
                    {
                        newAxis = new Coordinate_Axis();
                        newAxis.A1 = 0;
                        newAxis.A2 = 0;
                        newAxis.A3 = 0;
                        newAxis.A4 = 0;
                        newAxis.A5 = -1;
                        newAxis.A6 = 0;
                        KukaRobotControlObject.move_Axis(newAxis);
                        newAxis = null;
                    }
                    break;
                }
                case "A6":
                {
                    if (MoveType == "0")
                    {

                    }
                    else if (MoveType == "1")
                    {
                        newAxis = new Coordinate_Axis();
                        newAxis.A1 = 0;
                        newAxis.A2 = 0;
                        newAxis.A3 = 0;
                        newAxis.A4 = 0;
                        newAxis.A5 = 0;
                        newAxis.A6 = 1;
                        KukaRobotControlObject.move_Axis(newAxis);
                        newAxis = null;
                    }
                    else if (MoveType == "-1")
                    {
                        newAxis = new Coordinate_Axis();
                        newAxis.A1 = 0;
                        newAxis.A2 = 0;
                        newAxis.A3 = 0;
                        newAxis.A4 = 0;
                        newAxis.A5 = 0;
                        newAxis.A6 = -1;
                        KukaRobotControlObject.move_Axis(newAxis);
                        newAxis = null;
                    }
                    break;
                }
                case "Xyzabc":
                {
                    newXyzabc = new Coordinate_XYZABC();
                    oldXyzabc = new Coordinate_XYZABC();
                    newXyzabc.X = float.Parse(X_Command.Text);
                    newXyzabc.Y = float.Parse(Y_Command.Text);
                    newXyzabc.Z = float.Parse(Z_Command.Text);
                    newXyzabc.A = float.Parse(A_Value.Text);
                    newXyzabc.B = float.Parse(B_Value.Text);
                    newXyzabc.C = float.Parse(C_Value.Text);
                    oldXyzabc.X = float.Parse(X_Value.Text);
                    oldXyzabc.Y = float.Parse(Y_Value.Text);
                    oldXyzabc.Z = float.Parse(Z_Value.Text);
                    oldXyzabc.A = float.Parse(A_Value.Text);
                    oldXyzabc.B = float.Parse(B_Value.Text);
                    oldXyzabc.C = float.Parse(C_Value.Text);

                    //if (!KukaRobotControlObject.move_XYZABC_TogetherMode(newXyzabc, oldXyzabc))
                    if (!KukaRobotControlObject.move_XYZABC_TogetherMode(newXyzabc, oldXyzabc))
                        myListBox("Error : Xyzabc Out of limited!!", ListMsg);
                    oldXyzabc = null;
                    newXyzabc = null;
                    break;
                }
                case "Axis":
                {
                    if (MoveType == "0")
                    {
                        newAxis = new Coordinate_Axis();
                        oldAxis = new Coordinate_Axis();
                        newAxis.A1 = float.Parse(A1_Command.Text);
                        newAxis.A2 = float.Parse(A2_Command.Text);
                        newAxis.A3 = float.Parse(A3_Command.Text);
                        newAxis.A4 = float.Parse(A4_Command.Text);
                        newAxis.A5 = float.Parse(A5_Command.Text);
                        newAxis.A6 = float.Parse(A6_Command.Text);
                        oldAxis.A1 = float.Parse(A1_Value.Text);
                        oldAxis.A2 = float.Parse(A2_Value.Text);
                        oldAxis.A3 = float.Parse(A3_Value.Text);
                        oldAxis.A4 = float.Parse(A4_Value.Text);
                        oldAxis.A5 = float.Parse(A5_Value.Text);
                        oldAxis.A6 = float.Parse(A6_Value.Text);

                        //if (!KukaRobotControlObject.move_XYZABC_TogetherMode(newXyzabc, oldXyzabc))
                        if (!KukaRobotControlObject.move_SingleAxis(newAxis, oldAxis))
                            myListBox("Error : Axis Out of limited!!", ListMsg);
                        oldAxis = null;
                        newAxis = null;

                    }
                    else if (MoveType == "1")
                    {
                        newAxis = new Coordinate_Axis();
                        oldAxis = new Coordinate_Axis();
                        newAxis.A1 = float.Parse(A1_Command.Text);
                        newAxis.A2 = float.Parse(A2_Value.Text);
                        newAxis.A3 = float.Parse(A3_Value.Text);
                        newAxis.A4 = float.Parse(A4_Value.Text);
                        newAxis.A5 = float.Parse(A5_Value.Text);
                        newAxis.A6 = float.Parse(A6_Value.Text);
                        oldAxis.A1 = float.Parse(A1_Value.Text);
                        oldAxis.A2 = float.Parse(A2_Value.Text);
                        oldAxis.A3 = float.Parse(A3_Value.Text);
                        oldAxis.A4 = float.Parse(A4_Value.Text);
                        oldAxis.A5 = float.Parse(A5_Value.Text);
                        oldAxis.A6 = float.Parse(A6_Value.Text);

                        //if (!KukaRobotControlObject.move_XYZABC_TogetherMode(newXyzabc, oldXyzabc))
                        if (!KukaRobotControlObject.move_SingleAxis(newAxis, oldAxis))
                            myListBox("Error : Axis Out of limited!!", ListMsg);
                        oldAxis = null;
                        newAxis = null;
                    }
                    else if (MoveType == "2")
                    {
                        newAxis = new Coordinate_Axis();
                        oldAxis = new Coordinate_Axis();
                        newAxis.A1 = float.Parse(A1_Value.Text);
                        newAxis.A2 = float.Parse(A2_Command.Text);
                        newAxis.A3 = float.Parse(A3_Value.Text);
                        newAxis.A4 = float.Parse(A4_Value.Text);
                        newAxis.A5 = float.Parse(A5_Value.Text);
                        newAxis.A6 = float.Parse(A6_Value.Text);
                        oldAxis.A1 = float.Parse(A1_Value.Text);
                        oldAxis.A2 = float.Parse(A2_Value.Text);
                        oldAxis.A3 = float.Parse(A3_Value.Text);
                        oldAxis.A4 = float.Parse(A4_Value.Text);
                        oldAxis.A5 = float.Parse(A5_Value.Text);
                        oldAxis.A6 = float.Parse(A6_Value.Text);

                        //if (!KukaRobotControlObject.move_XYZABC_TogetherMode(newXyzabc, oldXyzabc))
                        if (!KukaRobotControlObject.move_SingleAxis(newAxis, oldAxis))
                            myListBox("Error : Axis Out of limited!!", ListMsg);
                        oldAxis = null;
                        newAxis = null;
                    }
                    else if (MoveType == "3")
                    {
                        newAxis = new Coordinate_Axis();
                        oldAxis = new Coordinate_Axis();
                        newAxis.A1 = float.Parse(A1_Value.Text);
                        newAxis.A2 = float.Parse(A2_Value.Text);
                        newAxis.A3 = float.Parse(A3_Command.Text);
                        newAxis.A4 = float.Parse(A4_Value.Text);
                        newAxis.A5 = float.Parse(A5_Value.Text);
                        newAxis.A6 = float.Parse(A6_Value.Text);
                        oldAxis.A1 = float.Parse(A1_Value.Text);
                        oldAxis.A2 = float.Parse(A2_Value.Text);
                        oldAxis.A3 = float.Parse(A3_Value.Text);
                        oldAxis.A4 = float.Parse(A4_Value.Text);
                        oldAxis.A5 = float.Parse(A5_Value.Text);
                        oldAxis.A6 = float.Parse(A6_Value.Text);

                        //if (!KukaRobotControlObject.move_XYZABC_TogetherMode(newXyzabc, oldXyzabc))
                        if (!KukaRobotControlObject.move_SingleAxis(newAxis, oldAxis))
                            myListBox("Error : Axis Out of limited!!", ListMsg);
                        oldAxis = null;
                        newAxis = null;
                    }
                    else if (MoveType == "4")
                    {
                        newAxis = new Coordinate_Axis();
                        oldAxis = new Coordinate_Axis();
                        newAxis.A1 = float.Parse(A1_Value.Text);
                        newAxis.A2 = float.Parse(A2_Value.Text);
                        newAxis.A3 = float.Parse(A3_Value.Text);
                        newAxis.A4 = float.Parse(A4_Command.Text);
                        newAxis.A5 = float.Parse(A5_Value.Text);
                        newAxis.A6 = float.Parse(A6_Value.Text);
                        oldAxis.A1 = float.Parse(A1_Value.Text);
                        oldAxis.A2 = float.Parse(A2_Value.Text);
                        oldAxis.A3 = float.Parse(A3_Value.Text);
                        oldAxis.A4 = float.Parse(A4_Value.Text);
                        oldAxis.A5 = float.Parse(A5_Value.Text);
                        oldAxis.A6 = float.Parse(A6_Value.Text);

                        //if (!KukaRobotControlObject.move_XYZABC_TogetherMode(newXyzabc, oldXyzabc))
                        if (!KukaRobotControlObject.move_SingleAxis(newAxis, oldAxis))
                            myListBox("Error : Axis Out of limited!!", ListMsg);
                        oldAxis = null;
                        newAxis = null;
                    }
                    else if (MoveType == "5")
                    {
                        newAxis = new Coordinate_Axis();
                        oldAxis = new Coordinate_Axis();
                        newAxis.A1 = float.Parse(A1_Value.Text);
                        newAxis.A2 = float.Parse(A2_Value.Text);
                        newAxis.A3 = float.Parse(A3_Value.Text);
                        newAxis.A4 = float.Parse(A4_Value.Text);
                        newAxis.A5 = float.Parse(A5_Command.Text);
                        newAxis.A6 = float.Parse(A6_Value.Text);
                        oldAxis.A1 = float.Parse(A1_Value.Text);
                        oldAxis.A2 = float.Parse(A2_Value.Text);
                        oldAxis.A3 = float.Parse(A3_Value.Text);
                        oldAxis.A4 = float.Parse(A4_Value.Text);
                        oldAxis.A5 = float.Parse(A5_Value.Text);
                        oldAxis.A6 = float.Parse(A6_Value.Text);

                        //if (!KukaRobotControlObject.move_XYZABC_TogetherMode(newXyzabc, oldXyzabc))
                        if (!KukaRobotControlObject.move_SingleAxis(newAxis, oldAxis))
                            myListBox("Error : Axis Out of limited!!", ListMsg);
                        oldAxis = null;
                        newAxis = null;
                    }
                    else if (MoveType == "6")
                    {
                        newAxis = new Coordinate_Axis();
                        oldAxis = new Coordinate_Axis();
                        newAxis.A1 = float.Parse(A1_Value.Text);
                        newAxis.A2 = float.Parse(A2_Value.Text);
                        newAxis.A3 = float.Parse(A3_Value.Text);
                        newAxis.A4 = float.Parse(A4_Value.Text);
                        newAxis.A5 = float.Parse(A5_Value.Text);
                        newAxis.A6 = float.Parse(A6_Command.Text);
                        oldAxis.A1 = float.Parse(A1_Value.Text);
                        oldAxis.A2 = float.Parse(A2_Value.Text);
                        oldAxis.A3 = float.Parse(A3_Value.Text);
                        oldAxis.A4 = float.Parse(A4_Value.Text);
                        oldAxis.A5 = float.Parse(A5_Value.Text);
                        oldAxis.A6 = float.Parse(A6_Value.Text);

                        //if (!KukaRobotControlObject.move_XYZABC_TogetherMode(newXyzabc, oldXyzabc))
                        if (!KukaRobotControlObject.move_SingleAxis(newAxis, oldAxis))
                            myListBox("Error : Axis Out of limited!!", ListMsg);
                        oldAxis = null;
                        newAxis = null;
                    }
                    else if (MoveType == "9")
                    {
                        newAxis = new Coordinate_Axis();
                        oldAxis = new Coordinate_Axis();
                        newAxis.A1 = float.Parse(Path[Path_count, 0].ToString());
                        newAxis.A2 = float.Parse(Path[Path_count, 1].ToString());
                        newAxis.A3 = float.Parse(Path[Path_count, 2].ToString());
                        newAxis.A4 = float.Parse(Path[Path_count, 3].ToString());
                        newAxis.A5 = float.Parse(Path[Path_count, 4].ToString());
                        newAxis.A6 = float.Parse(Path[Path_count, 5].ToString());
                        oldAxis.A1 = float.Parse(A1_Value.Text);
                        oldAxis.A2 = float.Parse(A2_Value.Text);
                        oldAxis.A3 = float.Parse(A3_Value.Text);
                        oldAxis.A4 = float.Parse(A4_Value.Text);
                        oldAxis.A5 = float.Parse(A5_Value.Text);
                        oldAxis.A6 = float.Parse(A6_Value.Text);

                        //if (!KukaRobotControlObject.move_XYZABC_TogetherMode(newXyzabc, oldXyzabc))
                        if (!KukaRobotControlObject.move_SingleAxis(newAxis, oldAxis))
                            myListBox("Error : Xyzabc Out of limited!!", ListMsg);
                        oldAxis = null;
                        newAxis = null;
                        Path_count++;
                        if (Path_count == 20)
                        {
                            Path_count = 0;
                        }
                    }
                    break;
                }
                case "XyzabcHome":
                {
                    newAxis = new Coordinate_Axis();
                    oldAxis = new Coordinate_Axis();
                    newAxis.A1 = 0;
                    newAxis.A2 = -90;
                    newAxis.A3 = 90;
                    newAxis.A4 = 0;
                    newAxis.A5 = -90;
                    newAxis.A6 = -180;
                    oldAxis.A1 = float.Parse(A1_Value.Text);
                    oldAxis.A2 = float.Parse(A2_Value.Text);
                    oldAxis.A3 = float.Parse(A3_Value.Text);
                    oldAxis.A4 = float.Parse(A4_Value.Text);
                    oldAxis.A5 = float.Parse(A5_Value.Text);
                    oldAxis.A6 = float.Parse(A6_Value.Text);

                    //if (!KukaRobotControlObject.move_XYZABC_TogetherMode(newXyzabc, oldXyzabc))
                    if (!KukaRobotControlObject.move_SingleAxis(newAxis, oldAxis))
                        myListBox("Error : Xyzabc Out of limited!!", ListMsg);
                    oldAxis = null;
                    newAxis = null;
                    break;

                }
                case "PoseEstimation":
                {
                    if (MoveType == "0")
                    {
                        newXyzabc = new Coordinate_XYZABC();
                        oldXyzabc = new Coordinate_XYZABC();
                        newXyzabc.X = KukaRobotControlObject.Serv.PoseRecognitionSystem_Data.CaptureImage_Position[0];
                        newXyzabc.Y = KukaRobotControlObject.Serv.PoseRecognitionSystem_Data.CaptureImage_Position[1];
                        newXyzabc.Z = KukaRobotControlObject.Serv.PoseRecognitionSystem_Data.GraspInitial_Position[2];
                        newXyzabc.A = float.Parse(A_Value.Text);
                        newXyzabc.B = float.Parse(B_Value.Text);
                        newXyzabc.C = float.Parse(C_Value.Text);
                        oldXyzabc.X = float.Parse(X_Value.Text);
                        oldXyzabc.Y = float.Parse(Y_Value.Text);
                        oldXyzabc.Z = float.Parse(Z_Value.Text);
                        oldXyzabc.A = float.Parse(A_Value.Text);
                        oldXyzabc.B = float.Parse(B_Value.Text);
                        oldXyzabc.C = float.Parse(C_Value.Text);
                        if (!KukaRobotControlObject.move_XYZABC_TogetherMode(newXyzabc, oldXyzabc))
                            myListBox("Error : Xyzabc Out of limited!!", ListMsg);                      
                        oldXyzabc = null;
                        newXyzabc = null;


                        newXyzabc = new Coordinate_XYZABC();
                        oldXyzabc = new Coordinate_XYZABC();
                        newXyzabc.X = float.Parse(X_Value.Text);
                        newXyzabc.Y = float.Parse(Y_Value.Text);
                        newXyzabc.Z = float.Parse(Z_Value.Text);
                        newXyzabc.A = float.Parse(A_Value.Text);
                        newXyzabc.B = KukaRobotControlObject.Serv.PoseRecognitionSystem_Data.CaptureImage_Position[4];
                        newXyzabc.C = float.Parse(C_Value.Text);
                        oldXyzabc.X = float.Parse(X_Value.Text);
                        oldXyzabc.Y = float.Parse(Y_Value.Text);
                        oldXyzabc.Z = float.Parse(Z_Value.Text);
                        oldXyzabc.A = float.Parse(A_Value.Text);
                        oldXyzabc.B = KukaRobotControlObject.Serv.PoseRecognitionSystem_Data.GraspInitial_Position[4];
                        oldXyzabc.C = float.Parse(C_Value.Text);
                        if (!KukaRobotControlObject.move_XYZABC_RotationMode(newXyzabc, oldXyzabc))
                            myListBox("Error : Xyzabc Out of limited!!", ListMsg);
                        oldXyzabc = null;
                        newXyzabc = null;

                        newXyzabc = new Coordinate_XYZABC();
                        oldXyzabc = new Coordinate_XYZABC();
                        newXyzabc.X = float.Parse(X_Value.Text);
                        newXyzabc.Y = float.Parse(Y_Value.Text);
                        newXyzabc.Z = KukaRobotControlObject.Serv.PoseRecognitionSystem_Data.CaptureImage_Position[2];
                        newXyzabc.A = float.Parse(A_Value.Text);
                        newXyzabc.B = float.Parse(B_Value.Text);
                        newXyzabc.C = float.Parse(C_Value.Text);
                        oldXyzabc.X = float.Parse(X_Value.Text);
                        oldXyzabc.Y = float.Parse(Y_Value.Text);
                        oldXyzabc.Z = float.Parse(Z_Value.Text);
                        oldXyzabc.A = float.Parse(A_Value.Text);
                        oldXyzabc.B = float.Parse(B_Value.Text);
                        oldXyzabc.C = float.Parse(C_Value.Text);
                        if (!KukaRobotControlObject.move_XYZABC_SingalMode(newXyzabc, oldXyzabc))
                            myListBox("Error : Xyzabc Out of limited!!", ListMsg);
                        oldXyzabc = null;
                        newXyzabc = null;
                     
                        IsArmCanMove = true;
                        KukaRobotControlObject.Serv.PoseRecognitionSystem_Data._State = 9;
                        
                    }
                    else if (MoveType == "1")
                    {
                        
                        newXyzabc = new Coordinate_XYZABC();
                        oldXyzabc = new Coordinate_XYZABC();
                        newXyzabc.X = float.Parse(X_Value.Text);
                        newXyzabc.Y = float.Parse(Y_Value.Text);
                        newXyzabc.Z = KukaRobotControlObject.Serv.PoseRecognitionSystem_Data.GraspInitial_Position[2];
                        newXyzabc.A = float.Parse(A_Value.Text);
                        newXyzabc.B = float.Parse(B_Value.Text);
                        newXyzabc.C = float.Parse(C_Value.Text);
                        oldXyzabc.X = float.Parse(X_Value.Text);
                        oldXyzabc.Y = float.Parse(Y_Value.Text);
                        oldXyzabc.Z = float.Parse(Z_Value.Text);
                        oldXyzabc.A = float.Parse(A_Value.Text);
                        oldXyzabc.B = float.Parse(B_Value.Text);
                        oldXyzabc.C = float.Parse(C_Value.Text);
                        if (!KukaRobotControlObject.move_XYZABC_SingalMode(newXyzabc, oldXyzabc))
                            myListBox("Error : Xyzabc Out of limited!!", ListMsg);
                        oldXyzabc = null;
                        newXyzabc = null;


                        newXyzabc = new Coordinate_XYZABC();
                        oldXyzabc = new Coordinate_XYZABC();
                        newXyzabc.X = float.Parse(X_Value.Text);
                        newXyzabc.Y = float.Parse(Y_Value.Text);
                        newXyzabc.Z = float.Parse(Z_Value.Text);
                        newXyzabc.A = float.Parse(A_Value.Text);
                        newXyzabc.B = KukaRobotControlObject.Serv.PoseRecognitionSystem_Data.GraspInitial_Position[4];
                        newXyzabc.C = float.Parse(C_Value.Text);
                        oldXyzabc.X = float.Parse(X_Value.Text);
                        oldXyzabc.Y = float.Parse(Y_Value.Text);
                        oldXyzabc.Z = float.Parse(Z_Value.Text);
                        oldXyzabc.A = float.Parse(A_Value.Text);
                        oldXyzabc.B = KukaRobotControlObject.Serv.PoseRecognitionSystem_Data.CaptureImage_Position[4];
                        oldXyzabc.C = float.Parse(C_Value.Text);
                        if (!KukaRobotControlObject.move_XYZABC_RotationMode(newXyzabc, oldXyzabc))
                            myListBox("Error : Xyzabc Out of limited!!", ListMsg);
                        oldXyzabc = null;
                        newXyzabc = null;


                        newXyzabc = new Coordinate_XYZABC();
                        oldXyzabc = new Coordinate_XYZABC();
                        newXyzabc.X = KukaRobotControlObject.Serv.PoseRecognitionSystem_Data.GraspInitial_Position[0];
                        newXyzabc.Y = KukaRobotControlObject.Serv.PoseRecognitionSystem_Data.GraspInitial_Position[1];
                        newXyzabc.Z = float.Parse(Z_Value.Text);
                        newXyzabc.A = float.Parse(A_Value.Text);
                        newXyzabc.B = float.Parse(B_Value.Text);
                        newXyzabc.C = float.Parse(C_Value.Text);
                        oldXyzabc.X = float.Parse(X_Value.Text);
                        oldXyzabc.Y = float.Parse(Y_Value.Text);
                        oldXyzabc.Z = float.Parse(Z_Value.Text);
                        oldXyzabc.A = float.Parse(A_Value.Text);
                        oldXyzabc.B = float.Parse(B_Value.Text);
                        oldXyzabc.C = float.Parse(C_Value.Text);
                        if (!KukaRobotControlObject.move_XYZABC_TogetherMode(newXyzabc, oldXyzabc))
                            myListBox("Error : Xyzabc Out of limited!!", ListMsg);
                        oldXyzabc = null;
                        newXyzabc = null;

                        IsArmCanMove = true;
                        KukaRobotControlObject.Serv.PoseRecognitionSystem_Data._State = 9;
                        
                    }
                    else if (MoveType == "2")
                    {
                        /*
                         *  抓取物件
                         */
                        GraspObjectType = KukaRobotControlObject.Serv.PoseRecognitionSystem_Data._ObjectType;
                        Start_A = float.Parse(A_Value.Text);

                        newXyzabc = new Coordinate_XYZABC();
                        oldXyzabc = new Coordinate_XYZABC();
                        newXyzabc.X = float.Parse(X_Value.Text);
                        newXyzabc.Y = float.Parse(Y_Value.Text);
                        newXyzabc.Z = float.Parse(Z_Value.Text);
                        newXyzabc.A = Start_A + KukaRobotControlObject.Serv.PoseRecognitionSystem_Data._A;
                        newXyzabc.B = float.Parse(B_Value.Text);
                        newXyzabc.C = float.Parse(C_Value.Text);
                        oldXyzabc.X = float.Parse(X_Value.Text);
                        oldXyzabc.Y = float.Parse(Y_Value.Text);
                        oldXyzabc.Z = float.Parse(Z_Value.Text);
                        oldXyzabc.A = Start_A;
                        oldXyzabc.B = float.Parse(B_Value.Text);
                        oldXyzabc.C = float.Parse(C_Value.Text);
                        if (!KukaRobotControlObject.move_XYZABC_RotationMode(newXyzabc, oldXyzabc))
                            myListBox("Error : Xyzabc Out of limited!!", ListMsg);
                        oldXyzabc = null;
                        newXyzabc = null;

                        newXyzabc = new Coordinate_XYZABC();
                        oldXyzabc = new Coordinate_XYZABC();
                        newXyzabc.X = KukaRobotControlObject.Serv.PoseRecognitionSystem_Data._X;
                        newXyzabc.Y = KukaRobotControlObject.Serv.PoseRecognitionSystem_Data._Y;
                        newXyzabc.Z = KukaRobotControlObject.Serv.PoseRecognitionSystem_Data._Z;
                        newXyzabc.A = float.Parse(A_Value.Text);
                        newXyzabc.B = float.Parse(B_Value.Text);
                        newXyzabc.C = float.Parse(C_Value.Text);
                        oldXyzabc.X = float.Parse(X_Value.Text);
                        oldXyzabc.Y = float.Parse(Y_Value.Text);
                        oldXyzabc.Z = float.Parse(Z_Value.Text);
                        oldXyzabc.A = float.Parse(A_Value.Text);
                        oldXyzabc.B = float.Parse(B_Value.Text);
                        oldXyzabc.C = float.Parse(C_Value.Text);
                        if (!KukaRobotControlObject.move_XYZABC_TogetherMode(newXyzabc, oldXyzabc))
                            myListBox("Error : Xyzabc Out of limited!!", ListMsg);
                        oldXyzabc = null;
                        newXyzabc = null;

                        /*
                         *  抓取物件
                         */
                        GraspState = 1;
                        Thread GraspControl = new Thread(new ThreadStart(GraspControlThread));
                        GraspControl.Start();
                        Thread.Sleep(200);

                        /*
                         *  往上移動至擷取圖像點
                         */

                        newXyzabc = new Coordinate_XYZABC();
                        oldXyzabc = new Coordinate_XYZABC();
                        newXyzabc.X = float.Parse(X_Value.Text);
                        newXyzabc.Y = float.Parse(Y_Value.Text);
                        newXyzabc.Z = KukaRobotControlObject.Serv.PoseRecognitionSystem_Data.GraspInitial_Position[2];
                        newXyzabc.A = float.Parse(A_Value.Text);
                        newXyzabc.B = float.Parse(B_Value.Text);
                        newXyzabc.C = float.Parse(C_Value.Text);
                        oldXyzabc.X = float.Parse(X_Value.Text);
                        oldXyzabc.Y = float.Parse(Y_Value.Text);
                        oldXyzabc.Z = float.Parse(Z_Value.Text);
                        oldXyzabc.A = float.Parse(A_Value.Text);
                        oldXyzabc.B = float.Parse(B_Value.Text);
                        oldXyzabc.C = float.Parse(C_Value.Text);
                        if (!KukaRobotControlObject.move_XYZABC_SingalMode(newXyzabc, oldXyzabc))
                            myListBox("Error : Xyzabc Out of limited!!", ListMsg);
                        oldXyzabc = null;
                        newXyzabc = null;

                        newXyzabc = new Coordinate_XYZABC();
                        oldXyzabc = new Coordinate_XYZABC();
                        newXyzabc.X = KukaRobotControlObject.Serv.PoseRecognitionSystem_Data.GraspInitial_Position[0];
                        newXyzabc.Y = KukaRobotControlObject.Serv.PoseRecognitionSystem_Data.GraspInitial_Position[1];
                        newXyzabc.Z = float.Parse(Z_Value.Text);
                        newXyzabc.A = float.Parse(A_Value.Text);
                        newXyzabc.B = float.Parse(B_Value.Text);
                        newXyzabc.C = float.Parse(C_Value.Text);
                        oldXyzabc.X = float.Parse(X_Value.Text);
                        oldXyzabc.Y = float.Parse(Y_Value.Text);
                        oldXyzabc.Z = float.Parse(Z_Value.Text);
                        oldXyzabc.A = float.Parse(A_Value.Text);
                        oldXyzabc.B = float.Parse(B_Value.Text);
                        oldXyzabc.C = float.Parse(C_Value.Text);
                        if (!KukaRobotControlObject.move_XYZABC_TogetherMode(newXyzabc, oldXyzabc))
                            myListBox("Error : Xyzabc Out of limited!!", ListMsg);
                        oldXyzabc = null;
                        newXyzabc = null;

                        newXyzabc = new Coordinate_XYZABC();
                        oldXyzabc = new Coordinate_XYZABC();
                        newXyzabc.X = float.Parse(X_Value.Text);
                        newXyzabc.Y = float.Parse(Y_Value.Text);
                        newXyzabc.Z = float.Parse(Z_Value.Text);
                        newXyzabc.A = Start_A;
                        newXyzabc.B = float.Parse(B_Value.Text);
                        newXyzabc.C = float.Parse(C_Value.Text);
                        oldXyzabc.X = float.Parse(X_Value.Text);
                        oldXyzabc.Y = float.Parse(Y_Value.Text);
                        oldXyzabc.Z = float.Parse(Z_Value.Text);
                        oldXyzabc.A = Start_A + KukaRobotControlObject.Serv.PoseRecognitionSystem_Data._A;
                        oldXyzabc.B = float.Parse(B_Value.Text);
                        oldXyzabc.C = float.Parse(C_Value.Text);
                        if (!KukaRobotControlObject.move_XYZABC_RotationMode(newXyzabc, oldXyzabc))
                            myListBox("Error : Xyzabc Out of limited!!", ListMsg);
                        oldXyzabc = null;
                        newXyzabc = null;

                       /* newXyzabc = new Coordinate_XYZABC();
                        oldXyzabc = new Coordinate_XYZABC();
                        newXyzabc.X = float.Parse(X_Value.Text);
                        newXyzabc.Y = float.Parse(Y_Value.Text);
                        newXyzabc.Z = float.Parse(Z_Value.Text);
                        newXyzabc.A = float.Parse(A_Value.Text);
                        newXyzabc.B = KukaRobotControlObject.Serv.PoseRecognitionSystem_Data.CaptureImage_Position[4];
                        newXyzabc.C = float.Parse(C_Value.Text);
                        oldXyzabc.X = float.Parse(X_Value.Text);
                        oldXyzabc.Y = float.Parse(Y_Value.Text);
                        oldXyzabc.Z = float.Parse(Z_Value.Text);
                        oldXyzabc.A = float.Parse(A_Value.Text);
                        oldXyzabc.B = KukaRobotControlObject.Serv.PoseRecognitionSystem_Data.GraspInitial_Position[4];
                        oldXyzabc.C = float.Parse(C_Value.Text);
                        if (!KukaRobotControlObject.move_XYZABC_RotationMode(newXyzabc, oldXyzabc))
                            myListBox("Error : Xyzabc Out of limited!!", ListMsg);
                        oldXyzabc = null;
                        newXyzabc = null;*/


                        /*newXyzabc = new Coordinate_XYZABC();
                        oldXyzabc = new Coordinate_XYZABC();
                        newXyzabc.X = float.Parse(X_Value.Text);
                        newXyzabc.Y = float.Parse(Y_Value.Text);
                        newXyzabc.Z = KukaRobotControlObject.Serv.PoseRecognitionSystem_Data.CaptureImage_Position[2];
                        newXyzabc.A = float.Parse(A_Value.Text);
                        newXyzabc.B = float.Parse(B_Value.Text);
                        newXyzabc.C = float.Parse(C_Value.Text);
                        oldXyzabc.X = float.Parse(X_Value.Text);
                        oldXyzabc.Y = float.Parse(Y_Value.Text);
                        oldXyzabc.Z = float.Parse(Z_Value.Text);
                        oldXyzabc.A = float.Parse(A_Value.Text);
                        oldXyzabc.B = float.Parse(B_Value.Text);
                        oldXyzabc.C = float.Parse(C_Value.Text);
                        if (!KukaRobotControlObject.move_XYZABC_SingalMode(newXyzabc, oldXyzabc))
                            myListBox("Error : Xyzabc Out of limited!!", ListMsg);
                        oldXyzabc = null;
                        newXyzabc = null;/*


                        /*
                         *    請求Server端擷新影像
                         */
                        KukaRobotControlObject.Serv.PoseRecognitionSystem_Data._State = 5;
                        /*while (KukaRobotControlObject.Serv.PoseRecognitionSystem_Data._State == 5)
                        {
                            //Thread.Sleep(50);
                        }*/

                        /*
                         *  放置物件
                         */
                       /* newXyzabc = new Coordinate_XYZABC();
                        oldXyzabc = new Coordinate_XYZABC();
                        newXyzabc.X = float.Parse(X_Value.Text);
                        newXyzabc.Y = float.Parse(Y_Value.Text);
                        newXyzabc.Z = KukaRobotControlObject.Serv.PoseRecognitionSystem_Data.GraspInitial_Position[2];
                        newXyzabc.A = float.Parse(A_Value.Text);
                        newXyzabc.B = float.Parse(B_Value.Text);
                        newXyzabc.C = float.Parse(C_Value.Text);
                        oldXyzabc.X = float.Parse(X_Value.Text);
                        oldXyzabc.Y = float.Parse(Y_Value.Text);
                        oldXyzabc.Z = float.Parse(Z_Value.Text);
                        oldXyzabc.A = float.Parse(A_Value.Text);
                        oldXyzabc.B = float.Parse(B_Value.Text);
                        oldXyzabc.C = float.Parse(C_Value.Text);
                        if (!KukaRobotControlObject.move_XYZABC_SingalMode(newXyzabc, oldXyzabc))
                            myListBox("Error : Xyzabc Out of limited!!", ListMsg);
                        oldXyzabc = null;
                        newXyzabc = null;*/

                        /*newXyzabc = new Coordinate_XYZABC();
                        oldXyzabc = new Coordinate_XYZABC();
                        newXyzabc.X = float.Parse(X_Value.Text);
                        newXyzabc.Y = float.Parse(Y_Value.Text);
                        newXyzabc.Z = float.Parse(Z_Value.Text);
                        newXyzabc.A = float.Parse(A_Value.Text);
                        newXyzabc.B = KukaRobotControlObject.Serv.PoseRecognitionSystem_Data.GraspInitial_Position[4];
                        newXyzabc.C = float.Parse(C_Value.Text);
                        oldXyzabc.X = float.Parse(X_Value.Text);
                        oldXyzabc.Y = float.Parse(Y_Value.Text);
                        oldXyzabc.Z = float.Parse(Z_Value.Text);
                        oldXyzabc.A = float.Parse(A_Value.Text);
                        oldXyzabc.B = KukaRobotControlObject.Serv.PoseRecognitionSystem_Data.CaptureImage_Position[4];
                        oldXyzabc.C = float.Parse(C_Value.Text);
                        if (!KukaRobotControlObject.move_XYZABC_RotationMode(newXyzabc, oldXyzabc))
                            myListBox("Error : Xyzabc Out of limited!!", ListMsg);
                        oldXyzabc = null;
                        newXyzabc = null;*/


                        //newXyzabc = new Coordinate_XYZABC();
                        //oldXyzabc = new Coordinate_XYZABC();
                        //newXyzabc.X = float.Parse(X_Value.Text);
                        //newXyzabc.Y = float.Parse(Y_Value.Text);
                        //newXyzabc.Z = KukaRobotControlObject.Serv.PoseRecognitionSystem_Data.GraspInitial_Position[2];
                        //newXyzabc.A = float.Parse(A_Value.Text);
                        //newXyzabc.B = float.Parse(B_Value.Text);
                        //newXyzabc.C = float.Parse(C_Value.Text);
                        //oldXyzabc.X = float.Parse(X_Value.Text);
                        //oldXyzabc.Y = float.Parse(Y_Value.Text);
                        //oldXyzabc.Z = float.Parse(Z_Value.Text);
                        //oldXyzabc.A = float.Parse(A_Value.Text);
                        //oldXyzabc.B = float.Parse(B_Value.Text);
                        //oldXyzabc.C = float.Parse(C_Value.Text);
                        //if (!KukaRobotControlObject.move_XYZABC_SingalMode(newXyzabc, oldXyzabc))
                        //    myListBox("Error : Xyzabc Out of limited!!", ListMsg);
                        //oldXyzabc = null;
                        //newXyzabc = null;

                        if (GraspObjectType == 0)
                        {
                            newXyzabc = new Coordinate_XYZABC();
                            oldXyzabc = new Coordinate_XYZABC();
                            newXyzabc.X = 600;
                            newXyzabc.Y = -200;
                            newXyzabc.Z = 90;
                            newXyzabc.A = float.Parse(A_Value.Text);
                            newXyzabc.B = float.Parse(B_Value.Text);
                            newXyzabc.C = float.Parse(C_Value.Text);
                            oldXyzabc.X = float.Parse(X_Value.Text);
                            oldXyzabc.Y = float.Parse(Y_Value.Text);
                            oldXyzabc.Z = float.Parse(Z_Value.Text);
                            oldXyzabc.A = float.Parse(A_Value.Text);
                            oldXyzabc.B = float.Parse(B_Value.Text);
                            oldXyzabc.C = float.Parse(C_Value.Text);
                            if (!KukaRobotControlObject.move_XYZABC_TogetherMode(newXyzabc, oldXyzabc))
                                myListBox("Error : Xyzabc Out of limited!!", ListMsg);
                            oldXyzabc = null;
                            newXyzabc = null;
                        }
                        else if (GraspObjectType == 1)
                        {
                            newXyzabc = new Coordinate_XYZABC();
                            oldXyzabc = new Coordinate_XYZABC();
                            newXyzabc.X = 300;
                            newXyzabc.Y = -200;
                            newXyzabc.Z = float.Parse(Z_Value.Text);
                            newXyzabc.A = float.Parse(A_Value.Text);
                            newXyzabc.B = float.Parse(B_Value.Text);
                            newXyzabc.C = float.Parse(C_Value.Text);
                            oldXyzabc.X = float.Parse(X_Value.Text);
                            oldXyzabc.Y = float.Parse(Y_Value.Text);
                            oldXyzabc.Z = float.Parse(Z_Value.Text);
                            oldXyzabc.A = float.Parse(A_Value.Text);
                            oldXyzabc.B = float.Parse(B_Value.Text);
                            oldXyzabc.C = float.Parse(C_Value.Text);
                            if (!KukaRobotControlObject.move_XYZABC_TogetherMode(newXyzabc, oldXyzabc))
                                myListBox("Error : Xyzabc Out of limited!!", ListMsg);
                            oldXyzabc = null;
                            newXyzabc = null;

                            newXyzabc = new Coordinate_XYZABC();
                            oldXyzabc = new Coordinate_XYZABC();
                            newXyzabc.X = float.Parse(X_Value.Text);
                            newXyzabc.Y = float.Parse(Y_Value.Text);
                            newXyzabc.Z = 90;
                            newXyzabc.A = float.Parse(A_Value.Text);
                            newXyzabc.B = float.Parse(B_Value.Text);
                            newXyzabc.C = float.Parse(C_Value.Text);
                            oldXyzabc.X = float.Parse(X_Value.Text);
                            oldXyzabc.Y = float.Parse(Y_Value.Text);
                            oldXyzabc.Z = float.Parse(Z_Value.Text);
                            oldXyzabc.A = float.Parse(A_Value.Text);
                            oldXyzabc.B = float.Parse(B_Value.Text);
                            oldXyzabc.C = float.Parse(C_Value.Text);
                            if (!KukaRobotControlObject.move_XYZABC_SingalMode(newXyzabc, oldXyzabc))
                                myListBox("Error : Xyzabc Out of limited!!", ListMsg);
                            oldXyzabc = null;
                            newXyzabc = null;
                        }
                        else if (GraspObjectType == 2)
                        {
                            newXyzabc = new Coordinate_XYZABC();
                            oldXyzabc = new Coordinate_XYZABC();
                            newXyzabc.X = 450;
                            newXyzabc.Y = -200;
                            newXyzabc.Z = 90;
                            newXyzabc.A = float.Parse(A_Value.Text);
                            newXyzabc.B = float.Parse(B_Value.Text);
                            newXyzabc.C = float.Parse(C_Value.Text);
                            oldXyzabc.X = float.Parse(X_Value.Text);
                            oldXyzabc.Y = float.Parse(Y_Value.Text);
                            oldXyzabc.Z = float.Parse(Z_Value.Text);
                            oldXyzabc.A = float.Parse(A_Value.Text);
                            oldXyzabc.B = float.Parse(B_Value.Text);
                            oldXyzabc.C = float.Parse(C_Value.Text);
                            if (!KukaRobotControlObject.move_XYZABC_TogetherMode(newXyzabc, oldXyzabc))
                                myListBox("Error : Xyzabc Out of limited!!", ListMsg);
                            oldXyzabc = null;
                            newXyzabc = null;
                        }

                       
                        /*newXyzabc = new Coordinate_XYZABC();
                        oldXyzabc = new Coordinate_XYZABC();
                        newXyzabc.X = float.Parse(X_Value.Text);
                        newXyzabc.Y = float.Parse(Y_Value.Text);
                        newXyzabc.Z = 90;
                        newXyzabc.A = float.Parse(A_Value.Text);
                        newXyzabc.B = float.Parse(B_Value.Text);
                        newXyzabc.C = float.Parse(C_Value.Text);
                        oldXyzabc.X = float.Parse(X_Value.Text);
                        oldXyzabc.Y = float.Parse(Y_Value.Text);
                        oldXyzabc.Z = float.Parse(Z_Value.Text);
                        oldXyzabc.A = float.Parse(A_Value.Text);
                        oldXyzabc.B = float.Parse(B_Value.Text);
             
                         * oldXyzabc.C = float.Parse(C_Value.Text);
                        if (!KukaRobotControlObject.move_XYZABC_SingalMode(newXyzabc, oldXyzabc))
                            myListBox("Error : Xyzabc Out of limited!!", ListMsg);
                        oldXyzabc = null;
                        newXyzabc = null;*/

                        Thread.Sleep(100);
                        GraspState = -1;
                        GraspControl = new Thread(new ThreadStart(GraspControlThread));
                        GraspControl.Start();

                        //newXyzabc = new Coordinate_XYZABC();
                        //oldXyzabc = new Coordinate_XYZABC();
                        //newXyzabc.X = float.Parse(X_Value.Text);
                        //newXyzabc.Y = float.Parse(Y_Value.Text);
                        //newXyzabc.Z = KukaRobotControlObject.Serv.PoseRecognitionSystem_Data.GraspInitial_Position[2];
                        //newXyzabc.A = float.Parse(A_Value.Text);
                        //newXyzabc.B = float.Parse(B_Value.Text);
                        //newXyzabc.C = float.Parse(C_Value.Text);
                        //oldXyzabc.X = float.Parse(X_Value.Text);
                        //oldXyzabc.Y = float.Parse(Y_Value.Text);
                        //oldXyzabc.Z = float.Parse(Z_Value.Text);
                        //oldXyzabc.A = float.Parse(A_Value.Text);
                        //oldXyzabc.B = float.Parse(B_Value.Text);
                        //oldXyzabc.C = float.Parse(C_Value.Text);
                        //if (!KukaRobotControlObject.move_XYZABC_SingalMode(newXyzabc, oldXyzabc))
                        //    myListBox("Error : Xyzabc Out of limited!!", ListMsg);
                        //oldXyzabc = null;
                        //newXyzabc = null;

                        newXyzabc = new Coordinate_XYZABC();
                        oldXyzabc = new Coordinate_XYZABC();
                        newXyzabc.X = KukaRobotControlObject.Serv.PoseRecognitionSystem_Data.GraspInitial_Position[0];
                        newXyzabc.Y = KukaRobotControlObject.Serv.PoseRecognitionSystem_Data.GraspInitial_Position[1];
                        newXyzabc.Z = KukaRobotControlObject.Serv.PoseRecognitionSystem_Data.GraspInitial_Position[2];
                        newXyzabc.A = float.Parse(A_Value.Text);
                        newXyzabc.B = float.Parse(B_Value.Text);
                        newXyzabc.C = float.Parse(C_Value.Text);
                        oldXyzabc.X = float.Parse(X_Value.Text);
                        oldXyzabc.Y = float.Parse(Y_Value.Text);
                        oldXyzabc.Z = float.Parse(Z_Value.Text);
                        oldXyzabc.A = float.Parse(A_Value.Text);
                        oldXyzabc.B = float.Parse(B_Value.Text);
                        oldXyzabc.C = float.Parse(C_Value.Text);
                        if (!KukaRobotControlObject.move_XYZABC_TogetherMode(newXyzabc, oldXyzabc))
                            myListBox("Error : Xyzabc Out of limited!!", ListMsg);
                        oldXyzabc = null;
                        newXyzabc = null;

                        IsArmCanMove = true;
                        KukaRobotControlObject.Serv.PoseRecognitionSystem_Data._State = 9;
                        
                    }
                    break;
                }
                default:
                {
                    break;
                }
            }
        }

        private void MoveX_Btn_Click(object sender, EventArgs e)
        {
            XyzabcMode = "X";
            MoveType = "0";
            Thread KukaMoveXyzabcMode = new Thread(new ThreadStart(KukaMoveXyzabcModeThread));
            KukaMoveXyzabcMode.Start();
        }

        private void MoveY_Btn_Click(object sender, EventArgs e)
        {
            XyzabcMode = "Y";
            MoveType = "0";
            Thread KukaMoveXyzabcMode = new Thread(new ThreadStart(KukaMoveXyzabcModeThread));
            KukaMoveXyzabcMode.Start();
        }

        private void MoveZ_Btn_Click(object sender, EventArgs e)
        {
            XyzabcMode = "Z";
            MoveType = "0";
            Thread KukaMoveXyzabcMode = new Thread(new ThreadStart(KukaMoveXyzabcModeThread));
            KukaMoveXyzabcMode.Start();
        }

        private void MoveA_Btn_Click(object sender, EventArgs e)
        {
            XyzabcMode = "A";
            MoveType = "0";
            Thread KukaMoveXyzabcMode = new Thread(new ThreadStart(KukaMoveXyzabcModeThread));
            KukaMoveXyzabcMode.Start();
        }

        private void MoveB_Btn_Click(object sender, EventArgs e)
        {
            XyzabcMode = "B";
            MoveType = "0";
            Thread KukaMoveXyzabcMode = new Thread(new ThreadStart(KukaMoveXyzabcModeThread));
            KukaMoveXyzabcMode.Start();
        }

        private void MoveC_Btn_Click(object sender, EventArgs e)
        {
            XyzabcMode = "C";
            MoveType = "0";
            Thread KukaMoveXyzabcMode = new Thread(new ThreadStart(KukaMoveXyzabcModeThread));
            KukaMoveXyzabcMode.Start();
        }

        private void button20_Click(object sender, EventArgs e)
        {
            XyzabcMode = "Xyzabc";
            MoveType = "0";
            Thread KukaMoveXyzabcMode = new Thread(new ThreadStart(KukaMoveXyzabcModeThread));
            KukaMoveXyzabcMode.Start();
        }

        private void GraspControlThread()
        {
            switch (GraspState)
            {
                case 1:
                {
                    KukaRobotControlObject.GraspControll(GraspState);
                    break;
                }
                case -1:
                {
                    KukaRobotControlObject.GraspControll(GraspState);
                    break;
                }
                case 0:
                {
                    KukaRobotControlObject.GraspControll(GraspState);
                    break;
                }
                default:
                {
                    break;
                }
            }
        }

     

        private void button1_Click(object sender, EventArgs e)
        {
            XyzabcMode = "XyzabcHome";
            Thread KukaMoveXyzabcMode = new Thread(new ThreadStart(KukaMoveXyzabcModeThread));
            KukaMoveXyzabcMode.Start();
        }


        private void SetVelocity_Click(object sender, EventArgs e)
        {
            Thread SetVelocity = new Thread(new ThreadStart(SetVelocityThread));
            SetVelocity.Start();
        }

        private void SetVelocityThread()
        {
            KukaRobotControlObject.Vel = float.Parse(VelocityValue.Text);
            myListBox("Correct : Velocity is be set " + KukaRobotControlObject.Vel + " !", ListMsg);
        }

        private void Add_A_Click(object sender, EventArgs e)
        {
            XyzabcMode = "A";
            MoveType = "1";
            Thread KukaMoveXyzabcMode = new Thread(new ThreadStart(KukaMoveXyzabcModeThread));
            KukaMoveXyzabcMode.Start();
        }

        private void Add_B_Click(object sender, EventArgs e)
        {
            XyzabcMode = "B";
            MoveType = "1";
            Thread KukaMoveXyzabcMode = new Thread(new ThreadStart(KukaMoveXyzabcModeThread));
            KukaMoveXyzabcMode.Start();
        }

        private void Add_C_Click(object sender, EventArgs e)
        {
            XyzabcMode = "C";
            MoveType = "1";
            Thread KukaMoveXyzabcMode = new Thread(new ThreadStart(KukaMoveXyzabcModeThread));
            KukaMoveXyzabcMode.Start();
        }

        private void Discount_A_Click(object sender, EventArgs e)
        {
            XyzabcMode = "A";
            MoveType = "-1";
            Thread KukaMoveXyzabcMode = new Thread(new ThreadStart(KukaMoveXyzabcModeThread));
            KukaMoveXyzabcMode.Start();
        }

        private void Discount_B_Click(object sender, EventArgs e)
        {
            XyzabcMode = "B";
            MoveType = "-1";
            Thread KukaMoveXyzabcMode = new Thread(new ThreadStart(KukaMoveXyzabcModeThread));
            KukaMoveXyzabcMode.Start();
        }

        private void Discount_C_Click(object sender, EventArgs e)
        {
            XyzabcMode = "C";
            MoveType = "-1";
            Thread KukaMoveXyzabcMode = new Thread(new ThreadStart(KukaMoveXyzabcModeThread));
            KukaMoveXyzabcMode.Start();
        }

        private void Add_X_Click(object sender, EventArgs e)
        {
            XyzabcMode = "X";
            MoveType = "1";
            Thread KukaMoveXyzabcMode = new Thread(new ThreadStart(KukaMoveXyzabcModeThread));
            KukaMoveXyzabcMode.Start();
        }

        private void Add_Y_Click(object sender, EventArgs e)
        {
            XyzabcMode = "Y";
            MoveType = "1";
            Thread KukaMoveXyzabcMode = new Thread(new ThreadStart(KukaMoveXyzabcModeThread));
            KukaMoveXyzabcMode.Start();
        }

        private void Add_Z_Click(object sender, EventArgs e)
        {
            XyzabcMode = "Z";
            MoveType = "1";
            Thread KukaMoveXyzabcMode = new Thread(new ThreadStart(KukaMoveXyzabcModeThread));
            KukaMoveXyzabcMode.Start();
        }

        private void Decrease_X_Click(object sender, EventArgs e)
        {
            XyzabcMode = "X";
            MoveType = "-1";
            Thread KukaMoveXyzabcMode = new Thread(new ThreadStart(KukaMoveXyzabcModeThread));
            KukaMoveXyzabcMode.Start();
        }

        private void Decrease_Y_Click(object sender, EventArgs e)
        {
            XyzabcMode = "Y";
            MoveType = "-1";
            Thread KukaMoveXyzabcMode = new Thread(new ThreadStart(KukaMoveXyzabcModeThread));
            KukaMoveXyzabcMode.Start();
        }

        private void Decrease_Z_Click(object sender, EventArgs e)
        {
            XyzabcMode = "Z";
            MoveType = "-1";
            Thread KukaMoveXyzabcMode = new Thread(new ThreadStart(KukaMoveXyzabcModeThread));
            KukaMoveXyzabcMode.Start();
        }

        private void Add_A1_Click(object sender, EventArgs e)
        {
            XyzabcMode = "A1";
            MoveType = "1";
            Thread KukaMoveXyzabcMode = new Thread(new ThreadStart(KukaMoveXyzabcModeThread));
            KukaMoveXyzabcMode.Start();
        }

        private void Add_A2_Click(object sender, EventArgs e)
        {
            XyzabcMode = "A2";
            MoveType = "1";
            Thread KukaMoveXyzabcMode = new Thread(new ThreadStart(KukaMoveXyzabcModeThread));
            KukaMoveXyzabcMode.Start();
        }

        private void Add_A3_Click(object sender, EventArgs e)
        {
            XyzabcMode = "A3";
            MoveType = "1";
            Thread KukaMoveXyzabcMode = new Thread(new ThreadStart(KukaMoveXyzabcModeThread));
            KukaMoveXyzabcMode.Start();
        }

        private void Add_A4_Click(object sender, EventArgs e)
        {
            XyzabcMode = "A4";
            MoveType = "1";
            Thread KukaMoveXyzabcMode = new Thread(new ThreadStart(KukaMoveXyzabcModeThread));
            KukaMoveXyzabcMode.Start();
        }

        private void Add_A5_Click(object sender, EventArgs e)
        {
            XyzabcMode = "A5";
            MoveType = "1";
            Thread KukaMoveXyzabcMode = new Thread(new ThreadStart(KukaMoveXyzabcModeThread));
            KukaMoveXyzabcMode.Start();
        }

        private void Add_A6_Click(object sender, EventArgs e)
        {
            XyzabcMode = "A6";
            MoveType = "1";
            Thread KukaMoveXyzabcMode = new Thread(new ThreadStart(KukaMoveXyzabcModeThread));
            KukaMoveXyzabcMode.Start();
        }

        private void Decrease_A1_Click(object sender, EventArgs e)
        {
            XyzabcMode = "A1";
            MoveType = "-1";
            Thread KukaMoveXyzabcMode = new Thread(new ThreadStart(KukaMoveXyzabcModeThread));
            KukaMoveXyzabcMode.Start();
        }

        private void Decrease_A2_Click(object sender, EventArgs e)
        {
            XyzabcMode = "A2";
            MoveType = "-1";
            Thread KukaMoveXyzabcMode = new Thread(new ThreadStart(KukaMoveXyzabcModeThread));
            KukaMoveXyzabcMode.Start();
        }

        private void Decrease_A3_Click(object sender, EventArgs e)
        {
            XyzabcMode = "A3";
            MoveType = "-1";
            Thread KukaMoveXyzabcMode = new Thread(new ThreadStart(KukaMoveXyzabcModeThread));
            KukaMoveXyzabcMode.Start();
        }

        private void Decrease_A4_Click(object sender, EventArgs e)
        {
            XyzabcMode = "A4";
            MoveType = "-1";
            Thread KukaMoveXyzabcMode = new Thread(new ThreadStart(KukaMoveXyzabcModeThread));
            KukaMoveXyzabcMode.Start();
        }

        private void Decrease_A5_Click(object sender, EventArgs e)
        {
            XyzabcMode = "A5";
            MoveType = "-1";
            Thread KukaMoveXyzabcMode = new Thread(new ThreadStart(KukaMoveXyzabcModeThread));
            KukaMoveXyzabcMode.Start();
        }

        private void Decrease_A6_Click(object sender, EventArgs e)
        {
            XyzabcMode = "A6";
            MoveType = "-1";
            Thread KukaMoveXyzabcMode = new Thread(new ThreadStart(KukaMoveXyzabcModeThread));
            KukaMoveXyzabcMode.Start();
        }

        private void MoveAxis_Btn_Click(object sender, EventArgs e)
        {
            XyzabcMode = "Axis";
            MoveType = "0";
            Thread KukaMoveXyzabcMode = new Thread(new ThreadStart(KukaMoveXyzabcModeThread));
            KukaMoveXyzabcMode.Start();
        }

        private void Path_Btn_Click(object sender, EventArgs e)
        {
            XyzabcMode = "Axis";
            MoveType = "9";
            Thread KukaMoveXyzabcMode = new Thread(new ThreadStart(KukaMoveXyzabcModeThread));
            KukaMoveXyzabcMode.Start();

        }

        private void MoveA1_Btn_Click(object sender, EventArgs e)
        {
            XyzabcMode = "Axis";
            MoveType = "1";
            Thread KukaMoveXyzabcMode = new Thread(new ThreadStart(KukaMoveXyzabcModeThread));
            KukaMoveXyzabcMode.Start();
        }

        private void SaveTCPPoint_Btn_Click(object sender, EventArgs e)
        {
            Thread SaveTCPPointFun = new Thread(new ThreadStart(SaveTCPPointFunThread));
            SaveTCPPointFun.Start();
        }

        private void SaveTCPPointFunThread()
        {
            if (TxtFileObject.WFileOpen)
            {
                NowTCPPoint = X_Value.Text + "\t" + Y_Value.Text + "\t" + Z_Value.Text + "\t" + A_Value.Text + "\t" + B_Value.Text + "\t" + C_Value.Text;
                TxtFileObject.WriteTextInFile(NowTCPPoint);
                myListBox("Correct : " + NowTCPPoint + " has written in " + WriteFileName.Text + "!!", ListMsg);
                NowTCPPoint = null;
            }
            else
            {
                myListBox("Error : File " + WriteFileName.Text + " is not opened!!", ListMsg);
            }

        }

        private void OpenWriteFileThread()
        {
            if (TxtFileObject == null)
            {
                TxtFileObject = new TxtFile(WriteFileName.Text);
                TxtFileObject.OpenTxtFile();
                myListBox("Correct : File " + WriteFileName.Text + " is opened now!!", ListMsg);
            }
            else
            {
                myListBox("Error : Don't open file " + WriteFileName.Text + " again!!", ListMsg);
            }
        }

        private void CloseWriteFileThread()
        {
            if ( TxtFileObject != null)
            {
                TxtFileObject.CloseWriteFile();
                myListBox("Correct : File" + WriteFileName.Text + " is close now!!", ListMsg);
                TxtFileObject = null;
            }
            else
            {
                myListBox("Error : File " + WriteFileName.Text + " is not opened!!", ListMsg);
            }
        }
        private void OpenWriteFile_Btn_Click(object sender, EventArgs e)
        {
            Thread OpenWriteFile = new Thread(new ThreadStart(OpenWriteFileThread));
            OpenWriteFile.Start();
        }

        private void CloseWriteFile_Btn_Click(object sender, EventArgs e)
        {
            Thread CloseWriteFile = new Thread(new ThreadStart(CloseWriteFileThread));
            CloseWriteFile.Start();
        }

        private void MoveA2_Btn_Click(object sender, EventArgs e)
        {
            XyzabcMode = "Axis";
            MoveType = "2";
            Thread KukaMoveXyzabcMode = new Thread(new ThreadStart(KukaMoveXyzabcModeThread));
            KukaMoveXyzabcMode.Start();
        }

        private void MoveA3_Btn_Click(object sender, EventArgs e)
        {
            XyzabcMode = "Axis";
            MoveType = "3";
            Thread KukaMoveXyzabcMode = new Thread(new ThreadStart(KukaMoveXyzabcModeThread));
            KukaMoveXyzabcMode.Start();
        }

        private void MoveA4_Btn_Click(object sender, EventArgs e)
        {
            XyzabcMode = "Axis";
            MoveType = "4";
            Thread KukaMoveXyzabcMode = new Thread(new ThreadStart(KukaMoveXyzabcModeThread));
            KukaMoveXyzabcMode.Start();
        }

        private void MoveA5_Btn_Click(object sender, EventArgs e)
        {
            XyzabcMode = "Axis";
            MoveType = "5";
            Thread KukaMoveXyzabcMode = new Thread(new ThreadStart(KukaMoveXyzabcModeThread));
            KukaMoveXyzabcMode.Start();
        }

        private void MoveA6_Btn_Click(object sender, EventArgs e)
        {
            XyzabcMode = "Axis";
            MoveType = "6";
            Thread KukaMoveXyzabcMode = new Thread(new ThreadStart(KukaMoveXyzabcModeThread));
            KukaMoveXyzabcMode.Start();
        }

        private void GraspResetBtn_Click(object sender, EventArgs e)
        {
            GraspState = 0;
            Thread GraspControl = new Thread(new ThreadStart(GraspControlThread));
            GraspControl.Start();
        }

        private void GraspCloseBtn_Click(object sender, EventArgs e)
        {
            GraspState = 1;
            Thread GraspControl = new Thread(new ThreadStart(GraspControlThread));
            GraspControl.Start();
        }

        private void GraspOpenBtn_Click(object sender, EventArgs e)
        {
            GraspState = -1;
            Thread GraspControl = new Thread(new ThreadStart(GraspControlThread));
            GraspControl.Start();
        }

        private void CaptureImagePos_Btn_Click(object sender, EventArgs e)
        {
            XyzabcMode = "PoseEstimation";
            MoveType = "-1";
            Thread KukaMoveXyzabcMode = new Thread(new ThreadStart(KukaMoveXyzabcModeThread));
            KukaMoveXyzabcMode.Start();
        }

        private void GraspTargetIniPos_Btn_Click(object sender, EventArgs e)
        {
            XyzabcMode = "PoseEstimation";
            MoveType = "0";
            Thread KukaMoveXyzabcMode = new Thread(new ThreadStart(KukaMoveXyzabcModeThread));
            KukaMoveXyzabcMode.Start();
        }

        private void StartPoseEstimation_Thread()
        {
            if (KukaRobotControlObject.Serv.IsConnectPoseRecognitionSystem == true)
            {
                myListBox("PoseEstimation : Start Pose Estimation Thread!!!", ListMsg);
                while (KukaRobotControlObject.Serv.IsConnectPoseRecognitionSystem == true)
                {
                    if (IsArmCanMove == true)
                    {
                        if (KukaRobotControlObject.Serv.PoseRecognitionSystem_Data._State == 0)
                        {
                            IsArmCanMove = false;
                            myListBox("PoseEstimation : Kuka move to capture image position now!!!", ListMsg);
                            XyzabcMode = "PoseEstimation";
                            MoveType = "0";
                            Thread KukaMoveXyzabcMode = new Thread(new ThreadStart(KukaMoveXyzabcModeThread));
                            KukaMoveXyzabcMode.Start();
                       
                        }
                        else if (KukaRobotControlObject.Serv.PoseRecognitionSystem_Data._State == 1)
                        {
                            IsArmCanMove = false;
                            myListBox("PoseEstimation : Kuka move to grasp initial position now!!!", ListMsg);
                            XyzabcMode = "PoseEstimation";
                            MoveType = "1";
                            Thread KukaMoveXyzabcMode = new Thread(new ThreadStart(KukaMoveXyzabcModeThread));
                            KukaMoveXyzabcMode.Start();
                        }
                        else if (KukaRobotControlObject.Serv.PoseRecognitionSystem_Data._State == 2)
                        {
                            IsArmCanMove = false;
                            myListBox("PoseEstimation : Kuka move to grasp target position now!!!", ListMsg);
                            XyzabcMode = "PoseEstimation";
                            MoveType = "2";
                            Thread KukaMoveXyzabcMode = new Thread(new ThreadStart(KukaMoveXyzabcModeThread));
                            KukaMoveXyzabcMode.Start();
                        }
                        else if (KukaRobotControlObject.Serv.PoseRecognitionSystem_Data._State == -1)
                        {
                            myListBox("PoseEstimation : Client is disconnected to server !!!", ListMsg);
                            break;
                        }
                    }
                    Thread.Sleep(50);

                }
            }
            else
            {
                myListBox("PoseEstimation : Clinent is not connected to server....", ListMsg);
            }
        }

        private void FollowPoseEstimation_Btn_Click(object sender, EventArgs e)
        {
            
            Thread StartPoseEstimation = new Thread(new ThreadStart(StartPoseEstimation_Thread));
            StartPoseEstimation.Start();
        }

        private void VelocityValue_TextChanged(object sender, EventArgs e)
        {

        }

    }
}
