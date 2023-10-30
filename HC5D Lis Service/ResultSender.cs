using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Data;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.IO;
using System.Threading;
namespace HC5D_Lis_Service
{
    public class MessageInput
    {
        public string order_id { get; set; }

        public string machine_code = "HC5D";
        public List<TestInput> result_obs { get; set; }

    }
    public class TestInput
    {
        public string code { get; set; }
        public string result { get; set; }
    }
    class ResultSender
    {
        Database database = null;
        Thread resultSender;
        ConfigFileHundler confighundler;
        Logger logger;
        public ResultSender()
        {
            logger = new Logger();
            confighundler = new ConfigFileHundler();
            database = new Database();
            Console.WriteLine("database initialized");
            logger.WriteLog(logger.logMessageWithFormat("Database initialized."));
        }

        public void startResultSender()
        {
            resultSender = new Thread(new ThreadStart(startSending));
            resultSender.Start();
        }
        public void stopResultSender()
        {
            resultSender.Join();
            resultSender.Abort();
        }
        public void startSending()
        {
            logger.WriteLog(logger.logMessageWithFormat("result sender started...","info"));
            logger.WriteLog(logger.logMessageWithFormat("Result Sender Started."));
            while (true)
            {

                MessageInput input = GetMessage();

                if (input != null)
                {
                    logger.WriteLog(logger.logMessageWithFormat("New result from machine arrived!","info"));
                    SendMessage(input).GetAwaiter(); 
                }
                Thread.Sleep(10);
            }
        }


        private async Task SendMessage(MessageInput input)
        {
            try
            {
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(input);
                var data = new System.Net.Http.StringContent(json, Encoding.UTF8, "application/json");

                Console.WriteLine("our data : {0}", json);

                //var url = "http://localhost:9000";
                logger.WriteLog(logger.logMessageWithFormat("url = " + confighundler.GetCmsApi(), "info"));
                var url = confighundler.GetCmsApi();
                var client = new HttpClient();
                logger.WriteLog(logger.logMessageWithFormat("sending data to " + url ));
                logger.WriteLog(logger.logMessageWithFormat("json body sent to the system : "+ json));
                var response = await client.PostAsync(url, data);

                string result = response.Content.ReadAsStringAsync().Result;

                Console.WriteLine("Response from the EMR  ->" + result);
                logger.WriteLog(logger.logMessageWithFormat("Response from the EMR : " + result));

            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                logger.WriteLog(logger.logMessageWithFormat(ex.Message,"error"));
            }
              
        }


        public MessageInput getResultObject(Hl7Message msg)
        {
            try
            {

                MessageInput input = new MessageInput();

                input.order_id = msg.segments[3].components[3].value.ToString().Trim().Split('^')[0];

                List<TestInput> tests = new List<TestInput>();
                int i = 0;
                foreach (Segment seg in msg.segments)
                {
                    string code = null;
                    if (i > 9 && i < 39)
                    {
                        code = msg.segments[i].components[3].value.Split('^')[1];

                        tests.Add(new TestInput() { code = code, result = msg.segments[i].components[5].value });

                    }


                    i++;
                }
                input.result_obs = tests;
                return input;

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
        public MessageInput GetMessage()
        {
            try
            {
                DataTable tbl = database.SelectAllResults();
                if (tbl.Rows.Count > 0)
                {
                    DataRow drow = tbl.Rows[0];
                    string savedMessage = drow["message"].ToString().Trim();

                    string[] splitedMessage = Regex.Split(savedMessage, "##");

                    Hl7Message msg = new Hl7Message(splitedMessage);

                    database.DeleteResult(int.Parse(drow["id"].ToString().Trim()));

                    return getResultObject(msg);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                logger.WriteLog(logger.logMessageWithFormat(ex.Message, "error"));
                return null;
            }
        }
    }
}
