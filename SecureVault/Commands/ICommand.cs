namespace SecureVault.Commands
{
    public interface ICommand
    {
        // Name of the command
        string Name { get; }
        // Description of the command
        string Description { get; }
        // Execute the command
        Task<int> ExecuteAsync(string[] args);
    }
}