using Sample.TMLink.Client.ViewModels;
using CommunityToolkit.Maui.Views;

namespace Sample.TMLink.Client.Views;

public partial class NewRecordView : Popup
{
	public NewRecordView(NewRecordViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
	}
}
