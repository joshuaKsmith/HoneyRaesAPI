using HoneyRaesAPI.Models;
using HoneyRaesAPI.Models.DTOs;
List<Customer> customers = new List<Customer>
{
    new Customer()
    {
        Id = 1,
        Name = "Rick",
        Address = "100 Road St"
    },
    new Customer()
    {
        Id = 2,
        Name = "Kev",
        Address = "200 Street Rd"
    },
    new Customer()
    {
        Id = 3,
        Name = "Ricardo",
        Address = "300 Lane Ave"
    }
};
List<Employee> employees = new List<Employee>
{
    new Employee()
    {
        Id = 1,
        Name = "Smooth'un",
        Specialty = "Cell Phone Repair"
    },
    new Employee()
    {
        Id = 2,
        Name = "Chex-Mix",
        Specialty = "Laptop Repair"
    }
};
List<ServiceTicket> serviceTickets = new List<ServiceTicket>
{
    new ServiceTicket()
    {
        Id = 1,
        CustomerId = 1,
        EmployeeId = 1,
        Description = "Andriod charging port repair",
        Emergency = false,
    },
    new ServiceTicket()
    {
        Id = 2,
        CustomerId = 1,
        EmployeeId = 1,
        Description = "Android screen repair",
        Emergency = false,
    },
    new ServiceTicket()
    {
        Id = 3,
        CustomerId = 2,
        EmployeeId = 1,
        Description = "iPhone screen repair",
        Emergency = false,
        DateCompleted = new DateOnly()
    },
    new ServiceTicket()
    {
        Id = 4,
        CustomerId = 2,
        EmployeeId = 2,
        Description = "Macbook screen repair",
        Emergency = false,
        DateCompleted = new DateOnly()
    },
    new ServiceTicket()
    {
        Id = 5,
        CustomerId = 3,
        Description = "Lenovo screen repair",
        Emergency = true,
        DateCompleted = new DateOnly()
    }
};

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//      //      //      //      //      //      //
app.MapGet("/serviceTickets", () =>
{
    return serviceTickets.Select(t => new ServiceTicketDTO
    {
        Id = t.Id,
        CustomerId = t.CustomerId,
        EmployeeId = t.EmployeeId,
        Description = t.Description,
        Emergency = t.Emergency,
        DateCompleted = t.DateCompleted
    });
});

app.MapGet("/serviceTickets/{id}", (int id) => 
{
    ServiceTicket serviceTicket = serviceTickets.FirstOrDefault(st => st.Id == id);

    return new ServiceTicketDTO
    {
        Id = serviceTicket.Id,
        CustomerId = serviceTicket.CustomerId,
        EmployeeId = serviceTicket.EmployeeId,
        Description = serviceTicket.Description,
        Emergency = serviceTicket.Emergency,
        DateCompleted = serviceTicket.DateCompleted
    };
});

app.Run();
