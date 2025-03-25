using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Cryptography;
using System.Text;
using Airbnb_Clone_Api.Data;
using Airbnb_Clone_Api.Middleware;
using Microsoft.Extensions.Logging;

var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger("JwtMiddleware");

var builder = WebApplication.CreateBuilder(args);
// Ensure appsettings.json is loaded
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        "server=localhost;database=airbnbclone;user=root;password=robertlangdon",
        new MySqlServerVersion(new Version(9, 2, 0)) // Ensure this matches your MySQL version
    ));

// Define the path for the secret key file
//string keyFilePath = "secretkey.txt";
//string secretKey;

// Check if the key file already exists
//if (File.Exists(keyFilePath))
//{
//    // Read the key from the file
//    secretKey = File.ReadAllText(keyFilePath);
//}
//else
//{
//    // Generate a new secure 256-bit key (32 bytes)
//    byte[] keyBytes = RandomNumberGenerator.GetBytes(32);
//    secretKey = Convert.ToBase64String(keyBytes);

//    // Save the key to a file so it persists between application restarts
//    File.WriteAllText(keyFilePath, secretKey);
//}


string? secretKey = builder.Configuration["Jwt:Key"];
byte[] keyBytes = Convert.FromBase64String(secretKey);
//Console.WriteLine($"Key Length: {keyBytes.Length} bytes");

if (string.IsNullOrEmpty(secretKey))
{
    throw new InvalidOperationException("JWT Secret is missing from configuration.");
}


//if (string.IsNullOrEmpty(secretKey))
//{
//    Console.WriteLine("ERROR: JWT Secret Key is missing or not loaded from appsettings.json!");
//}
//else
//{
//    Console.WriteLine($"Using JWT Secret Key: {secretKey}");
//}

// Add Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            //IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(secretKey)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true
        };

    //extract token from cookie or header    
        //options.Events = new JwtBearerEvents
        //{
        //    OnAuthenticationFailed = context =>
        //    {
        //        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
        //        logger.LogError($"Authentication failed: {context.Exception.Message}");
        //        return Task.CompletedTask;
        //    },
        //    OnMessageReceived = context =>
        //    {
        //        var token = context.Request.Cookies["jwt"];
        //        logger.LogInformation($"Extracted Token from Cookie: {token}");
        //        if (string.IsNullOrEmpty(token) && context.Request.Headers.ContainsKey("Authorization"))
        //        {
        //            token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        //            logger.LogInformation($"Extracted Token from header: {token}");
        //        }

        //        if (!string.IsNullOrEmpty(token))
        //        {
        //            context.Token = token;
        //        }

        //        return Task.CompletedTask;
        //    }
        //};

    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

//JWT Middleware should be before Authentication
app.UseMiddleware<JwtCookieMiddleware>();

//  Authentication & Authorization Middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
//using (var scope = app.Services.CreateScope())
//{
//    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
//    dbContext.Database.EnsureCreated(); // Ensures the DB exists
//    dbContext.Seed(); // Call a method to add test data
//}

