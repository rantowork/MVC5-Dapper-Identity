namespace DapperIdentity.Core.Entities
{
    /// <summary>
    /// Simple object used for account confirmation email examples.
    /// </summary>
    public class ConfirmationToken
    {
        public string Token { get; set; }
        public string Email { get; set; }
    }
}
