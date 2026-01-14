using Microsoft.EntityFrameworkCore;
using Ubs.Monitoring.Domain.Entities;
using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Infrastructure.Persistence.Seed;

public static class CountrySeed
{
    public static void SeedCountries(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Country>().HasData(
            // Americas
            new { Code = "AR", Name = "Argentina", RiskLevel = RiskLevel.Low },
            new { Code = "BR", Name = "Brazil", RiskLevel = RiskLevel.Low },
            new { Code = "CA", Name = "Canada", RiskLevel = RiskLevel.Low },
            new { Code = "CL", Name = "Chile", RiskLevel = RiskLevel.Low },
            new { Code = "CO", Name = "Colombia", RiskLevel = RiskLevel.Medium },
            new { Code = "MX", Name = "Mexico", RiskLevel = RiskLevel.Low },
            new { Code = "US", Name = "United States", RiskLevel = RiskLevel.Low },
            new { Code = "UY", Name = "Uruguay", RiskLevel = RiskLevel.Low },
            new { Code = "VE", Name = "Venezuela", RiskLevel = RiskLevel.High },
            new { Code = "PE", Name = "Peru", RiskLevel = RiskLevel.Low },
            new { Code = "EC", Name = "Ecuador", RiskLevel = RiskLevel.Low },
            new { Code = "BO", Name = "Bolivia", RiskLevel = RiskLevel.Low },

            // Europe
            new { Code = "AT", Name = "Austria", RiskLevel = RiskLevel.Low },
            new { Code = "BE", Name = "Belgium", RiskLevel = RiskLevel.Low },
            new { Code = "CH", Name = "Switzerland", RiskLevel = RiskLevel.Low },
            new { Code = "DE", Name = "Germany", RiskLevel = RiskLevel.Low },
            new { Code = "DK", Name = "Denmark", RiskLevel = RiskLevel.Low },
            new { Code = "ES", Name = "Spain", RiskLevel = RiskLevel.Low },
            new { Code = "FI", Name = "Finland", RiskLevel = RiskLevel.Low },
            new { Code = "FR", Name = "France", RiskLevel = RiskLevel.Low },
            new { Code = "GB", Name = "United Kingdom", RiskLevel = RiskLevel.Low },
            new { Code = "GR", Name = "Greece", RiskLevel = RiskLevel.Low },
            new { Code = "IE", Name = "Ireland", RiskLevel = RiskLevel.Low },
            new { Code = "IT", Name = "Italy", RiskLevel = RiskLevel.Low },
            new { Code = "NL", Name = "Netherlands", RiskLevel = RiskLevel.Low },
            new { Code = "NO", Name = "Norway", RiskLevel = RiskLevel.Low },
            new { Code = "PL", Name = "Poland", RiskLevel = RiskLevel.Low },
            new { Code = "PT", Name = "Portugal", RiskLevel = RiskLevel.Low },
            new { Code = "RU", Name = "Russia", RiskLevel = RiskLevel.High },
            new { Code = "SE", Name = "Sweden", RiskLevel = RiskLevel.Low },

            // Asia
            new { Code = "CN", Name = "China", RiskLevel = RiskLevel.Medium },
            new { Code = "IN", Name = "India", RiskLevel = RiskLevel.Low },
            new { Code = "ID", Name = "Indonesia", RiskLevel = RiskLevel.Low },
            new { Code = "JP", Name = "Japan", RiskLevel = RiskLevel.Low },
            new { Code = "KR", Name = "South Korea", RiskLevel = RiskLevel.Low },
            new { Code = "MY", Name = "Malaysia", RiskLevel = RiskLevel.Low },
            new { Code = "PH", Name = "Philippines", RiskLevel = RiskLevel.Medium },
            new { Code = "SG", Name = "Singapore", RiskLevel = RiskLevel.Low },
            new { Code = "TH", Name = "Thailand", RiskLevel = RiskLevel.Low },
            new { Code = "VN", Name = "Vietnam", RiskLevel = RiskLevel.Low },
            new { Code = "KP", Name = "North Korea", RiskLevel = RiskLevel.High },
            new { Code = "IR", Name = "Iran", RiskLevel = RiskLevel.High },

            // Middle East
            new { Code = "AE", Name = "United Arab Emirates", RiskLevel = RiskLevel.Low },
            new { Code = "IL", Name = "Israel", RiskLevel = RiskLevel.Low },
            new { Code = "SA", Name = "Saudi Arabia", RiskLevel = RiskLevel.Low },
            new { Code = "SY", Name = "Syria", RiskLevel = RiskLevel.High },

            // Africa
            new { Code = "EG", Name = "Egypt", RiskLevel = RiskLevel.Low },
            new { Code = "ZA", Name = "South Africa", RiskLevel = RiskLevel.Low },
            new { Code = "NG", Name = "Nigeria", RiskLevel = RiskLevel.Medium },
            new { Code = "KE", Name = "Kenya", RiskLevel = RiskLevel.Low },

            // Oceania
            new { Code = "AU", Name = "Australia", RiskLevel = RiskLevel.Low },
            new { Code = "NZ", Name = "New Zealand", RiskLevel = RiskLevel.Low }
        );
    }
}
