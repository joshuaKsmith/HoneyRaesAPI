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

app.MapGet("/servicetickets/{id}", (int id) =>
{
    ServiceTicket serviceTicket = serviceTickets.FirstOrDefault(st => st.Id == id);
    if (serviceTicket == null)
    {
        return Results.NotFound();
    }
    Employee employee = employees.FirstOrDefault(e => e.Id == serviceTicket.EmployeeId);
    Customer customer = customers.FirstOrDefault(c => c.Id == id);
    return Results.Ok(new ServiceTicketDTO
    {
        Id = serviceTicket.Id,
        CustomerId = serviceTicket.CustomerId,
        Customer = customer == null ? null : new CustomerDTO
        {
            Id = customer.Id,
            Name = customer.Name,
            Address = customer.Address
        },
        EmployeeId = serviceTicket.EmployeeId,
        Employee = employee == null ? null : new EmployeeDTO
        {
            Id = employee.Id,
            Name = employee.Name,
            Specialty = employee.Specialty
        },
        Description = serviceTicket.Description,
        Emergency = serviceTicket.Emergency,
        DateCompleted = serviceTicket.DateCompleted
    });
});

app.MapGet("/employees", () =>
{
    return employees.Select(e => new EmployeeDTO
    {
        Id = e.Id,
        Name = e.Name,
        Specialty = e.Specialty
    });
});

app.MapGet("/employees/{id}", (int id) =>
{
    Employee employee = employees.FirstOrDefault(e => e.Id == id);
    if (employee == null)
    {
        return Results.NotFound();
    }
    List<ServiceTicket> tickets = serviceTickets.Where(st => st.EmployeeId == id).ToList();
    return Results.Ok(new EmployeeDTO
    {
        Id = employee.Id,
        Name = employee.Name,
        Specialty = employee.Specialty,
        ServiceTickets = tickets.Select(t => new ServiceTicketDTO
        {
            Id = t.Id,
            CustomerId = t.CustomerId,
            EmployeeId = t.EmployeeId,
            Description = t.Description,
            Emergency = t.Emergency,
            DateCompleted = t.DateCompleted
        }).ToList()
    });
});

app.MapGet("/customers", () => 
{
    return customers.Select(c => new CustomerDTO
    {
        Id = c.Id,
        Name = c.Name,
        Address = c.Address
    });
});

app.MapGet("/customers/{id}", (int id) =>
{
    Customer customer = customers.FirstOrDefault(c => c.Id == id);
    if (customer == null)
    {
        return Results.NotFound();
    }
    List<ServiceTicket> tickets = serviceTickets.Where(st => st.CustomerId == id).ToList();
    return Results.Ok(new CustomerDTO
    {
        Id = customer.Id,
        Name = customer.Name,
        Address = customer.Address,
        ServiceTickets = tickets.Select(t => new ServiceTicketDTO
        {
            Id = t.Id,
            CustomerId = t.CustomerId,
            EmployeeId = t.EmployeeId,
            Description = t.Description,
            Emergency = t.Emergency,
            DateCompleted = t.DateCompleted
        }).ToList()
    });
});

app.MapPost("/servicetickets", (ServiceTicket serviceTicket) => 
{
    // get customer info of this ticket
    Customer customer = customers.FirstOrDefault(c => c.Id == serviceTicket.CustomerId);

    // error handling if there is no customer
    if (customer == null)
    {
        return Results.BadRequest();
    }

    // take current highest ticket id
    serviceTicket.Id = serviceTickets.Max(st => st.Id) + 1;
    serviceTickets.Add(serviceTicket);

    // Created returns a 201 status code with a link in the headers to where the new resource can be accessed
    return Results.Created($"/servicetickets/{serviceTicket.Id}", new ServiceTicketDTO
    {
        Id = serviceTicket.Id,
        CustomerId = serviceTicket.CustomerId,
        Customer = new CustomerDTO
        {
            Id = customer.Id,
            Name = customer.Name,
            Address = customer.Address
        },
        Description = serviceTicket.Description,
        Emergency = serviceTicket.Emergency
    });
});

app.Run();
