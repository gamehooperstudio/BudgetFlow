using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using System.Text.RegularExpressions;
using BudgetFlow.Models;

namespace BudgetFlow.ViewModels;

public sealed class ExpenseViewModel : ObservableViewModel
{
    private readonly ObservableCollection<TransactionRow> transactions;
    private TransactionRow? selectedExpense;
    private DateTime? date = DateTime.Today;
    private string dateText = DateTime.Today.ToString("dd/MM/yyyy", CultureInfo.GetCultureInfo("fr-FR"));
    private string description = "", category = "Alimentation", amount = "", message = "";
    public ExpenseViewModel(ObservableCollection<TransactionRow> transactions)
    {
        this.transactions = transactions;
        SaveCommand = new RelayCommand(Save);
        NewCommand = new RelayCommand(Reset);
        DeleteCommand = new RelayCommand(Delete);
        transactions.CollectionChanged += (_, _) => Refresh();
        Refresh();
    }
    public ObservableCollection<TransactionRow> Expenses { get; } = [];
    public string[] Categories { get; } = ["Alimentation", "Logement", "Transport", "Loisirs", "Santé", "Abonnements", "Autres"];
    public ICommand SaveCommand { get; }
    public ICommand NewCommand { get; }
    public ICommand DeleteCommand { get; }
    public DateTime? Date
    {
        get => date;
        set
        {
            Set(ref date, value);
            DateText = value?.ToString("dd/MM/yyyy", CultureInfo.GetCultureInfo("fr-FR")) ?? "";
        }
    }
    public string DateText
    {
        get => dateText;
        set
        {
            Set(ref dateText, value);
            date = DateTime.TryParse(value, CultureInfo.GetCultureInfo("fr-FR"), DateTimeStyles.None, out var parsed)
                ? parsed.Date : null;
            Notify(nameof(Date));
        }
    }
    public string Description { get => description; set => Set(ref description, value); }
    public string Category { get => category; set => Set(ref category, value); }
    public string Amount { get => amount; set => Set(ref amount, value); }
    public string Message { get => message; set => Set(ref message, value); }
    public string Total => DashboardViewModel.Money(-Expenses.Sum(x => x.Amount));
    public string Average => DashboardViewModel.Money(Expenses.Count == 0 ? 0 : -Expenses.Average(x => x.Amount));
    public string FormTitle => SelectedExpense is null ? "Ajouter une dépense" : "Modifier la dépense";
    public bool HasSelection => SelectedExpense is not null;
    public TransactionRow? SelectedExpense
    {
        get => selectedExpense;
        set
        {
            if (ReferenceEquals(selectedExpense, value)) return;
            selectedExpense = value;
            Notify();
            Notify(nameof(FormTitle));
            Notify(nameof(HasSelection));
            if (value is null) return;
            Date = value.Date; Description = value.Description; Category = value.Category;
            Amount = (-value.Amount).ToString("0.00", CultureInfo.GetCultureInfo("fr-FR"));
            Message = "";
        }
    }
    private void Refresh()
    {
        var rows = transactions.Where(x => x.Amount < 0).OrderByDescending(x => x.Date).ToArray();
        for (var i = Expenses.Count - 1; i >= 0; i--)
            if (!rows.Any(row => ReferenceEquals(row, Expenses[i]))) Expenses.RemoveAt(i);
        for (var i = 0; i < rows.Length; i++)
        {
            var currentIndex = Expenses.IndexOf(rows[i]);
            if (currentIndex < 0) Expenses.Insert(i, rows[i]);
            else if (currentIndex != i) Expenses.Move(currentIndex, i);
        }
        if (SelectedExpense is not null && !rows.Any(row => ReferenceEquals(row, SelectedExpense))) Reset();
        Notify(nameof(Total)); Notify(nameof(Average));
    }
    private void Save()
    {
        var normalized = Amount.Trim().Replace(',', '.');
        if (!Regex.IsMatch(normalized, @"^[+-]?\d+(?:\.\d{1,2})?$") ||
            !decimal.TryParse(normalized, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
            CultureInfo.InvariantCulture, out var parsed))
        { Message = "Saisissez un montant valide (ex. : 1250,50)."; return; }
        var fractionalPart = normalized.Contains('.') ? normalized.Split('.')[1].PadRight(2, '0') : "00";
        if (decimal.Abs(parsed % 1).ToString("F2", CultureInfo.InvariantCulture).Split('.')[1] != fractionalPart)
        { Message = "Le montant est trop grand pour conserver ses centimes exactement."; return; }
        try
        {
            if (!Categories.Contains(Category)) throw new ArgumentException("Choisissez une catégorie proposée.");
            var expense = Expense.Create(Date, Description, Category, parsed);
            var row = new TransactionRow(expense.Date, expense.Description, expense.Category, -expense.Amount)
            {
                Id = SelectedExpense?.Id ?? Guid.NewGuid()
            };
            var index = SelectedExpense is null ? -1 : transactions.IndexOf(SelectedExpense);
            if (SelectedExpense is not null && index < 0)
                throw new ArgumentException("Cette dépense n’existe plus. Cliquez sur Nouveau pour en ajouter une.");
            // Vérifier les totaux avant de modifier les données et de notifier les vues.
            var projected = transactions.Where((_, i) => i != index).Append(row).ToArray();
            _ = -projected.Where(x => x.Amount < 0).Sum(x => x.Amount);
            _ = projected.Sum(x => x.Amount);
            if (index >= 0) transactions[index] = row; else transactions.Add(row);
            Reset(); Message = "Dépense enregistrée pour cette session.";
        }
        catch (ArgumentException ex) { Message = ex.Message; }
        catch (OverflowException) { Message = "Le montant dépasse la capacité de calcul des totaux."; }
    }
    private void Delete()
    {
        if (SelectedExpense is null) return;
        transactions.Remove(SelectedExpense);
        Reset(); Message = "Dépense supprimée pour cette session.";
    }
    private void Reset()
    {
        SelectedExpense = null; Date = DateTime.Today; Description = ""; Category = "Alimentation"; Amount = ""; Message = "";
    }
}


