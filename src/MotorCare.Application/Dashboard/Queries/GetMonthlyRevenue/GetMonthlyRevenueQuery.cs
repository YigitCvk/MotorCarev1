using MediatR;
using MotorCare.Application.Common.Interfaces;

namespace MotorCare.Application.Dashboard.Queries.GetMonthlyRevenue;

public sealed record GetMonthlyRevenueQuery : IRequest<List<MonthlyRevenueStat>>;
