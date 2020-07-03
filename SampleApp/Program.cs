using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using GitHubJwt;
using Octokit;
namespace SampleApp
{
    class Program
    {
        static string gitHubAppName = "SampleApp";
        static int githubAppId = 71333;
        static int installationId = 10239152;
        static string githubPrivateKeyPath = @".\checks-api-pocca.2020-07-02.private-key.pem";
        
        public static IGitHubClient GetGitHubClient()
        {
            // TODO: cache with 5 min TTL
            var jwtToken = GetJWTToken();
            if (string.IsNullOrEmpty(jwtToken))
            {
                throw new NullReferenceException("Unable to generate token");
            }
            // Pass the JWT as a Bearer token to Octokit.net
            GitHubClient appClient = new GitHubClient(new ProductHeaderValue(gitHubAppName))
            {
                Credentials = new Credentials(jwtToken, AuthenticationType.Bearer)
            };
            return appClient;
        }

        public static async Task<IGitHubClient> GetGitHubInstallationClient(int installationId)
        {
            var appClient = GetGitHubClient();
            // TODO: cache with 5 min TTL
            var installationToken = await appClient.GitHubApps.CreateInstallationToken(installationId);
            // create a client with the installation token
            var installationClient = new GitHubClient(new ProductHeaderValue($"{gitHubAppName}_{installationId}"))
            {
                Credentials = new Credentials(installationToken.Token)
            };
            return installationClient;
        }

        public static string GetJWTToken()
        {
            var factoryOptions = new GitHubJwtFactoryOptions
            {
                AppIntegrationId = githubAppId, // The GitHub App Id
                ExpirationSeconds = 9 * 60 // 10 minutes is the maximum time allowed, but because exp claim is absolute time, lets have it 9 minutes to cover time skew
            };

            var generator = new GitHubJwtFactory(new FilePrivateKeySource(githubPrivateKeyPath), factoryOptions);
            return generator.CreateEncodedJwtToken();
        }

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

        static async Task Main(string[] args)
        {
            Console.WriteLine("Setting up the GitHub client...");
            var client = await GetGitHubInstallationClient(installationId);

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

            while (true)
            {
                Console.WriteLine("What do you want to do (type in the number of the action) ?\n " +
                    "1. Create a new check run \n " +
                    "2. Update an existing check run \n " +
                    "3. Leave the program");
                string action = Console.ReadLine();

                // Creating a new CR
                if (action.CompareTo("1") == 0)
                {
                    Console.Write("Please type in the name of your check run : ");
                    string CRName = Console.ReadLine();

                    var newCheckRun = new NewCheckRun(CRName, PR.Head.Sha);
                    var CR = await client.Check.Run.Create(repo.Id, newCheckRun);

                }

                // Updating an existing one
                else if (action.CompareTo("2") == 0)
                {
                    var CRUpdate = new CheckRunUpdate();

                    Console.Write("Type in the id of the CR ('/pull/[pull_request_id]/checks?check_run_id=[check_run_id]') : ");
                    long CRId = long.Parse(Console.ReadLine());

                    Console.Write("Please, type in the status (queued, in_progress, completed) of check run {0} : ", CRId );
                    string status = Console.ReadLine();

                    CRUpdate.Status = status;
                    
                    // if completed then need extra infos
                    if (status.CompareTo("completed") == 0)
                    {
                        Console.Write("Result of the check (success, failure, neutral, cancelled, timed_out, action_required) : ");
                        string checkRunResult = Console.ReadLine();

                        DateTime now = DateTime.Now;

                        CRUpdate.Conclusion = checkRunResult;
                        CRUpdate.CompletedAt = now;

                    }

                    await client.Check.Run.Update(repo.Id, CRId, CRUpdate);

                    Console.WriteLine("The check run {0} has been {1}", CRId, status);
                }

                // Leaving the program
                else if (action.CompareTo("3") == 0)
                {
                    Console.WriteLine("Thanks for using the program, exiting...");
                    Environment.Exit(0);
                }
            }

        }

        }
}
