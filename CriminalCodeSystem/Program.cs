// NOTE: fully-qualified namespaces should be used together with the classes if needed

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddCors(options =>
    {
        options.AddPolicy(name: "Development",
            builder =>
            {
                builder.WithOrigins("localhost")
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });

        options.AddPolicy(name: "Production",
            builder =>
            {
                builder.WithOrigins("*")
                    .SetIsOriginAllowedToAllowWildcardSubdomains()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
    });

/*
string connection = "";
builder.Services.AddDbContextPool<CriminalCodeSystem.Contexts.UserContext>(options => options.UseMySql(connection));
builder.Services.AddDbContextPool<CriminalCodeSystem.Contexts.DisabledTokenContext>(options => options.UseMySql(connection));
builder.Services.AddDbContextPool<CriminalCodeSystem.Contexts.StatusContext>(options => options.UseMySql(connection));
builder.Services.AddDbContextPool<CriminalCodeSystem.Contexts.CriminalCodeContext>(options => options.UseMySql(connection));
*/

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}
else
{
    // app.UseHsts();
}

if (app.Environment.IsDevelopment())
{
    app.UseCors("Development");
}
else
{
    app.UseCors("Production");
}

app.UseWhen(context => context.Request.Path.StartsWithSegments("/api/criminalcode"),
(appImage) => {
    appImage.UseMiddleware<CriminalCodeSystem.Middlewares.AuthorizationMiddleware>();
});

// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
