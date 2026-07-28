namespace Cygnus.TMLink.API.Maui
{
    public interface IUserDialogService
    {
        Task ShowMessage(string message, string cancel = "");
    }
}