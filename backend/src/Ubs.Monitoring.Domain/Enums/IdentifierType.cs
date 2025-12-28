namespace Ubs.Monitoring.Domain.Enums;

public enum IdentifierType
{
    CPF = 0,
    CNPJ = 1,
    TAX_ID = 2,
    PASSPORT = 3,
    LEI = 4,

    // Routing identifiers (globally unique)
    PIX_EMAIL = 5,
    PIX_PHONE = 6,
    PIX_RANDOM = 7,
    IBAN = 8,

    OTHER = 9
}
