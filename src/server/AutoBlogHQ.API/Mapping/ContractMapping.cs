using AutoBlogHQ.Application.Models;
using AutoBlogHQ.Contracts.Requests.Identity;

namespace AutoBlogHQ.API.Mapping;

public static class ContractMapping
{
    public static ApplicationUser MapToApplicationUser(this RegisterRequest request)
    {
        return new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = request.Email,
            UserName = request.UserName
        };
    }
}