using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;

public class IpInfo
{
    [JsonProperty("country")]
    public string Country { get; set; }

    [JsonProperty("city")]
    public string City { get; set; }
}

class Program
{
    static async Task<IpInfo> GetIpInfo(string ip)
    {
        using var client = new HttpClient();

        string url = $"https://ipinfo.io/{ip}/json";
        string response = await client.GetStringAsync(url);

        IpInfo info = JsonConvert.DeserializeObject<IpInfo>(response);
        return info;
    }

    static async Task Main()
    {
        // Читаем все IP из файла
        string[] ips = File.ReadAllLines("ips.txt");

        // Словарь: страна -> количество IP
        Dictionary<string, int> countryCount = new Dictionary<string, int>();

        // Словарь: страна -> список городов
        Dictionary<string, HashSet<string>> countryCities = new Dictionary<string, HashSet<string>>();

        Console.WriteLine("Обработка IP адресов...\n");

        foreach (string ip in ips)
        {
            IpInfo info = await GetIpInfo(ip);

            Console.WriteLine($"{ip} -> {info.Country}, {info.City}");

            // Увеличиваем счётчик страны
            if (countryCount.ContainsKey(info.Country))
                countryCount[info.Country]++;
            else
                countryCount[info.Country] = 1;

            // Добавляем город в список
            if (!countryCities.ContainsKey(info.Country))
                countryCities[info.Country] = new HashSet<string>();

            countryCities[info.Country].Add(info.City);
        }

        // Вывод статистики
        Console.WriteLine("\nСтатистика по странам:");
        foreach (var pair in countryCount)
        {
            Console.WriteLine($"{pair.Key} - {pair.Value}");
        }

        // Поиск страны с максимальным количеством IP
        string maxCountry = null;
        int maxCount = 0;

        foreach (var pair in countryCount)
        {
            if (pair.Value > maxCount)
            {
                maxCount = pair.Value;
                maxCountry = pair.Key;
            }
        }

        Console.WriteLine($"\nСтрана с максимальным количеством IP: {maxCountry}");

        Console.WriteLine("Города этой страны:");
        foreach (string city in countryCities[maxCountry])
        {
            Console.WriteLine(city);
        }
    }
}
