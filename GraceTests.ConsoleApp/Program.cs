
using Grace.DependencyInjection;
using Grace.DependencyInjection.Attributes;
using Grace.DependencyInjection.Impl;

DependencyInjectionContainer container = new DependencyInjectionContainer((c) => {
	c.AutoRegisterUnknown = true;
	c.Trace = (s => {
		Console.WriteLine(s);
	});
});

container.Configure((c) => {
	c.Export<Service>().AsKeyed<IService>("ServiceM");

	c.Export<ServiceAlt>().AsKeyed<IService>("ServiceA");

// Will default to this if a key import fails.
// Allows for the default [import()].
//c.Export<Service>().As<IService>();
});

var scope = container.BeginLifetimeScope("RootScope");

try {
	var alt = container.Locate<IService>(withKey: "ServiceA");
	var service = container.Locate<ServiceMain>();

	Console.WriteLine(service.Service.GetString());
}
catch (Exception e) {
	Console.WriteLine(InjectionContextValueProvider.Logs.ToString());
	Console.WriteLine(e);
	Console.WriteLine(e.Message);
}


interface IService {
	string GetString();
}

class Service : IService {
	public virtual string GetString() {
		return "Service";
	}
}

class ServiceAlt : IService {
	public string GetString() {
		return "Service AlternativeService";
	}
}

class ServiceMain {
	// Works.
	//[Import(Key = "ServiceA")]
	public IService Service { get; set; }

	// Works with export "c.Export<Service>().As<IService>();"
	//[Import()]

	// Throws error that type reference in Grace is null.
	[Import(Key = "ServiceA")]
	public void Recieve(IService service) {
		Console.WriteLine("Import through method");
		Service = service;
	}
}