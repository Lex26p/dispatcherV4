using System.ComponentModel.DataAnnotations;

namespace Dispatcher.Web.Models;

public sealed class CreateSnmpDeviceRequest
{
    [Required(ErrorMessage = "Введите название устройства.")]
    [StringLength(200, ErrorMessage = "Название не должно быть длиннее 200 символов.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите IP-адрес или DNS-имя.")]
    [StringLength(255, ErrorMessage = "Адрес не должен быть длиннее 255 символов.")]
    public string Host { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите community string.")]
    [StringLength(255, ErrorMessage = "Community не должен быть длиннее 255 символов.")]
    public string Community { get; set; } = "public";

    [Range(1, 3, ErrorMessage = "Версия SNMP должна быть 1, 2 или 3.")]
    public int Version { get; set; } = 2;

    [Range(1, 65535, ErrorMessage = "Порт должен быть от 1 до 65535.")]
    public int Port { get; set; } = 161;

    [Range(100, 600000, ErrorMessage = "Интервал опроса должен быть от 100 мс.")]
    public int PollIntervalMs { get; set; } = 5000;

    [Range(100, 600000, ErrorMessage = "Таймаут должен быть от 100 мс.")]
    public int TimeoutMs { get; set; } = 3000;

    [Range(0, 20, ErrorMessage = "Количество повторов должно быть от 0 до 20.")]
    public int RetryCount { get; set; } = 3;

    [StringLength(1000, ErrorMessage = "Описание не должно быть длиннее 1000 символов.")]
    public string? Description { get; set; }
}
