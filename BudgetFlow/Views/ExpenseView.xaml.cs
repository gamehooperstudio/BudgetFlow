using System.Windows.Controls;
namespace BudgetFlow.Views;
public partial class ExpenseView : UserControl
{
    public ExpenseView() => InitializeComponent();

    private void DatePicker_DateValidationError(object sender, DatePickerDateValidationErrorEventArgs e)
    {
        e.ThrowException = false;
        if (DataContext is BudgetFlow.ViewModels.ExpenseViewModel viewModel)
        {
            // Le DatePicker restaure sinon sa dernière date valide après une saisie erronée.
            ((DatePicker)sender).SetCurrentValue(DatePicker.SelectedDateProperty, null);
            viewModel.Date = null;
            viewModel.Message = "Saisissez une date valide au format jj/mm/aaaa.";
        }
    }
}

