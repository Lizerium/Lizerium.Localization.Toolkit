/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 04 мая 2026 06:52:49
 * Version: 1.0.7
 */

using System;
using System.Runtime.InteropServices;
using System.Threading;

using Microsoft.VisualStudio.Shell;

namespace Lizerium.Localization.Xaml.Vsix;

[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
[Guid(PackageGuidString)]
[ProvideOptionPage(
    typeof(LizeriumLocalizationOptionsPage),
    "Lizerium Localization",
    "AI Servers",
    0,
    0,
    true)]
public sealed class LizeriumLocalizationPackage : AsyncPackage
{
    public const string PackageGuidString = "89076764-3A19-4396-9C01-91F08A5B4D4F";

    protected override System.Threading.Tasks.Task InitializeAsync(
        CancellationToken cancellationToken,
        IProgress<ServiceProgressData> progress)
    {
        return base.InitializeAsync(cancellationToken, progress);
    }
}
