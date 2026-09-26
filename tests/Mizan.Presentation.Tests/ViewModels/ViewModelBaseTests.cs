using Mizan.Presentation.Models;
using Mizan.Presentation.ViewModels;
using Xunit;

namespace Mizan.Presentation.Tests.ViewModels;

public sealed class ViewModelBaseTests
{
    private sealed partial class TestViewModel : ViewModelBase
    {
    }

    [Fact]
    public void BaslangicDurumu_VarsayilanDegerleriTasar()
    {
        var vm = new TestViewModel();

        Assert.False(vm.IsBusy);
        Assert.Empty(vm.BusyMessage);
        Assert.True(vm.CanInteract);
        Assert.Equal(ScreenState.Content, vm.State);
        Assert.True(vm.IsContent);
        Assert.False(vm.IsLoading);
        Assert.False(vm.IsEmpty);
        Assert.False(vm.IsError);
    }

    [Fact]
    public void IsBusy_Degistiginde_CanInteractGuncellenirVeBildirimFirlatilir()
    {
        var vm = new TestViewModel();
        var canInteractChanged = false;
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(vm.CanInteract))
            {
                canInteractChanged = true;
            }
        };

        vm.IsBusy = true;

        Assert.False(vm.CanInteract);
        Assert.True(canInteractChanged);
    }

    [Fact]
    public void SetBusy_MesguliyetDurumunuVeMesajiniAyarla()
    {
        var vm = new TestViewModel();

        vm.SetBusy(true, "Hesaplanıyor...");

        Assert.True(vm.IsBusy);
        Assert.Equal("Hesaplanıyor...", vm.BusyMessage);
        Assert.False(vm.CanInteract);
    }

    [Fact]
    public void State_Degistirildiginde_Guncellenir()
    {
        var vm = new TestViewModel();

        vm.State = ScreenState.Error;

        Assert.Equal(ScreenState.Error, vm.State);
        Assert.True(vm.IsError);
        Assert.False(vm.IsContent);
    }
}
