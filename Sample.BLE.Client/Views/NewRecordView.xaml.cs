using Sample.BLE.Client.ViewModels;
using CommunityToolkit.Maui.Views;

namespace Sample.BLE.Client.Views;

public partial class NewRecordView : Popup
{
	public NewRecordView(NewRecordViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
	}
}
