using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Description;

namespace ASTMMessage
{

    class StateMessage
    {
        public Message Message;
    }
    internal class MessageInspector : IDispatchMessageInspector
    {
        private ServiceEndpoint _serviceEndpoint;

        public MessageInspector(ServiceEndpoint serviceEndpoint)
        {
            _serviceEndpoint = serviceEndpoint;
        }

        public object AfterReceiveRequest(ref Message request,
                                        IClientChannel channel,
                                        InstanceContext instanceContext)
        {
            StateMessage stateMsg = null;
            HttpRequestMessageProperty requestProperty = null;
            if (request.Properties.ContainsKey(HttpRequestMessageProperty.Name))
            {
                requestProperty = request.Properties[HttpRequestMessageProperty.Name]
                                  as HttpRequestMessageProperty;
            }

            if (requestProperty != null)
            {
                var origin = requestProperty.Headers["Origin"];
                if (!string.IsNullOrEmpty(origin))
                {
                    stateMsg = new StateMessage();

                    if (requestProperty.Method == "OPTIONS")
                    {
                        stateMsg.Message = Message.CreateMessage(request.Version, null);
                    }
                    request.Properties.Add("CrossOriginResourceSharingState", stateMsg);
                }
            }

            return stateMsg;
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            var stateMsg = correlationState as StateMessage;

            if (stateMsg != null)
            {
                if (stateMsg.Message != null)
                {
                    reply = stateMsg.Message;
                }

                HttpResponseMessageProperty responseProperty = null;

                if (reply.Properties.ContainsKey(HttpResponseMessageProperty.Name))
                {
                    responseProperty = reply.Properties[HttpResponseMessageProperty.Name]
                                       as HttpResponseMessageProperty;
                }

                if (responseProperty == null)
                {
                    responseProperty = new HttpResponseMessageProperty();
                    reply.Properties.Add(HttpResponseMessageProperty.Name,
                                         responseProperty);
                }

                // Access-Control-Allow-Origin should be added for all cors responses
                responseProperty.Headers.Set("Access-Control-Allow-Origin", "*");

                if (stateMsg.Message != null)
                {
                    // the following headers should only be added for OPTIONS requests
                    responseProperty.Headers.Set("Access-Control-Allow-Methods",
                                                 "POST, OPTIONS, GET");
                    responseProperty.Headers.Set("Access-Control-Allow-Headers",
                              "Content-Type, Accept, Authorization, x-requested-with");
                }
            }
        }

    }
}