using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using task;
using task.Dtos;
using task.ErrorHandleing;
using task.Helper;
using task.Middleware;
using task.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionMiddleware>();


app.MapGet("/products", async (ProductDbContext db, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10) =>
{
    int count = db.Products.AsQueryable().Count();
    var products = await db.Products.AsQueryable().Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
    return products != null ? Results.Ok(new Pagination<ProductToReturnDto>(pageIndex, pageSize, count,
                                          products.Select(p => new ProductToReturnDto 
                                          {
                                              Id = p.Id,
                                              Name = p.Name,
                                              Description = p.Description,
                                              Price = p.Price,
                                              CreatedDate = p.CreatedDate
                                          }).ToList())) : Results.Ok(new ApiException(404));

});


app.MapGet("/products/{id:int}", async (int id, ProductDbContext db) =>

{
    var product = await db.Products.FindAsync(id);
    return product != null ? Results.Ok(new ProductToReturnDto // Map to DTO
    {
        Id = product.Id,
        Name = product.Name,
        Description = product.Description,
        Price = product.Price,
        CreatedDate = product.CreatedDate
    }) : Results.Ok(new ApiException(404));
});


app.MapPost("/products", async (ProductToReturnDto product, ProductDbContext db) =>
{
    var data = db.Products.Add(new Product
    {
        Id = product.Id,
        Name = product.Name,
        Description = product.Description,
        Price = product.Price,
        CreatedDate = product.CreatedDate
    });
    await db.SaveChangesAsync();
    return Results.Ok(new { statusCode = 200, Message = "Product Added Successfully" });
});


app.MapPut("/products/{id:int}", async (int id, ProductToReturnDto input, ProductDbContext db) =>
{
    var product = await db.Products.FindAsync(id);
    if (product is null) return Results.Ok(new ApiException(404));

    product.Name = input.Name;
    product.Description = input.Description;
    product.Price = input.Price;
    await db.SaveChangesAsync();
    return Results.Ok(new { statusCode = 200, Message = "Product Updated Successfully" });
});


app.MapDelete("/products/{id:int}", async (int id, ProductDbContext db) =>
{
    var product = await db.Products.FindAsync(id);
    if (product is null) return Results.Ok(new ApiException(404));

    db.Products.Remove(product);
    await db.SaveChangesAsync();
    return Results.Ok(new { statusCode = 200, Message = "Product Deleted Successfully" });
});

app.Run();