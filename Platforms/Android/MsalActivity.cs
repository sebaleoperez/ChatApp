using Android.App;
using Android.Content;
using Android.Webkit;
using Microsoft.Identity.Client;

namespace ChatApp.Platforms.Android;

[Activity(Exported = true)]
[IntentFilter(new[] { Intent.ActionView },
        Categories = new[] { Intent.CategoryBrowsable, Intent.CategoryDefault },
        DataHost = "auth",
        DataScheme = "msald4918bf7-df0c-459e-b360-c5aa1d86e197")]
public class MsalActivity : BrowserTabActivity
{

}
