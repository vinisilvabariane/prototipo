using IrisWeb.Domain.Entities;
using IrisWeb.Domain.Interfaces;
using IrisWeb.Infrastructure.Data.Context;

namespace IrisWeb.Infrastructure.Repositories;

public sealed class EnrollmentConfirmationRepository(IrisWebDbContext context)
    : Repository<EnrollmentConfirmation, long>(context), IEnrollmentConfirmationRepository
{
}
