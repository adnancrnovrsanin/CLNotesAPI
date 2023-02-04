using Microsoft.EntityFrameworkCore;
using GraphQL.Server.Ui.Voyager;
using CommandGQL.GraphQL;
using CommandGQL.Data;
using CommandGQL.GraphQL.Platforms;
using CommandGQL.GraphQL.Commands;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddSubscriptionType<Subscription>()
    .AddType<PlatformType>()
    .AddType<CommandType>()
    .AddFiltering()
    .AddSorting()
    .AddInMemorySubscriptions();

builder.Services.AddPooledDbContextFactory<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("CorsPolicy", policy => {
        policy.AllowAnyMethod().AllowAnyHeader().AllowCredentials().SetIsOriginAllowed((host) => true);
    });
});

var app = builder.Build();

app.UseCors("CorsPolicy");

app.UseAuthorization();

app.MapControllers();

app.MapGraphQL();

app.UseWebSockets();

app.UseRouting();

app.UseGraphQLVoyager(new VoyagerOptions()
{
    GraphQLEndPoint = "/graphql"
}, "/graphql-voyager");


app.Run();
