namespace Mizan.Application.Models;

/// <summary>
/// Çalışma listesindeki bir denemenin hesaba girmemesinin sebebi (S76-5). Liste günlerce saklandığı için deneme
/// kurulduğu günkü hâlinde kalmaz; sorunu sessizce yutmak yanlış bir sonuç ya da bütün hesabın düşmesi demekti.
/// Ekran sorunu satırda işaretler, kullanıcı denemeyi düzenler ya da siler.
/// </summary>
public enum SimulationConditionIssue
{
    /// <summary>Sorun yok; açıksa hesaba girer.</summary>
    None,

    /// <summary>Denemenin tarihi bugünden önce kaldı (S76-3).</summary>
    DatePassed
}
