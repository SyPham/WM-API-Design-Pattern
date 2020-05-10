using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using WM.Application.AutoMapper;
using AutoMapper;
using WM.Infrastructure.Interfaces;
using Newtonsoft.Json.Serialization;
using WM.Data.EF;
using WM.Ultilities.Helpers;
using Microsoft.OpenApi.Models;
using WM.Data.IRepositories;
using WM.Data.EF.Repositories;
using WM.Application.Interface;
using WM.Application.Implementation;
using Microsoft.EntityFrameworkCore;

namespace WM.WebApi
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
            services.AddDbContext<AppDbContext>(options =>
                     options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"),
                         b => b.MigrationsAssembly("WM.Data.EF")));

            var appSettings = Configuration.GetSection("AppSettings").Get<AppSettings>();
            services.AddControllers();
            //Config authen
            services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(cfg =>
            {
                cfg.RequireHttpsMetadata = false;
                cfg.SaveToken = true;

                cfg.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidIssuer = appSettings.Issuer,
                    ValidAudience = appSettings.Issuer,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appSettings.Token))
                };
            });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WM API", Version = "v1" });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                     {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                },
                                Scheme = "oauth2",
                                Name = "Bearer",
                                In = ParameterLocation.Header,

                            },
                            new List<string>()
                    }
                });

            });
            services.AddCors(o => o.AddPolicy("CorsPolicy", builder =>
            {
                builder.WithOrigins(appSettings.CorsPolicy)
                    .AllowAnyHeader()
                    .AllowAnyMethod().Build();
            }));

            //Auto Mapper
            services.AddAutoMapper(typeof(Startup));
            services.AddScoped<IMapper>(sp =>
            {
                return new Mapper(AutoMapperConfig.RegisterMappings());
            });
            services.AddSingleton(AutoMapperConfig.RegisterMappings());

            services.AddTransient(typeof(IUnitOfWork), typeof(EFUnitOfWork));

            //Repository
            services.AddTransient<IChatRepository, ChatRepository>();
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<ICommentRepository, CommentRepository>();
            services.AddTransient<IDeputyRepository, DeputyRepository>();
            services.AddTransient<IFollowRepository, FollowRepository>();
            services.AddTransient<IHistoryRepository, HistoryRepository>();
            services.AddTransient<IManagerRepository, ManagerRepository>();
            services.AddTransient<INotificationRepository, NotificationRepository>();
            services.AddTransient<INotificationDetailRepository, NotificationDetailRepository>();
            services.AddTransient<IOCRepository, OCRepository>();
            services.AddTransient<IProjectRepository, ProjectRepository>();
            services.AddTransient<IRoleRepository, RoleRepository>();
            services.AddTransient<IRoomRepository, RoomRepository>();
            services.AddTransient<ITaskRepository, TaskRepository>();
            services.AddTransient<ITeamMemberRepository, TeamMemberRepository>();
            services.AddTransient<IOCUserRepository, OCUserRepository>();

            //Service
            services.AddTransient<IChatService, ChatService>();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<ICommentService, CommentService>();
            services.AddTransient<IDeputyService, DeputyService>();
            services.AddTransient<IFollowService, FollowService>();
            services.AddTransient<IHistoryService, HistoryService>();
            services.AddTransient<IManagerService, ManagerService>();
            services.AddTransient<INotificationService, NotificationService>();
            services.AddTransient<INotificationDetailService, NotificationDetailService>();
            services.AddTransient<IOCService, OCService>();
            services.AddTransient<IProjectService, ProjectService>();
            services.AddTransient<IRoleService, RoleService>();
            services.AddTransient<IRoomService, RoomService>();
            services.AddTransient<ITaskService, TaskService>();
            services.AddTransient<ITeamMemberService, TeamMemberService>();
            services.AddTransient<IOCUserService, OCUserService>();
            services.AddControllers().AddNewtonsoftJson(options =>
            {
                // Use the default property (Pascal) casing
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();
            });
           // services.AddTransient<DbInitializer>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Work Management System");
            });
            app.UseStaticFiles();
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
