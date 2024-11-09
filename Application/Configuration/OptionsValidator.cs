using MiniValidation;

namespace Application.Configuration;

internal static class OptionsValidator
{
    internal static bool Validate<TModel>(this TModel model)
    {
        bool valid = MiniValidator.TryValidate(model, out IDictionary<string, string[]> errors);
        
        valid = valid && errors.Count == 0;
        if (valid) return valid;
        
        Console.WriteLine($"{typeof(TModel).Name} has one or more vailidation errors.\n" +
                          $"Check the configuration file(s):");
        foreach (KeyValuePair<string, string[]> entry in errors)
        {
            Console.WriteLine($"  {entry.Key}:");
            foreach (string error in entry.Value)
            {
                Console.WriteLine($"  - {error}");
            }
        }
        
        Environment.Exit(1);
        
        return valid;
    }
}