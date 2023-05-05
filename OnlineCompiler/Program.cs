using Analyzer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

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
	if (context.Request.Path == "/postcode")
	{
		var form = context.Request.Form;
		string code = form["code1"];
		await context.Response.WriteAsync($"Code: ${code}");
	}

});

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