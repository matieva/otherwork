using System.Text.Json;

using var httpClient = new HttpClient();

var url = "https://raw.githubusercontent.com/dotnet/core/master/release-notes/releases-index.json";
var ts = await httpClient.GetStreamAsync(url);

using var resp = await JsonDocument.ParseAsync(ts);

var root = resp.RootElement.GetProperty("releases-index");

var elems = root.EnumerateArray();

while (elems.MoveNext())
{
    var node = elems.Current;
    Console.WriteLine(node);
}