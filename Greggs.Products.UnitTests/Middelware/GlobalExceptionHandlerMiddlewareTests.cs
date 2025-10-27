using Greggs.Products.Api.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Greggs.Products.UnitTests.Middleware
{
    /// <summary>
    /// Unit tests for the <see cref="GlobalExceptionHandlerMiddleware"/>.
    /// </summary>
    public class GlobalExceptionHandlerMiddlewareTests
    {
        private readonly Mock<ILogger<GlobalExceptionHandlerMiddleware>> _mockLogger;
        private readonly Mock<IHostEnvironment> _mockEnv;

        public GlobalExceptionHandlerMiddlewareTests()
        {
            _mockLogger = new Mock<ILogger<GlobalExceptionHandlerMiddleware>>();
            _mockEnv = new Mock<IHostEnvironment>();
        }

        [Fact]
        public async Task InvokeAsync_WhenNoError_CallsNextMiddleware()
        {
            // Arrange
            var middleware = new GlobalExceptionHandlerMiddleware(
                (innerContext) => Task.CompletedTask, // "Next" middleware
                _mockLogger.Object,
                _mockEnv.Object
            );
            var context = new DefaultHttpContext();

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.Equal(200, context.Response.StatusCode); // Default status
        }

        [Fact]
        public async Task InvokeAsync_WhenExceptionOccurs_Returns500AndLogsError()
        {
            // Arrange
            _mockEnv.Setup(e => e.EnvironmentName).Returns("Production");

            var middleware = new GlobalExceptionHandlerMiddleware(
                (innerContext) => throw new Exception("Test Exception"), // "Next" middleware throws
                _mockLogger.Object,
                _mockEnv.Object
            );

            var context = new DefaultHttpContext();
            // Need a real stream to write the response to
            context.Response.Body = new MemoryStream();

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            // 1. Check status code
            Assert.Equal(500, context.Response.StatusCode);
            Assert.Equal("application/json", context.Response.ContentType);

            // 2. Check that error was logged
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An unhandled exception has occurred: Test Exception")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once
            );

            // 3. Check the response body
            context.Response.Body.Seek(0, SeekOrigin.Begin); // Rewind stream
            var reader = new StreamReader(context.Response.Body);
            var responseBody = await reader.ReadToEndAsync();
            var responseJson = JsonDocument.Parse(responseBody);

            Assert.Equal("An internal server error occurred.", responseJson.RootElement.GetProperty("title").GetString());
            Assert.Equal(500, responseJson.RootElement.GetProperty("status").GetInt32());
            Assert.Equal(JsonValueKind.Null, responseJson.RootElement.GetProperty("detail").ValueKind); // Null in Production
        }
    }
}