using EddaStore.Data;
using EddaStore.Models.Identity;
using EddaStore.Repositories.Implementations;
using EddaStore.Repositories;
using EddaStore.Repositories.Interfaces;
using EddaStore.Services;
using EddaStore.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Database
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' was not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Identity - Guid keys
builder.Services
    .AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;

        options.User.RequireUniqueEmail = true;

        options.Password.RequiredLength = 10;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;

        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan =
            TimeSpan.FromMinutes(15);
    })
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Google OAuth
// يتم تفعيل Google فقط بعد تخزين ClientId و ClientSecret
// داخل User Secrets أو Configuration Provider آخر.
var googleClientId =
    builder.Configuration["Authentication:Google:ClientId"];

var googleClientSecret =
    builder.Configuration["Authentication:Google:ClientSecret"];

if (!string.IsNullOrWhiteSpace(googleClientId) &&
    !string.IsNullOrWhiteSpace(googleClientSecret))
{
    builder.Services
        .AddAuthentication()
        .AddGoogle(options =>
        {
            options.ClientId = googleClientId;
            options.ClientSecret = googleClientSecret;
            options.CallbackPath = "/signin-google";
            options.SaveTokens = true;

            // Google's OpenID Connect userinfo response includes
            // "email_verified" as a JSON boolean. Map it to a claim so
            // ExternalLogin can verify the e-mail address reliably.
            options.ClaimActions.MapJsonKey("email_verified", "email_verified");
            options.ClaimActions.MapJsonKey("email_verified", "verified_email");
        });
}

// Security: hardened application cookies.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
});
builder.Services.ConfigureExternalCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// Rate limiting for sensitive authentication endpoints.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth", context =>
    {
        var partitionKey = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        });
    });
});

// MVC and Razor Pages
builder.Services.AddHealthChecks().AddDbContextCheck<ApplicationDbContext>();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICatalogRepository, CatalogRepository>();
builder.Services.AddScoped<ICatalogService, CatalogService>();
builder.Services.AddScoped<IProductImageRepository, ProductImageRepository>();
builder.Services.AddScoped<IProductImageService, ProductImageService>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<ICouponService, CouponService>();
builder.Services.AddScoped<IReviewAdminService, ReviewAdminService>();
builder.Services.AddScoped<IRecycleBinRepository, RecycleBinRepository>();
builder.Services.AddScoped<IRecycleBinService, RecycleBinService>();
builder.Services.AddScoped<IStoreCatalogService, StoreCatalogService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<ICheckoutService, CheckoutService>();
builder.Services.AddTransient<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, EddaStore.Services.EmailSender>();
var app = builder.Build();

// HTTP pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider
        .GetRequiredService<RoleManager<IdentityRole<Guid>>>();

    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        var result = await roleManager.CreateAsync(
            new IdentityRole<Guid>("Admin"));

        if (!result.Succeeded)
        {
            var errors = string.Join(
                "; ",
                result.Errors.Select(error => error.Description));

            throw new InvalidOperationException(
                $"Could not create Admin role: {errors}");
        }
    }
}


app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    await next();
});
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHealthChecks("/health");
app.MapRazorPages();

// Development-only diagnostic endpoints.
// Registered only in Development and restricted to loopback clients.
if (app.Environment.IsDevelopment())
{
    app.MapGet("/dev/reset-link", async (HttpContext httpContext, string? email, UserManager<ApplicationUser> userManager) =>
    {
        if (httpContext.Connection.RemoteIpAddress is not null &&
            !System.Net.IPAddress.IsLoopback(httpContext.Connection.RemoteIpAddress))
        {
            return Results.Forbid();
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return Results.BadRequest("Query parameter 'email' is required.");
        }

        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return Results.NotFound();
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var code = WebEncoders.Base64UrlEncode(System.Text.Encoding.UTF8.GetBytes(token));
        var callbackUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/Identity/Account/ResetPassword?code={Uri.EscapeDataString(code)}&email={Uri.EscapeDataString(user.Email!)}";
        return Results.Ok(new { code, callbackUrl });
    });

    // Development-only diagnostic endpoint for verifying SMTP delivery.
    app.MapGet("/dev/test-email", async (HttpContext httpContext, string? to, IEmailSender emailSender, ILoggerFactory loggerFactory) =>
    {
        var logger = loggerFactory.CreateLogger("DevEmailTest");

        if (httpContext.Connection.RemoteIpAddress is not null &&
            !System.Net.IPAddress.IsLoopback(httpContext.Connection.RemoteIpAddress))
        {
            return Results.Forbid();
        }

        if (string.IsNullOrWhiteSpace(to))
        {
            return Results.BadRequest("Query parameter 'to' is required.");
        }

        try
        {
            await emailSender.SendEmailAsync(
                to,
                "EddaStore SMTP test",
                "<p>This is a development-only SMTP delivery test.</p>");

            logger.LogInformation("Development SMTP test completed for {Recipient}.", to.Contains('@') ? to[..1] + "***" + to[to.IndexOf('@')..] : "***");
            return Results.Ok("SMTP send completed. Check recipient inbox and Sent folder.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Development SMTP test failed.");
            return Results.Problem("SMTP send failed. Check application logs.");
        }
    });
}

app.Run();
