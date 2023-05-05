using WebSharpCompilerBusiness;

namespace WebSharpCompilerTest;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void TestCompilerNotNull()
    {
        WebSharpCompiler compiler = new WebSharpCompiler();
        Assert.IsNotNull(compiler.Compile(""));
    }

    [Test]
    public void TestCompilerSingleError()
    {
        WebSharpCompiler compiler = new WebSharpCompiler();
        string programText = @"
using **** System;
namespace HelloWorld
          {
class HelloWorldClass
              {
static void Main(string[] args)
                  {
Console.ReadLine();
                  }
              }
          }";
        List<string> compilerErrors = compiler.Compile(programText);
        Assert.AreEqual(compilerErrors.Count, 1);
    }
}
