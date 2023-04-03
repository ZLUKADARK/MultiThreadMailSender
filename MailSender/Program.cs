using MailSender.Models.ConfigModels;
using MailSender.Static;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Services
builder.Services.AddSingleton<IMailSenderService, MailSenderService>();

//Background services
builder.Services.AddHostedService<Consumer>();

//Configuration model binds 
builder.Services.Configure<MailConfigModel>(builder.Configuration.GetSection("MailConfig"));
builder.Services.Configure<RabbitConfigModel>(builder.Configuration.GetSection("RabbitConfig"));
builder.Services.Configure<MailSenderConfig>(builder.Configuration.GetSection("MailSenderConfig"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Services.GetService<ILoggerFactory>();

var loggerFactory = app.Services.GetService<ILoggerFactory>();
loggerFactory.AddFile(builder.Configuration["Logging:LogFilePath"].ToString());

app.UseAuthorization();

app.MapControllers();

app.Run();

