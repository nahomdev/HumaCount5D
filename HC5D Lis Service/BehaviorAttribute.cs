 
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading.Tasks;
using ASTMMessage;

namespace Maglumi
{
    public class BehaviorAttribute : Attribute, IEndpointBehavior,
                                IOperationBehavior
    {
        public void Validate(ServiceEndpoint endpoint) { }

        public void AddBindingParameters(ServiceEndpoint endpoint,
                                 BindingParameterCollection bindingParameters)
        { }


        public void ApplyDispatchBehavior(ServiceEndpoint endpoint,
                                          EndpointDispatcher endpointDispatcher)
        {
            endpointDispatcher.DispatchRuntime.MessageInspectors.Add(
                                                   new MessageInspector(endpoint));
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint,
                                        ClientRuntime clientRuntime)
        { }

        public void Validate(OperationDescription operationDescription) { }

        public void ApplyDispatchBehavior(OperationDescription operationDescription,
                                          DispatchOperation dispatchOperation)
        { }

        public void ApplyClientBehavior(OperationDescription operationDescription,
                                        ClientOperation clientOperation)
        { }

        public void AddBindingParameters(OperationDescription operationDescription,
                                  BindingParameterCollection bindingParameters)
        { }

    }
}