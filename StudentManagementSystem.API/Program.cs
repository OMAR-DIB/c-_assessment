
using NLog;
using StudentManagementSystem.API.Services;
using StudentManagementSystem.Data.Entities;
using StudentManagementSystem.Data.Repository;

namespace StudentManagementSystem.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            builder.Services.AddScoped<IStudentService, StudentService>();
            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            // Add services to the container
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();
            //builder.Services.AddEndpointsApiExplorer(); // needed for minimal APIs
            builder.Services.AddSwaggerGen();           // registers the Swagger generator


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwagger();               // Generates the Swagger JSON
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
