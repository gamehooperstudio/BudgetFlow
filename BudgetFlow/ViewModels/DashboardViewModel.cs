using System.Globalization;
using System.Collections.ObjectModel;

namespace BudgetFlow.ViewModels;

public sealed class DashboardViewModel : ObservableViewModel
{
    public ObservableCollection<TransactionRow> Transactions { get; } =
    [
        new(new(2024, 5, 31), "Salaire", "Salaire", 2000m),
        new(new(2024, 5, 30), "Courses supermarché", "Alimentation", -85.45m),
        new(new(2024, 5, 29), "Essence", "Transport", -45m),
        new(new(2024, 5, 28), "Restaurant", "Loisirs", -32.50m),
        new(new(2024, 5, 27), "Freelance", "Freelance", 450m),
        new(new(2024, 5, 26), "Pharmacie", "Santé", -18.90m),
        new(new(2024, 5, 25), "Loyer", "Logement", -800m),
        new(new(2024, 5, 24), "Courses et achats", "Alimentation", -223.65m),
        new(new(2024, 5, 23), "Abonnements", "Autres", -120m)
    ];

    public IncomeViewModel Income { get; }
    public ExpenseViewModel Expense { get; }
    public DashboardViewModel()
    {
        Income = new IncomeViewModel(Transactions);
        Expense = new ExpenseViewModel(Transactions);
        Transactions.CollectionChanged += (_, _) =>
        {
            Notify(nameof(TotalIncome)); Notify(nameof(TotalExpenses)); Notify(nameof(Balance));
        };
    }
    // Une seule source pour les totaux des cartes et du résumé.
    public string TotalIncome => Money(Transactions.Where(t => t.Amount > 0).Sum(t => t.Amount));
    public string TotalExpenses => Money(-Transactions.Where(t => t.Amount < 0).Sum(t => t.Amount));
    public string Balance => Money(Transactions.Sum(t => t.Amount));
    internal static string Money(decimal amount) => amount.ToString("C2", CultureInfo.GetCultureInfo("fr-FR"));
}

public sealed record TransactionRow(DateTime Date, string Description, string Category, decimal Amount)
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Type => Amount >= 0 ? "Revenu" : "Dépense";
    public string DisplayAmount => DashboardViewModel.Money(Amount);
}


