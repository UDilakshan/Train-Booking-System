using System.Security.Cryptography;

namespace RailwayReservation.Domain.Booking;

public static class BookingReferenceGenerator
{
    // Unambiguous alphabet (no 0/O/1/I) for references read aloud at a ticket counter.
    private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    private const int Length = 8;

    public static string Generate()
    {
        var chars = new char[Length];
        for (var i = 0; i < Length; i++)
        {
            chars[i] = Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)];
        }
        return $"RWY-{new string(chars)}";
    }
}
