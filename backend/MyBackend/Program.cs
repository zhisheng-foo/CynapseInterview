using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

var builder = WebApplication.CreateBuilder(args);

// Disable the default credentials provider
Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", null); // Ensure no ADC is used

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

// Manually specify the path to your Firebase service account key
var serviceAccountPath = builder.Configuration.GetValue<string>("Firebase:ServiceAccountKeyPath");
 
FirebaseApp.Create(new AppOptions
{
    Credential = GoogleCredential.FromFile(serviceAccountPath)  
});

var app = builder.Build();

app.UseCors("AllowAll");
app.UseRouting();
app.MapControllers();
app.Run();
