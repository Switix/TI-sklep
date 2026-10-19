using System.Security.Claims;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using TISklep.Service.Grpc;
using TISklep.Service.Model;

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
            Description = g.Description,
            Stock = g.Stock
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
    public override async Task<PurchaseResponse> PurchaseBoardGames(PurchaseRequest request, ServerCallContext context)
    {
        if (request.Items.Count == 0)
        {
            return new PurchaseResponse { Success = false, Message = "Koszyk jest pusty." };
        }
        
        var user = await dbContext.GetOrCreateUserAsync(context.GetHttpContext().User);
        
        var gameIds = request.Items.Select(i => i.BoardGameId).Distinct().ToList();
        var gamesInDb = await dbContext.BoardGames
            .Where(g => gameIds.Contains(g.Id))
            .ToDictionaryAsync(g => g.Id);

        var order = new Order
        {
            User = user,
            CreatedAt = DateTime.UtcNow
        };
        
        foreach (var item in request.Items)
        {
            if (item.Quantity <= 0)
            {
                return new PurchaseResponse { Success = false, Message = "Ilość musi być większa od zera." };
            }

            if (!gamesInDb.TryGetValue(item.BoardGameId, out var game))
            {
                return new PurchaseResponse { Success = false, Message = $"Nie znaleziono gry o ID {item.BoardGameId}." };
            }

            if (game.Stock < item.Quantity)
            {
                return new PurchaseResponse 
                { 
                    Success = false, 
                    Message = $"Brak wystarczającej ilości gry '{game.Title}'. Dostępne: {game.Stock} szt." 
                };
            }

            game.Stock -= item.Quantity;

            order.Items.Add(new OrderItem
            {
                BoardGameId = game.Id,
                Quantity = item.Quantity,
                UnitPrice = game.Price
            });

            order.TotalPrice += game.Price * item.Quantity;
        }

        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync();

        logger.LogInformation("Użytkownik {Username} ({KeycloakId}) złożył zamówienie #{OrderId} na kwotę {Total} zł.",
            user.Username, user.KeycloakId, order.Id, order.TotalPrice);

        return new PurchaseResponse
        {
            Success = true,
            Message = "Zakup zakończony pomyślnie!",
            OrderId = order.Id
        };
    }
}