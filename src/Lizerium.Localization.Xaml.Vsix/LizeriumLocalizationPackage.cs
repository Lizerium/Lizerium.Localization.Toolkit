/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 22 июля 2026 12:56:33
 * Version: 1.0.97
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
