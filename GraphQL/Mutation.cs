using CommandGQL.Data;
using CommandGQL.GraphQL.Commands;
using CommandGQL.GraphQL.Platforms;
using CommandGQL.Models;
using HotChocolate.Subscriptions;
using Microsoft.EntityFrameworkCore;

namespace CommandGQL.GraphQL
{
    public class Mutation
    {
        [UseDbContext(typeof(AppDbContext))]
        public async Task<AddPlatformPayload> AddPlatformAsync(AddPlatformInput input, [ScopedService] AppDbContext context, [Service] ITopicEventSender eventSender, CancellationToken cancellationToken)
        {
            var platform = new Platform
            {
                Name = input.Name,
            };

            context.Platforms.Add(platform);

            await context.SaveChangesAsync(cancellationToken);

            await eventSender.SendAsync(nameof(Subscription.OnPlatformAdded), platform, cancellationToken);

            return new AddPlatformPayload(platform);
        }

        [UseDbContext(typeof(AppDbContext))]
        public async Task<AddCommandPayload> AddCommandAsync(AddCommandInput input, [ScopedService] AppDbContext context, [Service] ITopicEventSender eventSender, CancellationToken cancellationToken)
        {
            var platform = await context.Platforms.FirstOrDefaultAsync(p => p.Name.ToLower() == input.PlatformName.ToLower(), cancellationToken);

            if (platform == null)
            {
                var newPlatform = new Platform
                {
                    Name = input.PlatformName,
                };

                var newCommand = new Command
                {
                    HowTo = input.HowTo,
                    CommandLine = input.CommandLine,
                };

                newCommand.PlatformId = newPlatform.Id;

                newPlatform.Commands.Add(newCommand);

                context.Platforms.Add(newPlatform);

                var result = await context.SaveChangesAsync(cancellationToken) > 0;

                if (!result)
                    throw new Exception("Problem saving changes");

                await eventSender.SendAsync(nameof(Subscription.OnPlatformAdded), newPlatform, cancellationToken);

                return new AddCommandPayload(newCommand);
            } else {
                var command = new Command
                {
                    HowTo = input.HowTo,
                    CommandLine = input.CommandLine,
                    PlatformId = platform.Id,
                };

                context.Commands.Add(command);

                var result = await context.SaveChangesAsync(cancellationToken) > 0;

                if (!result)
                    throw new Exception("Problem saving changes");

                return new AddCommandPayload(command);
            }
        }

        [UseDbContext(typeof(AppDbContext))]
        public async Task<EditCommandPayload> EditCommandAsync(EditCommandInput input, [ScopedService] AppDbContext context, [Service] ITopicEventSender eventSender, CancellationToken cancellationToken)
        {
            var command = await context.Commands.FirstOrDefaultAsync(c => c.Id == input.Id, cancellationToken);

            if (command == null)
                throw new Exception("Command not found");

            command.HowTo = input.HowTo;
            command.CommandLine = input.CommandLine;

            var platform = await context.Platforms.FirstOrDefaultAsync(p => p.Name.ToLower() == input.PlatformName.ToLower(), cancellationToken);

            if (platform == null)
            {
                var beforePlatform = await context.Platforms.FirstOrDefaultAsync(p => p.Id == command.PlatformId, cancellationToken);

                beforePlatform.Commands.Remove(command);

                var newPlatform = new Platform
                {
                    Name = input.PlatformName,
                };

                newPlatform.Commands.Add(command);

                context.Platforms.Add(newPlatform);
                
            } else if(platform.Id != command.PlatformId) {
                var beforePlatform = await context.Platforms.FirstOrDefaultAsync(p => p.Id == command.PlatformId, cancellationToken);

                beforePlatform.Commands.Remove(command);

                platform.Commands.Add(command);
            }

            var result = await context.SaveChangesAsync(cancellationToken) > 0;

            if (!result)
                throw new Exception("Problem saving changes");

            return new EditCommandPayload(command);
        }

        [UseDbContext(typeof(AppDbContext))]
        public async Task<DeleteCommandPayload> DeleteCommandAsync(DeleteCommandInput input, [ScopedService] AppDbContext context, [Service] ITopicEventSender eventSender, CancellationToken cancellationToken)
        {
            var command = await context.Commands.FirstOrDefaultAsync(c => c.Id == input.Id, cancellationToken);

            if (command == null)
                throw new Exception("Command not found");

            context.Commands.Remove(command);

            var result = await context.SaveChangesAsync(cancellationToken) > 0;

            if (!result)
                throw new Exception("Problem saving changes");

            return new DeleteCommandPayload(command);
        }
    }
}