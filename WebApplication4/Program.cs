using Microsoft.EntityFrameworkCore;
using WebApplication4.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add session and caching services
builder.Services.AddDistributedMemoryCache(); // Required for session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

#region Connect to MySQL
var connectionString = builder.Configuration.GetConnectionString("mySql");
builder.Services.AddDbContext<InvoiceSystemrahtkContext>(options =>
    options.UseMySql("server=localhost;database=InvoiceSystemrahtk;uid=root;pwd=root", ServerVersion.Parse("8.0.36-mysql")));
#endregion




//#region Connect to MySQL
//var connectionString = builder.Configuration.GetConnectionString("mySql");
//builder.Services.AddDbContext<InvoiceSystemrahtkContext>(options =>
//    options.UseMySql("Server=db20210.public.databaseasp.net; Database=db20210; User Id=db20210; Password=xK?7G%9oj5P!;", ServerVersion.Parse("8.0.36-mysql")));
//#endregion




//#region Connect to MySQL
//var connectionString = builder.Configuration.GetConnectionString("mySql");
//builder.Services.AddDbContext<InvoiceSystemrahtkContext>(options =>
//   options.UseMySql("server=InvoiceSystemrahtk.mssql.somee.com;database=InvoiceSystemrahtk;uid=mno4a_SQLLogin_1;pwd=woem9kb4xx", ServerVersion.Parse("8.0.36-mysql")));
//#endregion









var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

//  Enable session middleware
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
