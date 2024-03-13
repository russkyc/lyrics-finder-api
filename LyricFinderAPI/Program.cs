// MIT License
// 
// Copyright (c) 2023 Russell Camo (Russkyc)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using LyricFinderAPI.Models;
using LyricFinderAPI.Services;
using LyricsScraperNET;
using LyricsScraperNET.Models.Requests;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddLyricScraper();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/lyrics/{providers}/{artist}/{song}",
    async (
        ILyricsScraperClient client,
        string providers,
        string artist,
        string song) =>
    {
        if (providers.Contains("azlyrics"))
        {
            client.WithAZLyrics();
        }

        if (providers.Contains("genius"))
        {
            client.WithGenius();
        }

        if (providers.Contains("musixmatch"))
        {
            client.WithMusixmatch();
        }

        if (providers.Contains("lyricfind"))
        {
            client.WithLyricFind();
        }

        if (providers.Contains("songlyrics"))
        {
            client.WithSongLyrics();
        }

        var request = new ArtistAndSongSearchRequest(artist: artist, song: song);
        var searchResult = await client.SearchLyricAsync(request);

        if (searchResult.IsEmpty())
        {
            return Results.NotFound();
        }

        var lyricInfo = new LyricInfo
        {
            Title = request.Song,
            Author = request.Artist,
            Lyric = searchResult.LyricText
        };

        return Results.Ok(lyricInfo);
    });

app.MapGet("/lyrics/{artist}/{song}",
    async (
        ILyricsScraperClient client,
        string artist,
        string song) =>
    {
        client.WithAllProviders();

        var request = new ArtistAndSongSearchRequest(artist: artist, song: song);
        var searchResult = await client.SearchLyricAsync(request);

        if (searchResult.IsEmpty())
        {
            return Results.NotFound();
        }

        var lyricInfo = new LyricInfo
        {
            Title = request.Song,
            Author = request.Artist,
            Lyric = searchResult.LyricText
        };

        return Results.Ok(lyricInfo);
    });

app.MapPost("/lyrics",
    async (
        ILyricsScraperClient client,
        ArtistAndSongSearchRequest request) =>
    {
        client.WithAZLyrics()
            .WithGenius()
            .WithMusixmatch();

        var searchResult = await client.SearchLyricAsync(request);

        if (searchResult.IsEmpty())
        {
            return Results.NotFound();
        }

        var lyricInfo = new LyricInfo
        {
            Title = request.Song,
            Author = request.Artist,
            Lyric = searchResult.LyricText
        };

        return Results.Ok(lyricInfo);
    });

app.Run();