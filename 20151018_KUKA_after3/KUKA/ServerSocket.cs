using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Xml;

namespace KUKA
{
    class ServerCommunication
    {
        /*
         *      資料成員
         */
        private Mutex strRecieve_Mutex = new Mutex();
        private Mutex sendXml_Mutex = new Mutex();
        private Boolean isConnectKuka;
        private Socket serverSocket = null;
        private Socket[] clientSocket;
        string strRecieve = null;
        string strSend = null;
        private int dataLength = 1024;
        private int clientNumber = 20;
        private static int clientCount = 0;
        private string clinet_KukaIP;
        public String[] ClientIP = new String[20];
        private string[] xyzabcData = new string[6];
        private string[] axisData = new string[6];
        XmlDocument sendXml = new XmlDocument();
        XmlDocument recieveXml = new XmlDocument();


        public PoseRecognitionSystem PoseRecognitionSystem_Data = new PoseRecognitionSystem();
        private string clientPoseRecognitionSystemIP;
        private Boolean isConnectPoseRecognitionSystem;
        private string receivePoseRecognitionSystemStr = null;
        private int poseRecognitionSystemState;


        public string Clinet_KukaIP
        {
            get { return clinet_KukaIP; }
        }
        public Mutex StrRecieve_Mutex
        {
            set { strRecieve_Mutex = value; }
            get { return strRecieve_Mutex; }
        }

        public Mutex SendXml_Mutex
        {
            set { sendXml_Mutex = value; }
            get { return sendXml_Mutex; }
        }

        public Socket ServerSocket
        {
            set { serverSocket = value; }
            get { return serverSocket; }
        }
        public Socket[] ClientSocket
        {
            get { return clientSocket; }
        }
        public Boolean IsConnectKuka
        {
            get { return isConnectKuka; }
        }
        public int ClientNumber
        {
            get { return clientNumber; }
        }
        public int DataLength
        {
            get { return dataLength; }
        }
        public XmlDocument SendXml
        {
            set { sendXml = value; }
            get { return sendXml; }
        }
        public XmlDocument RecieveXml
        {
            set { recieveXml = value; }
            get { return recieveXml; }
        }
        public string[] XyzabcData
        {
            get { return xyzabcData; }
        }
        public string[] AxisData
        {
            get { return axisData; }
        }

        public ServerCommunication(int clinetNumber)
        {
            isConnectKuka = false;
            this.clientNumber = clinetNumber;
            clientSocket = new Socket[ClientNumber];
        }


        public string ClientPoseRecognitionSystemIP
        {
            get { return clientPoseRecognitionSystemIP; }
        }
        public Boolean IsConnectPoseRecognitionSystem
        {
            get { return isConnectPoseRecognitionSystem; }
        }
        public string ReceivePoseRecognitionSystemStr
        {
            get { return receivePoseRecognitionSystemStr; }
            set { receivePoseRecognitionSystemStr = value; }
        }
        public int PoseRecognitionSystemState
        {
            get { return poseRecognitionSystemState; }
            set { poseRecognitionSystemState = value; }
        }
        /*
         *      成員函數
         */

        public void StartSocketThread()
        {
            Thread StartSocket= new Thread(new ThreadStart(startListen));
            StartSocket.Start();
        }

        public void startListen()
        {

            // 1.Establish the Local_Endpoint for the  server socket.
            int Port = 6008;
            IPAddress ipAddress = IPAddress.Parse("100.100.100.1");
            IPEndPoint ipEndPoint = new System.Net.IPEndPoint(ipAddress, (int)Port);

            // 2.Create a TCP/IP server socket object.
            ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // 3.Open Socket and listen on network
            ServerSocket.Bind(ipEndPoint);
            ServerSocket.Listen(20);//最大Client連接數

            // 4.Waiting for Accept an incoming connection.
            clientSocket = new Socket[2];
            SckCWaitAccept();
        }

        private void SckCWaitAccept()
        {
            // SckSs[i] 若不為 null 表示已被實作過, 判斷是否有 Client 端連線
            if (ServerSocket != null)
            {
                Thread SckSAcceptTd = new Thread(SckSAcceptProc);
                SckSAcceptTd.Start();  // 開始執行 SckSAcceptTd 這個執行緒
            }
            else
            {
            }

        }

        private void SckSAcceptProc()
        {

            // 這裡加入 try 是因為 ServerSocket 若被 Close 的話, ServerSocket.Accept() 會產生錯誤
            try
            {
                // Data buffer for incoming data
                int IntAcceptData;
                byte[] clientData = new byte[DataLength];

                // 等待Client 端連線
                ClientSocket[clientCount] = ServerSocket.Accept();
                int Scki = clientCount; //儲存目前的ClinetIndex
                clientCount++;


                // 其中RDataLen為每次要接受來自 Client 傳來的資料長度
                ClientIP[Scki] = ClientSocket[Scki].RemoteEndPoint.ToString();//取得遠端IP
                int Ftdummy = ClientIP[Scki].IndexOf("1");
                int Sdummy = ClientIP[Scki].IndexOf(":");
                ClientIP[Scki] = ClientIP[Scki].Substring(Ftdummy, Sdummy - Ftdummy);


                if (ClientIP[Scki] == "100.100.100.2")
                {
                    clinet_KukaIP = ClientIP[Scki];
                    isConnectKuka = true;//isConnectKukaArm : 代表成功與KukaArm連接
                    Thread KukaData = new Thread(new ThreadStart(kukaDataThread));
                    KukaData.Start();
                    // 再產生另一個執行緒等待下一個 Client 連線
                    SckCWaitAccept();
                    while (IsConnectKuka)
                    {
                        
                        //Waiting for data and receive bytes
                        IntAcceptData = ClientSocket[Scki].Receive(clientData);
                        if (IntAcceptData == 0)
                        {
                            break; // Client closed Socket
                        }

                      //  StrRecieve_Mutex.WaitOne();

                        strRecieve = String.Concat(strRecieve, System.Text.Encoding.ASCII.GetString(clientData, 0, IntAcceptData));
                        //convert bytes to string
                        strRecieve = System.Text.Encoding.ASCII.GetString(clientData, 0, IntAcceptData);

                        //take a look to the end of data
                      
                        if ((strRecieve.LastIndexOf("</Rob>")) == -1)
                        {
                          //  StrRecieve_Mutex.ReleaseMutex();
                            continue;
                        }

                        else
                        {
                            SendXml_Mutex.WaitOne();

                            ////mirror the IPO counter you received yet
                            strSend = SendXml.InnerXml;
                            strSend = mirrorIPC(strRecieve, strSend);
                            ////send data as requested
                            byte[] msg = System.Text.Encoding.ASCII.GetBytes(strSend);
                            ClientSocket[Scki].Send(msg, 0, msg.Length, System.Net.Sockets.SocketFlags.None);

                            SendXml_Mutex.ReleaseMutex();
                           // StrRecieve_Mutex.ReleaseMutex();
                        }
                    }
                }
                else if (ClientIP[Scki] == "100.100.100.1")
                {
                    clientPoseRecognitionSystemIP = ClientIP[Scki];
                    isConnectPoseRecognitionSystem = true;
                    PoseRecognitionSystem_Data._State = 9; //初始為9，不動作
                    // 再產生另一個執行緒等待下一個 Client 連線
                    SckCWaitAccept();

                    while ( isConnectPoseRecognitionSystem )
                    {
                        ClientSocket[Scki].Receive(clientData);
                        ReceivePoseRecognitionSystemStr = System.Text.Encoding.Default.GetString(clientData);
                        PoseRecognitionSystemDataProcess();



                        if (  PoseRecognitionSystem_Data._State == 0 )
                        {
                            /*
                             *  移動至擷取影像點
                             */

                            //PoseRecognitionSystem_Data._State = 9 -> 完成跳開;

                            while (PoseRecognitionSystem_Data._State == 0)
                            {
                                Thread.Sleep(50);
                            }

                            byte[] msg = System.Text.Encoding.ASCII.GetBytes("I");
                            ClientSocket[Scki].Send(msg, 0, msg.Length, System.Net.Sockets.SocketFlags.None);

                            ClientSocket[Scki].Receive(clientData);
                            string ClinetStr;
                            ReceivePoseRecognitionSystemStr = System.Text.Encoding.Default.GetString(clientData);
                            int Start_dummy = ReceivePoseRecognitionSystemStr.IndexOf("S");
                            int End_dummy = ReceivePoseRecognitionSystemStr.IndexOf("E");
                            ClinetStr = ReceivePoseRecognitionSystemStr.Substring(Start_dummy + 1, End_dummy - 1 - Start_dummy);

                            if (ClinetStr == "TCP_Get")
                            {
                                Thread.Sleep(500);
                                String msg_TCP = String.Format("X{0:0.0000}Y{1:0.0000}Z{2:0.0000}A{3:0.0000}B{4:0.0000}C{5:0.0000}E", xyzabcData[0], xyzabcData[1], xyzabcData[2], xyzabcData[3], xyzabcData[4], xyzabcData[5]);
                                msg = System.Text.Encoding.ASCII.GetBytes(msg_TCP);
                                ClientSocket[Scki].Send(msg, 0, msg.Length, System.Net.Sockets.SocketFlags.None);

                                ClientSocket[Scki].Receive(clientData);
                                ReceivePoseRecognitionSystemStr = System.Text.Encoding.Default.GetString(clientData);
                                Start_dummy = ReceivePoseRecognitionSystemStr.IndexOf("S");
                                End_dummy = ReceivePoseRecognitionSystemStr.IndexOf("E");
                                ClinetStr = ReceivePoseRecognitionSystemStr.Substring(Start_dummy + 1, End_dummy - 1 - Start_dummy);

                                if (ClinetStr == "TCP_Done")
                                {
                                    msg = System.Text.Encoding.ASCII.GetBytes("C");
                                    ClientSocket[Scki].Send(msg, 0, msg.Length, System.Net.Sockets.SocketFlags.None);
                                }
                                else
                                {
                                    break;
                                }
                            }
                            else
                            {
                                break;
                            }
                           
                        }

                        else if (  PoseRecognitionSystem_Data._State == 1 )
                        {
                            /*
                            *  移動至抓取初始點
                            */

                            //PoseRecognitionSystem_Data._State = 9 -> 完成跳開;

                            while (PoseRecognitionSystem_Data._State == 1)
                            {
                                Thread.Sleep(50);
                            }

                            byte[] msg = System.Text.Encoding.ASCII.GetBytes("W");
                            ClientSocket[Scki].Send(msg, 0, msg.Length, System.Net.Sockets.SocketFlags.None);
                        }

                        else if (PoseRecognitionSystem_Data._State == 2)
                        {
                            /*
                            *  移動至抓取物件點
                            */

                            //PoseRecognitionSystem_Data._State = 5 -> 完成跳開;

                            while (PoseRecognitionSystem_Data._State == 2)
                            {
                                Thread.Sleep(50);
                            }

                            byte[] msg = System.Text.Encoding.ASCII.GetBytes("CA");
                            ClientSocket[Scki].Send(msg, 0, msg.Length, System.Net.Sockets.SocketFlags.None);
                        }

                        else if (PoseRecognitionSystem_Data._State == 5)
                        {

                            byte[] msg = System.Text.Encoding.ASCII.GetBytes("I");
                            ClientSocket[Scki].Send(msg, 0, msg.Length, System.Net.Sockets.SocketFlags.None);

                            ClientSocket[Scki].Receive(clientData);
                            string ClinetStr;
                            ReceivePoseRecognitionSystemStr = System.Text.Encoding.Default.GetString(clientData);
                            int Start_dummy = ReceivePoseRecognitionSystemStr.IndexOf("S");
                            int End_dummy = ReceivePoseRecognitionSystemStr.IndexOf("E");
                            ClinetStr = ReceivePoseRecognitionSystemStr.Substring(Start_dummy + 1, End_dummy - 1 - Start_dummy);

                            if (ClinetStr == "TCP_Get")
                            {
                                Thread.Sleep(500);
                                String msg_TCP = String.Format("X{0:0.0000}Y{1:0.0000}Z{2:0.0000}A{3:0.0000}B{4:0.0000}C{5:0.0000}E", xyzabcData[0], xyzabcData[1], xyzabcData[2], xyzabcData[3], xyzabcData[4], xyzabcData[5]);
                                msg = System.Text.Encoding.ASCII.GetBytes(msg_TCP);
                                ClientSocket[Scki].Send(msg, 0, msg.Length, System.Net.Sockets.SocketFlags.None);

                                ClientSocket[Scki].Receive(clientData);
                                ReceivePoseRecognitionSystemStr = System.Text.Encoding.Default.GetString(clientData);
                                Start_dummy = ReceivePoseRecognitionSystemStr.IndexOf("S");
                                End_dummy = ReceivePoseRecognitionSystemStr.IndexOf("E");
                                ClinetStr = ReceivePoseRecognitionSystemStr.Substring(Start_dummy + 1, End_dummy - 1 - Start_dummy);

                                if (ClinetStr == "TCP_Done")
                                {
                                    msg = System.Text.Encoding.ASCII.GetBytes("C");
                                    ClientSocket[Scki].Send(msg, 0, msg.Length, System.Net.Sockets.SocketFlags.None);
                                }
                                else
                                {
                                    break;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                        else if (PoseRecognitionSystem_Data._State == 7)
                        {
                            byte[] msg = System.Text.Encoding.ASCII.GetBytes("G");
                            ClientSocket[Scki].Send(msg, 0, msg.Length, System.Net.Sockets.SocketFlags.None);
                            while (PoseRecognitionSystem_Data._State == 7)
                            {
                                Thread.Sleep(50);
                            }
                        }
                        else if (PoseRecognitionSystem_Data._State == 6)
                        {                        
                            byte[] msg = System.Text.Encoding.ASCII.GetBytes("W");
                            ClientSocket[Scki].Send(msg, 0, msg.Length, System.Net.Sockets.SocketFlags.None);

                        }    
                        else if (PoseRecognitionSystem_Data._State == -1)
                        {
                            Thread.Sleep(50);
                            break;
                        }

                    }
                }
            }
            catch
            {



            }

        }

        private static string mirrorIPC(string receive, string send)
        {
            // separate IPO counter as string
            int startdummy = receive.IndexOf("<IPOC>") + 6;
            int stopdummy = receive.IndexOf("</IPOC>");
            string Ipocount;

            Ipocount = receive.Substring(startdummy, stopdummy - startdummy);
            startdummy = send.IndexOf("<IPOC>") + 6;
            stopdummy = send.IndexOf("</IPOC>");

            // remove the old value an insert the actualy value
            send = send.Remove(startdummy, stopdummy - startdummy);
            send = send.Insert(startdummy, Ipocount);
            return send;

        }

        public void kukaDataThread()
        {
            XmlNode Node;
            XmlElement Element;
            XmlAttribute Attribute;
            while (IsConnectKuka)
            {
                Thread.Sleep(12);
                try
                {
                   // StrRecieve_Mutex.WaitOne();
                    RecieveXml.LoadXml(strRecieve);
                }
                catch (Exception e)
                {
                   // StrRecieve_Mutex.ReleaseMutex();
                    continue;
                }

                Node = RecieveXml.SelectSingleNode("//AIPos");
                Element = (XmlElement)Node;
                Attribute = Element.GetAttributeNode("A1");
                axisData[0] = float.Parse(Attribute.Value).ToString();
                Attribute = Element.GetAttributeNode("A2");
                axisData[1] = float.Parse(Attribute.Value).ToString();
                Attribute = Element.GetAttributeNode("A3");
                axisData[2] = float.Parse(Attribute.Value).ToString();
                Attribute = Element.GetAttributeNode("A4");
                axisData[3] = float.Parse(Attribute.Value).ToString();
                Attribute = Element.GetAttributeNode("A5");
                axisData[4] = float.Parse(Attribute.Value).ToString();
                Attribute = Element.GetAttributeNode("A6");
                axisData[5] = float.Parse(Attribute.Value).ToString();


                Node = RecieveXml.SelectSingleNode("//RIst");
                Element = (XmlElement)Node;
                Attribute = Element.GetAttributeNode("X");
                xyzabcData[0] = float.Parse(Attribute.Value).ToString();
                Attribute = Element.GetAttributeNode("Y");
                xyzabcData[1] = float.Parse(Attribute.Value).ToString();
                Attribute = Element.GetAttributeNode("Z");
                xyzabcData[2] = float.Parse(Attribute.Value).ToString();
                Attribute = Element.GetAttributeNode("A");
                xyzabcData[3] = float.Parse(Attribute.Value).ToString();
                Attribute = Element.GetAttributeNode("B");
                xyzabcData[4] = float.Parse(Attribute.Value).ToString();
                Attribute = Element.GetAttributeNode("C");
                xyzabcData[5] = float.Parse(Attribute.Value).ToString();

              // StrRecieve_Mutex.ReleaseMutex();
            }
        }

        public void PoseRecognitionSystemDataProcess()
        {
            string count;

            int Xdummy = ReceivePoseRecognitionSystemStr.IndexOf("X");
            int Ydummy = ReceivePoseRecognitionSystemStr.IndexOf("Y");
            int Zdummy = ReceivePoseRecognitionSystemStr.IndexOf("Z");
            int Adummy = ReceivePoseRecognitionSystemStr.IndexOf("A");
            int Bdummy = ReceivePoseRecognitionSystemStr.IndexOf("B");
            int Cdummy = ReceivePoseRecognitionSystemStr.IndexOf("C");
            int Sdummy = ReceivePoseRecognitionSystemStr.IndexOf("S");
            int Odummy = ReceivePoseRecognitionSystemStr.IndexOf("O");
            int Edummy = ReceivePoseRecognitionSystemStr.IndexOf("E");

            count = ReceivePoseRecognitionSystemStr.Substring(Xdummy + 1, Ydummy - 1 - Xdummy);
            PoseRecognitionSystem_Data._X = float.Parse(count);
            count = ReceivePoseRecognitionSystemStr.Substring(Ydummy + 1, Zdummy - 1 - Ydummy);
            PoseRecognitionSystem_Data._Y = float.Parse(count);
            count = ReceivePoseRecognitionSystemStr.Substring(Zdummy + 1, Adummy - 1 - Zdummy);
            PoseRecognitionSystem_Data._Z = float.Parse(count);
            count = ReceivePoseRecognitionSystemStr.Substring(Adummy + 1, Bdummy - 1 - Adummy);
            PoseRecognitionSystem_Data._A = float.Parse(count);
            count = ReceivePoseRecognitionSystemStr.Substring(Bdummy + 1, Cdummy - 1 - Bdummy);
            PoseRecognitionSystem_Data._B = float.Parse(count);
            count = ReceivePoseRecognitionSystemStr.Substring(Cdummy + 1, Sdummy - 1 - Cdummy);
            PoseRecognitionSystem_Data._C = float.Parse(count);
            count = ReceivePoseRecognitionSystemStr.Substring(Sdummy + 1, Odummy - 1 - Sdummy);
            PoseRecognitionSystem_Data._State = Int32.Parse(count);
            count = ReceivePoseRecognitionSystemStr.Substring(Odummy + 1, Edummy - 1 - Odummy);
            PoseRecognitionSystem_Data._ObjectType = Int32.Parse(count);
            
        }
    }
}
