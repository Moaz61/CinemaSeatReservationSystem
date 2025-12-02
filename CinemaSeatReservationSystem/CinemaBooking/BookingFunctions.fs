module BookingFunctions

open Microsoft.Data.Sqlite
open Models
open Database

// ============================================================
// 1. FIND USER BY USERNAME
// ============================================================
let findUserByUsername username =
    use connection = getConnection()
    connection.Open()
    
    let sql = "SELECT UserId, Username, Password FROM Users WHERE Username = @username"
    use cmd = new SqliteCommand(sql, connection)
    cmd.Parameters.AddWithValue("@username", username) |> ignore
    
    use reader = cmd.ExecuteReader()
    if reader.Read() then
        Some {
            UserId = reader.GetInt32(0)
            Username = reader.GetString(1)
            Password = reader.GetString(2)
        }
    else
        None

// ============================================================
// 2. SAVE USER
// ============================================================
let saveUser (user: User) =
    use connection = getConnection()
    connection.Open()
    
    try
        let sql = "INSERT INTO Users (Username, Password) VALUES (@username, @password)"
        use cmd = new SqliteCommand(sql, connection)
        cmd.Parameters.AddWithValue("@username", user.Username) |> ignore
        cmd.Parameters.AddWithValue("@password", user.Password) |> ignore
        
        cmd.ExecuteNonQuery() |> ignore
        
        // Get the auto-generated UserId
        use lastIdCmd = new SqliteCommand("SELECT last_insert_rowid()", connection)
        let newUserId = lastIdCmd.ExecuteScalar() :?> int64 |> int
        
        Some { user with UserId = newUserId }
    with
    | :? SqliteException as ex ->
        printfn "Error saving user: %s" ex.Message
        None

// ============================================================
// 3. FIND SEAT BY COORDINATES
// ============================================================
let findSeatByCoordinates rowNumber seatNumber =
    use connection = getConnection()
    connection.Open()
    
    let sql = "SELECT SeatId, RowNumber, SeatNumber, IsReserved FROM Seats WHERE RowNumber = @row AND SeatNumber = @seat"
    use cmd = new SqliteCommand(sql, connection)
    cmd.Parameters.AddWithValue("@row", rowNumber) |> ignore
    cmd.Parameters.AddWithValue("@seat", seatNumber) |> ignore
    
    use reader = cmd.ExecuteReader()
    if reader.Read() then
        Some {
            SeatId = reader.GetInt32(0)
            RowNumber = reader.GetInt32(1)
            SeatNumber = reader.GetInt32(2)
            IsReserved = reader.GetInt32(3) = 1
        }
    else
        None

// ============================================================
// 4. SAVE SEAT RESERVATION
// ============================================================
let saveSeatReservation seatId =
    use connection = getConnection()
    connection.Open()
    
    try
        // First check if seat exists and is not reserved
        let checkSql = "SELECT IsReserved FROM Seats WHERE SeatId = @seatId"
        use checkCmd = new SqliteCommand(checkSql, connection)
        checkCmd.Parameters.AddWithValue("@seatId", seatId) |> ignore
        
        use reader = checkCmd.ExecuteReader()
        if reader.Read() then
            let isReserved = reader.GetInt32(0) = 1
            reader.Close()
            
            if isReserved then
                printfn "Seat is already reserved"
                None
            else
                // Update seat to reserved
                let updateSql = "UPDATE Seats SET IsReserved = 1 WHERE SeatId = @seatId"
                use updateCmd = new SqliteCommand(updateSql, connection)
                updateCmd.Parameters.AddWithValue("@seatId", seatId) |> ignore
                
                updateCmd.ExecuteNonQuery() |> ignore
                Some seatId
        else
            printfn "Seat doesn't exist"
            None
    with
    | :? SqliteException as ex ->
        printfn "Error reserving seat: %s" ex.Message
        None

// ============================================================
// 5. SAVE TICKET
// ============================================================
let saveTicket (ticket: Ticket) =
    use connection = getConnection()
    connection.Open()
    
    try
        let sql = "INSERT INTO Tickets (TicketId, SeatId, UserId) VALUES (@ticketId, @seatId, @userId)"
        use cmd = new SqliteCommand(sql, connection)
        cmd.Parameters.AddWithValue("@ticketId", ticket.TicketId) |> ignore
        cmd.Parameters.AddWithValue("@seatId", ticket.SeatId) |> ignore
        cmd.Parameters.AddWithValue("@userId", ticket.UserId) |> ignore
        
        cmd.ExecuteNonQuery() |> ignore
        Some ticket
    with
    | :? SqliteException as ex ->
        printfn "Error saving ticket: %s" ex.Message
        None

// ============================================================
// HELPER FUNCTIONS (For testing and display)
// ============================================================

// Get all seats from database
let getAllSeats() =
    use connection = getConnection()
    connection.Open()
    
    let sql = "SELECT SeatId, RowNumber, SeatNumber, IsReserved FROM Seats"
    use cmd = new SqliteCommand(sql, connection)
    use reader = cmd.ExecuteReader()
    
    let mutable seats = []
    while reader.Read() do
        let seat = {
            SeatId = reader.GetInt32(0)
            RowNumber = reader.GetInt32(1)
            SeatNumber = reader.GetInt32(2)
            IsReserved = reader.GetInt32(3) = 1
        }
        seats <- seat :: seats
    
    List.rev seats
