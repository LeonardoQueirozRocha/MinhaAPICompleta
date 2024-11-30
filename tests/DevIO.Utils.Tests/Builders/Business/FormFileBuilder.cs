using System.IO.Abstractions;
using Bogus;
using DevIO.Utils.Tests.Builders.Base;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualBasic;

namespace DevIO.Utils.Tests.Builders.Business;

public class FormFileBuilder : LazyFakerBuilder<FormFile>
{
    private FormFileBuilder()
    {
    }

    public static FormFileBuilder Instance => new();

    protected override Faker<FormFile> Factory()
    {
        const string content = "Hello World from a Fake File";
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(content);
        writer.Flush();
        stream.Position = 0;

        var file = new Faker<FormFile>(Locale)
            .CustomInstantiator(setter => new FormFile(
                stream,
                0,
                stream.Length,
                setter.System.FileName(),
                setter.System.FileName()));

        return file;
    }

    public FormFile BuildEmpty()
    {
        var stream = new MemoryStream();
        var file = new Faker<FormFile>(Locale)
            .CustomInstantiator(setter => new FormFile(
                stream,
                0,
                stream.Length,
                setter.System.FileName(),
                setter.System.FileName()));

        return file.Generate();
    }
}