using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Cloud7CMS.Data;
using Cloud7CMS.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Configuration;
using Cloud7CMS.Models;
using System.Data;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("Cloud7CMSContextConnection") ?? throw new InvalidOperationException("Connection string 'Cloud7CMSContextConnection' not found.");

builder.Services.AddDbContext<Cloud7CMSContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddDefaultIdentity<Cloud7CMSUser>(options => options.SignIn.RequireConfirmedAccount = true)    
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<Cloud7CMSContext>();

builder.Services.AddControllersWithViews()
            .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);
builder.Services.AddAntiforgery(options => options.HeaderName = "XSRF-TOKEN");

// Add services to the container.
//builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));

builder.Services.AddTransient<IEmailSender, EmailSender>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();


//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id?}");

app.UseEndpoints(endpoints =>
{
    endpoints.MapRazorPages();

    endpoints.MapControllerRoute(
		name: "default",
		pattern: "{controller=Home}/{action=Index}/{id?}"
	);
});

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var roles = new[] { "Admin","FunBoxManager"};

    foreach (var role in roles)
    {
        if (! await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Cloud7CMSUser>>();
    string email = "shadhinmusic@outlook.com";
    string password = "Test1234,";

    if (await userManager.FindByEmailAsync(email)==null)
    {
        var user = new Cloud7CMSUser();
        user.UserName= email;
        user.Email = email;
        user.EmailConfirmed= false;
        

        await userManager.CreateAsync(user,password);

        await userManager.AddToRoleAsync(user,"Admin");
    }

}


app.Run();
