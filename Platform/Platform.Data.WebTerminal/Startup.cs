using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Platform.Data.Triplets;

namespace Platform.Data.WebTerminal
{
    public class Startup
    {
        public Startup(IConfiguration configuration) => Configuration = configuration;

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddControllers();
            services.AddControllersWithViews();

            // Add Swagger/OpenAPI support
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Links Platform API",
                    Version = "v1",
                    Description = "REST API for direct HTTP connection to Links Platform"
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Links Platform API v1");
                    c.RoutePrefix = "api";
                });
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseRouting();

            // Enable WebSocket support
            var webSocketOptions = new WebSocketOptions
            {
                KeepAliveInterval = TimeSpan.FromMinutes(2)
            };
            app.UseWebSockets(webSocketOptions);

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Links}/{action=Infinite}/{id?}");

                // WebSocket endpoint
                endpoints.Map("/ws", async context =>
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                        await HandleWebSocketConnection(webSocket);
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                    }
                });
            });
        }

        private static async Task HandleWebSocketConnection(WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult? result = null;

            try
            {
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                while (!result.CloseStatus.HasValue)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    // Process the message and interact with Links
                    var response = ProcessLinkCommand(message);
                    var responseBytes = Encoding.UTF8.GetBytes(response);

                    await webSocket.SendAsync(
                        new ArraySegment<byte>(responseBytes, 0, responseBytes.Length),
                        result.MessageType,
                        result.EndOfMessage,
                        CancellationToken.None);

                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                }

                await webSocket.CloseAsync(
                    result.CloseStatus.Value,
                    result.CloseStatusDescription,
                    CancellationToken.None);
            }
            catch (Exception ex)
            {
                if (webSocket.State == WebSocketState.Open)
                {
                    await webSocket.CloseAsync(
                        WebSocketCloseStatus.InternalServerError,
                        ex.Message,
                        CancellationToken.None);
                }
            }
        }

        private static string ProcessLinkCommand(string message)
        {
            try
            {
                // Parse command format: "GET <id>" or "CREATE <source> <linker> <target>"
                var parts = message.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length == 0)
                {
                    return "ERROR: Empty command";
                }

                var command = parts[0].ToUpper();

                switch (command)
                {
                    case "GET":
                        if (parts.Length >= 2 && long.TryParse(parts[1], out var id))
                        {
                            var link = Link.Restore(id);
                            return $"OK: {link}";
                        }
                        return "ERROR: Invalid GET command. Usage: GET <id>";

                    case "CREATE":
                        if (parts.Length >= 4 &&
                            long.TryParse(parts[1], out var source) &&
                            long.TryParse(parts[2], out var linker) &&
                            long.TryParse(parts[3], out var target))
                        {
                            var sourceLink = Link.Restore(source);
                            var linkerLink = Link.Restore(linker);
                            var targetLink = Link.Restore(target);
                            var newLink = Link.Create(sourceLink, linkerLink, targetLink);
                            return $"OK: Created {newLink}";
                        }
                        return "ERROR: Invalid CREATE command. Usage: CREATE <source> <linker> <target>";

                    case "PING":
                        return "PONG";

                    default:
                        return $"ERROR: Unknown command '{command}'. Supported: GET, CREATE, PING";
                }
            }
            catch (Exception ex)
            {
                return $"ERROR: {ex.Message}";
            }
        }
    }
}
