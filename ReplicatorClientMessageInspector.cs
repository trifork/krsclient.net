using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace krsclient.net
{
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;

    class ReplicatorClientMessageInspector : IClientMessageInspector
    {
        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            // TODO: log the request.

            // If you return something here, it will be available in the 
            // correlationState parameter when AfterReceiveReply is called.
            return null;
        }

        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
            // TODO: log the reply.

            // If you returned something in BeforeSendRequest
            // it will be available in the correlationState parameter.
        }
    }
}
