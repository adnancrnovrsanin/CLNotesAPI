using CommandGQL.Models;

namespace CommandGQL.GraphQL
{
    public class Subscription
    {
        [Subscribe]
        [Topic]
        public Platform OnPlatformAdded([EventMessage] Platform platform)
        {
            return new Platform();
        }
    }
}