using Microsoft.AspNetCore.Mvc;

namespace Hotel_Booking.API.Error_Handler
{
    public static class ErrorMessageHandle
    {

        public static IServiceCollection MyMethod(this IServiceCollection services)
        {
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = actionContext =>
                {
                    var errors = actionContext.ModelState
                    .Where(e => e.Value.Errors.Count > 0)
                    .SelectMany(e => e.Value.Errors)
                    .Select(e => e.ErrorMessage).ToList();

                    var res = new ResponseValidationError(errors);

                    return new BadRequestObjectResult(res);
                };
            });

            return services;
        }

    }
}
