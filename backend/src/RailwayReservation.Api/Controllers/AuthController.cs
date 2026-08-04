using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RailwayReservation.Api.Dtos;
using RailwayReservation.Application.Auth;

namespace RailwayReservation.Api.Controllers;

[ApiController, Route("auth")]
public class AuthController(LoginUseCase loginUseCase) : ControllerBase
{
    [HttpPost("login")]
    public Task<LoginResult> Login(LoginRequest request) => loginUseCase.ExecuteAsync(request.Email, request.Password);

    [HttpPost("me"), Authorize]
    public object Me() => new
    {
        id = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub"),
        email = User.FindFirstValue(ClaimTypes.Email),
        name = User.FindFirstValue("name"),
        role = User.FindFirstValue(ClaimTypes.Role),
    };
}
