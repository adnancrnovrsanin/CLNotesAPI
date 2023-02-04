using CommandGQL.Data;
using CommandGQL.Models;

namespace CommandGQL.GraphQL.Platforms
{
    public class PlatformType : ObjectType<Platform>
    {
        protected override void Configure(IObjectTypeDescriptor<Platform> descriptor)
        {
            descriptor.Description("Represents any software or service that has a command line interface.");

            descriptor
                .Field(p => p.LicenseKey).Ignore();

            descriptor
                .Field(p => p.Commands)
                .ResolveWith<Resolvers>(p => p.GetCommands(default!, default!))
                .UseDbContext<AppDbContext>()
                .Description("This is the list of available commands for this platform.");
        }

        private class Resolvers 
        {
            public IQueryable<Command> GetCommands([Parent] Platform platform, [ScopedService] AppDbContext context)
            {
                return context.Commands.Where(p => p.Platform.Id == platform.Id);
            }
        }
    }
}