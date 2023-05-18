using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class Movie
{
    public int Id { get; set; }
    public string Title { get; set; }
    public bool Watched { get; set; }
    public string Genre { get; set; } 
}



public class TodoDbContext : DbContext
{
    public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Movie>();
    }

    public DbSet<Movie> Movies { get; set; }
}

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<TodoDbContext>(options => options.UseInMemoryDatabase("Movie"));
        services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        });

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, TodoDbContext dbContext)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        // Drop and recreate the database
         dbContext.Database.EnsureDeleted();
         dbContext.Database.EnsureCreated();

        // Seed the database with initial data
        if (!dbContext.Movies.Any())
        {
            var movies = new[]
            {
                new Movie
                {
                    Title = "Batman",
                    Genre = "Action",
                    Watched = true
                },
                new Movie
                {
                    Title = "Truman Show",
                    Genre = "Drama",
                    Watched = true
                },
                new Movie
                {
                    Title = "Star Wars",
                    Genre = "Sci-Fi",
                    Watched = true
                },
                new Movie
                {
                    Title = "John Wick",
                    Genre = "Action",
                    Watched = false
                },
                new Movie
                {
                    Title = "Pulp Fiction",
                    Genre = "Drama",
                    Watched = true
                },
                new Movie
                {
                    Title = "The Exorcist",
                    Genre = "Horror",
                    Watched = true
                },
                new Movie
                {
                    Title = "Footloose",
                    Genre = "Musical",
                    Watched = false
                },
                new Movie
                {
                    Title = "Gone Girl",
                    Genre = "Thriller",
                    Watched = true
                },
                new Movie
                {
                    Title = "Evil Dead",
                    Genre = "Horror",
                    Watched = true
                }
            };

            dbContext.Movies.AddRange(movies);
            dbContext.SaveChanges();
        }

        app.UseRouting();
        app.UseCors();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}

public class MovieController : ControllerBase
{
    private readonly TodoDbContext _dbContext;

    public MovieController(TodoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("/movies")]
    public async Task<ActionResult<List<Movie>>> GetMovies()
    {
        var movies = await _dbContext.Movies.ToListAsync();
        return movies;
    }

    [HttpGet("/movies/complete")]
    public async Task<ActionResult<List<Movie>>> GetCompleteMovies()
    {
        var movies = await _dbContext.Movies.Where(m => m.Watched).ToListAsync();
        return movies;
    }

    [HttpGet("/movies/{id}")]
    public async Task<ActionResult<Movie>> GetMovie(int id)
    {
        var movie = await _dbContext.Movies.FindAsync(id);
        if (movie == null)
            return NotFound();

        return movie;
    }

    [HttpGet("/movies/title/{title}")]
    public async Task<ActionResult<string>> GetTitle(string title)
    {
        var movie = await _dbContext.Movies.FirstOrDefaultAsync(m => m.Title == title);
        if (movie == null)
            return NotFound();

        return movie.Title;
    }

    [HttpPost("/movies")]
    public async Task<ActionResult<Movie>> CreateMovie(Movie movie)
    {
        _dbContext.Movies.Add(movie);
        await _dbContext.SaveChangesAsync();

        return Created($"/movies/{movie.Id}", movie);
    }

    [HttpPut("/movies/{id}")]
    public async Task<IActionResult> UpdateMovie(int id, Movie movie)
    {
        var existingMovie = await _dbContext.Movies.FindAsync(id);
        if (existingMovie == null)
            return NotFound();

        existingMovie.Title = movie.Title;
        existingMovie.Watched = movie.Watched;

        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("/movies/{id}")]
    public async Task<ActionResult<Movie>> DeleteMovie(int id)
    {
        var movie = await _dbContext.Movies.FindAsync(id);
        if (movie == null)
            return NotFound();
        _dbContext.Movies.Remove(movie);
        await _dbContext.SaveChangesAsync();

        return movie;
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
Host.CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(webBuilder =>
    {
        webBuilder.UseStartup<Startup>();
    });
}