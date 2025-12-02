module Database

open Microsoft.Data.Sqlite

// Database connection string
let connectionString = "Data Source=cinema.db"

// Get a new database connection
let getConnection() =
    new SqliteConnection(connectionString)

// Initialize database and create tables
let initializeDatabase() =
    use connection = getConnection()
    connection.Open()
    
    // Create Users table
    let createUsersTable = """
        CREATE TABLE IF NOT EXISTS Users (
            UserId INTEGER PRIMARY KEY AUTOINCREMENT,
            Username TEXT NOT NULL UNIQUE,
            Password TEXT NOT NULL
        )
    """
    
    // Create Seats table
    let createSeatsTable = """
        CREATE TABLE IF NOT EXISTS Seats (
            SeatId INTEGER PRIMARY KEY AUTOINCREMENT,
            RowNumber INTEGER NOT NULL,
            SeatNumber INTEGER NOT NULL,
            IsReserved INTEGER NOT NULL DEFAULT 0,
            UNIQUE(RowNumber, SeatNumber)
        )
    """
    
    // Create Tickets table
    let createTicketsTable = """
        CREATE TABLE IF NOT EXISTS Tickets (
            TicketId TEXT PRIMARY KEY,
            SeatId INTEGER NOT NULL,
            UserId INTEGER NOT NULL,
            FOREIGN KEY (SeatId) REFERENCES Seats(SeatId),
            FOREIGN KEY (UserId) REFERENCES Users(UserId)
        )
    """
    
    use cmd = new SqliteCommand(createUsersTable, connection)
    cmd.ExecuteNonQuery() |> ignore
    
    use cmd2 = new SqliteCommand(createSeatsTable, connection)
    cmd2.ExecuteNonQuery() |> ignore
    
    use cmd3 = new SqliteCommand(createTicketsTable, connection)
    cmd3.ExecuteNonQuery() |> ignore
    
    printfn "✓ Database initialized successfully"

// Initialize seats in database (10x10 grid)
let initializeSeats() =
    use connection = getConnection()
    connection.Open()
    
    // Check if seats already exist
    use checkCmd = new SqliteCommand("SELECT COUNT(*) FROM Seats", connection)
    let count = checkCmd.ExecuteScalar() :?> int64
    
    if count = 0L then
        printfn "Initializing seats in database..."
        use transaction = connection.BeginTransaction()
        
        for row in 1..10 do
            for seat in 1..10 do
                let insertSql = "INSERT INTO Seats (RowNumber, SeatNumber, IsReserved) VALUES (@row, @seat, 0)"
                use cmd = new SqliteCommand(insertSql, connection, transaction)
                cmd.Parameters.AddWithValue("@row", row) |> ignore
                cmd.Parameters.AddWithValue("@seat", seat) |> ignore
                cmd.ExecuteNonQuery() |> ignore
        
        transaction.Commit()
        printfn "Seats initialized (10 rows x 10 seats)"
    else
        printfn "Seats already exist in database"

// Helper: Display seating map
let displaySeatingMap() =
    use connection = getConnection()
    connection.Open()
    
    printfn "\n=== SEATING MAP ==="
    printfn "   1  2  3  4  5  6  7  8  9 10"
    
    for row in 1..10 do
        printf "%2d " row
        for seat in 1..10 do
            let sql = "SELECT IsReserved FROM Seats WHERE RowNumber = @row AND SeatNumber = @seat"
            use cmd = new SqliteCommand(sql, connection)
            cmd.Parameters.AddWithValue("@row", row) |> ignore
            cmd.Parameters.AddWithValue("@seat", seat) |> ignore
            
            use reader = cmd.ExecuteReader()
            if reader.Read() then
                let isReserved = reader.GetInt32(0) = 1
                if isReserved then
                    printf "[X]"
                else
                    printf "[ ]"
            else
                printf "[?]"
        printfn ""
    
    printfn "\n[ ] = Available  [X] = Reserved"