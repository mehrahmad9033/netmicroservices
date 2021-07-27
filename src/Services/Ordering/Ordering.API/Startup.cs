using EventBus.Message.Common;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Ordering.API.EventBusConsumer;
using Ordering.Application;
using Ordering.Infrastructure;

namespace Ordering.API
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddScoped<Mediator>();
			services.AddApplicationServices();

			//======MassTransit -RabbitMQ Configuration
			services.AddMassTransit(config =>
			{
				config.AddConsumer<BasketcheckoutConsumer>();
				config.UsingRabbitMq((ctx, cfg) =>
				{
					cfg.Host(Configuration["EventBusSettings:HostAddress"]);

					cfg.ReceiveEndpoint(EventBusConstants.BasketCheckoutQueue, c =>
					{
						c.ConfigureConsumer<BasketcheckoutConsumer>(ctx);
					});
				});
			});
			services.AddMassTransitHostedService();


			services.AddInfrastructureServices(Configuration);

			//====General Configuration
			services.AddAutoMapper(typeof(Startup));
			services.AddScoped<BasketcheckoutConsumer>();
			services.AddControllers();
			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new OpenApiInfo { Title = "Ordering.API", Version = "v1" });
			});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseSwagger();
				app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ordering.API v1"));
			}

			app.UseRouting();

			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}
