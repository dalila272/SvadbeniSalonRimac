namespace SvadbeniSalon.Common.Services;

public static class EnvBootstrap
{
    public static void LoadRootEnvFile()
    {
        for (var dir = new DirectoryInfo(Directory.GetCurrentDirectory()); dir != null; dir = dir.Parent)
        {
            var path = Path.Combine(dir.FullName, ".env");
            if (!File.Exists(path))
            {
                continue;
            }

            DotNetEnv.Env.Load(path);
            ApplyEnvAliasesToEnvironment();
            return;
        }

        ApplyEnvAliasesToEnvironment();
    }

    private static void ApplyEnvAliasesToEnvironment()
    {
        Map("JwtToken__Audience", "JWT_AUDIENCE");
        Map("JwtToken__Issuer", "JWT_ISSUER");
        Map("JwtToken__SecretKey", "JWT_SECRET_KEY");
        Map("JwtToken__DurationInMinutes", "JWT_DURATION_MINUTES");
        Map("Cors__AllowedOrigins", "CORS_ALLOWED_ORIGINS");

        Map("RabbitMQ__Host", "RABBITMQ_HOST");
        Map("RabbitMQ__Username", "RABBITMQ_USERNAME");
        Map("RabbitMQ__Password", "RABBITMQ_PASSWORD");

        Map("Smtp__Host", "SMTP_HOST");
        Map("Smtp__Port", "SMTP_PORT");
        Map("Smtp__Username", "SMTP_USERNAME");
        Map("Smtp__Password", "SMTP_PASSWORD");
        Map("Smtp__UseSsl", "SMTP_USE_SSL");
        Map("Smtp__From", "SMTP_FROM");
        Map("Smtp__AdminEmail", "SMTP_ADMIN_EMAIL");

        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")))
        {
            var password = Environment.GetEnvironmentVariable("SQL_SA_PASSWORD");
            if (!string.IsNullOrWhiteSpace(password))
            {
                var db = Environment.GetEnvironmentVariable("SQL_DATABASE") ?? "1210386";
                var port = Environment.GetEnvironmentVariable("SQL_PORT") ?? "1435";
                Environment.SetEnvironmentVariable(
                    "ConnectionStrings__DefaultConnection",
                    $"Server=localhost,{port};Database={db};User Id=sa;Password={password};TrustServerCertificate=True;");
            }
        }
    }

    private static void Map(string aspNetKey, string friendlyKey)
    {
        if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(aspNetKey)))
        {
            return;
        }

        var value = Environment.GetEnvironmentVariable(friendlyKey);
        if (!string.IsNullOrWhiteSpace(value))
        {
            Environment.SetEnvironmentVariable(aspNetKey, value);
        }
    }
}
