using System.Collections.Generic;
using System.Security.Claims;
using Amazon;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.Textract;
using Autofac;
using Billy.Expenses;
using Billy.Expenses.API;
using Billy.Infrastructure.Authentication;
using Billy.Infrastructure.Authentication.Basic;
using Billy.Infrastructure.Configs;
using Billy.Infrastructure.CorrelationId;
using Billy.Infrastructure.EventStore;
using Billy.Infrastructure.MassTransit;
using Billy.Infrastructure.Storage;
using Billy.MobileApi;
using Billy.Receipts;
using Billy.Receipts.API;
using Billy.Receipts.Infrastructure.Configs;
using Billy.Receipts.Storage.Minio;
using Billy.Users;
using Billy.Users.API;
using HealthChecks.NpgSql;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Prometheus.Client.AspNetCore;
using Prometheus.Client.HttpRequestDurations;
using RabbitMqConfig = Billy.Infrastructure.Configs.RabbitMqConfig;
using MassTransit;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace Billy.Infrastructure.WebApp
{
    public class Startup
    {
        private  IConfiguration _configuration;
        public Startup()
        {}

        public void ConfigureServices(IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            _configuration = (IConfiguration) serviceProvider.GetService(typeof(IConfiguration));

            var authenticationConfig = _configuration
                .GetSection(ConfigKeys.AuthenticationConfig)
                .Get<AuthenticationConfig>();

            var dbConfig = _configuration.GetSection(ConfigKeys.Database).Get<DatabaseConfig>();

            var storageConfig = _configuration.GetSection(ConfigKeys.StorageConfig).Get<StorageConfig>();
            
            var queueConfig = _configuration.GetSection(ConfigKeys.QueueConfig).Get<QueueConfig>();
            
            var awsAccessConfiguration = _configuration.GetSection(ConfigKeys.AWSAccessConfiguration).Get<AWSAccessConfiguration>();
            
            var ocrConfig = _configuration.GetSection(ConfigKeys.OCRConfig).Get<OCRConfig>();

            services
                .AddAuthorization(options =>
                {
                    options.AddPolicy("ServiceUsers", policy =>
                    {
                        policy.AddAuthenticationSchemes(AuthenticationScheme.BasicScheme);
                        policy.RequireAuthenticatedUser();
                        policy.RequireClaim(ClaimTypes.Name);
                    });
                    options.AddPolicy("ExternalApiUsers", policy =>
                    {
                        policy.AddAuthenticationSchemes(AuthenticationScheme.BearerScheme);
                        policy.RequireAuthenticatedUser();
                        policy.RequireClaim(authenticationConfig.Jwt.UserIdClaimName);
                    });
                })
                .AddAuthentication()
                .AddBasicAuthentication(options =>
                {
                    options.UserName = authenticationConfig.Basic.UserName;
                    options.Password = authenticationConfig.Basic.Password;
                })
                .AddJwtBearer(options =>
            {
                options.Authority = authenticationConfig.Jwt.Authority;
             
                options.RequireHttpsMetadata = authenticationConfig.Jwt.RequireHttpsMetadata;
                options.SaveToken = authenticationConfig.Jwt.SaveToken;
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateLifetime = authenticationConfig.Jwt.TokenValidationParameters.ValidateLifetime,
                    ValidateIssuerSigningKey = authenticationConfig.Jwt.TokenValidationParameters.ValidateIssuerSigningKey,
                    ValidateAudience = authenticationConfig.Jwt.TokenValidationParameters.ValidateAudience,
                    ValidateIssuer = authenticationConfig.Jwt.TokenValidationParameters.ValidateIssuer,
                    ValidAudience = authenticationConfig.Jwt.TokenValidationParameters.ValidAudience
                };
            });
            
            services.AddHttpClient();
            services.AddControllers();
            
            services
                .AddHealthChecks()
                .AddNpgSql(dbConfig.ConnectionString);
            
            services.SetupAWSAccessConfiguration(awsAccessConfiguration);
            
            if (storageConfig.StorageProvider == StorageProvider.S3)
                services.AddAWSService<IAmazonS3>();
            
            if(ocrConfig.OCRProvider == OCRProvider.AWS)
                services.AddAWSService<IAmazonTextract>();

            
            services.SetupMassTransit(awsAccessConfiguration, queueConfig);

            services.AddSwaggerGen(
                c =>
                {
                    c.SwaggerDoc(
                        "v1",
                        new OpenApiInfo()
                        {
                            Title = "Nacoposzlo",
                            Version = "v1"
                        }
                    );
                    c.CustomSchemaIds(x => x.FullName);
                    
                    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
                        In = ParameterLocation.Header, 
                        Description = "Please insert JWT with Bearer into field",
                        Name = "Jwt Authorization",
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer"
                    });
                    
                    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
                        { 
                            new OpenApiSecurityScheme 
                            { 
                                Reference = new OpenApiReference 
                                { 
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer" 
                                } 
                            },
                            new string[] { } 
                        } 
                    });
                    
                    c.AddSecurityDefinition("Basic", new OpenApiSecurityScheme()
                    {
                        Description = "Please insert user name and password into field",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.Http,
                        Scheme = "basic"
                    });
                    
                    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Basic"
                                },
                            },
                            new string[] { } 
                        }
                    });
                }
            );
        }
        
        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterInstance(_configuration);
            
            //Storage config
            var storageConfig = _configuration.GetSection(ConfigKeys.StorageConfig).Get<StorageConfig>();
            builder.RegisterInstance(storageConfig);
            builder.RegisterInstance(storageConfig.MinioStorageConfig);
            builder.RegisterInstance(storageConfig.S3StorageConfig);
            
            //OCR setup
            var ocrConfig = _configuration.GetSection(ConfigKeys.OCRConfig).Get<OCRConfig>();
            builder.RegisterInstance(ocrConfig);
            builder.RegisterInstance(ocrConfig.AWSConfig);
            
            var applicationConfig = _configuration.GetSection(ConfigKeys.Application).Get<ApplicationConfig>();
            builder.RegisterInstance(_configuration.GetSection(ConfigKeys.Database).Get<DatabaseConfig>());
            builder.RegisterInstance(_configuration.GetSection(ConfigKeys.QueueConfig).Get<QueueConfig>());
            builder.RegisterInstance(_configuration.GetSection(ConfigKeys.AuthenticationConfig).Get<AuthenticationConfig>());
            builder.RegisterInstance(_configuration.GetSection(ConfigKeys.AWSAccessConfiguration).Get<AWSAccessConfiguration>());
            
            builder.RegisterInstance(applicationConfig);
            builder.RegisterModule(new BillyAutofacModule());
            builder.RegisterModule(new ExpensesModule());
            builder.RegisterModule(new ReceiptsModule(storageConfig, ocrConfig));
            builder.RegisterModule(new UsersModule());
            builder.RegisterModule(new MobileApiModule());

            RegisterHostedServices(builder, applicationConfig);
        }

        private void RegisterHostedServices(ContainerBuilder builder, ApplicationConfig applicationConfig)
        {
            if (applicationConfig.RunProjections)
                builder.RegisterType<MartenProjectionsHost>()
                    .As<IHostedService>()
                    .AsSelf()
                    .SingleInstance();
        }


        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //TODO:[FP] what to do
            app.UseCors(builder => builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.UseHttpCorrelationIdMiddleware();

            //Has to be run after http correlation id middleware

            app.UseMiddleware<SerilogCorrelationIdMiddleware>();

            app.UsePrometheusRequestDurations(setup =>
            {
                setup.IncludeMethod = true;
                setup.IncludePath = true;
            });
            app.UsePrometheusServer(options =>
            {
                // After upgrade to Prometheus.CLient.AspNetCore 3.0.1 Collector registry instance is assigned to a static field
                // Due to this change we were registering multiple times 'GCCollectionCountCollector', check defined below prevents it.
                // CollectorRegistryInstance is asinged to Metrics.DefaultCollectorRegistry which is a static readonly field.
                if (options.CollectorRegistryInstance.TryGet("GCCollectionCountCollector", out var collector))
                {
                    options.UseDefaultCollectors = false;
                }
            });

            app.UseMiddleware<MobileApiErrorHandlingMiddleware>();
            app.UseMiddleware<ReceiptsApiErrorHandlingMiddleware>();
            app.UseMiddleware<ExpensesApiErrorHandlingMiddleware>();
            app.UseMiddleware<UsersApiErrorHandlingMiddleware>();
            
            app.UseRouting();
            
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health", new HealthCheckOptions()
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
                endpoints.MapControllers();
            });
            
            
            app.UseSwagger();

            app.UseSwaggerUI(
                c => c.SwaggerEndpoint(
                    "/swagger/v1/swagger.json", "Nacoposzlo v1"
                )
            );
        }
    }
}
