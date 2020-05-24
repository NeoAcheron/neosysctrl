using MQTTnet.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NeoAcheron.SystemMonitor.Web.Utils
{
    public class ConnectionInterceptor : IMqttServerSubscriptionInterceptor, IMqttServerUnsubscriptionInterceptor
    {
        public async Task InterceptSubscriptionAsync(MqttSubscriptionInterceptorContext context)
        {
            context.AcceptSubscription = true;
        }

        public async Task InterceptUnsubscriptionAsync(MqttUnsubscriptionInterceptorContext context)
        {
            context.AcceptUnsubscription = true;
        }
    }
}
