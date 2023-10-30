using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;

using System.Runtime.CompilerServices;
using System.IO;
using Subscriber.Sockets;
using System.Net.NetworkInformation;

namespace Subscriber.Sockets
{
    public static class SocketExtensions
    {
        public static bool IsConnected(this Socket socket)
        {
            try
            {
                return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
            }
            catch (SocketException)
            {
                return false;
            }
        }
    }
}

namespace HC5D_Lis_Service
{
    public class Logger
    {
        ConfigFileHundler confighundler;

        public Logger()
        {
            confighundler = new ConfigFileHundler();
        }
        public string logMessageWithFormat(string message, string status = "info",
          [CallerLineNumber] int lineNumber = 0,
          [CallerMemberName] string caller = null)
        {
            return DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.ff") + "       " + status.ToUpper() + " : " + message + " at line " + lineNumber + " (" + caller + ")";
        }
        public void WriteLog(string strLog)
        {
            try
            {

                string logFilePath = confighundler.getPathToLogs() + @"\Log-" + System.DateTime.Today.ToString("MM-dd-yyyy") + "." + "txt";
                FileInfo logFileInfo = new FileInfo(logFilePath);
                DirectoryInfo logDirInfo = new DirectoryInfo(logFileInfo.DirectoryName);
                if (!logDirInfo.Exists) logDirInfo.Create();
                using (FileStream fileStream = new FileStream(logFilePath, FileMode.Append))
                {
                    using (StreamWriter log = new StreamWriter(fileStream))
                    {
                        log.WriteLine(strLog);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
    class Subscriber
    {

        static string soh = char.ConvertFromUtf32(1);
        static string stx = char.ConvertFromUtf32(2);
        static string etx = char.ConvertFromUtf32(3);
        static string eot = char.ConvertFromUtf32(4);
        static string enq = char.ConvertFromUtf32(5);
        static string ack = char.ConvertFromUtf32(6);
        static string nack = char.ConvertFromUtf32(21);
        static string etb = char.ConvertFromUtf32(23);
        static string lf = char.ConvertFromUtf32(10);
        static string cr = char.ConvertFromUtf32(13);
        private IPEndPoint endpoint;
        private Socket listener;
        private static readonly byte[] ArshoIP = { 10, 10, 2, 28 };
        private static readonly byte[] Localhost = { 127, 0, 0, 1 };
        private const int Port = 9999;
        Thread resultSender;
        Thread listnerThread;
        Logger logger;

        public Subscriber()
        {
            logger = new Logger();
            ConfigFileHundler confighundler = new ConfigFileHundler();

            
            System.Net.IPEndPoint endPoint = confighundler.GetEndPoint();
           
            listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(endPoint);
            Console.WriteLine("Listening on port " + endPoint);
            logger.WriteLog(logger.logMessageWithFormat("Listening on port " + endPoint));
            logger.WriteLog(logger.logMessageWithFormat("update has been applied! v2.3"));
            listener.ReceiveTimeout = 10000;
            listener.Listen(1);
        }
        public void startServer()
        {
            listnerThread = new Thread(new ThreadStart(Listen));
            listnerThread.Start();
        }

        public void stopServer()
        {
            listnerThread?.Join();
            listnerThread.Abort();
        }
        
        private string[] SplitMessage(string dataToSend)
        {
            return Regex.Split(dataToSend, "##");
        }



        private static char END_OF_BLOCK = '\u001c';
        private static char START_OF_BLOCK = '\u000b';
        private static char CARRIAGE_RETURN = (char)13;


 
        
        public void Listen()
        {
            try {
               
            logger.WriteLog(logger.logMessageWithFormat("Subscriber Started."));
            int byteRec = 0;



            byte[] bytes = new byte[1];

            string responsedata = null;

            List<string> msg = new List<string>();
            string res = null;

            string full_message = null;
            bool flag = false;
                bool orc = false;
                bool fs = false;
            Socket receiver = listener.Accept();
             receiver.ReceiveTimeout = 120000;
                logger.WriteLog(logger.logMessageWithFormat("Machine connected successfully"));
                
            while (true)
            { 
                try
                {

                        if (!receiver.IsConnected())
                        {
                            logger.WriteLog(logger.logMessageWithFormat("connection dropout", "warning"));
                            Listen();
                        }


                        bytes = new byte[1];

                        try
                        {
                            byteRec = receiver.Receive(bytes);
                            //responsedata = Encoding.UTF8.GetString(bytes, 0, byteRec);

                            responsedata = Encoding.UTF8.GetString(bytes);

                        }
                        catch (SocketException ex)
                        {
                            if(ex.SocketErrorCode == SocketError.TimedOut)
                            {
                                logger.WriteLog(logger.logMessageWithFormat("Receive operation timed out. socket reset is required"));
                                receiver.Close();
                                Thread.Sleep(5000);
                                Listen();
                            }
                            else
                            {
                                logger.WriteLog(logger.logMessageWithFormat("socket Exception "+ ex.Message));
                                receiver.Close();
                                Thread.Sleep(5000);
                                Listen();
                            }
                            
                        }
                       
                    if (responsedata == char.ConvertFromUtf32(13))
                    {

                        msg.Add(res);
                         res = null;
                          if (orc && fs)
                            {
                                String sampleId = msg[1].Split('|')[3];
                                String controlId = msg[0].Split('|')[9]; 
                            }
 
                        if (flag == true)
                        { 
                            msg.Clear();
                            flag = false;
                        }


                        continue;
                    }


                    if (responsedata == char.ConvertFromUtf32(28))
                    {
                            full_message = String.Join("##", msg);
                            
                            String sampleId = msg[1].Split('|')[3];
                             if ( msg[1].StartsWith("ORC"))
                            {
                                orc = true;
                                fs = true;
   }
                            else
                            {
                                Database database = new Database();
                                  database.InsertResult(full_message);
                                
                            }

                            flag = true;

                    }

                       res += responsedata;
 

                }
                catch (Exception ex)
                {
                    logger.WriteLog(logger.logMessageWithFormat("Exception occured "+ex.ToString(),"error"));
                    Console.WriteLine(ex.Message);
                    Listen();
                }
                Thread.Sleep(10);
            }
            } catch(Exception ex)
            {
                logger.WriteLog("Exception occured " + ex.ToString());
                Listen();
                Thread.Sleep(10);
            }
        }
    }
}
