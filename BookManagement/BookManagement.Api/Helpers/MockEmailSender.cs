namespace BookManagement.Api.Helpers;

public static class MockEmailSender
{
    public static void SendOtpEmail(string email, string otp)
    {
        Console.WriteLine("\n=======================================================");
        Console.WriteLine("📧 MOCK EMAIL SENDER");
        Console.WriteLine("=======================================================");
        Console.WriteLine($"To: {email}");
        Console.WriteLine($"Subject: Ma xac thuc OTP cua ban");
        Console.WriteLine($"\nXin chao,");
        Console.WriteLine($"Ma xac thuc OTP (co hieu luc trong 5 phut) cua ban la: [{otp}]");
        Console.WriteLine("Vui long khong chia se ma nay cho bat ky ai.");
        Console.WriteLine("=======================================================\n");
    }
}
