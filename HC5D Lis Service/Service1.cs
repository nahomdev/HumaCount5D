using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.IO;

namespace HC5D_Lis_Service
{
    public partial class Service1 : ServiceBase
    {
        //private static readonly byte[] ArshoIP = { 10, 10, 1, 152 };
        //private static readonly byte[] Localhost = { 127, 0, 0, 1 };
        //private const int Port = 9999;

        Subscriber subscriber;
        ResultSender sender;

        bool doFlag;

        Thread resultSender;
        Thread listnerThread;

        public Service1()
        {
            InitializeComponent();
            //System.Net.IPAddress address = new IPAddress(Localhost);
            //System.Net.IPEndPoint endPoint = new IPEndPoint(address, Port);

            //subscriber = new Subscriber(endPoint);

           

        }

        protected override void OnStart(string[] args)
        {

            


                subscriber = new Subscriber();
                subscriber.startServer();

                sender = new ResultSender();
                sender.startResultSender();

                //    resultSender = new Thread(new ThreadStart(sender.startSending));
                //    listnerThread = new Thread(new ThreadStart(subscriber.Listen));


                //    resultSender.Start();
                //    listnerThread.Start();
          
           

        }

        protected override void OnStop()
        {
            subscriber.stopServer();
            sender.stopResultSender();
        }
    }
}
