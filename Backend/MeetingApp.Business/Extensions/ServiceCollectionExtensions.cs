using MeetingApp.Business.Abstractions.Email;
using MeetingApp.Business.Abstractions.File;
using MeetingApp.Business.Abstractions.Meeting;
using MeetingApp.Business.Abstractions.User;
using MeetingApp.Business.Handlers.Email;
using MeetingApp.Business.Services.Email;
using MeetingApp.Business.Services.File;
using MeetingApp.Business.Services.Jobs;
using MeetingApp.Business.Services.Meeting;
using MeetingApp.Business.Services.User;
using MeetingApp.DataAccess.Context;
using MeetingApp.DataAccess.Repositories.Abstractions;
using MeetingApp.DataAccess.Repositories.Implementations;
using MeetingApp.DataAccess.Repositories.Interfaces;
using MeetingApp.DataAccess.Repositories.UnitOfWork;
using MeetingApp.Models.Messages;
using MeetingApp.Models.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Config;
using Rebus.Routing.TypeBased;

namespace MeetingApp.Business
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBusinessServices(
            this IServiceCollection services,
            IConfiguration configuration,
            string connectionString)
        {
            services.AddDbContext<MeetingAppDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IMeetingRepository, MeetingRepository>();
            services.AddScoped<IMeetingDocumentRepository, MeetingDocumentRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IMeetingService, MeetingService>();
            services.AddScoped<IEmailSender, SmtpEmailSender>();
            services.AddScoped<IMeetingDocumentService, MeetingDocumentService>();
            services.AddScoped<IPasswordService, PasswordService>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IFileService, FileService>();



            services.AddScoped<IEmailTemplateService, EmailTemplateService>();
            services.AddScoped<IEmailSender, SmtpEmailSender>();
            services.AddScoped<IEmailService, EmailService>();



            var rabbitMqConnection = configuration["ConnectionStrings:MessageBroker"] ?? "amqp://guest:guest@localhost:5673";

            services.AddRebus(configure => configure
                .Transport(t => t.UseRabbitMq(rabbitMqConnection, "email-queue"))
                .Routing(r => r.TypeBased().Map<EmailMessage>("email-queue"))
            );

            services.AutoRegisterHandlersFromAssemblyOf<EmailMessageHandler>();


            services.AddScoped<MeetingCleanupJob>();


            return services;
        }


    }



}