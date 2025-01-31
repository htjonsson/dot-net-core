using Microsoft.EntityFrameworkCore;

using JwtApp;
using JwtApp.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

#region Swagger/OpenAPI Setup

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#endregion

#region Services Setup

builder.Services.AddScoped<IUserService, UserService>();

#endregion

#region Database Context Setup

builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DataContextSQLite")));

#endregion

#region JSON Web Token Setup

JwtApp.JwtHelper.AddAuthentication(builder);

#endregion

var app = builder.Build();

DataContext.CreateDatabase(app);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
