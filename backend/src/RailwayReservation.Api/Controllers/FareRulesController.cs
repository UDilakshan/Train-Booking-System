using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RailwayReservation.Api.Dtos;
using RailwayReservation.Application.Common.Exceptions;
using RailwayReservation.Domain.Entities;
using RailwayReservation.Infrastructure.Persistence;

namespace RailwayReservation.Api.Controllers;

[ApiController, Route("admin/fare-rules"), Authorize]
public class FareRulesController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IReadOnlyList<FareRule>> FindAll() =>
        await db.FareRules.OrderBy(r => r.RuleType).ThenByDescending(r => r.Priority).ToListAsync();

    [HttpGet("{id:guid}")]
    public async Task<FareRule> FindOne(Guid id) =>
        await db.FareRules.FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new NotFoundAppException("FARE_RULE_NOT_FOUND", $"Fare rule {id} was not found.");

    [HttpPost]
    public async Task<FareRule> Create(CreateFareRuleRequest request)
    {
        var rule = new FareRule
        {
            Id = Guid.NewGuid(), Name = request.Name, CoachType = request.CoachType, RuleType = request.RuleType, ValueType = request.ValueType,
            Value = request.Value, Priority = request.Priority, IsActive = request.IsActive, EffectiveFrom = request.EffectiveFrom, EffectiveTo = request.EffectiveTo,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
        };
        db.FareRules.Add(rule);
        await db.SaveChangesAsync();
        return rule;
    }

    [HttpPatch("{id:guid}")]
    public async Task<FareRule> Update(Guid id, UpdateFareRuleRequest request)
    {
        var rule = await db.FareRules.FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new NotFoundAppException("FARE_RULE_NOT_FOUND", $"Fare rule {id} was not found.");
        if (request.Name is not null) rule.Name = request.Name;
        if (request.CoachType is not null) rule.CoachType = request.CoachType;
        if (request.RuleType is not null) rule.RuleType = request.RuleType.Value;
        if (request.ValueType is not null) rule.ValueType = request.ValueType.Value;
        if (request.Value is not null) rule.Value = request.Value.Value;
        if (request.Priority is not null) rule.Priority = request.Priority.Value;
        if (request.IsActive is not null) rule.IsActive = request.IsActive.Value;
        if (request.EffectiveFrom is not null) rule.EffectiveFrom = request.EffectiveFrom;
        if (request.EffectiveTo is not null) rule.EffectiveTo = request.EffectiveTo;
        rule.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return rule;
    }

    [HttpDelete("{id:guid}")]
    public async Task<FareRule> Remove(Guid id)
    {
        var rule = await db.FareRules.FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new NotFoundAppException("FARE_RULE_NOT_FOUND", $"Fare rule {id} was not found.");
        db.FareRules.Remove(rule);
        await db.SaveChangesAsync();
        return rule;
    }
}
