using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Octokit;
namespace SampleApp
{
    class Program
    {
        // Method to find a repository in an array of repositories based on the name 
        static Repository findRepository(string repoName, IReadOnlyList<Repository> repositories)
        {
            foreach (var repo in repositories)
            {
                if (repo.Name.CompareTo(repoName) == 0)
                {
                    return repo;
                }
            }
            return null;
        }

        // Method to find a pull request in an array of pull requests based on the name 
        static PullRequest findPR(string PRName, IReadOnlyList<PullRequest> pullRequests)
        {
            foreach (var PR in pullRequests)
            {
                if (PR.Title.CompareTo(PRName) == 0)
                {
                    return PR;
                }
            }
            return null;
        }
        static async Task Main(string[] args)
        {
            var client = new GitHubClient(new ProductHeaderValue("SampleApp"));
            //github.Check.Suite.Create()
            string tokenPath = @"C:\Windows\System\.env";

            DotNetEnv.Env.Load(tokenPath);
            try
            {
                string token = DotNetEnv.Env.GetString("API_GH_PERSONAL_TOKEN", "Variable not found");

                Console.WriteLine("Token found, setting up the token...");

                var tokenAuth = new Credentials(token); 
                client.Credentials = tokenAuth;

                Console.WriteLine("Finding the current authenticated user...");

                var user = await client.User.Current();

                Console.Write("Please, enter the name of the organization you want to find the PR in : ");
                string orgName = Console.ReadLine();

                var org = new Organization();
                try
                {
                    org = await client.Organization.Get(orgName);
                } catch (NotFoundException e)
                {
                    Console.WriteLine("Couldn't find any matching organization with that name");
                    Environment.Exit(0);
                }

                var repoOrg = await client.Repository.GetAllForOrg(org.Login);


                // If there are 0 repo in the org, then there's no point running this program
                if (repoOrg.Count == 0) 
                {
                    Console.WriteLine("The given organization has no repository in it, please create a repo before trying to use this program.");
                    Environment.Exit(0);
                }


                Console.Write("Please, enter the name of the repository you want to find the PR in (the repo has to be in your organization) : ");
                string repoName = Console.ReadLine();

                Repository repo = findRepository(repoName, repoOrg);

                // Keep asking for a valid input if we don't find any matching repo in the given org
                while (repo == null)
                {
                    Console.WriteLine($"We couldn't find any matching repository in {orgName} organization, please re-enter you repository name.");
                    Console.WriteLine("If you wan't to leave the program, please type in 'Exit'");
                    repoName = Console.ReadLine();
                    if (repoName.CompareTo("Exit") == 0)
                    {
                        Environment.Exit(0);
                    }
                    repo = findRepository(repoName, repoOrg);
                }

                Console.Write($"Please, enter the id of the PR in {repoName} (you'll find it in the url of the PR '/pull/[id]') : ");
                int PRId = Convert.ToInt32(Console.ReadLine());

                var AllPRs = await client.PullRequest.GetAllForRepository(repo.Id);

                PullRequest PR = AllPRs[PRId - 1];

                Console.WriteLine(PR.Title);
                //client.Check.Suite.Create(user.Login,)


            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e);
                Environment.Exit(0);
            }

        }
    }
}
