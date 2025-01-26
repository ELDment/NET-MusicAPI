using System.Reflection;
using System;

namespace MusicAPI.Tests;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public async Task NeteaseTest()
    {
        Dictionary<string, string> headers1 = new();
        //var api = new NeteaseAPI();
        var api = new TencentAPI();

        //headers1.Add("Accept", "NNNNNNNNNNNNNNNNNNNNONE");
        //headers1.Add("addition", "NONE");
        api.Headers = headers1;

        //headers.Clear();
        //headers = api.GetHeaders();
        

        var result = await api.Search("secret base", page: 1);
        //var song = result[0]!;
        //Console.WriteLine(result);
        
        //var lyric = await api.GetLyric(song.Id);
        //Console.WriteLine(lyric);
        //Assert.IsNotNull(result);
    }

    //public void PrintClass(object obj) {
    //    Type type = typeof(obj);

    //    FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

    //    foreach (FieldInfo field in fields) {
    //        Console.WriteLine($"Field Name: {field.Name}, Field Value: {field.GetValue(obj)}");
    //    }

    //    return;
    //}
}