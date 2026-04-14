using Data;
using Logic.Helper;
using Logic.Logic;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

//builder.Services.AddSwaggerGen(options =>
//{
//    options.SwaggerDoc("v1", new OpenApiInfo
//    {
//        Title = "Absence Manager API",
//        Version = "v1"
//    });

//    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
//    {
//        Type = SecuritySchemeType.Http,
//        Scheme = "bearer",
//        BearerFormat = "JWT",
//        Description = "Microsoft Entra access token"
//    });

//    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
//    {
//        [new OpenApiSecuritySchemeReference("bearer", document)] = []
//    });
//});

builder.Services.AddSwaggerGen();

builder.Services.AddScoped(typeof(Repository<>));
builder.Services.AddSingleton<DtoProvider>();
builder.Services.AddScoped<OfficeBookingLogic>();
builder.Services.AddScoped<OfficeManagementLogic>();
builder.Services.AddScoped<UserLogic>();
builder.Services.AddScoped<IAppUserResolver, AppUserResolver>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<AbsenceManagerDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://login.microsoftonline.com/1878a48b-63d6-4d12-a900-07d4267f6762/v2.0";
        options.Audience = "cacb868f-e5d8-4113-acde-780f810c824d";
    });

builder.Services.AddAuthorizationBuilder();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAngular");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();