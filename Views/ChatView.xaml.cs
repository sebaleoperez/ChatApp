using ChatApp.ViewModels;
using Microsoft.Datasync.Client;
using Microsoft.Identity.Client;
using System.Diagnostics;

namespace ChatApp.Views;

public partial class ChatView : ContentPage
{
    public IPublicClientApplication IdentityClient { get; set; }

    public ChatView()
	{
		InitializeComponent();
    }

    void Button_Clicked(System.Object sender, System.EventArgs e)
    {
        if (!string.IsNullOrEmpty(txtMessage.Text))
        {
            ((ChatViewModel)this.BindingContext).AddMessage(txtMessage.Text);
            txtMessage.IsEnabled = false;
            txtMessage.Text = string.Empty;
            txtMessage.IsEnabled = true;
        }
    }

    async void Button_Clean(System.Object sender, System.EventArgs e)
    {
        bool response = await DisplayAlert("Confirmar", "¿Deseas eliminar el historial?", "Aceptar", "Cancelar");
        if (response)
        {
            btnSend.IsEnabled = false;
            await ((ChatViewModel)this.BindingContext).Reset();
            btnSend.IsEnabled = true;
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var token = await this.GetAuthenticationToken();

        await ((ChatViewModel)this.BindingContext).Initialize();
        btnSend.IsEnabled = true;
    }

    async void Button_Save(System.Object sender, System.EventArgs e)
    {
        await ((ChatViewModel)this.BindingContext).Persist();

        await DisplayAlert("Guardado", "Chat guardado", "Aceptar");
    }

    public async Task<AuthenticationToken> GetAuthenticationToken()
    {
        if (IdentityClient == null)
        {
#if ANDROID
            IdentityClient = PublicClientApplicationBuilder
                .Create(Constants.ApplicationId)
                .WithAuthority(AzureCloudInstance.AzurePublic, "common")
                .WithRedirectUri($"msal{Constants.ApplicationId}://auth")
                .WithParentActivityOrWindow(() => Platform.CurrentActivity)
                .Build();
#elif IOS
        IdentityClient = PublicClientApplicationBuilder
            .Create(Constants.ApplicationId)
            .WithAuthority(AzureCloudInstance.AzurePublic, "common")
            .WithIosKeychainSecurityGroup("com.microsoft.adalcache")
            .WithRedirectUri($"msal{Constants.ApplicationId}://auth")
            .Build();
#else
        IdentityClient = PublicClientApplicationBuilder
            .Create(Constants.ApplicationId)
            .WithAuthority(AzureCloudInstance.AzurePublic, "common")
            .WithRedirectUri("https://login.microsoftonline.com/common/oauth2/nativeclient")
            .Build();
#endif
        }

        var accounts = await IdentityClient.GetAccountsAsync();
        AuthenticationResult result = null;
        bool tryInteractiveLogin = false;

        try
        {
            result = await IdentityClient
                .AcquireTokenSilent(Constants.Scopes, accounts.FirstOrDefault())
                .ExecuteAsync();
        }
        catch (MsalUiRequiredException)
        {
            tryInteractiveLogin = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"MSAL Silent Error: {ex.Message}");
        }

        if (tryInteractiveLogin)
        {
            try
            {
                result = await IdentityClient
                    .AcquireTokenInteractive(Constants.Scopes)
                    .ExecuteAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MSAL Interactive Error: {ex.Message}");
            }
        }

        return new AuthenticationToken
        {
            DisplayName = result?.Account?.Username ?? "",
            ExpiresOn = result?.ExpiresOn ?? DateTimeOffset.MinValue,
            Token = result?.AccessToken ?? "",
            UserId = result?.Account?.Username ?? ""
        };
    }

}
