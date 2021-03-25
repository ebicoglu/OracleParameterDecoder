# OracleParameterDecoder

Decodes raw SQL of EF Core for Oracle ODP.Net provider

---

To enable EF Core logging

In EntityFrameworkCore project enable sensitive data logging:

```
Configure<AbpDbContextOptions>(options =>
{
	options.UseOracle();
  
	options.PreConfigure(x =>
	{
		x.DbContextOptions.EnableSensitiveDataLogging();
	});
}
```

In `program.cs` (DbMigrator or Web), set `LogEventLevel` to `Information`. And add `Enrich.FromLogContext();`

```
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .MinimumLevel.Override("Volo.Abp", LogEventLevel.Warning)
                .MinimumLevel.Override("MyCompanyName.MyProjectName", LogEventLevel.Debug)
                .Enrich.FromLogContext()
                .WriteTo.Async(c => c.File("Logs/logs.txt"))
                .Enrich.With(new MyEnrcher())
                .WriteTo.Async(c => c.Console(outputTemplate:
                    "[{Timestamp:HH:mm:ss} {Level:u3}] ({SourceContext}) {Message:lj}{NewLine}{Exception}"))
                .CreateLogger();
                
internal class MyEnrcher : ILogEventEnricher
{
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (logEvent.Level == LogEventLevel.Error)
            {

            }
        }
 }
```
