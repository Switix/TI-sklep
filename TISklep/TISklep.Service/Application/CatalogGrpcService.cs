using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using TISklep.Service.Grpc;

namespace TISklep.Service.Application;

public class CatalogGrpcService(BoardGamesContext dbContext, ILogger<CatalogGrpcService> logger) : Catalog.CatalogBase
{
    public override async Task<BoardGamesListResponse> GetBoardGames(GetBoardGamesRequest request, ServerCallContext context)
    {
        logger.LogInformation("Pobieranie gier planszowych. Kategoria: {Category}, Sortowanie rosnąco: {SortAsc}", 
            request.Category, request.SortByCategoryAsc);
        
        var query = dbContext.BoardGames.Include(b => b.Category).AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            query = query.Where(b => b.Category.Name.ToLower() == request.Category.ToLower());
        }

        query = request.SortByCategoryAsc
            ? query.OrderBy(b => b.Category.Name).ThenBy(b => b.Title)
            : query.OrderByDescending(b => b.Category.Name).ThenBy(b => b.Title);

        var games = await query.ToListAsync();

        var response = new BoardGamesListResponse();
        
        response.BoardGames.AddRange(games.Select(g => new BoardGamesDto
        {
            Id = g.Id,
            Title = g.Title,
            Category = g.Category.Name,
            Price = (double)g.Price,
            Description = g.Description
        }));

        return response;
    }

    public override async Task<CategoryListResponse> GetCategories(EmptyRequest request, ServerCallContext context)
    {
        var categories = await dbContext.Categories.OrderBy(c => c.Name).ToListAsync();
        
        var response = new CategoryListResponse();
        response.Categories.AddRange(categories.Select(c => new CategoryDto
        {
            Id = c.Id,
            Name = c.Name
        }));

        return response;
    }
}