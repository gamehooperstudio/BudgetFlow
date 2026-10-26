using BudgetFlow.ViewModels;
using BudgetFlow.Models;
void Check(bool condition, string name) { if (!condition) throw new Exception(name); Console.WriteLine("PASS: " + name); }
var dashboard = new DashboardViewModel();
var vm = dashboard.Income;
var original = dashboard.Transactions.Sum(x => x.Amount);
int changes = 0;
dashboard.PropertyChanged += (_, _) => changes++;
vm.Description = "Projet test"; vm.Amount = "1250,50";
vm.SaveCommand.Execute(null);
Check(vm.Incomes.Count == 3 && dashboard.Transactions.Sum(x => x.Amount) == original + 1250.50m && changes == 3, "Ajout et totaux synchronises");
vm.SelectedIncome = vm.Incomes.Single(x => x.Description == "Projet test");
vm.Amount = "750.25"; vm.SaveCommand.Execute(null);
Check(vm.Incomes.Count == 3 && dashboard.Transactions.Sum(x => x.Amount) == original + 750.25m, "Modification sans doublon");
foreach (var invalid in new[] { "0", "-12", "1,234", "abc" })
{
    vm.Description = "Invalide"; vm.Amount = invalid; vm.SaveCommand.Execute(null);
    Check(vm.Incomes.Count == 3, "Refus montant " + invalid);
}
vm.Description = " "; vm.Amount = "10"; vm.SaveCommand.Execute(null);
Check(vm.Incomes.Count == 3, "Description obligatoire");
vm.Description = "Test"; vm.Date = null; vm.SaveCommand.Execute(null);
Check(vm.Incomes.Count == 3, "Date obligatoire");
vm.SelectedIncome = vm.Incomes.Single(x => x.Description == "Projet test");
vm.DeleteCommand.Execute(null);
Check(vm.Incomes.Count == 2 && dashboard.Transactions.Sum(x => x.Amount) == original, "Suppression et restauration du solde");
foreach (var row in vm.Incomes.ToArray()) { vm.SelectedIncome = row; vm.DeleteCommand.Execute(null); }
Check(vm.Incomes.Count == 0 && vm.Total == DashboardMoney(0) && vm.Average == DashboardMoney(0), "Etat vide et moyenne nulle");
string DashboardMoney(decimal value) => value.ToString("C2", System.Globalization.CultureInfo.GetCultureInfo("fr-FR"));
var uiThread = new System.Threading.Thread(() =>
{
    try
    {
        var window = new BudgetFlow.MainWindow();
        window.Opacity = 0;
        window.ShowActivated = false;
        window.ShowInTaskbar = false;
        window.Show();
        window.Measure(new System.Windows.Size(1440, 860));
        window.Arrange(new System.Windows.Rect(0, 0, 1440, 860));
        window.UpdateLayout();
        Check(window.FindName("IncomeNav") is System.Windows.Controls.RadioButton, "Chargement WPF et ressources");
        var nav = (System.Windows.Controls.RadioButton)window.FindName("IncomeNav");
        nav.IsChecked = true;
        System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.ContextIdle);
        window.UpdateLayout();
        var view = Descendants<BudgetFlow.Views.IncomeView>(window).Single();
        Check(view.Visibility == System.Windows.Visibility.Visible, "Navigation vers Revenus");
        var model = ((DashboardViewModel)window.DataContext).Income;
        var datePicker = (System.Windows.Controls.DatePicker)view.FindName("IncomeDatePicker");
        Check(datePicker.SelectedDate == DateTime.Today, "Date initiale francaise");
        datePicker.SetCurrentValue(System.Windows.Controls.DatePicker.TextProperty, "31/02/2026");
        datePicker.GetBindingExpression(System.Windows.Controls.DatePicker.TextProperty)?.UpdateSource();
        model.Description = "Date invalide"; model.Amount = "10";
        model.SaveCommand.Execute(null);
        Check(model.Incomes.Count == 2 && model.Date == null, "Date invalide refusee depuis le controle WPF");
        datePicker.SetCurrentValue(System.Windows.Controls.DatePicker.TextProperty, "23/10/2026");
        datePicker.GetBindingExpression(System.Windows.Controls.DatePicker.TextProperty)?.UpdateSource();
        model.SaveCommand.Execute(null);
        Check(model.Incomes.Count == 3 && model.Incomes.Any(x => x.Date == new DateTime(2026, 10, 23)), "Date francaise valide enregistree");
        ((System.Windows.Controls.RadioButton)window.FindName("DashboardNav")).IsChecked = true;
        System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.ContextIdle);
        Check(view.Visibility == System.Windows.Visibility.Collapsed, "Retour au tableau de bord");
        var expenseNav = (System.Windows.Controls.RadioButton)window.FindName("ExpenseNav");
        expenseNav.IsChecked = true;
        System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.ContextIdle);
        window.UpdateLayout();
        var expenseView = Descendants<BudgetFlow.Views.ExpenseView>(window).Single();
        Check(expenseView.IsVisible && !view.IsVisible, "Navigation vers Depenses");
        var expenseDate = (System.Windows.Controls.DatePicker)expenseView.FindName("ExpenseDatePicker");
        Check(expenseDate.SelectedDate == DateTime.Today, "Date initiale depense");
        ((System.Windows.Controls.RadioButton)window.FindName("DashboardNav")).IsChecked = true;
        System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.ContextIdle);
        Check(!expenseView.IsVisible, "Retour depuis Depenses");
        window.Close();
    }
    catch (Exception ex) { Console.Error.WriteLine(ex); Environment.ExitCode = 1; }
});
uiThread.SetApartmentState(System.Threading.ApartmentState.STA);
uiThread.Start(); uiThread.Join();

var duplicateDashboard = new DashboardViewModel();
var first = new TransactionRow(DateTime.Today, "Identique", "Salaire", 10m);
var second = new TransactionRow(DateTime.Today, "Identique", "Salaire", 10m);
duplicateDashboard.Transactions.Add(first); duplicateDashboard.Transactions.Add(second);
duplicateDashboard.Income.SelectedIncome = second;
duplicateDashboard.Income.Amount = "20";
duplicateDashboard.Income.SaveCommand.Execute(null);
Check(duplicateDashboard.Transactions.Any(x => ReferenceEquals(x, first)) && !duplicateDashboard.Transactions.Any(x => ReferenceEquals(x, second)), "Modification du bon revenu parmi deux identiques");
duplicateDashboard.Income.SelectedIncome = first;
duplicateDashboard.Transactions.Add(new(DateTime.Today, "Autre", "Salaire", 30m));
Check(ReferenceEquals(duplicateDashboard.Income.SelectedIncome, first), "Selection conservee apres un ajout externe");
duplicateDashboard.Income.NewCommand.Execute(null);
var countBeforeOverflow = duplicateDashboard.Transactions.Count;
duplicateDashboard.Income.Description = "Montant enorme";
duplicateDashboard.Income.Amount = decimal.MaxValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
duplicateDashboard.Income.SaveCommand.Execute(null);
Check(duplicateDashboard.Transactions.Count == countBeforeOverflow && duplicateDashboard.Income.Message.Contains("capacité"), "Depassement des totaux refuse avant mutation");

var expenseDashboard = new DashboardViewModel();
var expenses = expenseDashboard.Expense;
var initialBalance = expenseDashboard.Transactions.Sum(x => x.Amount);
var initialCount = expenses.Expenses.Count;
expenses.Description = "Courses test"; expenses.Amount = "42,50";
expenses.SaveCommand.Execute(null);
Check(expenses.Expenses.Count == initialCount + 1 && expenseDashboard.Transactions.Sum(x => x.Amount) == initialBalance - 42.50m, "Depense ajoutee et solde debite");
expenses.SelectedExpense = expenses.Expenses.Single(x => x.Description == "Courses test");
var expenseId = expenses.SelectedExpense.Id;
Check(expenses.Amount == "42,50", "Edition avec montant positif");
expenses.Amount = "30.25"; expenses.SaveCommand.Execute(null);
Check(expenses.Expenses.Count == initialCount + 1 && expenses.Expenses.Single(x => x.Id == expenseId).Amount == -30.25m, "Modification depense sans doublon");
foreach (var invalid in new[] { "0", "-12", "1,234", "abc", decimal.MaxValue.ToString() })
{
    expenses.Description = "Invalide"; expenses.Amount = invalid; expenses.SaveCommand.Execute(null);
    Check(expenses.Expenses.Count == initialCount + 1, "Refus depense invalide " + invalid);
}
expenses.NewCommand.Execute(null); expenses.Description = "Test"; expenses.Amount = "10";
expenses.DateText = "31/02/2026"; expenses.SaveCommand.Execute(null);
Check(expenses.Expenses.Count == initialCount + 1 && expenses.Date == null, "Date depense invalide refusee");
expenses.Date = DateTime.Today; expenses.Category = "Inconnue"; expenses.SaveCommand.Execute(null);
Check(expenses.Expenses.Count == initialCount + 1, "Categorie depense invalide refusee");
expenses.SelectedExpense = expenses.Expenses.Single(x => x.Id == expenseId);
expenses.DeleteCommand.Execute(null);
Check(expenseDashboard.Transactions.Sum(x => x.Amount) == initialBalance, "Suppression depense restaure le solde");
foreach (var row in expenses.Expenses.ToArray()) { expenses.SelectedExpense = row; expenses.DeleteCommand.Execute(null); }
Check(expenses.Total == DashboardMoney(0) && expenses.Average == DashboardMoney(0), "Depenses vides et moyenne nulle");
static IEnumerable<T> Descendants<T>(System.Windows.DependencyObject root) where T : System.Windows.DependencyObject
{
    for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(root); i++)
    {
        var child = System.Windows.Media.VisualTreeHelper.GetChild(root, i);
        if (child is T match) yield return match;
        foreach (var descendant in Descendants<T>(child)) yield return descendant;
    }
}



