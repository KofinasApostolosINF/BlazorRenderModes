using BlazorRenderModes.Client.Models;
using Microsoft.AspNetCore.Mvc;
using BlazorRenderModes.Components;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

string tmdbApiKey = "b2b90f9e1a4b1b08f3e57cd4db272957";
string tmdbReadAccessToken = "eyJhbGciOiJIUzI1NiJ9.eyJhdWQiOiJiMmI5MGY5ZTFhNGIxYjA4ZjNlNTdjZDRkYjI3Mjk1NyIsInN1YiI6IjY1ZTVjYjBhNDRhNDI0MDE2MzExOGM4ZiIsInNjb3BlcyI6WyJhcGlfcmVhZCJdLCJ2ZXJzaW9uIjoxfQ.dEPRMt7FykKb0iKPfDc5TJduRzQMxxkP5EthF4JXwkA";

builder.Services.AddScoped(sp => {
    var client = new HttpClient();
    client.BaseAddress = new Uri("https://api.themoviedb.org/3/");
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tmdbReadAccessToken);

    return client;
});

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BlazorRenderModes.Client._Imports).Assembly);


// Minimal API endpoints to proxy requests to TMDB for our client pages
// this way, our TMDB API key is never visible to the client

app.MapGet("/movie/popular", async ([FromServices] HttpClient http) =>
{
    PopularMovieResponse? response = await http.GetFromJsonAsync<PopularMovieResponse>("movie/popular");

    return response is not null ? Results.Ok(response) : Results.Problem();
});

app.MapGet("/movie/{id}", async ([FromServices] HttpClient http, int? id) =>
{
    if (id.HasValue)
    {
        MovieDetails? response = await http.GetFromJsonAsync<MovieDetails?>($"movie/{id.Value}");

        return Results.Ok(response);
    }

    return Results.BadRequest();
});

app.Run();
