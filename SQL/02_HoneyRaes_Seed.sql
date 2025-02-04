\c HoneyRaes

INSERT INTO Customer (Name, Address)
    VALUES 
        ('Rick', '100 Road St'),
        ('Kev', '200 Street Rd'),
        ('Ricardo', '300 Lane Ave');
INSERT INTO Employee (Name, Specialty)
    VALUES
        ('Smoothun', 'Cell Phone Repair'),
        ('Chex-Mix', 'Laptop Repair');
INSERT INTO ServiceTicket (CustomerId, EmployeeId, Description, Emergency, DateCompleted)
    VALUES
        (1, 1, 'Charging Port', FALSE, NULL),
        (1, 1, 'Screen Repair', FALSE, NULL),
        (2, 1, 'Screen Repair', FALSE, NULL),
        (2, 2, 'Screen Repair', FALSE, NULL),
        (3, NULL, 'Screen Repair', TRUE, NULL);
