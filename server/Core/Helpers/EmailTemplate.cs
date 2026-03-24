namespace Core.Helpers
{
    public static class EmailTemplate
    {
        public static string GetWelcomeTemplate(string username) => $@"
        <div style='font-family: sans-serif; max-width: 600px; margin: auto; border: 1px solid #eee; padding: 20px;'>
            <h2 style='color: #0d6efd;'>Welcome to ChatApp, {username}!</h2>
            <p>We're excited to have you on board. Your account has been successfully created.</p>
            <hr style='border: 0; border-top: 1px solid #eee;' />
            <p style='font-size: 0.8em; color: #666;'>If you didn't sign up for this account, please ignore this email.</p>
        </div>";

        public static string GetNewMessageTemplate(string receiver, string sender, string messageContent) => $@"
            <div style='font-family: sans-serif; max-width: 600px; margin: auto; border: 1px solid #eee; padding: 20px;'>
                <h3 style='color: #0d6efd;'>You have a new message on ChatApp!</h3>
                <p>Hello <strong>{receiver}</strong>,</p>
                <p>User <strong>{sender}</strong> has sent you a message:</p>
                <div style='background: #f8f9fa; padding: 15px; border-left: 4px solid #0d6efd; margin: 10px 0;'>
                    <em>""{messageContent}""</em>
                </div>
                <a href='http://localhost:4200' style='display: inline-block; padding: 10px 20px; background: #0d6efd; color: white; text-decoration: none; border-radius: 5px; margin-top: 10px;'>Reply</a>
            </div>";
    }
}
