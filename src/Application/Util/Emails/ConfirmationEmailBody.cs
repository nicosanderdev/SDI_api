namespace Sdi_Api.Application.Util.EmailBodies;

public static class ConfirmationEmailBody
{
    public static string GetBody(string confirmationLink)
    {
        return $@"
            <html>
                <body>
                    <h1>Confirm Your Email Address</h1>
                    <p>Thank you for registering with us! Please confirm your email address by clicking the link below:</p>
                    <a href='{confirmationLink}'>Confirm Email</a>
                    <p>If you did not register, please ignore this email.</p>
                </body>
            </html>";
    }
}
