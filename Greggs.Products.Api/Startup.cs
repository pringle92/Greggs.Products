using Greggs.Products.Api.Configuration;
using Greggs.Products.Api.DataAccess;
using Greggs.Products.Api.Middleware;
using Greggs.Products.Api.Models;
using Greggs.Products.Api.Services;
using Greggs.Products.Api.Services.CurrencyConversion;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Greggs.Products.Api
{
    /// <summary>
    /// The main startup class for the application.
    /// This class configures services and the HTTP request pipeline.
    /// </summary>
    public class Startup
    {
        #region Constructor

        /// <summary>
        /// Initialises a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">The application's configuration properties.</param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the application's configuration properties.
        /// </summary>
        public IConfiguration Configuration { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddSwaggerGen();

            // Register the in-memory cache service
            services.AddMemoryCache();

            #region Application Services and Configuration

            // Register strongly-typed configuration options.
            // This binds the "CurrencySettings" section from appsettings.json to the CurrencySettings class.
            // .ValidateDataAnnotations() and .ValidateOnStart() ensure the application
            // fails at startup if the configuration is invalid (e.g., EurExchangeRate <= 0).
            services.AddOptions<CurrencySettings>()
                .Bind(Configuration.GetSection(nameof(CurrencySettings)))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            // Register and validate pagination configuration.
            services.AddOptions<PaginationSettings>()
                .Bind(Configuration.GetSection(nameof(PaginationSettings)))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            // Register application services with a scoped lifetime (one instance per HTTP request).

            // This "plugs in" the data access layer.
            // The IDataAccess<Product> interface will be resolved by ProductAccess.
            services.AddScoped<IDataAccess<Product>, ProductAccess>();

            // This registers the new business logic service.
            // The IProductService interface will be resolved by ProductService.
            services.AddScoped<IProductService, ProductService>();

            // Register all the currency conversion strategies.
            // When IProductService asks for IEnumerable<ICurrencyConverter>,
            // the DI container will provide both of these.
            services.AddScoped<ICurrencyConverter, GbpConverter>();
            services.AddScoped<ICurrencyConverter, EurConverter>();

            #endregion
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="env">The web hosting environment.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

            if (env.IsDevelopment())
            {
                // app.UseDeveloperExceptionPage(); this is not needed due to global exception handler
            }
            else
            {
                // In production, use HSTS (HTTP Strict Transport Security)
            }

            app.UseSwagger();
            app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Greggs Products API V1"); });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }

        #endregion
    }
}
