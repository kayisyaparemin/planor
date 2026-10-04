namespace Mizan.Architecture.Tests;

/// <summary>
/// CI iş akışlarının ve kapsam betiğinin S80'deki sözleri tuttuğunu sabitler (K2b).
/// Kaynak dosya okuduğu için burada durur (K7).
/// </summary>
public sealed class WorkflowTests
{
    private const string GoodCi = """
        on:
          pull_request:
          workflow_call:
        jobs:
          build:
            steps:
              - uses: actions/setup-java@v4
              - run: dotnet workload install maui-android
              - run: ./scripts/verify-coverage.ps1
              - run: ./scripts/verify-apk.ps1 -Apk a.apk
        """;

    private const string GoodRelease = """
        on:
          push:
            tags: ['v*.*.*']
        jobs:
          kapi:
            uses: ./.github/workflows/ci.yml
          yayin:
            needs: kapi
            steps:
              - env: { A: "${{ secrets.MIZAN_KEYSTORE_BASE64 }}", B: "${{ secrets.MIZAN_KEYSTORE_PASSWORD }}" }
              - env: { C: "${{ secrets.MIZAN_KEY_ALIAS }}", D: "${{ secrets.MIZAN_KEY_PASSWORD }}" }
              - run: ./scripts/verify-apk.ps1 -Apk a.apk -Tag "$GITHUB_REF_NAME" -RequireReleaseSignature
              - run: ./scripts/release-notes.ps1 -Tag "$GITHUB_REF_NAME" -Output release-notes.md
              - run: gh release create "$GITHUB_REF_NAME" a.apk --verify-tag --notes-file release-notes.md
        """;

    private const string GoodScript = """
        [double]$DomainThreshold = 95.0,
        [double]$ApplicationThreshold = 95.0,
        [double]$PresentationThreshold = 90.0,
        [double]$InfrastructureThreshold = 80.0
        "Mizan.Infrastructure"
        """;

    [Fact]
    public void CiIsAkisi_PrKapisiniVeApkDogrulamasiniTasir()
    {
        Assert.Empty(WorkflowRules.CheckCiWorkflow());
    }

    [Fact]
    public void ReleaseIsAkisi_DebugImzayiReddeder()
    {
        Assert.Empty(WorkflowRules.CheckReleaseWorkflow());
    }

    [Fact]
    public void KapsamBetigi_S80EsikleriniTasir()
    {
        Assert.Empty(WorkflowRules.CheckCoverageScript());
    }

    [Fact]
    public void KurallarDogruIcerikte_IhlalBulmaz()
    {
        Assert.Empty(WorkflowRules.CheckCiContent(GoodCi));
        Assert.Empty(WorkflowRules.CheckReleaseContent(GoodRelease));
        Assert.Empty(WorkflowRules.CheckCoverageScriptContent(GoodScript));
    }

    [Theory]
    [InlineData("pull_request:", "x:")]
    [InlineData("workflow_call:", "x:")]
    [InlineData("maui-android", "x")]
    [InlineData("setup-java", "x")]
    [InlineData("verify-coverage.ps1", "x")]
    [InlineData("verify-apk.ps1", "x")]
    public void CiKurali_EksikParcayiYakalar(string remove, string with)
    {
        Assert.NotEmpty(WorkflowRules.CheckCiContent(GoodCi.Replace(remove, with)));
    }

    [Fact]
    public void CiKurali_ContinueOnErrorYakalar()
    {
        Assert.NotEmpty(WorkflowRules.CheckCiContent(GoodCi + "\n    continue-on-error: true"));
    }

    [Theory]
    [InlineData("-RequireReleaseSignature", "")]
    [InlineData("-Tag", "-X")]
    [InlineData("./.github/workflows/ci.yml", "./x.yml")]
    [InlineData("v*.*.*", "*")]
    [InlineData("MIZAN_KEY_ALIAS", "ANDROID_KEY_ALIAS")]
    public void ReleaseKurali_EksikParcayiYakalar(string remove, string with)
    {
        Assert.NotEmpty(WorkflowRules.CheckReleaseContent(GoodRelease.Replace(remove, with)));
    }

    [Theory]
    [InlineData("./scripts/release-notes.ps1", "./scripts/x.ps1")]
    [InlineData("--notes-file", "--notes")]
    public void ReleaseKurali_NotunChangelogtanOkunmasiniZorunluKilar(string remove, string with)
    {
        Assert.NotEmpty(WorkflowRules.CheckReleaseContent(GoodRelease.Replace(remove, with)));
    }

    [Theory]
    [InlineData("--generate-notes")]
    [InlineData("echo \"- Elle yazılmış madde\" >> release-notes.md")]
    public void ReleaseKurali_ElleUretilmisSurumNotunuYakalar(string forbidden)
    {
        Assert.NotEmpty(WorkflowRules.CheckReleaseContent(GoodRelease + "\n      - run: " + forbidden));
    }

    [Theory]
    [InlineData("$DomainThreshold = 95.0", "$DomainThreshold = 90.0")]
    [InlineData("$ApplicationThreshold = 95.0", "$ApplicationThreshold = 80.0")]
    [InlineData("$PresentationThreshold = 90.0", "$PresentationThreshold = 70.0")]
    [InlineData("$InfrastructureThreshold = 80.0", "$InfrastructureThreshold = 0.0")]
    [InlineData("\"Mizan.Infrastructure\"", "\"Mizan.Yok\"")]
    public void KapsamKurali_GevsemisEsigiYakalar(string remove, string with)
    {
        Assert.NotEmpty(WorkflowRules.CheckCoverageScriptContent(GoodScript.Replace(remove, with)));
    }

    [Fact]
    public void KapsamKurali_EksikKatmaniAtlayanBetigiYakalar()
    {
        Assert.NotEmpty(WorkflowRules.CheckCoverageScriptContent(GoodScript + "\nEşik atlandı."));
    }
}
