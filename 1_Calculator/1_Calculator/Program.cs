using OpenTelemetry.Metrics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddOpenTelemetry()
    .WithMetrics(meterProviderBuilder =>
    {
        meterProviderBuilder.AddPrometheusExporter();


        meterProviderBuilder.AddMeter("Microsoft.AspNetCore.Hosting",
                                      "Microsoft.AspNetCore.Server.Kestrel");

     
        meterProviderBuilder.AddMeter("Microsoft.AspNetCore.Http.Connections");

       
        meterProviderBuilder.AddView("http.server.request.duration",
            new ExplicitBucketHistogramConfiguration
            {
                Boundaries = new double[]
                {
                    0, 0.005, 0.01, 0.025, 0.05, 0.075, 0.1, 0.25, 0.5, 0.75, 1, 2.5, 5, 7.5, 10
                }
            });
    });
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
