namespace BudgetFlow.Models;

public sealed record Expense(DateTime Date, string Description, string Category, decimal Amount)
{
    public static Expense Create(DateTime? date, string description, string category, decimal amount)
    {
        if (date is null) throw new ArgumentException("Choisissez une date.");
        if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Saisissez une description.");
        if (description.Trim().Length > 120) throw new ArgumentException("La description est limitée à 120 caractères.");
        if (string.IsNullOrWhiteSpace(category)) throw new ArgumentException("Choisissez une catégorie.");
        if (amount <= 0 || decimal.Round(amount, 2) != amount)
            throw new ArgumentException("Le montant doit être positif, avec deux décimales maximum.");
        return new(date.Value.Date, description.Trim(), category, amount);
    }
}

