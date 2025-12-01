module BookingFunctions

open Models
open System.IO


//1. Find User By Username 
let FindUserByUsername username (users: User list) =
    users |> List.tryFind (fun user -> user.Username = username)


//2. Save User
let SaveUser (user: User) (users: User list) =
    let updatedUsers = user :: users
    
    // Convert users to CSV format
    let userLines = 
        updatedUsers 
        |> List.map (fun u -> sprintf "%d,%s,%s" u.UserId u.Username u.Password)
    
    File.WriteAllLines("users.txt", userLines)
    
    updatedUsers


//3. Find Seat By Coordinates
let FindSeatByCoordinates rowNumber seatNumber (seats: Seat list) =
    seats 
    |> List.tryFind (fun seat -> 
        seat.RowNumber = rowNumber && seat.SeatNumber = seatNumber)


//4. Save Seat Reservation
let SaveSeatReservation seatId (seats: Seat list) =
    // Update the seat to reserved
    let updatedSeats = 
        seats 
        |> List.map (fun seat ->
            if seat.SeatId = seatId then 
                { seat with IsReserved = true }
            else 
                seat)
    
    let seatLines = 
        updatedSeats 
        |> List.map (fun s -> 
            sprintf "%d,%d,%d,%b" s.SeatId s.RowNumber s.SeatNumber s.IsReserved)
    
    File.WriteAllLines("seats.txt", seatLines)
    
    updatedSeats


//5. Save Ticket
let SaveTicket (ticket: Ticket) (tickets: Ticket list) =
    // Add ticket to the list
    let updatedTickets = ticket :: tickets
    
    let ticketLines = 
        updatedTickets 
        |> List.map (fun t -> 
            sprintf "%s,%d,%d,%s" 
                t.TicketId 
                t.SeatId 
                t.UserId 
                (t.BookingDate.ToString("yyyy-MM-dd HH:mm:ss")))
    
    File.WriteAllLines("tickets.txt", ticketLines)
    
    updatedTickets



// ========== HELPER: Load Users from File ==========
let LoadUsers() =
    if File.Exists("users.txt") then
        File.ReadAllLines("users.txt")
        |> Array.toList
        |> List.map (fun line ->
            let parts = line.Split(',')
            {
                UserId = int parts.[0]
                Username = parts.[1]
                Password = parts.[2]
            })
    else
        []

// ========== HELPER: Load Seats from File ==========
let LoadSeats() =
    if File.Exists("seats.txt") then
        File.ReadAllLines("seats.txt")
        |> Array.toList
        |> List.map (fun line ->
            let parts = line.Split(',')
            {
                SeatId = int parts.[0]
                RowNumber = int parts.[1]
                SeatNumber = int parts.[2]
                IsReserved = bool.Parse(parts.[3])
            })
    else
        []

// ========== HELPER: Load Tickets from File ==========
let LoadTickets() =
    if File.Exists("tickets.txt") then
        File.ReadAllLines("tickets.txt")
        |> Array.toList
        |> List.map (fun line ->
            let parts = line.Split(',')
            {
                TicketId = parts.[0]
                SeatId = int parts.[1]
                UserId = int parts.[2]
                BookingDate = System.DateTime.Parse(parts.[3])
            })
    else
        []