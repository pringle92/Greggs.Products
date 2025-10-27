using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Greggs.Products.Api.Middleware
{
    /// <summary>
    /// Global exception handling middleware to catch all unhandled exceptions.
    /// This centralises error handling, logs the exception, and returns
    /// a generic, safe-to-display error message to the client,
    /// preventing stack traces or sensitive information from leaking.
    /// </summary>
    public class GlobalExceptionHandlerMiddleware
    {
        #region Fields

        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
        private readonly IHostEnvironment _env;

        #endregion

        #region Constructor

        /// <summary>
        /// Initialises a new instance of the <see cref="GlobalExceptionHandlerMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="logger">The logger instance.</param>
        /// <param name="env">The hosting environment.</param>
        public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Invokes the middleware.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Call the next middleware in the pipeline
                await _next(context);
            }
            catch (Exception ex)
            {
                // An unhandled exception occurred. Log it.
                _logger.LogError(ex, "An unhandled exception has occurred: {ErrorMessage}", ex.Message);

                // Set the response status code
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";

                // Create a response model that is safe to send to the client
                // We can show more detail (like the exception message) in Development
                var response = new
                {
                    title = "An internal server error occurred.",
                    detail = _env.IsDevelopment() ? ex.Message : null,
                    status = StatusCodes.Status500InternalServerError
                };

                // Serialise and write the response
                var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                await context.Response.WriteAsync(jsonResponse);
            }
        }

        #endregion
    }
}