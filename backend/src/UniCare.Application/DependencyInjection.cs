using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using UniCare.Application.Features.MedicalProfiles;
using UniCare.Application.Features.Students;

namespace UniCare.Application;

/// <summary>
/// Registers the Application layer's own services. Mirrors
/// UniCare.Infrastructure.DependencyInjection so Program.cs reads as one line
/// per layer rather than a wall of registrations.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<IMedicalProfileService, MedicalProfileService>();

        return services;
    }
}
