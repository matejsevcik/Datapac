using Datapac.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Datapac.Endpoints;

public static class UsersEndpoints
{
    public static RouteGroupBuilder MapUsersEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetAllUsers);
        return group;
    }

    public static async Task<Ok<List<User>>> GetAllUsers([FromServices] LoansContext context)
        => TypedResults.Ok(await context.Users.ToListAsync());
}