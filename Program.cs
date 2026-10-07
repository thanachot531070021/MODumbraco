
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

#if DEBUG
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);
#endif

// CKAN (uWSGI) can't read chunked request bodies; see Ckan/BufferedRequestContentHandler.cs
builder.Services.AddTransient<MODumbraco.Ckan.BufferedRequestContentHandler>();
builder.Services.AddHttpClient("ckan").AddHttpMessageHandler<MODumbraco.Ckan.BufferedRequestContentHandler>();

builder.CreateUmbracoBuilder()
    .AddBackOffice()
    .AddWebsite()
    .AddComposers()
    .Build();

WebApplication app = builder.Build();


await app.BootUmbracoAsync();


app.UseUmbraco()
    .WithMiddleware(u =>
    {
        u.UseBackOffice();
        u.UseWebsite();
    })
    .WithEndpoints(u =>
    {
        u.UseBackOfficeEndpoints();
        u.UseWebsiteEndpoints();
    });

await app.RunAsync();
