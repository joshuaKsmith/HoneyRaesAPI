using Npgsql;
using HoneyRaesAPI.Models;
using HoneyRaesAPI.Models.DTOs;
var connectionString = "Host=localhost;Port=5432;Username=postgres;Password=testpassword;Database=HoneyRaes";

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
        DateCompleted = new DateTime()
    },
    new ServiceTicket()
    {
        Id = 4,
        CustomerId = 2,
        EmployeeId = 2,
        Description = "Macbook screen repair",
        Emergency = false,
        DateCompleted = new DateTime()
    },
    new ServiceTicket()
    {
        Id = 5,
        CustomerId = 3,
        Description = "Lenovo screen repair",
        Emergency = true,
        DateCompleted = new DateTime()
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
    // make an empty list
    List<Employee> employees = new List<Employee>();

    // create Postgres connection string
    using NpgsqlConnection connection = new NpgsqlConnection(connectionString);

    // open connection
    connection.Open();

    // create an sql command 
    using NpgsqlCommand command = connection.CreateCommand();
    command.CommandText = "SELECT * FROM Employee";

    // send the command
    using NpgsqlDataReader reader = command.ExecuteReader();

    // read command results row by row
    while (reader.Read())  // reader.Read() returns a boolean, to say whether there is a row or not; it also advances down to that row if it exists
    {
        // add a new C# Employee object using the data reader's current row
        employees.Add(new Employee
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),  // find what position the Id column is in, then get the integer store
            Name = reader.GetString(reader.GetOrdinal("Name")),
            Specialty = reader.GetString(reader.GetOrdinal("Specialty"))
        });
    };

    // once all row are read, return employee list back to client as JSON
    return employees;

});

app.MapGet("/employees/{id}", (int id) =>
{
    Employee employee = null;
    using NpgsqlConnection connection = new NpgsqlConnection(connectionString);
    connection.Open();
    using NpgsqlCommand command = connection.CreateCommand();
    command.CommandText = @"
        SELECT 
            e.Id,
            e.Name,
            e.Specialty,
            st.Id AS serviceTicketId,
            st.customerId,
            st.Description,
            st.Emergency,
            st.DateCompleted
        FROM Employee e
        LEFT JOIN ServiceTicket st ON st.EmployeeId = e.Id
        WHERE e.Id = @id
    ";

    // use command parameters to add the specific Id we are looking for to the query
    command.Parameters.AddWithValue("@id", id);
    using NpgsqlDataReader reader = command.ExecuteReader();

    while (reader.Read())
    {
        if (employee == null)
        {
            employee = new Employee
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Specialty = reader.GetString(reader.GetOrdinal("Specialty")),
                ServiceTickets = new List<ServiceTicket>()
            };
        }

        // reader.IsDBNull checks if a column in a particular position is null
        if (!reader.IsDBNull(reader.GetOrdinal("serviceTicketId")))
        {
            employee.ServiceTickets.Add(new ServiceTicket
            {
                Id = reader.GetInt32(reader.GetOrdinal("serviceTicketId")),
                CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),

                // we don't need to get this from the database, as we already know it
                EmployeeId = id,
                Description = reader.GetString(reader.GetOrdinal("Description")),
                Emergency = reader.GetBoolean(reader.GetOrdinal("Emergency")),

                // Npgsql can't automatically convert NULL in the database to C# null, so we have to check whether it's null before trying to get it
                DateCompleted = reader.IsDBNull(reader.GetOrdinal("DateCompleted")) ? null : reader.GetDateTime(reader.GetOrdinal("DateCompleted"))
            });
        }
    }

    // return 404 if the employee is never set AKA reader.Read() immediately returned false
    return employee == null ? Results.NotFound() : Results.Ok(employee);
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

app.MapDelete("/servicetickets/{id}", (int id) => 
{
    ServiceTicket ticketToDelete = serviceTickets.FirstOrDefault(st => st.Id == id);

    if (ticketToDelete == null)
    {
        return Results.NotFound();
    }
    if (ticketToDelete.Id != id)
    {
        return Results.BadRequest();
    }
    serviceTickets.Remove(ticketToDelete);
    return Results.NoContent();
});

app.MapPut("/servicetickets/{id}", (int id, ServiceTicket serviceTicket) => 
{
    ServiceTicket ticketToUpdate = serviceTickets.FirstOrDefault(st => st.Id == id);

    if (ticketToUpdate == null)
    {
        return Results.NotFound();
    }
    if (id != serviceTicket.Id)
    {
        return Results.BadRequest();
    }

    ticketToUpdate.CustomerId = serviceTicket.CustomerId;
    ticketToUpdate.EmployeeId = serviceTicket.EmployeeId;
    ticketToUpdate.Description = serviceTicket.Description;
    ticketToUpdate.Emergency = serviceTicket.Emergency;
    ticketToUpdate.DateCompleted = serviceTicket.DateCompleted;
    
    return Results.NoContent();
});

app.MapPost("/servicetickets/{id}/complete", (int id) => 
{
    ServiceTicket ticketToComplete = serviceTickets.FirstOrDefault(st => st.Id == id);
    ticketToComplete.DateCompleted = DateTime.Today;
    return Results.NoContent();
});

app.MapPost("/employees", (Employee employee) => 
{
    using NpgsqlConnection connection = new NpgsqlConnection(connectionString);
    connection.Open();
    using NpgsqlCommand command = connection.CreateCommand();
    command.CommandText = @"
        INSERT INTO Employee (Name, Specialty)
        VALUES (@name, @specialty)
        RETURNING Id
    ";
    command.Parameters.AddWithValue("@name", employee.Name);
    command.Parameters.AddWithValue("@specialty", employee.Specialty);

    // the databse will return the new Id for the employee, add it to the C# object
    employee.Id = (int)command.ExecuteScalar();

    return employee;
});

app.MapPut("/employees/{id}", (int id, Employee employee) => 
{
    if (id != employee.Id)
    {
        return Results.BadRequest();
    }
    using NpgsqlConnection connection = new NpgsqlConnection(connectionString);
    connection.Open();
    using NpgsqlCommand command = connection.CreateCommand();
    command.CommandText = @"
        UPDATE Employee
        SET Name = @name,
            Specialty = @specialty
        WHERE Id = @id
    ";
    command.Parameters.AddWithValue("@name", employee.Name);
    command.Parameters.AddWithValue("@specialty", employee.Specialty);
    command.Parameters.AddWithValue("@id", id);

    command.ExecuteNonQuery();  // used for data changes when no return data is needed or expected
    return Results.NoContent(); // 204 response
});

app.MapDelete("/employees/{id}", (int id) => 
{
    using NpgsqlConnection connection = new NpgsqlConnection(connectionString);
    connection.Open();
    using NpgsqlCommand command = connection.CreateCommand();
    command.CommandText = @"
        DELETE FROM Employee WHERE Id=@id
    ";
    command.Parameters.AddWithValue("@id", id);
    command.ExecuteNonQuery();
    return Results.NoContent();
});





app.Run();
