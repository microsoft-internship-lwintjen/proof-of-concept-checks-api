using System;
using System.Collections;
using System.IO;
using Octokit;
namespace SampleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var github = new GitHubClient(new ProductHeaderValue("SampleApp"));
            //github.Check.Suite.Create()
            string tokenPath = @"C:\Windows\System\.env";
            DotNetEnv.Env.Load(tokenPath);
            try
            {
                string token = DotNetEnv.Env.GetString("API_GH_PERSONAL_TOKEN", "Variable not found");
                if (token.CompareTo("Variable not found") == 0)
                {
                    throw new FileNotFoundException("The github token was not found in your .env file, please make sure that the key 'API_GH_PERSONAL_TOKEN' is set in your .env file (C:\\Windows\\System\\.env)");
                }
            } catch (FileNotFoundException e)
            {
                Console.WriteLine(e);
                Environment.Exit(0);

            }

        }
    }
}
