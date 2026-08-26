using System.Net;
using System.Security.Claims;
using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SvadbeniSalon.Model.Exceptions;
using SvadbeniSalon.WebAPI.Services.AccessManager;

namespace SvadbeniSalon.WebAPI.Filters
{
    public class ExceptionFilter : ExceptionFilterAttribute
    {
        private readonly ILogger<ExceptionFilter> _logger;

        public ExceptionFilter(ILogger<ExceptionFilter> logger)
        {
            _logger = logger;
        }

        public override void OnException(ExceptionContext context)
        {
            var http = context.HttpContext;
            var request = http.Request;
            var correlationId = http.TraceIdentifier;
            var userId = http.User?.FindFirst(ClaimNames.Id)?.Value
                         ?? http.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? "anonymous";
            var method = request.Method;
            var path = request.Path.Value ?? string.Empty;
            var query = request.QueryString.HasValue ? request.QueryString.Value : string.Empty;

            using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId,
                ["UserId"] = userId,
                ["HttpMethod"] = method,
                ["Path"] = path,
                ["Query"] = query ?? string.Empty,
            }))
            {
                if (context.Exception is ValidationException fvEx)
                {
                    foreach (var error in fvEx.Errors)
                    {
                        context.ModelState.AddModelError(
                            error.PropertyName ?? string.Empty,
                            error.ErrorMessage);
                    }

                    context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    _logger.LogWarning(
                        context.Exception,
                        "Validation failed. {Method} {Path}{Query} UserId={UserId} CorrelationId={CorrelationId} Details={Details}",
                        method,
                        path,
                        query,
                        userId,
                        correlationId,
                        FormatValidationErrors(fvEx));
                }
                else if (context.Exception is BusinessException be)
                {
                    context.ModelState.AddModelError("clientError", be.Message);
                    context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    _logger.LogWarning(
                        context.Exception,
                        "Business rule violated. {Method} {Path}{Query} UserId={UserId} CorrelationId={CorrelationId} Message={Message}",
                        method,
                        path,
                        query,
                        userId,
                        correlationId,
                        be.Message);
                }
                else if (context.Exception is NotFoundException nfe)
                {
                    context.ModelState.AddModelError("notFound", nfe.Message);
                    context.HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    _logger.LogWarning(
                        context.Exception,
                        "Resource not found. {Method} {Path}{Query} UserId={UserId} CorrelationId={CorrelationId} Message={Message}",
                        method,
                        path,
                        query,
                        userId,
                        correlationId,
                        nfe.Message);
                }
                else if (context.Exception is KeyNotFoundException knf)
                {
                    context.ModelState.AddModelError("notFound", knf.Message);
                    context.HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    _logger.LogWarning(
                        context.Exception,
                        "Resource not found. {Method} {Path}{Query} UserId={UserId} CorrelationId={CorrelationId} Message={Message}",
                        method,
                        path,
                        query,
                        userId,
                        correlationId,
                        knf.Message);
                }
                else if (context.Exception is UnauthorizedAccessException uae)
                {
                    context.ModelState.AddModelError("unauthorized", uae.Message);
                    context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    _logger.LogWarning(
                        context.Exception,
                        "Unauthorized. {Method} {Path}{Query} UserId={UserId} CorrelationId={CorrelationId} Message={Message}",
                        method,
                        path,
                        query,
                        userId,
                        correlationId,
                        uae.Message);
                }
                else
                {
                    context.ModelState.AddModelError(
                        "serverError",
                        "Server side error, please check logs.");
                    context.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    _logger.LogError(
                        context.Exception,
                        "Unhandled exception. {Method} {Path}{Query} UserId={UserId} CorrelationId={CorrelationId}",
                        method,
                        path,
                        query,
                        userId,
                        correlationId);
                }
            }

            var list = context.ModelState
                .Where(c => c.Value is { Errors.Count: > 0 })
                .ToDictionary(
                    c => c.Key,
                    c => c.Value!.Errors.Select(z => z.ErrorMessage).ToList());

            var allMessages = list.Values
                .SelectMany(v => v)
                .Where(m => !string.IsNullOrWhiteSpace(m))
                .ToList();
            var message = allMessages.FirstOrDefault()
                ?? (context.Exception is BusinessException ? context.Exception.Message : null)
                ?? "Request could not be processed.";

            context.Result = new JsonResult(new
            {
                message,
                correlationId,
                errors = list
            });
            context.ExceptionHandled = true;
        }

        private static string FormatValidationErrors(ValidationException ex)
        {
            var sb = new StringBuilder();
            foreach (var error in ex.Errors)
            {
                if (sb.Length > 0) sb.Append("; ");
                sb.Append(error.PropertyName).Append(": ").Append(error.ErrorMessage);
            }

            return sb.ToString();
        }
    }
}
