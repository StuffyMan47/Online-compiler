using Analyzer;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages(options =>
{
	// отключаем глобально Antiforgery-токен
	options.Conventions.ConfigureFilter(new IgnoreAntiforgeryTokenAttribute());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapRazorPages();
});

app.MapRazorPages();

app.Run(async (context) =>
{
	context.Response.ContentType = "text/html; charset=utf-8";

    // если обращение идет по адресу "/postuser", получаем данные формы
    if (context.Request.Path == "/Test")
    {
        var form = context.Request.Form;
        string name = form["code2"].ToString();
        await context.Response.WriteAsync($"<div><p>Name: {name}</p>");
        Console.WriteLine(form["code2"].ToString());
    }
    else
    {
        await context.Response.SendFileAsync("html/index.html");
    }
});

app.Run();

//class Back { 
//public string errors;
//public string warnings;
//public string consoleOut;
//	public void ReadTextArea()
//	{
//		var code = Request.Form[""];
//		var codeStart = new Code(code);

//		//компиляция и заполнение поля strArrayOfOutput
//		codeStart.CodeCompilation(code);

//		this.warnings = codeStart.Warnings.ToString();
//		this.errors = codeStart.CompilationErrors.ToString();
//		this.consoleOut = string.Join("\n", codeStart.strArrayOfOutput);
//	}
//}